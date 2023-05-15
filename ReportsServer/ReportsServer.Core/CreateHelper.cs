using Newtonsoft.Json;
using System.IO;

namespace ReportsServer.Core
{
    public static class CreateHelper
    {
        public static T Create<T>(string path)
        {
            using (var file = File.OpenText(path))
            {
                var serializer = new JsonSerializer();
                return (T) serializer.Deserialize(file, typeof(T));
            }
        }
    }
}