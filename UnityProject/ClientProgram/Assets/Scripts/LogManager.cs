using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LogManager : MonoBehaviour {

    private static string path = string.Empty;

    private static System.Object logLock = new object();
    public static void WriteLog(string content)
    {
        lock (logLock)
        {
            string log = string.Format("{0}:{1}", System.DateTime.Now.ToString("HH-mm-ss"), content);
            string filePath = string.Format("{0}/Log_{1}.txt", path, System.DateTime.Now.ToString("yyyy-MM-dd"));
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

    private void SetPath()
    {
        if (string.IsNullOrEmpty(path))
        {
            switch (Application.platform)
            {
                case RuntimePlatform.IPhonePlayer:
                    path = string.Format("file:///{0}/Log", Application.streamingAssetsPath);
                    break;
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    {
                        Directory.CreateDirectory(string.Format("{0}/Log", Application.dataPath));
                        path = string.Format("{0}/Log", Application.dataPath);
                        break;
                    }
                default:
                    path = string.Empty;
                    break;
            }
        }
    }

    private void Start()
    {
        SetPath();
    }

}
