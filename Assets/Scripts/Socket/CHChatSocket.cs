using System.Collections;
using UnityEngine;

public class CHChatSocket : CHSocket
{
    private static CHChatSocket _instance;
    public static CHChatSocket instance
    {
        get
        {
            if (null == _instance)
            {
                CreateInstance();
            }
            return _instance;
        }
    }
    private static void CreateInstance()
    {
        string name = typeof(CHSocket).ToString();
        GameObject obj = UnityEngine.GameObject.Find(name);
        if (obj == null)
        {
            obj = new GameObject(name);
            UnityEngine.Object.DontDestroyOnLoad(obj);
        }
        _instance = obj.AddComponent<CHChatSocket>();
    }

    private const float HeartBeatSeconds = 5f;
    const string HeartBeatString = "ok";
    int DISCONNECT_NUM = 5;
    void Awake()
    {
        ifCompress = false;
        Init("192.168.1.211", 50500, ConnectType.CHAT);
        //StartCoroutine(OnSendChatHeatBeat());
    }
    public override void ConnectSuccess()
    {
        Debug.Log("connect successful");
        //if (!firstConnect)
        //{
        //    SendHeartBeat();
        //}
    }
    public override void ReadMsg()
    {
        lock (lockObj)
        {
            int len = _receiveList.Count;
            if (len > 0)
            {
                for (int i = 0; i < len; i++)
                {
                    if (_receiveList[i] != null && !string.IsNullOrEmpty(_receiveList[i]) && _receiveList[i] != HeartBeatString)
                    {
                        Debug.Log("[Chat Get Text]:" + _receiveList[i]);
                    }
                }
                if (_receiveList != null && _receiveList.Count >= len)
                    _receiveList.RemoveRange(0, len);
                heartReply = 0;
            }
        }
    }
    /// <summary>
    /// 聊天主动心跳
    /// </summary>
    /// <returns></returns>
    IEnumerator OnSendChatHeatBeat()
    {
        while (true)
        {
            yield return new WaitForSeconds(5); /*Yielders.Get(HeartBeatSeconds);*/
            if (!firstConnect && IsConnected())
            {
                SendHeartBeat();
                if (heartReply >= DISCONNECT_NUM)
                {
                    heartReply = 0;
                    SetReConnectStatus();
                }
            }
        }
    }
    public override void SendHeartBeat()
    {
        heartReply++;
        string initMsg = "{\"uid\":}" + heartReply;
        ChatSendMsg(initMsg);
    }
    public void GetOffLineMessageLine()
    {
        string sendMsg = "{\"type\":\"" + 7 + "\",\"uid\":\"" + "\"}";
        ChatSendMsg(sendMsg);
    }

    public void ChatSendMsg(string message)
    {
        Debug.LogFormat("[Heart Text]:{0}", message);
        SendMsg(message);
    }
    public override void OnDisconnect()
    {
        StartCoroutine(OnContinueReconnect());
    }
    IEnumerator OnContinueReconnect()
    {
        yield return new WaitForSeconds(30);
        RenewConnect();
    }

    public void CloseAndDestory()
    {
        Close();
        StopAllCoroutines();
        Destroy(this);
    }
}
