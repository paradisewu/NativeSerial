using UnityEngine;
using System.Collections;

namespace LuaFramework
{
    public class Main : MonoBehaviour
    {
        void Start()
        {
            Screen.SetResolution(1920, 1080, true);

            AppFacade.Instance.StartUp();   //启动游戏
        }
    }
}