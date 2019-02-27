using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MyServer
{
    static class ResourceManager
    {
        private static string path = Directory.GetCurrentDirectory() + "/Data";

        public static void SaveFile<T>(string _path, T t)
        {
            Directory.CreateDirectory(path +"/"+ _path.Substring(0, _path.LastIndexOf('/')));
            string filePath = string.Format("{0}/{1}", path, _path);
            try
            {
                StreamWriter writer = new StreamWriter(filePath, false);
                writer.WriteLine(JsonConvert.SerializeObject(t, Formatting.Indented));
                writer.Close();
            }
            catch (IOException)
            {
                return;
            }
            return;
        }

        public static T LoadFile<T>(string _path)
        {
            string filePath = string.Format("{0}/{1}", path, _path);
            string fileData = string.Empty;
            try
            {
                StreamReader streamReader = new StreamReader(filePath);
                fileData = streamReader.ReadToEnd();
                streamReader.Close();
            }
            catch (IOException)
            {
                return default(T);
            }
            T t = JsonConvert.DeserializeObject<T>(fileData);
            return t;
        }

        public static bool CheckFile(string _path)
        {
            return File.Exists(string.Format("{0}/{1}", path, _path));
        }
    }
}
