using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Loading : MonoBehaviour {

    Image image_Loading;
    Text text_Loading;
    IEnumerator coroutine_Animation;
    bool isRunning = false;

    static Vector3 movePosition = new Vector3(10000, 0, 0);

    private void Awake()
    {
        image_Loading = transform.Find("Image_Loading").GetComponent<Image>();
        text_Loading = transform.Find("Text_Loading").GetComponent<Text>();
        coroutine_Animation = LoadingAnimation();
        transform.localPosition = movePosition;
    }

    public void SetText(string content)
    {
        if (text_Loading == null) return;
        text_Loading.text = content;
    }

    public void StartLoading()
    {
        if (coroutine_Animation == null) return;
        if (isRunning) return;
        transform.localPosition = Vector3.zero;
        StartCoroutine(coroutine_Animation);
        isRunning = true;
    }

    public void StartLoading(string content)
    {
        if (coroutine_Animation == null) return;
        SetText(content);
        if (isRunning) return;
        transform.localPosition = Vector3.zero;
        SetText(content);
        StartCoroutine(coroutine_Animation);
        isRunning = true;
    }

    public void StopLoading()
    {
        if (!isRunning) return;
        transform.localPosition = movePosition;
        StopAllCoroutines();
        isRunning = false;
    }

    private IEnumerator LoadingAnimation()
    {
        while (true)
        {
            Vector3 rotateVector = image_Loading.transform.localRotation.eulerAngles;
            image_Loading.transform.localRotation = Quaternion.Euler(rotateVector.x, rotateVector.y, rotateVector.z + (90 * Time.deltaTime));
            yield return null;
        }
    }

}
