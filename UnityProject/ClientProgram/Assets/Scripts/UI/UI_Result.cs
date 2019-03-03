using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Result : MonoBehaviour {

    Text text_Result;

    private void Awake()
    {
        text_Result = transform.Find("Text_Result")?.GetComponent<Text>();
    }

    public void SetText_Result(string content)
    {
        if (text_Result == null) return;
        text_Result.text = content;
    }

    public void SetActive(bool param)
    {
        if (param)
        {
            if(text_Result == null) text_Result = transform.Find("Text_Result")?.GetComponent<Text>();
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }


}
