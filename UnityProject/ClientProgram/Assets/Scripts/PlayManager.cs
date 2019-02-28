using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayManager : MonoBehaviour{

    private static PlayManager instance = null;
    public static PlayManager Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new GameObject("PlayManager").AddComponent<PlayManager>();
                instance.Init();
                DontDestroyOnLoad(instance.gameObject);
            }
            return instance;
        }
    }

    public User user;

    private void Init()
    {
        
    }

}
