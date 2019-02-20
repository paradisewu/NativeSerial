﻿/*
 * 脚本名(ScriptName)：    MonsterPlayerStateIdle.cs
 * 作者(Author):           小宝
 * 官网(Url):              http://www.youke.pro
 */
using UnityEngine;
using System.Collections;

public class MonsterPlayerStateIdle : StateBase 
{

    public MonsterPlayerStateIdle(PlayerBase player)
        : base(player)
    {

    }

    public override uint GetStateID()
    {
        return StateDef.idle;
    }

    public override void OnEnter(StateMachine machine, IState prevState, object param1, object param2)
    {
        mPlayer.Play("idle");
    }

    public override void OnLeave(IState nextState, object param1, object param2)
    {
    }

    public override void OnUpate()
    {
    }

    public override void OnFixedUpdate()
    {
    }

    public override void OnLeteUpdate()
    {
    }
}
