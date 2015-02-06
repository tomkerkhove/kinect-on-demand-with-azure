using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Microsoft.Kinect;

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

        public MainWindow()
        {
            InitializeComponent();

            // Initialize Kinect
            InitializeKinect();

            // Recording action 
            StartRecordingButton.Click += OnStartRecordingHandler;
            StopRecordingButton.Click += OnStopRecordingHandler;
        }

        /// <summary>
        /// Initialize Kinect sensor
        /// </summary>
        private void InitializeKinect()
        {
            // Get first Kinect
            _kinect = KinectSensor.GetDefault();

            if (_kinect == null) return;

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
        /// Process each color frame
        /// </summary>
        private void OnColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
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
                }
            }
        }

        /// <summary>
        /// Handle when recording is requested
        /// </summary>
        private void OnStartRecordingHandler(object sender, RoutedEventArgs e)
        {
            // Validate temporary folder
            if (ValidateTemporaryFolder()) return;

            // TODO: Implementation

            // Toggle controls
            VideoCaption.IsReadOnly = true;
            TemporaryFolder.IsReadOnly = true;
            StartRecordingButton.IsEnabled = false;
            StopRecordingButton.IsEnabled = true;
        }

        /// <summary>
        /// Handle when stop recording is requested
        /// </summary>
        private void OnStopRecordingHandler(object sender, RoutedEventArgs e)
        {
            // Toggle controls
            StartRecordingButton.IsEnabled = true;
            StopRecordingButton.IsEnabled = false;

            // TODO: Implementation

            // Reset caption
            VideoCaption.Text = string.Empty;
            VideoCaption.IsReadOnly = false;
            TemporaryFolder.IsReadOnly = false;
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
                MessageBox.Show("The specified temporary folder should end with a '\'.",
                        "Invalid Temporary Folder", MessageBoxButton.OK, MessageBoxImage.Information);
                isValid = false;
            }

            return isValid;
        }
    }
}
