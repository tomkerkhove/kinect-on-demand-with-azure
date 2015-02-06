using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using K4W.KinectVOD.Client.Shared.Extensions;
using K4W.KinectVOD.Client.WPF.MicrosoftAzure;
using K4W.KinectVOD.Client.WPF.Processors;
using K4W.KinectVOD.Shared;
using Microsoft.Kinect;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace K4W.KinectVOD.Client.WPF
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Representation of the Kinect Sensor
        /// </summary>
        private KinectSensor _kinect = null;

        /// <summary>
        /// Size fo the RGB pixel in bitmap
        /// </summary>
        private readonly int _bytePerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;

        /// <summary>
        /// Array of color pixels
        /// </summary>
        private byte[] _colorPixels = null;

        /// <summary>
        /// FrameReader for our coloroutput
        /// </summary>
        private ColorFrameReader _colorReader = null;

        /// <summary>
        /// Color WriteableBitmap linked to our UI
        /// </summary>
        private WriteableBitmap _colorBitmap = null;

        /// <summary>
        /// Current count of the image
        /// </summary>
        private int _sequenceNr = 1;

        /// <summary>
        /// Indication whether we are recording
        /// </summary>
        private bool _isRecording = false;

        /// <summary>
        /// Unique recording ID
        /// </summary>
        private string _recordingID = string.Empty;

        /// <summary>
        /// Media Services agent (Microsoft Azure Media Services)
        /// </summary>
        private MediaServicesAgent _mediaAgent = new MediaServicesAgent(ConfigurationManager.AppSettings.Get("MediaAccount"), ConfigurationManager.AppSettings.Get("MediaKey"));

        /// <summary>
        /// Notification Agent (Microsoft Azure Notification Hubs)
        /// </summary>
        private NotificationHubAgent _notificationAgent = new NotificationHubAgent(ConfigurationManager.AppSettings.Get("NotificationHub"), ConfigurationManager.ConnectionStrings["servicebus-ns"].ConnectionString);

        public MainWindow()
        {
            InitializeComponent();

            // Initialize Kinect
            InitializeKinect();

            // Recording action 
            StartRecordingButton.Click += OnStartRecordingHandler;
            StopRecordingButton.Click += OnStopRecordingHandler;
        }

        #region Kinect + Color
        /// <summary>
        /// Initialize Kinect sensor
        /// </summary>
        private void InitializeKinect()
        {
            // Get first Kinect
            _kinect = KinectSensor.GetDefault();

            if (_kinect == null) return;

            // Hook-up availability event
            _kinect.IsAvailableChanged += OnKinectAvailabilityChanged;

            // Setup initial controls
            if (_kinect.IsAvailable == false)
            {
                StartRecordingButton.IsEnabled = false;
                Status.Content = "Kinect is unavailable.";
                KinectCamera.Visibility = Visibility.Collapsed;
                KinectUnavailable.Visibility = Visibility.Visible;
            }

            // Open connection
            _kinect.Open();

            // Get frame description for the color output
            FrameDescription desc = _kinect.ColorFrameSource.FrameDescription;

            // Get the framereader for Color
            _colorReader = _kinect.ColorFrameSource.OpenReader();

            // Allocate pixel array
            _colorPixels = new byte[desc.Width * desc.Height * _bytePerPixel];

            // Create new WriteableBitmap
            _colorBitmap = new WriteableBitmap(desc.Width, desc.Height, 96, 96, PixelFormats.Bgr32, null);

            // Link WBMP to UI
            KinectCamera.Source = _colorBitmap;

            // Hook-up event
            _colorReader.FrameArrived += OnColorFrameArrived;
        }

        /// <summary>
        /// Handle the availability change o the sensor
        /// </summary>
        private async void OnKinectAvailabilityChanged(object sender, IsAvailableChangedEventArgs e)
        {
            if (e.IsAvailable == false)
            {
                // Update status
                Status.Content = "Kinect is unavailable.";

                if (_isRecording)
                {
                    // Stop recording and render as-is
                    await StopRecording();
                }
                else
                {
                    // Disable recording
                    StartRecordingButton.IsEnabled = false;
                }

                // Update UI
                KinectCamera.Visibility = Visibility.Collapsed;
                KinectUnavailable.Visibility = Visibility.Visible;
            }
            else
            {
                // Update status
                Status.Content = "Kinect is available.";

                // Update UI
                StartRecordingButton.IsEnabled = true;
                KinectCamera.Visibility = Visibility.Visible;
                KinectUnavailable.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Process each color frame
        /// </summary>
        private async void OnColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            // Get the reference to the color frame
            ColorFrameReference colorRef = e.FrameReference;

            if (colorRef == null) return;

            // Acquire frame for specific reference
            ColorFrame frame = colorRef.AcquireFrame();

            // It's possible that we skipped a frame or it is already gone
            if (frame == null) return;

            using (frame)
            {
                // Get frame description
                FrameDescription frameDesc = frame.FrameDescription;

                // Check if width/height matches
                if (frameDesc.Width == _colorBitmap.PixelWidth && frameDesc.Height == _colorBitmap.PixelHeight)
                {
                    // Copy data to array based on image format
                    if (frame.RawColorImageFormat == ColorImageFormat.Bgra)
                    {
                        frame.CopyRawFrameDataToArray(_colorPixels);
                    }
                    else frame.CopyConvertedFrameDataToArray(_colorPixels, ColorImageFormat.Bgra);

                    // Copy output to bitmap
                    _colorBitmap.WritePixels(
                            new Int32Rect(0, 0, frameDesc.Width, frameDesc.Height),
                            _colorPixels,
                            frameDesc.Width * _bytePerPixel,
                            0);

                    // Save image when recording
                    if (_isRecording)
                    {
                        // Create a new byte-array
                        byte[] imageData = new byte[_colorPixels.Length];

                        // Copy the orginal array in the new one
                        Array.Copy(_colorPixels, imageData, _colorPixels.Length);

                        // Save the image in the local folder
                        await ImageProcessor.SaveJpegAsync(imageData, frameDesc.Width, frameDesc.Height, frameDesc.Width * _bytePerPixel, TemporaryFolder.Text, string.Format("{0}_{1:000000}", _recordingID, _sequenceNr));

                        // Increment the sequence number
                        _sequenceNr++;
                    }
                }
            }
        }
        #endregion Kinect + Color

        #region Start/Stop Recording
        /// <summary>
        /// Handle when recording is requested
        /// </summary>
        private void OnStartRecordingHandler(object sender, RoutedEventArgs e)
        {
            StartRecording();
        }

        /// <summary>
        /// Handle when stop recording is requested
        /// </summary>
        private async void OnStopRecordingHandler(object sender, RoutedEventArgs e)
        {
            await StopRecording();
        }

        /// <summary>
        /// Start recording
        /// </summary>
        private void StartRecording()
        {
            // Validate temporary folder
            if (ValidateTemporaryFolder() == false) return;

            // Setup recording
            _sequenceNr = 1;
            _isRecording = true;
            _recordingID = Guid.NewGuid().ToString();

            // Update status
            Status.Content = "Recording...";

            // Toggle controls
            VideoCaption.IsReadOnly = true;
            TemporaryFolder.IsReadOnly = true;
            StartRecordingButton.IsEnabled = !_isRecording;
            StopRecordingButton.IsEnabled = _isRecording;
        }

        /// <summary>
        /// Stop recording
        /// </summary>
        private async Task StopRecording()
        {
            // Stop recording
            _isRecording = false;

            // Disable stop controls
            StopRecordingButton.IsEnabled = false;

            // Process the recorded frames
            await ProcessFrames();

            // Reset caption & Enable start
            VideoCaption.Text = string.Empty;
            VideoCaption.IsReadOnly = false;
            TemporaryFolder.IsReadOnly = false;
            StartRecordingButton.IsEnabled = true;
        }

        /// <summary>
        /// Validate the specified temporary folder
        /// </summary>
        /// <returns></returns>
        private bool ValidateTemporaryFolder()
        {
            bool isValid = true;

            if (string.IsNullOrEmpty(TemporaryFolder.Text))
            {
                MessageBox.Show("The specified temporary folder is invalid!\nPlease specify a folder.",
                    "Invalid Temporary Folder", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                isValid = false;
            }

            if (TemporaryFolder.Text.Substring(TemporaryFolder.Text.Length - 1, 1) != @"\")
            {
                MessageBox.Show("The specified temporary folder should end with a '\\'.",
                        "Invalid Temporary Folder", MessageBoxButton.OK, MessageBoxImage.Information);
                isValid = false;
            }

            return isValid;
        }
        #endregion Start/Stop Recording

        #region Render, Upload, Encode, Package & Deliver
        /// <summary>
        /// Process recorded frames 
        /// </summary>
        private async Task ProcessFrames()
        {
            Status.Content = "Starting video render...";

            // Render video locally
            string videoPath =
                await VideoProcessor.RenderVideoAsync(15, 1920, 1080, 100, TemporaryFolder.Text, _recordingID);

            // Save recording timestamp
            DateTime recordedStamp = DateTime.Now;

            Status.Content = "Done rendering video.";

            // Host video in Microsoft Azure
            string streamUrl = await HostVideoInAzure(videoPath);

            Status.Content = "Video is available on-demand.";

            // Send notifications to clients
            await SendNotification(streamUrl, recordedStamp);

            // Remove saved images & local video afterwards
            await RemoveLocalAssets();
        }

        /// <summary>
        /// Upload the rendered video to the cloud, encode to MP4 and deliver as Smooth Stream
        /// </summary>
        /// <param name="videoPath">Path to the local video</param>
        private async Task<string> HostVideoInAzure(string videoPath)
        {
            Status.Content = "Starting video upload...";

            // Upload the video as an Asset
            IAsset rawAsset = await _mediaAgent.UploadAsset(videoPath, UploadAssetHandler);

            Status.Content = "Starting encoding & packaging...";

            // Encode & Package in Media Services
            IJob encodedAssetId = await _mediaAgent.EncodeAndPackage(string.Format("Encoding '{0}' into Mp4 & package to SS", rawAsset.Name), rawAsset, JobStateChangedHandler);

            Status.Content = "Creating locator endpoint...";

            // Create a new Smooth Streaming Locator
            Uri ssUri = _mediaAgent.CreateNewSsLocator(encodedAssetId.OutputMediaAssets[1], LocatorType.OnDemandOrigin, AccessPermissions.Read, TimeSpan.FromDays(7));

            return ssUri.ToString();
        }

        #region Progress Handlers
        /// <summary>
        /// Handles the progress of the Asset upload
        /// </summary>
        private void UploadAssetHandler(object sender, UploadProgressChangedEventArgs e)
        {
            Dispatcher.Invoke(() => Status.Content = string.Format("Uploading Asset - {0}%", Math.Round(e.Progress, 0)));
        }

        /// <summary>
        /// Handles the progress of the encoding & packaging of the asset
        /// </summary>
        private void JobStateChangedHandler(object sender, JobStateChangedEventArgs e)
        {
            Dispatcher.Invoke(() => Status.Content = string.Format("Job is currently {0}", e.CurrentState));
        }
        #endregion Progress Handlers

        /// <summary>
        /// Send the streaming URL & caption to the clients
        /// </summary>
        /// <param name="streamUrl">Url of the stream</param>
        private async Task SendNotification(string streamUrl, DateTime stamp)
        {
            // Create metadata for the client (will be used in the launch-property of the tile)
            RecordingData recordingData = new RecordingData(VideoCaption.Text, streamUrl, _recordingID, stamp);

            // Assign properties for the notification
            Dictionary<string, string> properties = new Dictionary<string, string>()
            {
                {"Caption", recordingData.Caption},
                {"SmoothStreamUrl", recordingData.SmoothStreamUrl},
                {"RecordingId", recordingData.RecordingId},
                {"RecordingStamp", recordingData.RecordingStamp.ToString()},
                {"RecordingData", recordingData.SerializeToJson()}
            };

            // Send the notification
            await _notificationAgent.SendTemplateNotificationAsync(properties);
        }

        /// <summary>
        /// Remove saved images & local video in temporary folder
        /// </summary>
        private async Task RemoveLocalAssets()
        {
            string tempFolder = TemporaryFolder.Text;
            Task cleanupT = Task.Run(() =>
            {
                foreach (string file in Directory.GetFiles(tempFolder, string.Format("{0}*", _recordingID)))
                {
                    File.Delete(file);
                }
            });

            await cleanupT;
        }
        #endregion Render, Upload, Encode, Package & Deliver
    }
}
