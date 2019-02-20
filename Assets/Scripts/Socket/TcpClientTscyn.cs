using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
public class TcpClientTscyn
{
    private static TcpClient client;
    private NetworkStream stream;
    private byte[] buffer = new byte[1024 * 8];
    private Thread ConnectThread;

    private string ip = "192.168.2.103";
    private int port = 19741;
    public Action<string> ServerCallBack;

    public TcpClientTscyn(Action<string> action)
    {
        Debug.Log(".....");
        ServerCallBack = action;
        ConnectThread = new Thread(OnConnect);
        ConnectThread.Start();
    }

    enum SocketState
    {
        Connected,
        DisConnected,
        Close
    }
    private SocketState m_State = SocketState.DisConnected;

    private bool IsConnecting = false;

    private bool IsStart = false;
    public void ConnectToServer(string ip, int port)
    {
        this.ip = ip;
        this.port = port;
        IsStart = true;
    }

    public void OnConnect()
    {
        while (true)
        {
            if (!IsStart)
            {
                Thread.Sleep(1000);
            }
            else
            {
                if (IsConnecting)
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    if (m_State == SocketState.Connected)
                    {
                        Debug.Log("connected");
                        //Console.WriteLine("Connected");
                        Thread.Sleep(1000);
                    }
                    else if (m_State == SocketState.DisConnected)
                    {
                        Debug.Log("DisConnected");
                        //Console.WriteLine("DisConnected");
                        IsConnecting = true;
                        try
                        {
                            client = new TcpClient(AddressFamily.InterNetwork);
                            client.BeginConnect(ip, port, new AsyncCallback(CallBack), client);
                        }
                        catch (Exception ex)
                        {
                            IsConnecting = false;
                            Console.WriteLine(ex);
                        }
                    }
                }
            }
        }
    }

    private void CallBack(IAsyncResult iar)
    {
        client = (TcpClient)iar.AsyncState;
        try
        {
            client.EndConnect(iar);
        }
        catch (Exception ex)
        {
            IsConnecting = false;
            m_State = SocketState.DisConnected;
            Console.WriteLine(ex);
            return;
        }
        if ((client != null) && (client.Connected))
        {
            m_State = SocketState.Connected;
            IsConnecting = false;
            stream = client.GetStream();
            if (stream.CanRead)
            {
                stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(AsyncReadCallBack), null);
            }
            if (ServerCallBack != null)
            {
                ServerCallBack("连接到服务器");
            }
        }
    }

    private void AsyncReadCallBack(IAsyncResult ar)
    {
        if (m_State == SocketState.Connected)
        {
            int length = 0;
            try
            {
                length = client.GetStream().EndRead(ar);
            }
            catch (Exception ex)
            {
                IsConnecting = false;
                m_State = SocketState.DisConnected;
                Console.WriteLine(ex);
                return;
            }
            if (length < 1)
            {
                IsConnecting = false;
                client.Close();
                m_State = SocketState.DisConnected;
                return;
            }
            string result = Encoding.UTF8.GetString(buffer, 0, length);
            if (ServerCallBack != null)
            {
                ServerCallBack(result);
            }
            if (stream.CanRead)
            {
                stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(AsyncReadCallBack), null);
            }
        }
    }

    public void SendMessageClient(string message)
    {
        if (client.Connected)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.BeginWrite(data, 0, data.Length, new AsyncCallback(SendSuccessful), null);
        }
    }

    private void SendSuccessful(IAsyncResult ar)
    {
        client.GetStream().EndWrite(ar);
        Console.WriteLine("发送成功");
    }


    public void UnLoad()
    {
        if (client != null && client.Connected)
        {
            m_State = SocketState.DisConnected;
            client.Close();
            ConnectThread.Abort();
        }
    }
}
