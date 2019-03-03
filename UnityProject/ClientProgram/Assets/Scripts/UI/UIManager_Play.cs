using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_Play : MonoBehaviour {

    public static UIManager_Play instance = null;

    private Text text_MyID;
    private Text text_EnemyID;
    private Text text_EnemyHand;

    private Image image_Wait;

    private Button button_Scissor;
    private Button button_Rock;
    private Button button_Paper;

    public UI_Result ui_Result;

    private void Awake()
    {
        instance = this;

        text_MyID = transform.Find("Text_MyID")?.GetComponent < Text>();
        text_EnemyID = transform.Find("Text_EnemyID")?.GetComponent<Text>();
        text_EnemyHand = transform.Find("Text_EnemyHand")?.GetComponent<Text>();

        image_Wait = transform.Find("Image_Wait")?.GetComponent<Image>();

        button_Scissor = transform.Find("Button_Scissor")?.GetComponent<Button>();
        button_Rock = transform.Find("Button_Rock")?.GetComponent<Button>();
        button_Paper = transform.Find("Button_Paper")?.GetComponent<Button>();

        ui_Result = transform.Find("UI_Result")?.GetComponent<UI_Result>();

        ui_Result.SetActive(false);

        button_Scissor.transform.Find("Image").gameObject.SetActive(false);
        button_Rock.transform.Find("Image").gameObject.SetActive(false);
        button_Paper.transform.Find("Image").gameObject.SetActive(false);

        button_Scissor.onClick.AddListener(OnClick_Scissor);
        button_Rock.onClick.AddListener(OnClick_Rock);
        button_Paper.onClick.AddListener(OnClick_Paper);
    }

    public void SetText_MyID(string content)
    {
        if (text_MyID == null) return;
        text_MyID.text = content;
    }

    public void SetText_EnemyID(string content)
    {
        if (text_EnemyID == null) return;
        text_EnemyID.text = content;
    }

    public void SetText_EnemyHand(string content)
    {
        if (text_EnemyHand == null) return;
        text_EnemyHand.text = content;
    }

    public void SetValue_Wait(float value)
    {
        if (image_Wait == null) return;
        image_Wait.fillAmount = value;
    }

    public void ClearSelection()
    {
        button_Scissor.transform.Find("Image").gameObject.SetActive(false);
        button_Rock.transform.Find("Image").gameObject.SetActive(false);
        button_Paper.transform.Find("Image").gameObject.SetActive(false);
    }

    public void SetSelection(int value)
    {
        button_Scissor.transform.Find("Image").gameObject.SetActive(value == 0);
        button_Rock.transform.Find("Image").gameObject.SetActive(value == 1);
        button_Paper.transform.Find("Image").gameObject.SetActive(value == 2);
    }

    private void OnClick_Scissor()
    {
        button_Scissor.transform.Find("Image").gameObject.SetActive(true);
        button_Rock.transform.Find("Image").gameObject.SetActive(false);
        button_Paper.transform.Find("Image").gameObject.SetActive(false);
        PlayManager.Instance.SetHand(0);
    }

    private void OnClick_Rock()
    {
        button_Scissor.transform.Find("Image").gameObject.SetActive(false);
        button_Rock.transform.Find("Image").gameObject.SetActive(true);
        button_Paper.transform.Find("Image").gameObject.SetActive(false);
        PlayManager.Instance.SetHand(1);
    }

    private void OnClick_Paper()
    {
        button_Scissor.transform.Find("Image").gameObject.SetActive(false);
        button_Rock.transform.Find("Image").gameObject.SetActive(false);
        button_Paper.transform.Find("Image").gameObject.SetActive(true);
        PlayManager.Instance.SetHand(2);
    }
}
