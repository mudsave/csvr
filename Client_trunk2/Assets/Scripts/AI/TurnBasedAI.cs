using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
public class TurnBasedAI : MonoBehaviour
{

    public enum EnermyAIState
    {
        AI_Null,
        AI_Born,        //出生状态
        AI_Idle,        //空闲状态
        AI_FightThink,  //追击状态
        AI_CastSpell,   //攻击状态
        AI_Reset,       //回到出生点
    }
    public EnermyAIState vStartState;
    EnermyAIState vLostState;

    public enum BornType //出生类型
    {
        NUll,
        Born_Dissolve, //溶解出生
    }
    public BornType bornType = BornType.Born_Dissolve;
    public float bornTime = 4.0f;
    public GameObject cuttingDeathObject;
    public float alertRadius = 6.0f;//警戒范围
    public float attackMoveSpeed = 3.0f;

    Vector3 Dis; //空闲状态随机移动位置
    Vector3 srcPoint;//出生坐标
    Vector3 srcForward;
    float LastThinkTime = 0.0f;
    Vector3 targetPosition = Vector3.zero;//徘徊一次的目标点
    float targetSpeed = 0.0f;//徘徊一次的速度
    Animator m_animator;
    bool attackFlag = false;//攻击标记
    bool isAttack = true;//是否在攻击中标记
    UnityEngine.AI.NavMeshAgent m_navMeshAgent;
    float randomValue;
    bool isAIRuning = true; //用于开启和暂停AI

    private AvatarComponent own;
    private AvatarComponent target = null;   //目标

    public int[] skillList = { 2000001 };      //技能id列表
    private SPELL.Spell currentSpell = null; //当前要使用的技能
    float lastTime = 0.0f;
    bool bornFlag = false;

    private float accumulateTime = 0.0f;

    void Awake()
    {
        if (bornType == BornType.Born_Dissolve)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = false;
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        lastTime = Time.time;
        vStartState = EnermyAIState.AI_Born;
        vLostState = EnermyAIState.AI_Null;
        srcPoint = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        srcForward = transform.forward;

        m_animator = this.GetComponent<Animator>();
        m_navMeshAgent = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
        m_navMeshAgent.acceleration = 1000;
        own = gameObject.GetComponent<AvatarComponent>();

        own.eventObj.register("Event_OnActionRestrictChanged", this, "OnActionRestrictChanged");
        own.eventObj.register("Event_OnStatusChanged", this, "OnStatusChanged");
        own.eventObj.register("Event_OnReceiveDamage", this, "onReceiveDamage");
        own.eventObj.register("Event_OnChangeSpeed", this, "onChangeSpeed");
        own.eventObj.register("Event_OnDead", this, "onDead");
        own.eventObj.register("EventObj_ActionReady", this, "OnActionReady");
    }

    public void OnActionRestrictChanged(int newRestrict, int oldRestrict)
    {
        int offset = newRestrict ^ oldRestrict;
        if ((offset & (1 << (int)eActionRestrict.ForbidMove)) > 0)
        {
            bool value = (newRestrict & (1 << (int)eActionRestrict.ForbidMove)) > 0;
            if (value)
            {
                stopAI();
            }
            else
            {
                startAI();
            }
        }
    }

    public void OnStatusChanged(eEntityStatus newStatus, eEntityStatus oldStatus)
    {
    }

    public void onChangeSpeed(float fSpeed)
    {
        //hoverWalkSpeed += fSpeed;
        //m_navMeshAgent.speed = hoverWalkSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAIRuning)
            return;

        switch (vStartState)
        {
            case EnermyAIState.AI_Born:
                born();
                break;
            case EnermyAIState.AI_Idle:
                idle();
                break;
            case EnermyAIState.AI_FightThink:
                fightThink();
                break;
            case EnermyAIState.AI_CastSpell:
                attack();
                break;
            case EnermyAIState.AI_Reset:
                homing();
                break;
        }
    }

    public void stopAI()
    {
        isAIRuning = false;
        vStartState = EnermyAIState.AI_FightThink;
        if (m_navMeshAgent.enabled == true)
        {
            m_navMeshAgent.ResetPath();
            m_navMeshAgent.enabled = false;
        }
        m_animator.SetInteger("runInt", 0);
    }

    public void startAI()
    {
        isAIRuning = true;
        m_navMeshAgent.enabled = true;
    }

    public void onReceiveDamage(AvatarComponent attacker)
    {
        //if (attacker)
        //{
        //    target = attacker;
        //}
        //vStartState = EnermyAIState.AI_FightThink;
    }

    public void OnActionReady()
    {
        if (target)
        {
            vStartState = EnermyAIState.AI_FightThink;
        }
    }

