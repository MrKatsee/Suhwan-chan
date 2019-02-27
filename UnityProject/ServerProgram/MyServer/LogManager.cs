using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServer
{
    static class LogManager
    {
        private static string path = Directory.GetCurrentDirectory() + "/Log";

        private static System.Object logLock = new object();
        public static void WriteLog(string content)
        {
            Console.WriteLine(content);
            Directory.CreateDirectory(path);
            string log = string.Format("{0}:{1}", System.DateTime.Now.ToString("HH-mm-ss"), content);
            string filePath = string.Format("{0}/Log_{1}.txt", path, System.DateTime.Now.ToString("yyyy-MM-dd"));
            lock (logLock)
            {
                try
                {
                    using (StreamWriter writer =
                        (!File.Exists(filePath)) ? File.CreateText(filePath) : File.AppendText(filePath))
                    {
                        writer.WriteLine(log);
                    }
                }
                catch (IOException) { }
            }
        }
    }
}
