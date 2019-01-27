using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class MyDebug {

    public static void Log(object content)
    {
        if (Application.platform == RuntimePlatform.WindowsEditor) Debug.Log(content);
    }

}
