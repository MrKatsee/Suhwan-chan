using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MyUser
{
    public static MyUser ParseData(string data)
    {
        return JsonConvert.DeserializeObject<MyUser>(data);
    }

    public string ID { get; set; }
    public string PW { get; set; }
    public int Win { get; set; }
    public int Lose { get; set; }
    public float Rate
    {
        get
        {
            if (Win + Lose <= 0) return 100f;
            return (float)Win / (Win + Lose) * 100f;
        }
    }

    public bool IsAvaliable
    {
        get
        {
            if (!string.IsNullOrEmpty(ID) && !string.IsNullOrEmpty(PW)) return true;
            else return false;
        }
    }

    public MyUser()
    {
        ID = string.Empty;
        PW = string.Empty;
    }

    public MyUser(string _id, string _pw)
    {
        ID = _id;
        PW = _pw;
    }

    public bool CheckID(string _id)
    {
        return string.Equals(ID, _id);
    }

    public bool CheckPassword(string _pw)
    {
        return string.Equals(PW, _pw);
    }

    public string ToData()
    {
        return JsonConvert.SerializeObject(this, Formatting.None);
    }
}
