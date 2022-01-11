using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrengthenMonsterAI : MonoBehaviour
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
        /// <summary>闪避状态</summary>
        AI_Duck,       
        /// <summary>徘徊状态</summary>
        AI_ChaseEntity,
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
    public float BornTime = 4.0f;
    public GameObject cuttingDeathObject;
    /// <summary>警戒范围/summary>
    public float AlertRadius = 6.0f;
    /// <summary>追击半径(超过此半径，怪物应脱离战斗并回到出生点)</summary>
    public float ChaseRadius = 50.0f; 
    /// <summary>徘徊半径（内圈）</summary>
    public float HoverRadiusMin = 3.0f;
    /// <summary>徘徊半径（外圈）</summary>
    public float HoverRadiusMax = 5.0f;
    /// <summary>战斗徘徊移动速度</summary>
    public float HoverWalkSpeed = 3.0f;
    /// <summary>攻击积极性</summary>
    public float AttackActive = 0.7f;
    /// <summary>闪避时间</summary>
    public float DuckTime = 0.2f;
    /// <summary>闪避触发延迟时间</summary>
    public float FeiJianDuckDelayTime = 0.0f;
    /// <summary>闪避触发延迟时间</summary>
    public float NormalDuckDelayTime = 0.5f;
    /// <summary>徘徊扇形角度</summary>
    public float WanderAngle = 100;
    public EnermyAIState vStartState;

    private EnermyAIState vLostState;
    /// <summary>累计已经闪避的时间</summary>
    private float CumDuckTime = 0.0f;
    /// <summary>空闲状态随机移动位置</summary>
    private Vector3 Dis;
    /// <summary>出生坐标</summary>
    private Vector3 bornPoint;
    /// <summary>徘徊一次的目标点</summary>
    private Vector3 targetPosition = Vector3.zero;
    /// <summary>徘徊一次的速度</summary>
    private float targetSpeed = 0.0f;
    private Animator m_animator;
    /// <summary> </summary>
    private bool attackFlag = false;//攻击标记
    /// <summary>是否在攻击中标记</summary>
    private bool isAttack = true;
    /// <summary>是否在闪避中</summary>
    private bool isDucking = true;
    private UnityEngine.AI.NavMeshAgent m_navMeshAgent;
    private float randomValue;
    /// <summary>/用于开启和暂停AI</summary>
    private bool isAIRuning = true; 

    private AvatarComponent owner;
    /// <summary>目标</summary>
    private AvatarComponent atkTarget = null;
    /// <summary>技能id列表</summary>
    public int[] skillList = { 2000001 };       
    /// <summary>当前要使用的技能</summary>
    private SPELL.Spell currentSpell = null;  
    float lastTime = 0.0f;
    private bool bornFlag = false;
    private Vector3 targetDuckPosition;
    private float duckSpeed;
    private CapsuleCollider capsuleCollider = null;

    void Awake()
    {
        if (BornEffectType == BornType.Born_Dissolve)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = false;
            }
        }
        capsuleCollider = gameObject.GetComponent<CapsuleCollider>();
    }

    // Use this for initialization
    void Start()
    {
        lastTime = Time.time;
        vStartState = EnermyAIState.AI_Born;
        vLostState = EnermyAIState.AI_Null;
        bornPoint = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        m_animator = this.GetComponent<Animator>();
        m_navMeshAgent = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
        owner = gameObject.GetComponent<AvatarComponent>();
        owner.eventObj.register("Event_OnActionRestrictChanged", this, "OnActionRestrictChanged");
        owner.eventObj.register("Event_OnStatusChanged", this, "OnStatusChanged");
        owner.eventObj.register("Event_OnReceiveDamage", this, "OnReceiveDamage");
        owner.eventObj.register("Event_OnChangeSpeed", this, "OnChangeSpeed");
        owner.eventObj.register("Event_OnCastFeiJian", this, "OnCastFeiJian");
        owner.eventObj.register("Event_OnDead", this, "OnDead");
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAIRuning)
            return;

        switch (vStartState)
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
            case EnermyAIState.AI_ChaseEntity:
                Chase();
                break;
            case EnermyAIState.AI_CastSpell:
                Attack();
                break;
            case EnermyAIState.AI_Duck:
                Duck();
                break;
            case EnermyAIState.AI_Reset:
                Homing();
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
                StopAI();
            }
            else
            {
                StartAI();
            }
        }
    }

    public void OnStatusChanged(eEntityStatus newStatus, eEntityStatus oldStatus)
    {
    }

    public void OnChangeSpeed(float fSpeed)
    {
        HoverWalkSpeed += fSpeed;
        m_navMeshAgent.speed = fSpeed;
    }

    public void StopAI()
    {
        isAIRuning = false;
        capsuleCollider.enabled = true;
        vStartState = EnermyAIState.AI_FightThink;
        if (m_navMeshAgent.enabled == true)
        {
            m_navMeshAgent.ResetPath();
            m_navMeshAgent.enabled = false;
        }
        m_animator.SetInteger("runInt", 0);
    }

    public void StartAI()
    {
        isAIRuning = true;
        m_navMeshAgent.enabled = true;
    }

    public void OnReceiveDamage(AvatarComponent attacker)
    {
        if (attacker)
        {
            atkTarget = attacker;
        }
        //当在空闲状态下的时候受到来自玩家的伤害则进入战斗思考，否则当被
        //命中的时候进行一次闪避
        if (vStartState == EnermyAIState.AI_Idle)
        {
            vStartState = EnermyAIState.AI_FightThink;
        }
        else
        {
            // vStartState = EnermyAIState.AI_Duck;
            if (isAIRuning)
            {
                StartCoroutine(OnTriggerDuck(NormalDuckDelayTime, attacker));
            }      
        }
    }

    //玩家释放飞剑技能的时候通知怪物闪避
    public void OnCastFeiJian(AvatarComponent attacker)
    {   
        if(isAIRuning)
        {
            StartCoroutine(OnTriggerDuck(FeiJianDuckDelayTime, attacker));
        }          
    }

    private IEnumerator OnTriggerDuck(float delay, AvatarComponent attacker)
    {
        capsuleCollider.enabled = false;
        yield return new WaitForSeconds(delay);
        if (attacker)
        {
            atkTarget = attacker;
        }
        vStartState = EnermyAIState.AI_Duck;
    }

    public void OnDead(CDeadType deadType)
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
                owner.effectManager.AddModelEffect("DissolveEffect");
                StartCoroutine(DelayDestroy(6.0f));
                break;
            case CDeadType.Cutting:
                owner.HideModel();
                GameObject obj = Instantiate(cuttingDeathObject, transform.position, transform.rotation) as GameObject;
                StartCoroutine(DelayDestroy(6.0f));
                break;
            case CDeadType.Fracture:
                owner.HideModel();
                owner.effectManager.AddModelEffect("FractureEffect");
                StartCoroutine(DelayDestroy(6.0f));
                break;
            default:
                break;
        }
    }

    /// <summary>延迟销毁gameObject</summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    private IEnumerator DelayDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        owner.Destroy();
    }

    void Born()
    {
        if (vStartState != vLostState)
        {
            m_animator.SetInteger("runInt", 0);
            vLostState = vStartState;
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
            vStartState = EnermyAIState.AI_Idle;
        }
    }

    void Idle()
    {
        if (vStartState != vLostState)
        {
            m_animator.SetInteger("runInt", 0);
            vLostState = vStartState;
        }
        List<AvatarComponent> objs = AvatarComponent.AvatarInRange(AlertRadius, owner, Vector3.zero);
        foreach (AvatarComponent obj in objs)
        {
            if (obj.status != eEntityStatus.Death && (owner.CheckRelationship(obj) == eTargetRelationship.HostilePlayers || owner.CheckRelationship(obj) == eTargetRelationship.HostileMonster))
                atkTarget = obj;
        }
        if (!atkTarget)
            return;
        if (Vector3.Distance(transform.position, atkTarget.transform.position) < AlertRadius)
        {
            vStartState = EnermyAIState.AI_FightThink;
        }
    }

    void FightThink()
    {
        if (atkTarget.status == eEntityStatus.Death)
        {
            atkTarget = null;
            vStartState = EnermyAIState.AI_Reset;
            return;
        }

        if (vStartState != vLostState)
        {
            randomValue = Random.value;
            if (randomValue < AttackActive) //根据攻击积极性判断是徘徊还是攻击
            {
                attackFlag = true;
            }
            else
            {
                attackFlag = false;
            }

            m_animator.SetBool("fight", true);
            vLostState = vStartState;
        }

        //只要离出生点太远了，就要脱离战斗
        if (Vector3.Distance(bornPoint, transform.position) > ChaseRadius &&
            Vector3.Distance(transform.position, atkTarget.transform.position) > AlertRadius)
        {
            vStartState = EnermyAIState.AI_Reset;
            return;
        }

        currentSpell = SelectSpell();
        if (currentSpell != null && attackFlag)
        {
            vStartState = EnermyAIState.AI_CastSpell;
            return;
        }
        vStartState = EnermyAIState.AI_ChaseEntity;
    }

    void Chase()
    {
        if (vStartState != vLostState)
        {
            vLostState = vStartState;
            _chaseEntity();
            m_navMeshAgent.enabled = true;
            m_navMeshAgent.ResetPath();
            m_navMeshAgent.updateRotation = false;
            m_navMeshAgent.speed = targetSpeed;
        }

        Vector3 dir1 = transform.position;
        dir1.y = 0.0f;
        Vector3 dir2 = atkTarget.transform.position;
        dir2.y = 0.0f;
        Vector3 dir = dir2 - dir1;
        Vector3.Normalize(dir);
        transform.forward = dir;

        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(targetPosition, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
        {
            m_navMeshAgent.enabled = true;
            targetPosition = hit.position;
            actionChange();
            m_navMeshAgent.destination = targetPosition;
        }
        else
        {
            _chaseEntity();
            Chase();
        }

        float disposition = Vector2.Distance(new Vector2(targetPosition.x, targetPosition.z), new Vector2(transform.position.x, transform.position.z));
        if (disposition < 0.1f)
        {
            m_animator.SetInteger("runInt", 0);
            vStartState = EnermyAIState.AI_FightThink;
        }
    }

    void Attack()
    {
        if (vStartState != vLostState)
        {
            vLostState = vStartState;
        }

        if (currentSpell.distance < Vector3.Distance(transform.position, atkTarget.transform.position) && isAttack)
        {
            m_animator.SetInteger("runInt", 1);
            transform.LookAt(atkTarget.transform.position);
            m_navMeshAgent.enabled = true;
            m_navMeshAgent.ResetPath();
            m_navMeshAgent.updateRotation = false;
            m_navMeshAgent.speed = HoverWalkSpeed;
            m_navMeshAgent.destination = atkTarget.transform.position;
        }
        else
        {
            if (m_navMeshAgent.enabled)
            {
                m_navMeshAgent.ResetPath();
                m_navMeshAgent.enabled = false;
            }
            CastSpell();
        }

        bool isCastFlag = SPELL.Spell.Locked(gameObject);
        if (!isCastFlag && !isAttack)
        {
            currentSpell = null;
            isAttack = true;
            m_animator.SetInteger("runInt", 0);
            vStartState = EnermyAIState.AI_FightThink;
        }
    }

    void CastSpell()
    {
        if (isAttack)
        {
            m_animator.SetInteger("runInt", 0);
            isAttack = false;
            transform.LookAt(atkTarget.transform.position);
            CastSpell(currentSpell);
        }
    }

    void Duck()
    {
        if (vStartState != vLostState)
        {
            vLostState = vStartState;
        }

        if (m_navMeshAgent.enabled)
        {
            m_navMeshAgent.ResetPath();
            m_navMeshAgent.enabled = false;
        }

        CastDuck();

        Vector3 dir1 = transform.position;
        dir1.y = 0.0f;
        Vector3 dir2 = atkTarget.transform.position;
        dir2.y = 0.0f;
        Vector3 dir = dir2 - dir1;
        Vector3.Normalize(dir);
        transform.forward = dir;

        CumDuckTime += Time.deltaTime;
        if (!isDucking && CumDuckTime > DuckTime)
        {
            CumDuckTime = 0.0f;
            isDucking = true;
            capsuleCollider.enabled = true;
            vStartState = EnermyAIState.AI_FightThink;
        }
    }

    void CastDuck()
    {
        if (isDucking)
        {
            m_animator.SetInteger("runInt", 0);
            //startDuckTime = Time.time;
            isDucking = false;      
            int k = Random.Range(0, 3);
            if (k == 0)
            {
                targetDuckPosition = transform.position - transform.forward * 2.0f;
                m_animator.Play("DodgeBack");
                duckSpeed = 2.0f / 0.5f;
            }
            else if (k == 1)
            {
                targetDuckPosition = transform.position - transform.right * 2.0f;
                m_animator.Play("Dodgeleft");
                duckSpeed = 2.0f / 0.3f;
            }
            else
            {
                targetDuckPosition = transform.position - transform.right * 2.0f;
                m_animator.Play("Dodgeright");
                duckSpeed = 2.0f / 0.3f;
            }
        }

        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(targetDuckPosition, out hit, 2f, UnityEngine.AI.NavMesh.AllAreas))
        {
            targetDuckPosition = hit.position;
        }

        float step = duckSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetDuckPosition, step);
    }

    void Homing()
    {
        if (vStartState != vLostState)
        {
            m_animator.SetInteger("runInt", 1);
            vLostState = vStartState;
            transform.LookAt(bornPoint);
            m_navMeshAgent.enabled = true;
            m_navMeshAgent.ResetPath();
            m_navMeshAgent.updateRotation = false;
            m_navMeshAgent.speed = HoverWalkSpeed;
            m_navMeshAgent.destination = bornPoint;
        }

        if (Vector2.Distance(new Vector2(bornPoint.x, bornPoint.z), new Vector2(transform.position.x, transform.position.z)) < 1)
        {
            vStartState = EnermyAIState.AI_Idle;
        }
    }

    void calcRandomWalkPosition()
    {
        Vector3 center = bornPoint;
        float r = Random.Range(1, 3);
        float b = 360.0f * Random.value;
        float x = r * Mathf.Cos(b);
        float y = r * Mathf.Sin(b);
        Dis = new Vector3(center.x + x, transform.position.y, center.z + y);
    }

    Vector3 retreatCalcHoverPosition(Vector3 distance, Vector3 center)
    {
        float len = distance.magnitude;
        float half = (HoverRadiusMin + HoverRadiusMax) / 2;
        float l = 0;
        if (len <= half)
            l = HoverRadiusMax;
        else
            l = HoverRadiusMin;

        Vector3 dispos = distance.normalized * l + center;
        return dispos;
    }

    Vector3 sectorHoverPosition()
    {
        float startValue = 90 - WanderAngle * 0.5f;
        float endValue = startValue + WanderAngle;
        float startAngle = (startValue - atkTarget.transform.eulerAngles.y) * Mathf.PI / 180;
        float endAngle = (endValue - atkTarget.transform.eulerAngles.y) * Mathf.PI / 180;
        float randAngle = startAngle + randValue() * (endAngle - startAngle);
        float randRadius = Vector3.Distance(transform.position, atkTarget.transform.position);
        float randX = atkTarget.transform.position.x + randRadius * Mathf.Cos(randAngle);
        float randY = atkTarget.transform.position.z + randRadius * Mathf.Sin(randAngle);
        return new Vector3(randX, 0, randY);
    }

    float randValue()
    {
        float fAngle = Random.value;
        if (fAngle < 0.15f)//为了增强体验，不让随机的角度太小做的一个限制
        {
            randValue();
        }
        return fAngle;
    }

    void _chaseEntity()
    {
        float dis = Vector3.Distance(transform.position, atkTarget.transform.position);
        if (HoverRadiusMax + 0.1f < dis)
        {
            Vector3 dir = transform.position - atkTarget.transform.position;
            targetPosition = atkTarget.transform.position + dir.normalized * HoverRadiusMax;
            targetSpeed = HoverWalkSpeed;
        }
        else
        {
            if (Random.value > 0.5f)
            {
                Vector3 distance = transform.position - atkTarget.transform.position;
                Vector3 pos = retreatCalcHoverPosition(distance, atkTarget.transform.position);
                targetPosition = pos;
                targetSpeed = HoverWalkSpeed;
            }
            else
            {
                Vector3 pos = sectorHoverPosition();
                targetPosition = pos;
                targetSpeed = HoverWalkSpeed;
            }
        }
    }

    Vector3 rotalteXZ(Vector3 v, float angle)
    {
        //向量按照angle度数旋转
        //argle是弧度
        float x = v[0] * Mathf.Cos(angle) + 0 + (-v[2] * Mathf.Sin(angle));
        float y = v[1];
        float z = v[0] * Mathf.Sin(angle) + 0 + v[2] * Mathf.Cos(angle);
        return new Vector3(x, y, z);
    }

    float angleEx(Vector3 position)
    {
        Vector3 a = transform.forward;
        Vector3 b = position - transform.position;
        a.y = b.y = 0.0f;

        float angle = Vector3.Angle(a, b);
        Vector3 tempDir = Vector3.Cross(a, b);
        if (tempDir.y < 0)
            angle = -angle;
        return angle;
    }

    //根据不同速度播放动作，其中actionFlag = 1表示run, 2为left,3为right,4为back，5为walk
    void actionChange()
    {
        float angle = angleEx(targetPosition);
        if (angle >= -45 && angle <= 45)
        {
            m_animator.SetInteger("runInt", 1);
        }
        else if (angle >= -135 && angle < -45)
        {
            m_animator.SetInteger("runInt", 2);
        }
        else if (angle > 45 && angle <= 135)
        {
            m_animator.SetInteger("runInt", 3);
        }
        else
        {
            m_animator.SetInteger("runInt", 4);
        }
    }

    /// <summary>
    /// 技能列表里选择一个可以释放的技能
    /// </summary>
    /// <returns></returns>
    private SPELL.Spell SelectSpell()
    {
        SPELL.SpellTargetData targetData = new SPELL.SpellTargetData();
        foreach (int id in skillList)
        {
            var spell = SpellLoader.instance.GetSpell(id);
            if (spell == null)
                continue;
            if (spell.CanStart(owner, targetData) == SPELL.SpellStatus.OK)
                return spell;
        }
        return null;
    }

    /// <summary>
    /// 释放技能
    /// </summary>
    /// <param name="spell"></param>
    private void CastSpell(SPELL.Spell spell)
    {
        SPELL.SpellTargetData targetData = new SPELL.SpellTargetData();
        spell.Cast(owner, targetData);
    }
}
