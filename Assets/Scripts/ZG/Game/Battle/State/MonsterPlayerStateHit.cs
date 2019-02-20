﻿/*
 * 脚本名(ScriptName)：    MonsterPlayerStateHit.cs
 * 作者(Author):           小宝
 * 官网(Url):              http://www.youke.pro
 */
using UnityEngine;
using System.Collections;
using LuaFramework;

public class MonsterPlayerStateHit : StateBase 
{
    public MonsterPlayerStateHit(PlayerBase player)
        : base(player)
    {

    }

    public override uint GetStateID()
    {
        return StateDef.hit;
    }

    public override void OnEnter(StateMachine machine, IState prevState, object param1, object param2)
    {
        mPlayer.Play("hurt");
    }

    public override void OnLeave(IState nextState, object param1, object param2)
    {
       // AppFacade.Instance.GetManager<GameServerMgr>(ManagerName.HttpServer).RequsterNotifier(CombatDef.AttackOver, mPlayer, nextState.GetStateID());
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

    public override void OnAnimationAttackOver(string clipName)
    {
        Debug.Log(22);
    }

    public override void OnAnimationEventEnd(string clipName)
    {
        Debug.Log(11);
        mPlayer.SwitchState(StateDef.idle);
    }
}
