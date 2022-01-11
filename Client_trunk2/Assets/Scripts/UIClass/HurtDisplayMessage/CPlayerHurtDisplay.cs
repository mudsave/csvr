using UnityEngine;
using System.Collections;

/// <summary>
/// 玩家伤害显示
/// </summary>
public class CPlayerHurtDisplay : CHurtDisplay
{
    private PlayerComponent playerCompnent;
    private CPlayerHurtMessage playerHurtMessage;

    public CPlayerHurtDisplay(PlayerComponent playerComp)
    {
        playerCompnent = playerComp;
        OnEventRegister();
    }

    void OnDisable()
    {
        OnDisRegister();
    }

    void OnEventRegister()
    {
        //if (playerCompnent != null && playerCompnent.entity.eventObj != null)
        //{
        //    playerCompnent.entity.eventObj.register("Event_OntriggerFightResultFS", this, "OnFightResultFS");
        //}
    }

    void OnDisRegister()
    {
        //if (playerCompnent.entity != null && playerCompnent.entity.eventObj != null)
        //{
        //    playerCompnent.entity.eventObj.deregister(this);
        //}
    }

    /// <summary>
    /// 战斗结果回调
    /// </summary>
    public void OnFightResultFS(System.UInt16 result, System.Int64 damage)
    {
        //if (playerCompnent.entity == null)
        //{
        //    return;
        //}

        //string getDam = damage.ToString();
        //if ((result & (System.UInt16)eFightResultType.Hit) <= 0)       //闪避判断
        //{
        //    return;
        //}
        //else
        //{
        //    CreatFightResult(getDam);
        //}
    }

    /// <summary>
    /// 创建提示物体
    /// </summary>
    public void CreatFightResult(string damage)
    {
        if (playerHurtMessage == null)
        {
            //玩家伤害添加到主摄像机下
            GameObject hurtObject = Object.Instantiate(Resources.Load(CPrefabPaths.PlayerHurtMessage)) as GameObject;
            hurtObject.name = "PlayerHurtMessage";
            Transform cameraTransform = VRInputManager.Instance.camera.gameObject.transform;
            hurtObject.transform.parent = cameraTransform;
            hurtObject.transform.localPosition = Vector3.zero;
            hurtObject.transform.rotation = cameraTransform.rotation;
            playerHurtMessage = hurtObject.GetComponent<CPlayerHurtMessage>();
        }
        playerHurtMessage.Display();
    }
}
