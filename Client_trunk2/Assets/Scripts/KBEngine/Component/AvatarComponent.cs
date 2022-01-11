using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class AvatarComponent : GameObjComponent
{
    public int m_hp = 500;     //生命值
    public int HP
    {
        get { return m_hp; }
        set 
        { 
            m_hp = value;
            OnHPChange();
        }
    }
    public int m_maxHP = 500;  //生命值上限
    public int MaxHP
    {
        get { return m_maxHP; }
        set 
        { 
            m_maxHP = value;
            OnHPChange();
        }
    }

    public float actionTime = 5.0f;

    public int magicSpar = 0; //魔法晶石
    public int MagicSpar
    {
        get { return magicSpar; }
        set { magicSpar = value; }
    }

    private int m_maxMagicSpar = 10; //最大晶石数量
    public int MaxMagicSpar
    {
        get { return m_maxMagicSpar; }
        set { m_maxMagicSpar = value; }
    }

    public List<int> BloodBead;

    [HideInInspector]
    public int effectStatus;   // 效果状态
    [HideInInspector]
    public int actionRestrict; // 行为限制
    [HideInInspector]
    public eEntityStatus status = eEntityStatus.Idle;

    public GameObject rightWeapon = null;
    public GameObject leftWeapon = null;

    [HideInInspector]
    public SPELL.CooldownManager cooldowns;
    private int[] effectStatusCounter = new int[(int)eEffectStatus.Max];
    private int[] actionRestrictCounter = new int[(int)eActionRestrict.Max];

    public static Dictionary<Int32, AvatarComponent> avatarComponentList = new Dictionary<Int32, AvatarComponent>();

    /// <summary>
    /// 存储buff数据的字典
    /// </summary>
    public Dictionary<Int32, Alias.BuffDataType> buffs = new Dictionary<Int32, Alias.BuffDataType>();

    /// <summary>
    /// 临时动态数据存放处，通常用于技能等一些需要动态存放临时数据的时候使用
    /// </summary>
    private Dictionary<string, object> m_tempMapping = new Dictionary<string, object>();

    /// <summary>
    /// 精力系统
    /// </summary>
    [HideInInspector]
    public EnergyMgr energyMgr = null;

    public new void Awake()
    {
        base.Awake();

        AddAvatarComponent(this);
        cooldowns = new SPELL.CooldownManager();
    }

    void OnDestroy()
    {
        RemoveAvataComponent(id);
    }

    public override void Destroy()
    {
        base.Destroy();
    }

    public void SwitchCollider(bool open)
    {
        if (gameObject.GetComponent<Collider>() != null)
        {
            gameObject.GetComponent<Collider>().enabled = open;
        }
    }

    #region Buff Function
    public void AddBuff(Alias.BuffDataType buffData)
    {
        buffs.Add(buffData.index, buffData);
    }

    public void InterruptBuff(Int32 interruptCode)
    {
        List<Int32> keyList = new List<Int32>(buffs.Keys);
        for (int i = 0; i < keyList.Count; i++)
        {
            var buff = (SPELL.SpellBuff)buffs[keyList[i]].GetBuff();
            buff.Interrupt(this, buffs[keyList[i]], interruptCode);
        }
    }

    public void RemoveBuff(Int32 index)
    {
        if (!buffs.ContainsKey(index))
            return;

        buffs.Remove(index);
    }

    public Alias.BuffDataType GetBuffByID(Int32 buffID)
    {
        foreach (var buffData in buffs.Values)
        {
            if (buffData.buffID == buffID)
                return buffData;
        }
        return null;
    }

    public Alias.BuffDataType GetBuffByIndex(Int32 index)
    {
        if (!buffs.ContainsKey(index))
            return null;

        return buffs[index];
    }
    #endregion Buff Function

    #region Mapping Function

    /// <summary>
    /// 保存一个临时数据
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void SetMapping(string key, object value)
    {
        m_tempMapping[key] = value;
    }

    /// <summary>
    /// 查询一个临时数据
    /// </summary>
    /// <param name="key"></param>
    /// <returns>返回key所对应的临时数据，如果数据不存在则返回null</returns>
    public object QueryMapping(string key)
    {
        if (m_tempMapping.ContainsKey(key))
            return m_tempMapping[key];
        return null;
    }

    public void DelMapping(string key)
    {
        if (m_tempMapping.ContainsKey(key))
            m_tempMapping.Remove(key);
    }

    public object PopMapping(string key)
    {
        if (m_tempMapping.ContainsKey(key))
        {
            var result = m_tempMapping[key];
            m_tempMapping.Remove(key);
            return result;
        }
        return null;
    }

    #endregion Mapping Function

    public virtual void SetAnimatorValue(string name, bool value)
    {
        //if (entity.isPlayer())
        //    return;

        //if (animator != null)
        //    animator.SetBool(name, value);
    }

    public virtual void OnSpellCastOverMSG(SPELL.SpellEx spell)
    {
    }

    public virtual void OnSpellFireMSG(SPELL.SpellEx spell)
    {
    }

    /// <summary>
    /// 行为限制计数器加一
    /// </summary>
    /// <param name="flag"></param>
    public void ActionRestrictCounterIncr(int flag)
    {
        actionRestrictCounter[flag] += 1;
        if (actionRestrictCounter[flag] == 1)
        {
            int oldActionRestrict = actionRestrict;
            actionRestrict |= (1 << flag);
            OnActionRestrictChanged(actionRestrict, oldActionRestrict);
        }
    }

    /// <summary>
    /// 行为限制计数器减一
    /// </summary>
    /// <param name="flag"></param>
    public void ActionRestrictCounterDecr(int flag)
    {
        actionRestrictCounter[flag] -= 1;
        if (actionRestrictCounter[flag] == 0)
        {
            int oldActionRestrict = actionRestrict;
            actionRestrict &= ~(1 << flag);
            OnActionRestrictChanged(actionRestrict, oldActionRestrict);
        }
    }

    /// <summary>
    /// 某个行为是否被限制了
    /// </summary>
    /// <param name="flag"></param>
    /// <returns></returns>
    public bool HasActionRestrict(eActionRestrict flag)
    {
        return (actionRestrict & (1 << (int)flag)) > 0;
    }

    /// <summary>
    /// 效果状态计数器加一
    /// </summary>
    /// <param name="flag"></param>
    public void EffectStatusCounterIncr(int flag)
    {
        effectStatusCounter[flag] += 1;
        if (effectStatusCounter[flag] == 1)
        {
            int oldEffectStatus = effectStatus;
            effectStatus |= (1 << flag);
            OnEffectStatusChanged(effectStatus, oldEffectStatus);

            foreach (int _flag in Status2Action.map[flag])
            {
                ActionRestrictCounterIncr(_flag);
            }
        }
    }

    /// <summary>
    /// 效果状态计数器减一
    /// </summary>
    /// <param name="flag"></param>
    public void EffectStatusCounterDecr(int flag)
    {
        effectStatusCounter[flag] -= 1;
        if (effectStatusCounter[flag] == 0)
        {
            int oldEffectStatus = effectStatus;
            effectStatus &= ~(1 << flag);
            OnEffectStatusChanged(effectStatus, oldEffectStatus);

            foreach (int _flag in Status2Action.map[flag])
            {
                ActionRestrictCounterDecr(_flag);
            }
        }
    }

    /// <summary>
    /// 是否获得了某个效果状态
    /// </summary>
    /// <param name="flag"></param>
    /// <returns></returns>
    public bool HasEffectStatus(eEffectStatus flag)
    {
        return (effectStatus & (1 << (int)flag)) > 0;
    }

    public virtual void OnStatusChanged(eEntityStatus newStatus, eEntityStatus oldStatus)
    {
        eventObj.fire("Event_OnStatusChanged", new object[] { newStatus, oldStatus });
    }

    public virtual void OnActionRestrictChanged(int newRestrict, int oldRestrict)
    {
        eventObj.fire("Event_OnActionRestrictChanged", new object[] { newRestrict, oldRestrict });
    }

    public virtual void OnEffectStatusChanged(int newStatus, int oldStatus)
    {

    }

    #region #### Stand-alone Function ####
    public static AvatarComponent GetAvatar(Int32 id)
    {
        if (avatarComponentList.ContainsKey(id))
            return avatarComponentList[id];

        return null;
    }

    public static void AddAvatarComponent(AvatarComponent component)
    {
        avatarComponentList[component.id] = component;
    }

    public static void RemoveAvataComponent(Int32 id)
    {
        avatarComponentList.Remove(id);
    }

    /// <summary>
    /// 施法
    /// </summary>
    /// <param name="skillID"></param>
    public virtual void CastSpell(int skillID, SPELL.SpellTargetData targetData)
    {
        var spell = SpellLoader.instance.GetSpell(skillID);
        spell.Cast(this, targetData);
    }

    /// <summary>
    /// 改变状态
    /// </summary>
    /// <param name="flag"></param>
    public void changeEntityStatus(eEntityStatus flag)
    {
        eEntityStatus old = status;
        if (status == flag)
            return;

        status = flag;

        foreach (int _flag in EntityStatus2Action.map[(int)old])
        {
            ActionRestrictCounterDecr(_flag);
        }

        foreach (int _flag in EntityStatus2Action.map[(int)flag])
        {
            ActionRestrictCounterIncr(_flag);
        }

        OnStatusChanged(status, old);
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="damage"></param>
    public void receiveDamage(AvatarComponent attacker, int damage, CDeadType deadType)
    {
        if (status == eEntityStatus.Death)
            return;

        ChangeHP(-damage);
        OnReceiveDamage(-damage, attacker);
        if (HP <= 0)
        {
            changeEntityStatus(eEntityStatus.Death);
            OnDead(attacker, deadType);
            attacker.OnKilled(this);
        }
    }

    public virtual void OnReceiveDamage(int damag, AvatarComponent attacker)
    {
    }

    /// <summary>
    /// 恢复生命
    /// </summary>
    /// <param name="value"></param>
    public void RecoveryHp(int value)
    {
        if (status == eEntityStatus.Death || value <= 0)
            return;

        ChangeHP(value);
        OnRecoveryHP(value);
    }

    public virtual void OnRecoveryHP(int value)
    {
    }

    /// <summary>
    /// 改变生命值
    /// </summary>
    /// <param name="value"></param>
    public void ChangeHP(int value)
    {
        int newHP = HP + value;
        if (newHP < 0)
            newHP = 0;
        else if (newHP > MaxHP)
            newHP = MaxHP;
        HP = newHP;
        
    }

    protected virtual void OnHPChange()
    {
    }

    /// <summary>
    /// 我被谁杀死了
    /// </summary>
    /// <param name="attacker"></param>
    public virtual void OnDead(AvatarComponent attacker, CDeadType deadType)
    {
    }

    /// <summary>
    /// 我杀死了谁
    /// </summary>
    public virtual void OnKilled(AvatarComponent victim)
    {
    }

    public eCampRelationship CheckCampRelationship(AvatarComponent obj)
    {
        if (objectType == CEntityType.Player)
        {
            if (obj.objectType == CEntityType.Monster)
                return eCampRelationship.Hostile;
            else
                return eCampRelationship.Friendly;
        }
        else if (objectType == CEntityType.Monster)
        {
            if (obj.objectType == CEntityType.Player)
                return eCampRelationship.Hostile;
            else
                return eCampRelationship.Friendly;
        }
        else
            return eCampRelationship.Friendly;
    }

    public eTargetRelationship CheckRelationship(AvatarComponent obj)
    {
        if (id == obj.id)
            return eTargetRelationship.Own;

        if(CheckCampRelationship(obj) == eCampRelationship.Irrelative)
            return eTargetRelationship.None;

        else if (CheckCampRelationship(obj) == eCampRelationship.Friendly)
        {
            if (obj.objectType == CEntityType.Monster)
            {
                return eTargetRelationship.FriendlyMonster;
            }
            else
            {
                return eTargetRelationship.FriendlyPlayers;
            }
        }
        else if (CheckCampRelationship(obj) == eCampRelationship.Hostile)
            if (obj.objectType == CEntityType.Monster)
            {
                return eTargetRelationship.HostileMonster;
            }
            else
            {
                return eTargetRelationship.HostilePlayers;
            }
        else
            if (obj.objectType == CEntityType.NPC)
        {
            return eTargetRelationship.NeutralityNPC;
        }
        else
        {
            return eTargetRelationship.NeutralityPlayers;
        }
    }

    public static List<AvatarComponent> AvatarInRange(float radius, AvatarComponent finder, Vector3 center)
    {
        Vector3 pos = center;
        if (pos == Vector3.zero)
            pos = finder.transform.position;

        Dictionary<Int32, AvatarComponent> objs = avatarComponentList;
        List<AvatarComponent> result = new List<AvatarComponent>();

        var ide = objs.GetEnumerator();
        while (ide.MoveNext())
        {
            KeyValuePair<Int32, AvatarComponent> obj = ide.Current;

            if (obj.Value != null && Vector3.Distance(obj.Value.transform.position, pos) <= radius)
            {
                result.Add(obj.Value);
            }
        }
        return result;
    }

    /// <summary>
    /// 向量旋转一定的角度得到新向量
    /// </summary>
    /// <param name="v"></param>
    /// <param name="angle">弧度</param>
    /// <returns></returns>
    public static Vector3 rotalteXZ(Vector3 v, float angle)
    {
        float x = v.x * Mathf.Cos(angle) + 0 + (-v.z * Mathf.Sin(angle));
        float y = v.y;
        float z = v.x * Mathf.Sin(angle) + 0 + v.z * Mathf.Cos(angle);
        return new Vector3(x, y, z);
    }

    /// <summary>
    /// 检测魔法晶石数量是否足够
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool CheckMagicSparCount(int value)
    {
        if (MagicSpar < value)
            return false;

        return true;
    }

    public void MagicSparCountChange(int value)
    {
        if (value == 0)
            return;

        int oldValue = MagicSpar;
        MagicSpar += value;

        if (MagicSpar < 0)
            MagicSpar = 0;

        if (MagicSpar > MaxMagicSpar)
            MagicSpar = MaxMagicSpar;

        OnMagicSparCountChange(oldValue, MagicSpar);
    }

    protected virtual void OnMagicSparCountChange(int oldValue, int newValue)
    {
    }

    public virtual void OnEnergyChanged(int newValue, int oldValue, int maxValue)
    {
    }

    public virtual void HideModel()
    {
    }

    public virtual void ShowModel()
    {
    }
    #endregion #### Stand-alone Function ####
}