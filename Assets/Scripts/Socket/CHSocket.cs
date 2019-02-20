using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
public class CHSocket : MonoBehaviour, ICHMessenger
{
    public class CHSocketMsg
    {
        public uint key = 0;
        public bool ifretry = false;
        public uint retryCount = 0;
        public string path;
        public string query;
        public string sendText;
        public string receiveText;
        public float sendDt;
        public CallBeforFun successCallBackBeforeFunc;
        public CallBackData successCallBack;
        public CallAfterFun successCallBackAfterFunc;
        public CallBackError callBackErr;
    }
    public delegate void CallBackError(string err);
    public delegate void CallBackData(string data);
    public delegate void CallBeforFun();
    public delegate void CallAfterFun();

    public object lockObj = new object();

    //public List<CHMatchReceiveMsg> _receiveList = new List<CHMatchReceiveMsg>();
    public List<string> _receiveList = new List<string>();

    private DynamicBuffer _sendBuffer = new DynamicBuffer();
    private int bufferPos;
    public int heartReply = 0;
    public int _reconnectNum = 8/*PublicMatchData.MAX_RECONNECT_COUNT*/;
    int _retryNum = 1; /* PublicMatchData.MAX_RETRY_NUM;*/
    public Error lastError = Error.NONE;
    public ConnectStatus currentStatus = ConnectStatus.NONE;
    public ConnectType _currentConnectType = ConnectType.FUNC;
    float RetryConnectWaitSeconds = 2f;
    public int sendCount = 0;
    public float beginConnectTimeout = 5f;
    public float beginConnectElapsed = 0f;
    bool isConnectCallback = false;
    public bool firstConnect = true;
    public bool ifCompress = true;
    CHSocketReader reader = null;
    Socket _socket = null;
    string _ip;
    int _port;

  
    public void Init(string ip, int port, ConnectType type)
    {
        _ip = ip;
        _port = port;
        _currentConnectType = type;
        StartCoroutine(Connect());
        StartCoroutine(OnSendMessage());
    }
    /// <summary>
    /// 错误码
    /// </summary>
    public enum Error
    {
        NONE,
        TIMEOUT,
        LOGIN_FAIL,
        DISCONNECTED,
    }
    /// <summary>
    /// 连接状态
    /// </summary>
    public enum ConnectStatus
    {
        NONE,
        CONNECTING,
        CONNECTED,
        RECONNECT,
        DISCONNECTED,
        READY_TO_RECONNECT,
        CONNECT_FAIL,
    }
    public enum ConnectType
    {
        FUNC,
        MATCH,
        CHAT,
    }
    /// <summary>
    /// 建立连接
    /// </summary>
    /// <returns></returns>
    public IEnumerator Connect()
    {
        currentStatus = ConnectStatus.CONNECTING;
        if (_socket != null && _socket.Connected)
        {
            Close();
        }
        if (!firstConnect)
            ShowLoadingBar();
        reader = null;
        _sendBuffer.CleanALL();
        lock (lockObj)
        {
            _receiveList.Clear();
        }
        isConnectCallback = false;

        IPAddress[] ips = Dns.GetHostAddresses(_ip);
        if (ips == null || ips.Length <= 0)
        {
            ProcessConectionError(Error.TIMEOUT, "Dns.GetHostAddresses Fail!");
            yield break;
        }
        IPAddress _ipAddr = IPAddress.Parse(_ip);

        AddressFamily addressFamily = AddressFamily.InterNetwork;
#if UNITY_IOS && !UNITY_EDITOR
		_ip = IphoneUtility.ConvertIPAddress_IOS (_ip, _port.ToString());
		if (_ip.Contains (":")) {
		addressFamily = AddressFamily.InterNetworkV6;
		} else {
		addressFamily = AddressFamily.InterNetwork;
		}
#else
        addressFamily = AddressFamily.InterNetwork;
#endif

        //_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _socket = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);
        IAsyncResult ar = _socket.BeginConnect(_ip, _port, ConnectCallback, null);
        beginConnectElapsed = 0f;

        while (!ar.IsCompleted)
        {
            if (beginConnectElapsed > beginConnectTimeout)
            {
                ProcessConectionError(Error.TIMEOUT, "Network Module Initialization Fail!");
                yield break;
            }
            beginConnectElapsed += Time.deltaTime;
            yield return null;
        }

        while (isConnectCallback == false)
        {
            yield return null;
        }

        if (_socket == null || !_socket.Connected)
        {
            ProcessConectionError(Error.DISCONNECTED, "Can't Connect Remote Server!");
            yield break;
        }

        _reconnectNum = 1;/*PublicMatchData.MAX_RECONNECT_COUNT;*/
        _retryNum = 1; /*PublicMatchData.MAX_RETRY_NUM;*/
        reader = new CHSocketReader(_socket, ReadFailed, this);

        #region 验证
        //try
        //{
        //    int sent = _socket.Send(GetLoginSendByte(), SocketFlags.None);
        //    if (sent < 0)
        //    {
        //        throw new Exception("Socket sent return " + sent);
        //    }
        //}
        //catch (Exception e)
        //{
        //    int errorCode = 0;
        //    if (e is SocketException)
        //    {
        //        errorCode = (e as SocketException).ErrorCode;
        //    }
        //    Debug.LogWarning("!!!Socket login auth, send Error msg: " + e.Message + "  |||Error code:" + errorCode);
        //    ProcessConectionError(Error.LOGIN_FAIL, "Can't Connect Remote Server!");
        //    yield break;
        //}
        #endregion


        if (!firstConnect)
            HideLoadingBar();
        firstConnect = false;
        currentStatus = ConnectStatus.CONNECTED;
        ConnectSuccess();
    }

