using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace K4W.KinectVOD.Client.WPF.MicrosoftAzure
{
    public class MediaServicesAgent
    {
        /// <summary>
        /// Media services credentials
        /// </summary>
        private MediaServicesCredentials _mediaCredentials;

        /// <summary>
        /// Media Context
        /// </summary>
        private CloudMediaContext _mediaContext;

        /// <summary>
        /// Default CTOR
        /// </summary>
        /// <param name="mediaAccount"></param>
        /// <param name="mediaKey"></param>
        public MediaServicesAgent(string mediaAccount, string mediaKey)
        {
            _mediaCredentials = new MediaServicesCredentials(mediaAccount, mediaKey);
            _mediaContext = new CloudMediaContext(_mediaCredentials);
        }

        /// <summary>
        /// Upload an Asset
        /// </summary>
        /// <param name="filePath">Path of the video</param>
        /// <param name="uploadHandler">Handler to retrieve the progress</param>
        /// <returns></returns>
        public async Task<IAsset> UploadAsset(string filePath, EventHandler<UploadProgressChangedEventArgs> uploadHandler = null)
        {
            Task<IAsset> uploadTask = Task.Run(() =>
            {
                // Retrieve filename
                string assetName = Path.GetFileName(filePath);

                // Create a new asset in the context
                IAsset asset = _mediaContext.Assets.Create(assetName, AssetCreationOptions.None);

                // Create a new asset file
                IAssetFile file = asset.AssetFiles.Create(assetName);

                // Hook-up the event if handler is specified
                if (uploadHandler != null)
                    file.UploadProgressChanged += uploadHandler;

                // Upload the video
                file.Upload(filePath);

                return asset;
            });

            await uploadTask;

            return uploadTask.Result;

            // Snippet when you want to use the Microsoft Azure Media Services extensions
            //return await _mediaContext.Assets.CreateFromFileAsync(filePath, AssetCreationOptions.None, cancellationToken);
        }

        /// <summary>
        /// Encode the raw asset to H264 MP4 and package into a Smooth Stream
        /// </summary>
        /// <param name="jobName">Name of the job</param>
        /// <param name="rawAsset">Raw asset representing the rendered video</param>
        /// <returns></returns>
        public async Task<IJob> EncodeAndPackage(string jobName, IAsset rawAsset, EventHandler<JobStateChangedEventArgs> jobHandler = null)
        {
            Task<IJob> t = Task.Run(() =>
            {
                // Create a new job
                IJob job = _mediaContext.Jobs.Create(jobName);

                /* Taks I - Encode into MP4
                   Retrieve the encoder */
                IMediaProcessor latestWameMediaProcessor = (from p in _mediaContext.MediaProcessors
                                                            where p.Name == "Windows Azure Media Encoder"
                                                            select p).ToList().OrderBy(wame => new Version(wame.Version)).LastOrDefault();

                // Select the requested preset (Same as in the portal)
                string encodingPreset = "H264 Adaptive Bitrate MP4 Set SD 16x9";

                // Add a new task to the job for the encoding
                ITask encodeTask = job.Tasks.AddNew("Encoding", latestWameMediaProcessor, encodingPreset, TaskOptions.None);

                // Add our rendered video as input
                encodeTask.InputAssets.Add(rawAsset);

                // Add a new asset as output
                encodeTask.OutputAssets.AddNew(rawAsset.Name + "_MP4", AssetCreationOptions.None);


                /* Taks II - Package into Smooth Streaming
                   Retrieve the packager */
                IMediaProcessor latestPackagerMediaProcessor = (from p in _mediaContext.MediaProcessors
                                                                where p.Name == "Windows Azure Media Packager"
                                                                select p).ToList().OrderBy(wame => new Version(wame.Version)).LastOrDefault();

                // Read the config from XML
                string SSConfig = File.ReadAllText(Path.GetFullPath(@"D:\Source Control\Kinect for Windows\Second Generation Kinect\Kinect - VOD Media Services\K4W.KinectVOD\K4W.KinectVOD.Client.WPF\Assets\Media_Services_MP4_to_Smooth_Streams.xml"));

                // Add a new packaging task
                ITask packagingSSTask = job.Tasks.AddNew("Packing into Smooth Streaming", latestPackagerMediaProcessor, SSConfig, TaskOptions.None);

                // Add the output of the encoding
                packagingSSTask.InputAssets.Add(encodeTask.OutputAssets[0]);

                // Create a new output Asset
                packagingSSTask.OutputAssets.AddNew("Result_SS_" + rawAsset.Name, AssetCreationOptions.None);

                // Hook-up the handler if reaquired
                if (jobHandler != null)
                    job.StateChanged += jobHandler;

                // Submit the job
                job.Submit();

                // Execute
                job.GetExecutionProgressTask(CancellationToken.None).Wait();

                return job;
            });

            await t;

            return t.Result;
        }

        /// <summary>
        /// Create a new locator
        /// </summary>
        /// <param name="packagedAsset">Smooth Streaming Asset</param>
        /// <param name="locatorType">Type of requested delivery mode (Storage or Media Services)</param>
        /// <param name="accessPermissions">Access permissions on the locator</param>
        /// <param name="duration">Period of time that the resource is available</param>
        /// <returns>Uri to the asset</returns>
        public Uri CreateNewSsLocator(IAsset packagedAsset, LocatorType locatorType, AccessPermissions accessPermissions, TimeSpan duration)
        {
            if (packagedAsset == null) throw new Exception("Invalid encoded asset");

            // Create a new access policy to the video
            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("Streaming policy", duration, accessPermissions);

            // Create a new locator to that resource with our new policy
            _mediaContext.Locators.CreateLocator(locatorType, packagedAsset, policy);

            // Return the uri by using the extensions package
            return packagedAsset.GetSmoothStreamingUri();
        }
    }
}
