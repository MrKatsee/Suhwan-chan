using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class NetworkConnection {

    public static int MAXINDEX = 64;

    static Encoding encodingType = Encoding.UTF8;

    static NetworkConnection[] conn = new NetworkConnection[MAXINDEX];

    public static int Count
    {
        get
        {
            lock (conn)
            {
                int count = 0;
                for(int i = 0; i < MAXINDEX; ++i)
                {
                    if (conn[i] != null) count++;
                }
                return count;
            }
        }
    }

    static int EmptyIndex
    {
        get
        {
            lock (conn)
            {
                for (int i = 0; i < MAXINDEX; ++i)
                {
                    if (conn[i] == null) return i; 
                }
                return MAXINDEX;
            }
        }
    }

    public static NetworkConnection GetConnection(int _index)
    {
        if (0 <= _index && _index < MAXINDEX && conn[_index] != null) return conn[_index];
        else return null;
    }

    public static NetworkConnection GetConnection(string _name)
    {
        for(int i = 0; i < MAXINDEX; ++i)
        {
            if(conn[i] != null)
            {
                if (conn[i].name == _name) return conn[i];
            }
        }
        return null;
    }

    public static void CreateConnection(Socket _socket)
    {
        if (EmptyIndex < MAXINDEX)
        {
            NetworkConnection newConnection = new NetworkConnection(EmptyIndex, _socket);
            conn[EmptyIndex] = newConnection;
            LogManager.WriteLog(
                string.Format("No. {0} client connected : address is {1}", newConnection.index, newConnection.Address));
        }
    }

    public static void Clear()
    {
        for (int i = 0; i < Count; ++i)
        {
            NetworkConnection conn = GetConnection(i);
            if (conn != null) conn.ShutDown();
        }
    }

    public bool IsConnected { get; private set; }
    public int index;
    public string name;
    public string Address { get; private set; }

    Socket socket;
    NetworkStream networkStream;
    StreamReader reader;
    StreamWriter writer;
    Thread thread_Receiver;

    public NetworkConnection(int _index, Socket _socket)
    {
        IsConnected = true;
        index = _index;
        socket = _socket;
        Address = socket.RemoteEndPoint.ToString();
        networkStream = new NetworkStream(socket);
        reader = new StreamReader(networkStream, encodingType);
        writer = new StreamWriter(networkStream, encodingType);
        thread_Receiver = new Thread(ReceiveMsg);
        thread_Receiver.Start();
    }

    public void SetName(string _name)
    {
        name = _name;
        int sameCount = 0;
        for (int i = 0; i < MAXINDEX; ++i)
        {
            if (i != index)
            {
                NetworkConnection conn = GetConnection(i);
                if (conn != null)
                {
                    if (conn.name == name)
                    {
                        name = name + "_" + (++sameCount);
                    }
                }
            }
        }
    }

    public void SendMsg(string str)
    {
        if (IsConnected)
        {
            try
            {
                writer.WriteLine(str);
                writer.Flush();
            }
            catch (Exception e)
            {
                LogManager.WriteLog(
                    string.Format("Error for No. {0} client : {1}", this.index, e.Message));
            }
        }
    }

    private void ReceiveMsg()
    {
        string receivedString = string.Empty;
        try
        {
            while (IsConnected)
            {
                receivedString = reader.ReadLine();
                if(!string.IsNullOrEmpty(receivedString)) NetworkMessage.SyncEnqueue(string.Format("[{0}]{1}", index, receivedString));
            }
        }
        catch (Exception e)
        {
            LogManager.WriteLog(
                    string.Format("Error for No. {0} client : {1}", this.index, e.Message));
        }
        ShutDown();
    }

    System.Object shutDownLock = new object();
    private void ShutDown()
    {
        lock (shutDownLock)
        {
            IsConnected = false;
            try
            {
                reader.Close();
                writer.Close();
            }
            catch (Exception e)
            {
                MyDebug.Log(e.Message);
            }

            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception e)
            {
                MyDebug.Log(e.Message);
            }
            finally
            {
                socket.Close();
                conn[index] = null;
                LogManager.WriteLog(
                    string.Format("No. {0} client disconnected : address is {1}", this.index, this.Address));
            }
        }
    }

}
