using Newtonsoft.Json;

namespace K4W.KinectVOD.Client.Shared.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        ///  Converts an object to JSON
        /// </summary>
        public static string SerializeToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        ///  Converts JSON to an object
        /// </summary>
        public static T DeserializeFromJson<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
