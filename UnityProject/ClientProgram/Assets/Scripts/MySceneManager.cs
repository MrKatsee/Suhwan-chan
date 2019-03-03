using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static MyEnum;

public class MySceneManager : MonoBehaviour
{
    private Image image_Progress;

    private void Start()
    {
        image_Progress = transform.Find("Image_Progress")?.GetComponent<Image>();
        StartCoroutine(_LoadSceneAsync(nextScene));
    }

    private static string nextScene;
    public static string currentScene
    {
        get
        {
            return SceneManager.GetActiveScene().name;
        }
    }

    public static  void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public static void LoadSceneAsync(string sceneName)
    {
        nextScene = sceneName;
        SceneManager.LoadScene("LoadingScene");
    }

    public IEnumerator _LoadSceneAsync(string sceneName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        float timer = 0f;
        while (!op.isDone)
        {
            yield return null;
            timer += Time.deltaTime;
            if(op.progress >= 0.9f)
            {
                image_Progress.fillAmount = Mathf.Lerp(image_Progress.fillAmount, 1f, timer);

                if (image_Progress.fillAmount >= 1.0f)
                {
                    if (nextScene == "PlayScene")
                    {
                        yield return new WaitForSeconds(2f);
                        ClientManager.Send("/" + MessageType.MATCH + " " + MatchType.READY);
                        yield return new WaitUntil(() => PlayManager.Instance.IsReady);
                        op.allowSceneActivation = true;
                    }
                }
            }
            else
            {
                image_Progress.fillAmount = Mathf.Lerp(image_Progress.fillAmount, op.progress, timer);
                if(image_Progress.fillAmount >= op.progress)
                {
                    timer = 0f;
                }
            }
        }
    }

}
