using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace K4W.KinectVOD.Client.WPF.Processors
{
    public class ImageProcessor
    {
        /// <summary>
        /// Save a buffer as a JPEG
        /// </summary>
        /// <param name="data">Image data</param>
        /// <param name="width">Width of the image</param>
        /// <param name="height">Height of the image</param>
        /// <param name="stride">Stride of the image</param>
        /// <param name="folder">Output folder</param>
        /// <param name="filename">Filename</param>
        public static async Task SaveJpegAsync(byte[] data, int width, int height, int stride, string folder, string filename)
        {
            Task saveJpegTask = Task.Run(() =>
            {
                if (data != null)
                {
                    // Create a new bitmap
                    WriteableBitmap bmp = new WriteableBitmap(width, height, 96.0, 96.0, PixelFormats.Bgr32, null);

                    // write pixels to bitmap
                    bmp.WritePixels(new Int32Rect(0, 0, width, height), data, stride, 0);

                    // create jpg encoder from bitmap
                    JpegBitmapEncoder enc = new JpegBitmapEncoder();

                    // create frame from the writable bitmap and add to encoder
                    enc.Frames.Add(BitmapFrame.Create(bmp));

                    // Create whole path
                    string path = Path.Combine(folder, filename + ".jpg");

                    try
                    {
                        // write the new file to disk
                        using (FileStream fs = new FileStream(path, FileMode.Create))
                        {
                            enc.Save(fs);
                        }
                    }
                    catch (IOException ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error! Exception - " + ex.Message);
                    }
                }
            });

            await saveJpegTask;
        }
    }
}
