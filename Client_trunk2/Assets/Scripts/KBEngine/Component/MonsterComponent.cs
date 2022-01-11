using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterComponent : AvatarComponent
{
    private CMonsterHurtDisplay monsterHurtDisplay;   //伤害值显示
    
    public new void Awake()
    {
        base.Awake();
        SwitchCollider(true);
        objectType = CEntityType.Monster;
    }

    public override void Destroy()
    {
        base.Destroy();
        Destroy(gameObject);
    }

    public override void EnterWorld()
    {
        SwitchCollider(true);
    }

    public override void LeaveWorld()
    {
        SwitchCollider(false);
    }

    public override void OnStatusChanged(eEntityStatus newStatus, eEntityStatus oldStatus)
    {
    }

    public override void OnEffectStatusChanged(int newStatus, int oldStatus)
    {
        base.OnEffectStatusChanged(newStatus, oldStatus);

        int offset = newStatus ^ oldStatus;
        if ((offset & (1 << (int)eEffectStatus.HitBy)) > 0)
        {
            //animatPosSyncFlag = eAnimatPosSyncFlag.NotSyncOnCollideStoper;
        }
    }

    ///// <summary>
    ///// 延迟销毁gameObject
    ///// </summary>
    ///// <param name="delay"></param>
    ///// <returns></returns>
    //private IEnumerator DelayDestroy(float delay)
    //{
    //    yield return new WaitForSeconds(delay);
    //    RemoveAvataComponent(id);
    //    Destroy(this.gameObject);
    //}

    public void CastSpell(int skillID)
    {
        var spell = SpellLoader.instance.GetSpell(skillID);
        SPELL.SpellTargetData targetData = new SPELL.SpellTargetData();
        spell.Cast(this, targetData);
    }

    public override void OnReceiveDamage(int damage, AvatarComponent attacker)
    {
        base.OnReceiveDamage(damage, attacker);
        eventObj.fire("Event_OnReceiveDamage", new object[] { attacker });
    }

    public override void OnDead(AvatarComponent attacker, CDeadType deadType)
    {
        CreateBloodBead();
        effectManager.ClearAllEffects();  
        eventObj.fire("Event_OnDead", new object[] { deadType });
    }

    /// <summary>
    /// 隐藏模型
    /// 根据当前怪物模型结构的临时写法
    /// </summary>
    public override void HideModel()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 显示模型
    /// 根据当前怪物模型结构的临时写法
    /// </summary>
    public override void ShowModel()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    public void CreateBloodBead()
    {
        foreach (int value in BloodBead)
        {
            GameObject obj = Instantiate(Resources.Load("Effects/skill_helth/BloodBead")) as GameObject;
            Vector3 startPos = transform.position;
            startPos.y += 1.2f;
            startPos += Random.onUnitSphere * 0.5f;
            obj.transform.position = startPos;

            BloodBeadTrack bt = obj.gameObject.AddComponent<BloodBeadTrack>();
            bt.Init(value);
        }
    }
}