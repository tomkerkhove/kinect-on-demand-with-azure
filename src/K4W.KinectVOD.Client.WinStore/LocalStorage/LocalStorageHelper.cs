using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using K4W.KinectVOD.Client.Shared.Extensions;

namespace K4W.KinectVOD.Client.WinStore.LocalStorage
{
    public class LocalStorageHelper
    {
        /// <summary>
        /// Load a local file and retrieve the content
        /// </summary>
        /// <typeparam name="T">Requested result type</typeparam>
        /// <param name="fileName">Local filename</param>
        /// <returns>Local content</returns>
        public static async Task<T> LoadFileContentAsync<T>(string fileName)
        {
            try
            {
                StorageFile localFile = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
                return (localFile != null) ? (await FileIO.ReadTextAsync(localFile)).DeserializeFromJson<T>() : default(T);
            }
            catch (FileNotFoundException ex)
            {
                return default(T);
            }
        }

        /// <summary>
        /// Save content to a local file
        /// </summary>
        /// <typeparam name="T">Content Type</typeparam>
        /// <param name="fileName">Requested filename</param>
        /// <param name="content">Content to save</param>
        public static async Task SaveFileContentAsync<T>(string fileName, T content)
        {
            StorageFile localFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(localFile, content.SerializeToJson());
        }
    }
}
