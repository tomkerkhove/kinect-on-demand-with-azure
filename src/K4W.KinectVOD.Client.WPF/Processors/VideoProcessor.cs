using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using SharpAvi.Codecs;
using SharpAvi.Output;

namespace K4W.KinectVOD.Client.WPF.Processors
{
    public class VideoProcessor
    {
        /// <summary>
        /// Render a video based on JPEG-images
        /// </summary>
        /// <param name="fps">Requested frames-per-second</param>
        /// <param name="width">Width of the images</param>
        /// <param name="height">Height of the images</param>
        /// <param name="quality">Requested quality</param>
        /// <param name="path">Path to the folder containing frame-images</param>
        /// <param name="renderGuid">Unique GUID for this frame-batch</param>
        /// <returns>Path to the video</returns>
        public static async Task<string> RenderVideoAsync(int fps, int width, int height, int quality, string path, string renderGuid)
        {
            if(quality < 1 && quality > 100) throw new ArgumentException("Quality can only be between 1 and 100.");

            Task<string> renderT = Task.Run(() =>
            {
                // Compose output path
                string outputPath = string.Format("{0}/{1}.avi", path, renderGuid);

                // Create a new writer with the requested FPS
                AviWriter writer = new AviWriter(outputPath)
                {
                    FramesPerSecond = fps
                };

                // Create a new stream to process it
                IAviVideoStream stream = writer.AddEncodingVideoStream(new MotionJpegVideoEncoderWpf(width, height, quality));
                stream.Width = width;
                stream.Height = height;

                // Create an output stream
                byte[] frameData = new byte[stream.Width * stream.Height * 4];

                // Retrieve all iamges for this batch
                string[] images = Directory.GetFiles(path, string.Format("{0}*.jpg", renderGuid));

                // Process image per image
                foreach (string file in images)
                {
                    // Decode the bitmap
                    JpegBitmapDecoder decoder = new JpegBitmapDecoder(new Uri(file), BitmapCreateOptions.None, BitmapCacheOption.Default);

                    // Get bitmap source
                    BitmapSource source = decoder.Frames[0];
                    
                    // Copy pixels
                    source.CopyPixels(frameData, 1920 * 4, 0);

                    // Write it to the stream
                    stream.WriteFrame(true, frameData, 0, frameData.Length);
                }

                // Close writer
                writer.Close();

                return outputPath;
            });

            await renderT;

            return renderT.Result;
        }
    }
}
