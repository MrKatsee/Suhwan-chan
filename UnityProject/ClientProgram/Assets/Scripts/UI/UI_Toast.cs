using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Toast : MonoBehaviour {

    GameObject toastObject;
    Transform toastTransform;

    private void Init()
    {
        toastObject = transform.Find("ToastObject").gameObject;
        toastTransform = transform.Find("ToastTransform");
    }

    private void Awake()
    {
        Init();
    }

    public void MakeToast(string content, float time)
    {
        GameObject newToast = Instantiate(toastObject);
        newToast.SetActive(true);
        newToast.transform.SetParent(toastTransform);
        newToast.GetComponent<UI_ToastObject>().MakeToast(content, time);
    }

}
