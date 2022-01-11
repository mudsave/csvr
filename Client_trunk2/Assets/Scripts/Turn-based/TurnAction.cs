using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 测试用的回合行为脚本
/// </summary>
public class TurnAction : MonoBehaviour
{
    public AvatarComponent owner = null;
    public bool isStart = false;
    public bool actionReady = false;

    private float calcTime = 0.0f;
   
    public void Init(AvatarComponent component)
    {
        owner = component;

        if (owner)
        {
            owner.eventObj.register("EventObj_ActionEnd", this, "OnActionEnd");
        }
    }

    void Update ()
    {
        if (!actionReady)
        {
            calcTime += Time.deltaTime;
            if (calcTime >= owner.actionTime)
            {
                owner.eventObj.fire("EventObj_ActionReady");
                actionReady = true;
                calcTime = 0.0f;
            }
        }
    }

    /// <summary>
    /// 行为结束
    /// </summary>
    public void OnActionEnd()
    {
        if (actionReady)
        {
            actionReady = false;
        }
    }
}
