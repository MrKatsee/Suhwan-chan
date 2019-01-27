using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ResourceManager : MonoBehaviour {

    private static string path = string.Empty;
    /*
    public static void SaveFile(string _path, byte[] bytes)
    {
        string filePath = string.Format("{0}/{1}", path, _path);
        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                writer.BaseStream.Write(bytes, 0, bytes.Length);
            }
        }
        catch (IOException)
        {
            return;
        }
        return;
    }*/
    public static void SaveFile<T>(string _path, T t)
    {
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

    private void SetPath()
    {
        if (string.IsNullOrEmpty(path))
        {
            switch (Application.platform)
            {
                case RuntimePlatform.IPhonePlayer:
                    path = string.Format("file:///{0}", Application.streamingAssetsPath);
                    break;
                case RuntimePlatform.WindowsEditor:
                    path = string.Format("{0}/Data", Application.dataPath);
                    break;
                case RuntimePlatform.Android:
                    break;
                case RuntimePlatform.WindowsPlayer:
                    {
                        Directory.CreateDirectory(string.Format("{0}/Data", Application.dataPath));
                        Directory.CreateDirectory(string.Format("{0}/Data/User", Application.dataPath));
                        path = string.Format("{0}/Data", Application.dataPath);
                        break;
                    }
                default:
                    path = string.Empty;
                    break;
            }
        }
    }

    private void Awake()
    {
        SetPath();
    }

}