    const string AuthLoginKey = "chao.163.com";
    public byte[] GetLoginSendByte()
    {
        byte[] keyByte = Encoding.UTF8.GetBytes(AuthLoginKey);
        MemoryStream mem = new MemoryStream();
        int len = keyByte.Length;

        len = System.Net.IPAddress.HostToNetworkOrder(len);
        byte[] lenByte = BitConverter.GetBytes(len);
        mem.Write(lenByte, 0, lenByte.Length);
        mem.Write(keyByte, 0, keyByte.Length);

        byte[] bytes = mem.ToArray();
        mem.Close();
        return bytes;
    }

    /// <summary>
    /// 连接回调
    /// </summary>
    /// <param name="result"></param>
    public void ConnectCallback(IAsyncResult result)
    {
        isConnectCallback = true;
    }
    /// <summary>
    /// 显示重连连接条
    /// </summary>
    public virtual void ShowLoadingBar() { }
    /// <summary>
    /// 隐藏重连连接条
    /// </summary>
    public virtual void HideLoadingBar() { }
    /// <summary>
    /// 连接成功
    /// </summary>
    public virtual void ConnectSuccess() { }
    /// <summary>
    /// 关闭socket
    /// </summary>
    public void Close()
    {
        Debug.Log("socket close");
        if (_socket != null)
        {
            Debug.Log("CHSocket Tcp Close()");
            _socket.Close();
            _socket = null;
            reader = null;
            _sendBuffer.CleanALL();
            lock (lockObj)
            {
                _receiveList.Clear();
            }
        }
        currentStatus = ConnectStatus.DISCONNECTED;
    }
 
    public void ReadFailed()
    {
        if (currentStatus == ConnectStatus.CONNECTED)
            SetReConnectStatus();
    }
    /// <summary>
    /// 循环检测重连状态
    /// </summary>
    void Update()
    {
        if (currentStatus == ConnectStatus.RECONNECT)
        {
            currentStatus = ConnectStatus.READY_TO_RECONNECT;
            StartCoroutine(Connect());
        }
        if (IsConnected())
            ReadMsg();
    }
    /// <summary>
    /// 读取接受的数据
    /// </summary>
    public virtual void ReadMsg() { }
    /// <summary>
    /// 连接错误处理
    /// </summary>
    /// <param name="error"></param>
    /// <param name="errorLog"></param>
    public void ProcessConectionError(Error error, string errorLog)
    {
        currentStatus = ConnectStatus.CONNECT_FAIL;
        lastError = error;
        Debug.LogWarning(errorLog);
        Close();
        beginConnectElapsed = 0f;
        if (_retryNum > 0)
        {
            _retryNum--;
            SetReConnectStatus();
        }
        else
        {
            if (_reconnectNum > 0)
            {
                _reconnectNum--;
                StartCoroutine(OnWaitForReconnect());
            }
            else
            {
                OnDisconnect();
            }
        }
    }
    
