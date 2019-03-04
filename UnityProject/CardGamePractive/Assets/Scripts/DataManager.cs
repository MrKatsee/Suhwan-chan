using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    //DB에서 찍어내는 역할

    private static DataManager instance = null;
    public static DataManager Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }


}
