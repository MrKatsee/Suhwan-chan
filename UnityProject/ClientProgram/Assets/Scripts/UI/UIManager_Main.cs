using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MyEnum;

public class UIManager_Main : MonoBehaviour {

    public static UIManager_Main instance = null;

    public UI_Toast ui_Toast { get; private set; }
    public UI_Loading ui_Loading { get; private set; }
    public UI_Matching ui_Matching { get; private set; }
    public UI_Login ui_Login { get; private set; }

    private Text text_ID;
    private Text text_Win;
    private Text text_WinRate;

    private Button button_Match;

    public bool lockMatchButton = false;

    private void Awake()
    {
        instance = this;
        ui_Login = transform.Find("UI_Login").GetComponent<UI_Login>();
        ui_Loading = transform.Find("UI_Loading").GetComponent<UI_Loading>();
        ui_Matching = transform.Find("UI_Matching").GetComponent<UI_Matching>();
        ui_Toast = transform.Find("UI_Toast").GetComponent<UI_Toast>();

        text_ID = transform.Find("Text_ID").GetComponent<Text>();
        text_Win = transform.Find("Text_Win").GetComponent<Text>();
        text_WinRate = transform.Find("Text_WinRate").GetComponent<Text>();

        button_Match = transform.Find("Button_Match")?.GetComponent<Button>();
        button_Match.onClick.AddListener(OnClick_Match);

        ui_Login.SetActive(PlayManager.Instance.user == null);
    }

    private void Start()
    {
        if (!ClientManager.IsConnected)
        {
            ClientManager.Init();
            StartCoroutine(ConnectSequenceUI());
        }
    }

    public void SetText_UserInfo(MyUser _user)
    {
        text_ID.text = _user.ID;
        text_Win.text = _user.Win.ToString();
        text_WinRate.text = _user.Rate.ToString() + "%";
    }

    private void OnClick_Match()
    {
        if (!lockMatchButton)
        {
            if (ui_Matching.IsRunning)
            {
                ui_Matching.StopMatching();
                ClientManager.Send("/" + MessageType.MATCH.ToString() + " " + MatchType.CANCEL.ToString());

            }
            else
            {
                ui_Matching.StartMatching("상대를 찾는 중......");
                ClientManager.Send("/" + MessageType.MATCH.ToString() + " " + MatchType.START.ToString());
            }
        }
    }

    private void Update()
    {
        if (PlayManager.Instance.user != null) SetText_UserInfo(PlayManager.Instance.user);
    }

    IEnumerator ConnectSequenceUI()
    {
        ui_Loading.StartLoading("접속 시도 중 : " + ClientManager.IPAddress);
        yield return new WaitUntil(() => ClientManager.IsConnected);
        ui_Loading.StopLoading();
    }

}
