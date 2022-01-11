 using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShaChongAI : MonoBehaviour
{
    public enum EnermyAIState
    {
        AI_Null,
        /// <summary>出生状态</summary>
        AI_Born,
        /// <summary>空闲状态</summary>
        AI_Idle,
        /// <summary>追击状态</summary>
        AI_FightThink,
        /// <summary>攻击状态</summary>
        AI_CastSpell,
        /// <summary>眩晕状态</summary>
        AI_Dizziness,
        /// <summary>下沉状态</summary>
        AI_Disappear,
        /// <summary>钻出状态</summary>
        AI_Appear,
        /// <summary>回到出生点</summary>
        AI_Reset,
    }
    /// <summary>出生类型</summary>
    public enum BornType
    {
        NUll,
        /// <summary>溶解出生</summary>
        Born_Dissolve,
    }
    public BornType BornEffectType = BornType.Born_Dissolve;
    /// <summary>技能id列表</summary>
    /// [ToolTips("技能id列表")]
    public int[] SkillList = { 3000001, 3000001 };
    public float BornTime = 2.0f;
    /// <summary>虚弱眩晕持续时间</summary>
    public float TotalDizzinessTime = 20.0f;
    /// <summary>警戒范围</summary>
    public float AlertRadius = 20.0f;
    public Object SummonObject;
    public EnermyAIState CurrentState;

    private EnermyAIState vLostState;
    private Animator m_animator;
    private AvatarComponent owner;
    /// <summary>攻击目标</summary>
    private AvatarComponent atkTarget = null;
    /// <summary>用于开启和暂停AI</summary>
    private bool isAIRuning = true;
    /// <summary>是否在攻击中</summary>
    private bool isAttacking = true;
    private bool bornFlag = false;
    /// <summary>是否在下沉隐藏中</summary>
    private bool isDisappearHiding = false;
    private float lastTime = 0.0f;
    /// <summary>招唤小怪的击杀计数</summary>
    private int killCount = 0;
    ///<summary>当前要使用的技能</summary> 
    private SPELL.Spell currentSpell = null;
    private int skillIdIndex = 0;
    private float startDisappearTime = 0.0f;
    ///<summary>开始钻出地面的时间</summary> 
    private float startAppearTime = 0.0f;
    private CapsuleCollider capsuleCollider;
    private List<float> weakHpPercentList = new List<float>();
    /// <summary>已被眩晕的时间</summary>
    private float dizzinessedTime = 0.0f;
    /// <summary>可以被虚弱</summary>
    private bool isCanWeak = true;

    public void Awake()
    {
        if (BornEffectType == BornType.Born_Dissolve)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = false;
            }
        }
    }

    // Use this for initialization
    void Start ()
    {
        lastTime = Time.time;
        CurrentState = EnermyAIState.AI_Born;
        vLostState = EnermyAIState.AI_Null;
        m_animator = GetComponent<Animator>();
        owner = gameObject.GetComponent<AvatarComponent>();
        capsuleCollider = gameObject.GetComponent<CapsuleCollider>();
        owner.eventObj.register("Event_OnActionRestrictChanged", this, "OnActionRestrictChanged");
        owner.eventObj.register("Event_OnReceiveDamage", this, "OnReceiveDamage");
        owner.eventObj.register("Event_OnDead", this, "OnDead");
        GlobalEvent.register("Event_KillCount", this, "SetKillCount");
        //添加霸体状态不让BUFF眩晕
        owner.EffectStatusCounterIncr((int)eEffectStatus.SuperBody);
    }
  
    // Update is called once per frame
    void Update ()
    {
        if (!isAIRuning) return;
        if (atkTarget && CurrentState != EnermyAIState.AI_CastSpell && CurrentState != EnermyAIState.AI_Dizziness)
        {
            //transform.LookAt(atkTarget.transform);
            TraceTargetDirection();
        }
        if (killCount == 3)
        {
            isDisappearHiding = false;
            m_animator.SetBool("disappearFlg", false);
            owner.ChangeHP((int)(owner.MaxHP * 0.2f));
            CurrentState = EnermyAIState.AI_Appear;
        }
        if (isDisappearHiding) return;
        switch (CurrentState)
        {
            case EnermyAIState.AI_Born:
                Born();
                break;
            case EnermyAIState.AI_Idle:
                Idle();
                break;
            case EnermyAIState.AI_FightThink:
                FightThink();
                break;
            case EnermyAIState.AI_CastSpell:
                Attack();
                break;
            case EnermyAIState.AI_Dizziness:
                Dizziness();
                break;
            case EnermyAIState.AI_Disappear:
                Disappear();
                break;
            case EnermyAIState.AI_Appear:
                Appear();
                break;
        }
    }

    public void OnActionRestrictChanged(int newRestrict, int oldRestrict)
    {
        int offset = newRestrict ^ oldRestrict;
        if ((offset & (1 << (int)eActionRestrict.ForbidMove)) > 0)
        {
            bool value = (newRestrict & (1 << (int)eActionRestrict.ForbidMove)) > 0;
            if (value)
            {
                isAIRuning = false;
            }
            else
            {
                isAIRuning = true;
            }
        }
    }

    public void OnReceiveDamage(AvatarComponent attacker)
    {
        if (CurrentState != EnermyAIState.AI_Born && CurrentState != EnermyAIState.AI_Disappear && isCanWeak && !isDisappearHiding && atkTarget)
        {
            float CurHp = owner.HP;
            float MaxHp = owner.MaxHP;
            if (weakHpPercentList.Count == 0)
            {
                for (int i = 1; i < 5; i++)
                {
                    weakHpPercentList.Add(MaxHp * (1 - i * 0.2f));
                }
            }
            if (weakHpPercentList.Count == 0) return;
            if (CurHp <= weakHpPercentList[0])
            {
                if (CurrentState != EnermyAIState.AI_Dizziness)
                {
                    dizzinessedTime = 0;
                    CurrentState = EnermyAIState.AI_Dizziness;
                }
                weakHpPercentList.RemoveAt(0);
                if (weakHpPercentList.Count == 0) isCanWeak = false;
            }
        }  
    }

    public void SetKillCount()
    {
        killCount++;
    }

    public void Idle()
    {
        if (CurrentState != vLostState)
        {
            m_animator.SetInteger("runInt", 0);
            vLostState = CurrentState;
        }
        List<AvatarComponent> FindAvatarObjList = AvatarComponent.AvatarInRange(AlertRadius, owner, Vector3.zero);
        foreach (AvatarComponent AvatarObj in FindAvatarObjList)
        {
            if (AvatarObj.status != eEntityStatus.Death && (owner.CheckRelationship(AvatarObj) == eTargetRelationship.HostilePlayers || owner.CheckRelationship(AvatarObj) == eTargetRelationship.HostileMonster))
            {
                atkTarget = AvatarObj;
                CurrentState = EnermyAIState.AI_FightThink;
               // Debug.LogError(Vector3.Distance(owner.transform.position,atkTarget.transform.position));
            }
        }
    }

    public void Born()
    {
        if (CurrentState != vLostState)
        {
            vLostState = CurrentState;
        }
        if (!bornFlag)
        {
            bornFlag = true;
            if (owner)
            {
                if (BornEffectType == BornType.Born_Dissolve)
                {
                    owner.effectManager.AddModelEffect("InverseDissolveEffect");
                }
            }
        }
        if (Time.time - lastTime > BornTime)
        {         
            CurrentState = EnermyAIState.AI_Idle;
        }
    }

    public void OnDead(CDeadType deadType)
    {
        isAIRuning = false;
        m_animator.SetBool("die",true);
        StartCoroutine(DelayDestroy(3.5f));
    }

    /// <summary>延迟销毁gameObject</summary>
    /// <param name="delay"></param>
    private IEnumerator DelayDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        owner.Destroy();
    }

    void FightThink()
    {
        if (atkTarget.status == eEntityStatus.Death)
        {
            atkTarget = null;
            CurrentState = EnermyAIState.AI_Idle;
            return;
        }
        if (CurrentState != vLostState)
        {
            m_animator.SetBool("fight", true);
            vLostState = CurrentState;
        }
        currentSpell = SelectSpell();
        if (currentSpell != null)
        {
            CurrentState = EnermyAIState.AI_CastSpell;
            return;
        }
    }

    void Attack()
    {
        if (CurrentState != vLostState)
        {
            vLostState = CurrentState;
        }
        CastSpell();
        bool isCastFlag = SPELL.Spell.Locked(gameObject);
        if (!isCastFlag && !isAttacking)
        {
            currentSpell = null;
            isAttacking = true;
            CurrentState = EnermyAIState.AI_FightThink;
        }
    }

    /// <summary>眩晕</summary>
    private void Dizziness()
    {
        dizzinessedTime += Time.deltaTime;
        if (CurrentState != vLostState)
        {
            vLostState = CurrentState;
            m_animator.SetInteger("runInt", 1);
            m_animator.SetBool("dizziness", true);
        }
        Debug.LogError(dizzinessedTime);
        if (dizzinessedTime >= TotalDizzinessTime)
        {
            m_animator.SetBool("dizziness", false);
            m_animator.SetBool("disappearFlg", true); 
            CurrentState = EnermyAIState.AI_Disappear;
        }
    }

    /// <summary>沉入地下</summary>
    void Disappear()
    {
        if (CurrentState != vLostState)
        {
            isDisappearHiding = true;
            capsuleCollider.enabled = false;
            startDisappearTime = Time.time;
            m_animator.Play("disappear");
            for (int i = 0; i < 3; i++ )
            {
                UseSummonSkill();
            }
            vLostState = CurrentState;
        }

        float fTime = Time.time - startDisappearTime;
        if(fTime >= 2.7f)
        {
            m_animator.speed = 0;
        }
    }

    /// <summary>使用招唤技能招唤小怪</summary>
    void UseSummonSkill()
    {
        GameObject summonMonster = Instantiate(SummonObject) as GameObject;
        Vector3 targetPosition = CalcHoverPosition(atkTarget.transform.position);
        Vector3 direction1 = transform.position;
        direction1.y = 0.0f;
        Vector3 direction2 = targetPosition;
        direction2.y = 0.0f;
        Vector3 direction = direction2 - direction1;
        Vector3.Normalize(direction);

        summonMonster.transform.position = SamplePosition(targetPosition);
        summonMonster.transform.forward = direction;
    }

    private Vector3 SamplePosition(Vector3 _position)
    {
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(_position, out hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
        {
            _position = hit.position;
        }
        else
        {
            Vector3 targetPosition = CalcHoverPosition(atkTarget.transform.position);
            SamplePosition(targetPosition);
        }
        return _position;
    }

    /// <summary>钻出地面</summary>
    void Appear()
    {
        if (CurrentState != vLostState)
        {
            m_animator.speed = 1;
            startAppearTime = Time.time;
            m_animator.Play("appear");
            vLostState = CurrentState;
        }
        killCount = 0;
        float PastTime = Time.time - startAppearTime;
        if (PastTime > 1.2f)
        {
            capsuleCollider.enabled = true;
            CurrentState = EnermyAIState.AI_FightThink;
        }
    }

    public void CastSpell()
    {
        if (isAttacking)
        {
            m_animator.SetInteger("runInt", 0);
            isAttacking = false;

            //Vector3 dir1 = transform.position;
            //dir1.y = 0.0f;
            //Vector3 dir2 = atkTarget.transform.position;
            //dir2.y = 0.0f;
            //Vector3 dir = dir2 - dir1;
            //Vector3.Normalize(dir);
            //transform.forward = dir;

            CastAssignSpell(currentSpell);
        }
    }

    public void CastAssignSpell(SPELL.Spell spell)
    {
        SPELL.SpellTargetData targetData = new SPELL.SpellTargetData();
        spell.Cast(owner, targetData);
    }

    public SPELL.Spell SelectSpell()
    {
        skillIdIndex = Random.Range(0, SkillList.Length);
        SPELL.SpellTargetData targetData = new SPELL.SpellTargetData();
        var spell = SpellLoader.instance.GetSpell(SkillList[skillIdIndex]);
        if (spell == null)
            return null;
        if (spell.CanStart(owner, targetData) == SPELL.SpellStatus.OK)
            return spell;
        return null;
    }

    private Vector3 CalcHoverPosition(Vector3 center)
    {
        float r = 3 * Random.value + 3;
        float b = 360.0f * Random.value;
        float x = r * Mathf.Cos(b);
        float z = r * Mathf.Sin(b);
        Vector3 pos = new Vector3(center.x + x, center.y, center.z + z);
        return pos;
    }

    private void TraceTargetDirection()
    {
        if(atkTarget)
        {
            Vector3 dir1 = transform.position;
            dir1.y = 0.0f;
            Vector3 dir2 = atkTarget.transform.position;
            dir2.y = 0.0f;
            Vector3 dir = dir2 - dir1;
            Vector3.Normalize(dir);
            transform.forward = dir;
        }
    }
}
