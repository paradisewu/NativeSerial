using LuaFramework;
using RenderHeads.Media.AVProVideo;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SceneVideo : SceneBase
{
    private string videoName = "华银01 大门侧屏.mp4";
    private DisplayUGUI vPlayer;

    #region 初始化相关
    protected override void OnInitSkin()
    {
        base.SetMainSkinPath("UI/SceneVideo");
        base.OnInitSkin();
        _type = SceneType.SceneVideo;
    }

    protected override void OnInitDone()
    {
        base.OnInitDone();
        MediaPlayerMgr.m_Loop = true;

        vPlayer = gameObject.GetComponentInChildren<DisplayUGUI>();
        vPlayer._mediaPlayer = MediaPlayerMgr;

        vPlayer._mediaPlayer = MediaPlayerMgr;
        MediaPlayerMgr.Events.AddListener(FinishVideo);

        VideoPlay(videoName);
    }

    protected override void OnClick(GameObject click)
    {
        base.OnClick(click);
        ClickButton(click);
    }

    public override void OnResetArgs(params object[] sceneArgs)
    {
        base.OnResetArgs(sceneArgs);
    }

    #endregion


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
    protected override void OnDestroyFront()
    {
        base.OnDestroyFront();
        RemoveMessage(this, MessageList);
    }
    public override void OnMessage(IMessage message)
    {
        switch (message.Name)
        {
            case MessageDef.VideoPlay:      //更新消息
                MediaPlayerMgr.Rewind(false);
                MediaPlayerMgr.Play();
                break;
        }
    }

    #endregion





    public void VideoPlay(string data)
    {
        string message = data;
        string url = Util.VideoDicPath + data;
        if (!File.Exists(url))
        {
            return;
        }
        MediaPlayerMgr.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, url, true);
    }

    private void FinishVideo(MediaPlayer media, MediaPlayerEvent.EventType type, ErrorCode error)
    {
        if (type == MediaPlayerEvent.EventType.FinishedPlaying && error == ErrorCode.None)
        {
            media.Stop();
        }
    }

    public void ClickButton(GameObject click)
    {
        if (click.name.Equals("AVPro Video"))
        {
            MediaPlayerMgr.Stop();
            UIManager.ShowPanel(PanelType.PanelHandControl);
        }
    }

}