    public void onDead(CDeadType deadType)
    {
        isAIRuning = false;
        switch (deadType)
        {
            case CDeadType.None:
                break;
            case CDeadType.Normal:
                m_animator.SetBool("die", true);
                StartCoroutine(DelayDestroy(3.0f));
                break;
            case CDeadType.Dissolution:
                m_animator.SetBool("die", true);
                own.effectManager.AddModelEffect("DissolveEffect");
                StartCoroutine(DelayDestroy(6.0f));
                break;
            case CDeadType.Cutting:
                own.HideModel();
                GameObject obj = Instantiate(cuttingDeathObject, transform.position, transform.rotation) as GameObject;
                StartCoroutine(DelayDestroy(6.0f));
                break;
            case CDeadType.Fracture:
                own.HideModel();
                own.effectManager.AddModelEffect("FractureEffect");
                StartCoroutine(DelayDestroy(6.0f));
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 延迟销毁gameObject
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    private IEnumerator DelayDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        own.Destroy();
    }

    void born()
    {
        if (vStartState != vLostState)
        {
            m_animator.SetInteger("runInt", 0);
            vLostState = vStartState;
        }

        if (!bornFlag)
        {
            bornFlag = true;
            if (own)
            {
                if (bornType == BornType.Born_Dissolve)
                {
                    own.effectManager.AddModelEffect("InverseDissolveEffect");
                }
            }
        }

        if (Time.time - lastTime > bornTime)
        {
            vStartState = EnermyAIState.AI_Idle;
        }
    }

    void idle()
    {

        if (vStartState != vLostState)
        {
            m_animator.SetInteger("runInt", 0);
            vLostState = vStartState;
        }

        if (target)
        {
            return;
        }

        List<AvatarComponent> objs = AvatarComponent.AvatarInRange(alertRadius, own, Vector3.zero);

        foreach (AvatarComponent obj in objs)
        {
            if (obj.status != eEntityStatus.Death && (own.CheckRelationship(obj) == eTargetRelationship.HostilePlayers || own.CheckRelationship(obj) == eTargetRelationship.HostileMonster))
                target = obj;
        }
    }

    void fightThink()
    {
        if (target.status == eEntityStatus.Death)
        {
            target = null;
            vStartState = EnermyAIState.AI_Reset;
            return;
        }

        if (vStartState != vLostState)
        {
            m_animator.SetBool("fight", true);
            vLostState = vStartState;
        }

        currentSpell = SelectSpell();
        if (currentSpell != null)
        {
            vStartState = EnermyAIState.AI_CastSpell;
            return;
        }
    }

    void attack()
    {
        if (vStartState != vLostState)
        {
            vLostState = vStartState;
        }

        if (currentSpell == null)
        {

            vStartState = EnermyAIState.AI_Reset;

            return;
        }

        if (currentSpell.distance < Vector3.Distance(transform.position, target.transform.position) && isAttack)
        {
            m_animator.SetInteger("runInt", 1);
            transform.LookAt(target.transform.position);
            m_navMeshAgent.enabled = true;
            m_navMeshAgent.ResetPath();
            m_navMeshAgent.updateRotation = false;
            m_navMeshAgent.speed = attackMoveSpeed;
            m_navMeshAgent.destination = target.transform.position;
        }
        else
        {
            if (m_navMeshAgent.enabled)
            {
                m_navMeshAgent.ResetPath();
                m_navMeshAgent.enabled = false;
                m_animator.SetInteger("runInt", 0);
            }
            castSpell();
        }

        bool isCastFlag = SPELL.Spell.Locked(gameObject);
        if (!isCastFlag && !isAttack)
        {
            currentSpell = null;
            isAttack = true;
        }
    }

    void homing()
    {
        if (vStartState != vLostState)
        {
            m_navMeshAgent.enabled = true;
            m_navMeshAgent.ResetPath();
            m_navMeshAgent.updateRotation = false;

            float distance = Vector2.Distance(new Vector2(srcPoint.x, srcPoint.z), new Vector2(transform.position.x, transform.position.z));
            if (distance > 0.1f)
            {
                m_animator.SetInteger("runInt", 1);
                vLostState = vStartState;
                transform.LookAt(srcPoint);
                m_navMeshAgent.speed = distance / 1.0f;
                m_navMeshAgent.destination = srcPoint;
            }
            else
            {
                vStartState = EnermyAIState.AI_Idle;
                own.eventObj.fire("EventObj_ActionEnd");
                return;
            }
        }

        if (Vector2.Distance(new Vector2(srcPoint.x, srcPoint.z), new Vector2(transform.position.x, transform.position.z)) < 0.1f)
        {
            m_navMeshAgent.ResetPath();
            m_navMeshAgent.enabled = false;
            transform.LookAt(target.transform.position);
            vStartState = EnermyAIState.AI_Idle;
            own.eventObj.fire("EventObj_ActionEnd");
        }
    }

    void castSpell()
    {
        if (isAttack)
        {
            isAttack = false;
            m_navMeshAgent.updateRotation = false;
            transform.LookAt(target.transform.position);
            CastSpell(currentSpell);
        }
    }

    /// <summary>
    /// 技能列表里选择一个可以释放的技能
    /// </summary>
    /// <returns></returns>
    private SPELL.Spell SelectSpell()
    {
        SPELL.SpellTargetData targetData = new SPELL.SpellTargetData();
        int id = Random.Range(0, skillList.Length);

        var spell = SpellLoader.instance.GetSpell(skillList[id]);
        if (spell == null)
            return null;
        if (spell.CanStart(own, targetData) == SPELL.SpellStatus.OK)
            return spell;
        return null;
    }

    /// <summary>
    /// 释放技能
    /// </summary>
    /// <param name="spell"></param>
    private void CastSpell(SPELL.Spell spell)
    {
        SPELL.SpellTargetData targetData = new SPELL.SpellTargetData();
        spell.Cast(own, targetData);
    }
}