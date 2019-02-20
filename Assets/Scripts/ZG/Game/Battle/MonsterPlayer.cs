﻿/*
 * 脚本名(ScriptName)：    MonsterPlayer.cs
 * 作者(Author):           小宝
 * 官网(Url):              http://www.youke.pro
 */
using UnityEngine;
using System.Collections;

public class MonsterPlayer : PlayerBase 
{
    public override void SwitchState(uint stateId, object param1 = null, object param2 = null)
    {
        base.SwitchState(stateId, param1, param2);
    }

    public override void OnAwake()
    {
        base.OnAwake();
        this.camp = 1;
        //注册状态
        mPlayerStateMachine.RegisterState(new MonsterPlayerStateAttack(this));
        mPlayerStateMachine.RegisterState(new MonsterPlayerStateIdle(this));
        mPlayerStateMachine.RegisterState(new MonsterPlayerStateHit(this));
    }
}
