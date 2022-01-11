using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CGameSceneState : CBaseState
{

    public override void Enter()
    {
        GlobalEvent.register("playerLeaveSpace", this, "OnLeaveSpace");

        GlobalEvent.fire("GameSceneState_Enter");
    }

    public override void Leave()
    {
        GlobalEvent.deregister(this);
        GlobalEvent.fire("GameSceneState_Leave");
    }

    public void OnLeaveSpace()
    {
        CGameState.Instance().ChangeState(GameStateEnum.ChangeSceneState);
    }

}
