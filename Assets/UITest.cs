using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITest : View
{
    public Text m_text;
    #region 消息注册
    List<string> MessageList
    {
        get
        {
            return new List<string>()
            {
                MessageDef.VideoPlay,
            };
        }
    }

    void Awake()
    {
        RemoveMessage(this, MessageList);
        RegisterMessage(this, MessageList);
    }
    protected void OnDestroy()
    {
        RemoveMessage(this, MessageList);
    }
    public override void OnMessage(IMessage message)
    {
        switch (message.Name)
        {
            case MessageDef.VideoPlay:      //更新消息
                m_text.text = "拿起的是" + (int)message.Body;
                break;
        }
    }

    #endregion


}
