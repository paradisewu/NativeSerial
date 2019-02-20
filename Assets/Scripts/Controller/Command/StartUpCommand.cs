using UnityEngine;
using System.Collections;
using LuaFramework;
using RenderHeads.Media.AVProVideo;

public class StartUpCommand : ControllerCommand
{
    public override void Execute(IMessage message)
    {

        AppFacade.Instance.AddManager<SoundManager>(ManagerName.Sound);

        AppFacade.Instance.AddManager<UIManager>(ManagerName.UIManager);

        AppFacade.Instance.AddManager<MediaPlayer>(ManagerName.MediaPlayer);

        AppFacade.Instance.AddManager<ResourceManager>(ManagerName.Resource);

        AppFacade.Instance.AddManager<GameManager>(ManagerName.GameManager);
    }
}