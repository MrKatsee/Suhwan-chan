using System.Collections;
using System.Collections.Generic;

public static class NetworkMessage
{
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