    IEnumerator OnWaitForReconnect()
    {
        yield return new WaitForSeconds(30); /*Yielders.Get(RetryConnectWaitSeconds);*/
        _retryNum = 1;/*PublicMatchData.MAX_RETRY_NUM;*/
        SetReConnectStatus();
    }
    /// <summary>
    /// 弹出是否需要重连提示
    /// </summary>
    public virtual void OnDisconnect()
    {
        //POPWindowUI.instance.ShowOkCancel(Localization.Localize("GAME_ERR_21"), base.gameObject, "RenewConnect", "LoadTitle");
    }
    /// <summary>
    /// 重新开始一次完整重连
    /// </summary>
    public void RenewConnect()
    {
        _reconnectNum = 1;/* PublicMatchData.MAX_RECONNECT_COUNT;*/
        _retryNum = 1; /*PublicMatchData.MAX_RETRY_NUM;*/
        SetReConnectStatus();
    }
    /// <summary>
    /// 是否连上
    /// </summary>
    /// <returns></returns>
    public bool IsConnected()
    {
        return _socket != null && _socket.Connected && currentStatus == ConnectStatus.CONNECTED;
    }
    /// <summary>
    /// 设置重连状态
    /// </summary>
    public void SetReConnectStatus()
    {
        if (currentStatus != ConnectStatus.CONNECTING && currentStatus != ConnectStatus.READY_TO_RECONNECT && currentStatus != ConnectStatus.RECONNECT)
        {
            currentStatus = ConnectStatus.RECONNECT;
        }
    }
    /// <summary>
    /// 往消息队列中发数据
    /// </summary>
    /// <param name="message"></param>
    public void SendMsg(string message)
    {
        if (IsConnected())
        {
            byte[] msg = ifCompress ? CHMatchReceiveMsg.EncodeMessage(message) : Encoding.UTF8.GetBytes(message);
            _socket.BeginSend(msg, 0, msg.Length, SocketFlags.None, SendSuccess, null);
            //int msgLent = msg.Length;
            //_sendBuffer.Write(msgLent);
            //_sendBuffer.Write(msg);
        }
    }

    private void SendSuccess(IAsyncResult ar)
    {
        Debug.Log("发送成功");
    }

    /// <summary>
    /// 循环检测连接状态，监测发送队列是否为空并发送消息
    /// </summary>
    /// <returns></returns>
    IEnumerator OnSendMessage()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame(); /*Yielders.EndOfFrame;*/
            if (IsConnected())
            {
                if (_sendBuffer.ReadPosition < _sendBuffer.WritePosition)
                {
                    try
                    {
                        int sent = _socket.Send(_sendBuffer.Buffer, _sendBuffer.ReadPosition, (_sendBuffer.WritePosition - _sendBuffer.ReadPosition), SocketFlags.None);
                        if (sent >= 0)
                        {
                            _sendBuffer.AddReadPosition(sent);
                        }
                        else
                        {
                            throw new Exception("Socket sent return " + sent);
                        }
                    }
                    catch (Exception e)
                    {
                        int errorCode = 0;
                        if (e is SocketException)
                        {
                            errorCode = (e as SocketException).ErrorCode;
                        }
                        Debug.LogWarning("!!!Socket send error Error msg: " + e.Message + "  |||Error code:" + errorCode);
                        SetReConnectStatus();
                    }
                }
            }
            else
            {
                if (currentStatus == ConnectStatus.CONNECTED)
                {
                    Debug.LogWarning("Socket send Reconnect!!!!!");
                    SetReConnectStatus();
                }
            }
        }
    }
    /// <summary>
    /// 主动检测心跳
    /// </summary>
    public virtual void SendHeartBeat() { }
    /// <summary>
    /// 心跳返回
    /// </summary>
    /// <param name="text"></param>
    public virtual void OnSuccessHeartReply(string text)
    {
        if (this == null) return;
        heartReply = 0;
    }
    public void HandleSocketMsg(CHSocketMsg msg)
    {
        try
        {
            if (msg == null || string.IsNullOrEmpty(msg.receiveText))
            {
                return;
            }
            string text = msg.receiveText;
            text = WWW.UnEscapeURL(text);
            CHErrorMsg errorData = CHErrorMsg.GetError(text);
            if (errorData.err_type != 0)
            {
                UnityEngine.Debug.Log("error type : " + errorData.err_type + " error reason: " + errorData.reason);
                string reason = errorData.reason;
                if (errorData.err_type == 2)
                {
                    //CHFuncSocket.instance.CloseAndDestory();
                    //FEPOPWindowUI.instance.ShowOk(errorData.reason, gameObject, errorData.ok_load);
                }
                else if (errorData.err_type == 3)
                {
                   // FEPOPWindowUI.instance.ShowOkCancel(errorData.reason, gameObject, errorData.ok_load, errorData.cancel_load);
                }
                else if (errorData.err_type == 4)
                {
                    if (msg.callBackErr != null && msg.callBackErr.Target != null)
                        msg.callBackErr(reason);
                }
                else
                {
                    //FEPOPWindowUI.instance.ShowOk(errorData.reason, gameObject, "");
                }
            }
            else
            {
                if (msg.successCallBackBeforeFunc != null && msg.successCallBackBeforeFunc.Target != null)
                    msg.successCallBackBeforeFunc();
                if (msg.successCallBack != null)// && msg.successCallBack.Target != null)
                    msg.successCallBack(text);
                if (msg.successCallBackAfterFunc != null)// && msg.successCallBackAfterFunc.Target != null)
                    msg.successCallBackAfterFunc();
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }
    void OnDestroy()
    {
        reader = null;
        Close();
    }
}
