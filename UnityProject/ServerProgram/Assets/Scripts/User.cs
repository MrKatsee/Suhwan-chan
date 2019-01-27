using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User
{
    public static string DIRECTORY = "User";

    public string ID { get; set; }
    public string PW { get; set; }
    public bool isAvaliable
    {
        get
        {
            if (!string.IsNullOrEmpty(ID) && !string.IsNullOrEmpty(PW)) return true;
            else return false;
        }
    }

    public User()
    {
        ID = string.Empty;
        PW = string.Empty;
    }

    public User(string _id, string _pw)
    {
        ID = _id;
        PW = _pw;
    }

    public bool CheckPassword(string _pw)
    {
        return string.Equals(PW, _pw);
    }

}
