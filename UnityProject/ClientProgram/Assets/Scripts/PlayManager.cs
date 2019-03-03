using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MyEnum;
using Random = UnityEngine.Random;

public class PlayManager : MonoBehaviour{

    private static PlayManager instance = null;
    public static PlayManager Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new GameObject("PlayManager").AddComponent<PlayManager>();
                instance.Init();
                DontDestroyOnLoad(instance.gameObject);
            }
            return instance;
        }
    }

    public MyUser user;
    public MyUser enemy;

    private int hand;
    private IEnumerator matchRoutine;

    public bool IsReady { get; private set; } = false;
    public bool IsMatch { get; private set; } = false;

    public void InitMatch(MyUser enemyData)
    {
        if (!IsMatch && !IsReady)
        {
            hand = -1;
            enemy = enemyData;
            StartCoroutine(_InitMatch());
        }
    }

    public void StartMatch(float timer)
    {
        if (!IsReady)
        {
            IsReady = true;
            if (!IsMatch)
            {
                IsMatch = true;
                matchRoutine = MatchRoutine(timer);
                StartCoroutine(matchRoutine);
            }
        }
    }

    public void StopMatch()
    {
        if (IsMatch)
        {
            StopCoroutine(matchRoutine);
            IsMatch = false;
            IsReady = false;
            MySceneManager.LoadScene("MainScene");
            UIManager_Main.instance.ui_Toast.MakeToast("상대방과의 연결이 끊겼습니다!", 3f);
        }
    }

    public void EndMatch(string endType)
    {
        if (IsMatch)
        {
            StopCoroutine(matchRoutine);
            IsMatch = false;
            StartCoroutine(EndRoutine(endType));
        }
    }

    private IEnumerator _InitMatch()
    {
        UIManager_Main.instance.lockMatchButton = true;
        UIManager_Main.instance.ui_Matching.StopMatching();
        UIManager_Main.instance.ui_Toast.MakeToast("상대를 찾았습니다!", 3f);
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(3f);
        UIManager_Main.instance.lockMatchButton = false;
        MySceneManager.LoadSceneAsync("PlayScene");
    }

    private IEnumerator MatchRoutine(float timer)
    {
        yield return new WaitUntil(() => MySceneManager.currentScene == "PlayScene");
        yield return new WaitForEndOfFrame();
        UIManager_Play.instance.SetText_MyID(user.ID);
        UIManager_Play.instance.SetText_EnemyID(enemy.ID);
        for(float matchTimer = timer; matchTimer >= 0 && IsMatch; matchTimer -= Time.deltaTime)
        {
            UIManager_Play.instance.SetValue_Wait(matchTimer / timer);
            yield return null;
        }
        if (hand <= -1) SetHand(Random.Range(0, 3));
        ClientManager.Send("/" + MessageType.MATCH.ToString() + " " + MatchType.BATTLE.ToString());
    }

    private IEnumerator EndRoutine(string endType)
    {
        yield return new WaitForEndOfFrame();
        switch (Parse<MatchType>(endType))
        {
            case MatchType.WIN:
            case MatchType.LOSE:
                {
                    UIManager_Play.instance.ui_Result.SetActive(true);
                    UIManager_Play.instance.ui_Result.SetText_Result(endType + "!");
                    yield return new WaitForSeconds(3f);
                    UIManager_Play.instance.ui_Result.SetActive(false);
                    ClientManager.Send("/" + MessageType.NOTICE.ToString() + " " + NoticeType.SYNCUSER.ToString());
                    MySceneManager.LoadScene("MainScene");
                    break;
                }
            case MatchType.DRAW:
                {
                    UIManager_Play.instance.ui_Result.SetActive(true);
                    UIManager_Play.instance.ui_Result.SetText_Result(endType + "!");
                    yield return new WaitForSeconds(3f);
                    UIManager_Play.instance.ui_Result.SetActive(false);
                    UIManager_Play.instance.ClearSelection();
                    UIManager_Play.instance.SetText_EnemyHand("");
                    IsReady = false;
                    IsMatch = false;
                    hand = -1;
                    ClientManager.Send("/" + MessageType.MATCH + " " + MatchType.READY);
                    break;
                }
        }
    }

    public void SetHand(int value)
    {
        if (IsMatch)
        {
            hand = value;
            UIManager_Play.instance.SetSelection(hand);
            ClientManager.Send("/" + MessageType.MATCH.ToString() + " " + MatchType.HAND.ToString() + " " + value.ToString());
        }
    }

    private void Init()
    {
        IsReady = false;
        IsMatch = false;
    }

    public void Execute(string message)
    {
        string matchType = message;
        string messageData = string.Empty;
        if (message.Contains(' '))
        {
            matchType = message.Substring(0, message.IndexOf(' '));
            messageData = message.Substring(message.IndexOf(' ') + 1);
        }
        switch (Parse<MatchType>(matchType))
        {
            case MatchType.DEFAULT: break;
            case MatchType.START:
                {
                    InitMatch(MyUser.ParseData(messageData));
                    break;
                }
            case MatchType.WAIT:
                {
                    StartMatch(Convert.ToSingle(messageData));
                    break;
                }
            case MatchType.STOP:
                {
                    StopMatch();
                    break;
                }
            case MatchType.HAND:
                {
                    int enemyHand = Convert.ToInt32(messageData);
                    string enemyHandString = string.Empty;
                    if (enemyHand == 0) enemyHandString = "가위";
                    else if (enemyHand == 1) enemyHandString = "바위";
                    else if (enemyHand == 2) enemyHandString = "보";
                    UIManager_Play.instance.SetText_EnemyHand(enemyHandString);
                    break;
                }
            case MatchType.WIN:
            case MatchType.LOSE:
            case MatchType.DRAW:
                {
                    EndMatch(matchType);
                    break;
                }
        }
    }

}
