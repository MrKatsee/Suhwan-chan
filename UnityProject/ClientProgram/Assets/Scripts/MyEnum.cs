using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyEnum
{
    public enum MessageType
    {
        DEFAULT = 0,
        NOTICE,
        LOGIN,
        ERROR,
        MATCH,
    }

    public enum NoticeType
    {
        DEFAULT = 0,
        MESSAGE
    }

    public enum LoginType
    {
        DEFAULT = 0,
        SIGNIN,
        SIGNUP
    }

    public enum ErrorType
    {
        DEFAULT = 0,
        EXIST,
        ACCESS,
        WRONGID,
        WRONGPW,
        SHUTDOWN
    }

    public enum MatchType
    {
        DEFAULT = 0,
        START,
        READY,
        WAIT,
        HAND,
        STOP,
        BATTLE,
        WIN,
        LOSE,
        DRAW
    }

    public enum CastType
    {
        DEFAULT = 0,
        UNICAST,
        XORCAST,
        MULTICAST,
        BROADCAST
    }

    public enum CommandType
    {
        ERROR = 0,
        EXIT
    }

    public static T Parse<T>(string value)
    {
        if (!Enum.IsDefined(typeof(T), value)) return default(T);
        return (T)Enum.Parse(typeof(T), value);
    }
}