using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 回合管理器
/// </summary>
public class TurnManager : Singleton<TurnManager>
{
    private List<AvatarComponent> battleList = new List<AvatarComponent>();
    private bool pauseFlag = false;

    public void Init()
    {
        battleList.Clear();

        if (VRInputManager.Instance.playerComponent != null)
        {
            battleList.Add(VRInputManager.Instance.playerComponent);
        }
    }

    public void StartBattle()
    {
        foreach (var component in battleList)
        {
            TurnAction turnAction = component.gameObject.AddComponent<TurnAction>();
            turnAction.Init(component);
        }
    }

    public void AddBattleList(AvatarComponent component)
    {
        battleList.Add(component);
    }

    public void RemoveBattleList(AvatarComponent component)
    {
        battleList.Remove(component);
    }

    public void CreateNewBattleObj(AvatarComponent component)
    {
        TurnAction turnAction = component.gameObject.AddComponent<TurnAction>();
        turnAction.Init(component);

        battleList.Add(component);
    }
}
