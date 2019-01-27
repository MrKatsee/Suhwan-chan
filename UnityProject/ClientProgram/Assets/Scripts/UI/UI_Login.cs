using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Login : MonoBehaviour {

    private InputField inputField_ID;
    private InputField inputField_PW;

    private void Awake()
    {
        inputField_ID = transform.Find("InputField_ID").GetComponent<InputField>();
        inputField_PW = transform.Find("InputField_PW").GetComponent<InputField>();
        transform.Find("Button_SignIn").GetComponent<Button>().onClick.AddListener(OnClickSignIn);
        transform.Find("Button_SignUp").GetComponent<Button>().onClick.AddListener(OnClickSignUp);
    }

    public void ClearField()
    {
        inputField_ID.text = "";
        inputField_PW.text = "";
    }

    void OnClickSignIn()
    {
        if (!inputField_ID || !inputField_PW) return;
        if (string.IsNullOrEmpty(inputField_ID.text) || string.IsNullOrEmpty(inputField_PW.text)) return;
        UIManager_Main.instance.ui_Loading.StartLoading("서버의 응답을 기다리는 중......");
        ClientManager.Send(NetworkMessage.MessageType.SIGNIN, string.Format("ID={0}&PW={1}", inputField_ID.text, inputField_PW.text));
        
    }

    void OnClickSignUp()
    {
        if (!inputField_ID || !inputField_PW) return;
        if (string.IsNullOrEmpty(inputField_ID.text) || string.IsNullOrEmpty(inputField_PW.text)) return;
        UIManager_Main.instance.ui_Loading.StartLoading("서버의 응답을 기다리는 중......");
        ClientManager.Send(NetworkMessage.MessageType.SIGNUP, string.Format("ID={0}&PW={1}", inputField_ID.text, inputField_PW.text));
        
    }

}
