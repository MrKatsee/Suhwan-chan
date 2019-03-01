using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class UI_Login : MonoBehaviour {

    private InputField inputField_ID;
    private InputField inputField_PW;

    private static string REGEX_ID = @"/^[a-z0-9_-]{3,16}$/";
    private static string REGEX_PW = @"/^[a-z0-9_-]{6,18}$/";

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

    public void SetActive(bool param)
    {
        gameObject.SetActive(param);
    }

    void OnClickSignIn()
    {
        if (!inputField_ID || !inputField_PW) return;
        if (string.IsNullOrEmpty(inputField_ID.text) || (string.IsNullOrEmpty(inputField_PW.text)))
        {
            UIManager_Main.instance.ui_Toast.MakeToast("올바르지 않은 ID 또는 PW입니다!", 3f);
            return;
        }
        UIManager_Main.instance.ui_Loading.StartLoading("서버의 응답을 기다리는 중......");
        MyUser newUser = new MyUser(inputField_ID.text, inputField_PW.text);
        ClientManager.Send("/LOGIN SIGNIN " + newUser.ToData());
    }

    void OnClickSignUp()
    {
        if (!inputField_ID || !inputField_PW) return;
        if (string.IsNullOrEmpty(inputField_ID.text) || (string.IsNullOrEmpty(inputField_PW.text)))
        {
            UIManager_Main.instance.ui_Toast.MakeToast("올바르지 않은 ID 또는 PW입니다!", 3f);
            return;
        }
        UIManager_Main.instance.ui_Loading.StartLoading("서버의 응답을 기다리는 중......");
        MyUser newUser = new MyUser(inputField_ID.text, inputField_PW.text);
        ClientManager.Send("/LOGIN SIGNUP " + newUser.ToData());
    }

}
