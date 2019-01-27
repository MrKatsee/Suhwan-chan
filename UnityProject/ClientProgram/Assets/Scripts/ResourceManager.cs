using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ResourceManager : MonoBehaviour {

    private static string path = string.Empty;

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
                case RuntimePlatform.WindowsPlayer:
                    {
                        Directory.CreateDirectory(string.Format("{0}/Data", Application.persistentDataPath));
                        path = string.Format("{0}/Data", Application.persistentDataPath);
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
