using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KBEngine;
using UnityEngine.UI;

/// <summary>
///  怪物去血信息
/// </summary>
public class CMonsterHurtDisplay : CHurtDisplay
{
    private const float MinDistance = 4f;
    private const float MaxDistance = 10f;  //参照距离，最远以这个距离UI显示大小为标准

    private MonsterComponent monsterCompnent;   

    public CMonsterHurtDisplay(MonsterComponent monsterComp)
    {
        monsterCompnent = monsterComp;
        OnEventRegister();
    }

    ~CMonsterHurtDisplay()
    {
        OnDisRegister();
    }

    void OnEventRegister()
    {
        //if (monsterCompnent != null && monsterCompnent.entity.eventObj != null)
        //{
        //    monsterCompnent.entity.eventObj.register("Event_OntriggerFightResultFS", this, "OnFightResultFS");
        //}
    }

    void OnDisRegister()
    {
        //if (monsterCompnent.entity != null && monsterCompnent.entity.eventObj != null)
        //{
        //    monsterCompnent.entity.eventObj.deregister(this);
        //}
    }

    /// <summary>
    /// 战斗结果回调
    /// </summary>
    /// <param name="result"></param>
    /// <param name="damage"></param>
    public void OnFightResultFS(UInt16 result, Int64 damage)
    {
        //if (monsterCompnent.entity == null)
        //{
        //    return;
        //}

        //string getDam = damage.ToString();
        //if ((result & (UInt16)eFightResultType.Hit) <= 0)       //闪避判断
        //{
        //    CreatFightResult("闪避");
        //    return;
        //}
        //else 
        //{

        //    if ((result & (UInt16)eFightResultType.Crit) <= 0)      //是否是普通攻击
        //    {
        //        CreatFightResult(getDam);
        //        return;
        //    }
        //    else    //暴击判断
        //    {
        //        CreatFightResult("暴击" + getDam);
        //        return;
        //    }
        //}
    }

    /// <summary>
    /// 创建提示物体
    /// </summary>
    public void CreatFightResult(string damage)
    {

        GameObject rootObject = GetDynamicMessageRoot();
        //生成伤害信息
        GameObject hurtObj = ResourceManager.InstantiateAssetBundleResource(CPrefabPaths.MonsterHurtMessage);
        hurtObj.name = "MonsterHurtMessage";
        hurtObj.transform.parent = rootObject.transform;
        hurtObj.transform.position = monsterCompnent.modelCompent.upPos.position;
        hurtObj.transform.rotation = VRInputManager.Instance.camera.transform.rotation;

        //根据距离调整UI的缩放
        float distance = Vector3.Distance(VRInputManager.Instance.camera.transform.position, hurtObj.transform.position);
        distance = Mathf.Clamp(distance, MinDistance, MaxDistance);
        float aspect = distance / MaxDistance;
        hurtObj.transform.localScale = Vector3.one * aspect;
        hurtObj.GetComponent<CMonsterHurtMessage>().Display(damage, aspect);
    }
}
