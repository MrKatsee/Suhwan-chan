﻿using System.Collections;
using System.Collections.Generic;

public static class NetworkMessage
{
    public enum MessageType
    {
        ERROR = 0,
        SIGNIN,
        SIGNUP,
        DISCONNECT,
        NOTICE,
    }

    public enum NoticeType
    {
        ERROR = 0,
        LOGINSTATE,
        
    }

    public enum LoginState
    {
        ERROR = 0,
        CREATE,                     // 계정 생성
        SIGNIN,                     // 계정 접속
        WARNING_EXIST,              // ( 생성 시 ) ID가 이미 존재함
        ERROR_ACCESS,               // ( 로그인 시 ) 이미 접속 중인 ID
        ERROR_WRONGID,              // ( 로그인 시 ) 존재하지 않는 ID
        ERROR_WRONGPW,              // ( 로그인 시 ) 잘못된 PW
        ERROR_WRONGDATA,            // 잘못된 데이터 ( 통신 문제 )
    }

    public static string Encapsulation(MessageType messageType, string data)
    {
        return string.Format("/{0} {1}", System.Enum.GetName(typeof(MessageType), messageType), data);
    }

    public static T ParseData<T>(string orderType)
    {
        if (!System.Enum.IsDefined(typeof(T), orderType)) return default(T);
        return (T)System.Enum.Parse(typeof(T), orderType);
    }

    private static Queue<string> messageQueue = new Queue<string>();

    public static int GetCount()
    {
        int count;
        lock (messageQueue)
        {
            count = messageQueue.Count;
        }
        return count;
    }

    public static void Enqueue(string str)
    {
        messageQueue.Enqueue(str);
    }

    public static void SyncEnqueue(string str)
    {
        lock (messageQueue)
        {
            messageQueue.Enqueue(str);
        }
    }

    public static string Dequeue()
    {
        return messageQueue.Dequeue();
    }

    public static string SyncDequeue()
    {
        string str;
        lock (messageQueue)
        {
            str = messageQueue.Dequeue();
        }
        return str;
    }
}
