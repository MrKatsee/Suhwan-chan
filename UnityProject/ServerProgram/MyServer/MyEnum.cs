using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServer
{
    class MyEnum
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
            MESSAGE,
        DISCONNECT
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

        public static T Parse<T>(string value) where T : Enum
        {
            if (!Enum.IsDefined(typeof(T), value)) return default;
            return (T)Enum.Parse(typeof(T), value);
        }
    }
}
