using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_Main : MonoBehaviour {

    public static UIManager_Main instance = null;

    public UI_Toast ui_Toast { get; private set; }
    public UI_Loading ui_Loading { get; private set; }
    public UI_Login ui_Login { get; private set; }

    private Text text_ID;
    private Text text_Win;
    private Text text_WinRate;

    private void Awake()
    {
        instance = this;
        ui_Login = transform.Find("UI_Login").GetComponent<UI_Login>();
        ui_Loading = transform.Find("UI_Loading").GetComponent<UI_Loading>();
        ui_Toast = transform.Find("UI_Toast").GetComponent<UI_Toast>();

        text_ID = transform.Find("Text_ID").GetComponent<Text>();
        text_Win = transform.Find("Text_Win").GetComponent<Text>();
        text_WinRate = transform.Find("Text_WinRate").GetComponent<Text>();

    }

    public void SetText_UserInfo(User _user)
    {
        text_ID.text = _user.ID;
        text_Win.text = _user.Win.ToString();
        text_WinRate.text = _user.Rate.ToString() + "%";
    }

    private void Update()
    {
        if (PlayManager.Instance.user != null) SetText_UserInfo(PlayManager.Instance.user);
    }

}
