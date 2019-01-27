using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager_Main : MonoBehaviour {

    public static UIManager_Main instance = null;

    public UI_Toast ui_Toast { get; private set; }
    public UI_Loading ui_Loading { get; private set; }
    public UI_Login ui_Login { get; private set; }

    private void Awake()
    {
        instance = this;
        ui_Login = transform.Find("UI_Login").GetComponent<UI_Login>();
        ui_Loading = transform.Find("UI_Loading").GetComponent<UI_Loading>();
        ui_Toast = transform.Find("UI_Toast").GetComponent<UI_Toast>();
    }

}
