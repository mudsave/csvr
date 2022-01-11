/// ---------------------------------------------------------
/// 飞剑攻击
/// ---------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VRInputFlySwordAttack1 : MonoBehaviour
{
    /// <summary>
    /// 飞剑状态
    /// </summary>
    public enum SwordStatus
    {
        NULL,               //空
        CLOSING,            //关闭中
        CLOSED,             //关闭
        OPENED,             //开启
        ATTACKING,          //攻击中
        POST_ATTACK,        //攻击后
        END_ATTACK,         //攻击结束
        FOLLOWING,          //跟随
        HOLD,               //手持
        IDLE,               //闲置
    }

    //攻击手柄
    public Hand controllerHand = Hand.RIGHT;
    //飞剑
    public GameObject swordPrefab;
    public GameObject holdSword;
    private Transform sword;
    public Transform swordOrigin;
    //可碰撞层
    public LayerMask attackRaycastLayer;
    public LayerMask followRaycastLayer;
    public LineRenderer line;
    //招回最小手柄速度
    public float mixRecalledVelocity = 4;
    public float followSpeed = 3;
    public float attackSpeed = 8;
    public float postAttackSpeed = 10;
    public bool showLine = false;

    private SwordStatus currentStatus = SwordStatus.CLOSED;
    private SwordStatus nextStatus = SwordStatus.NULL;
    //手
    private Transform hand = null;
    private Transform handTipForward = null;
    //头
    private Transform head = null;
    //飞剑碰撞盒
    private BoxCollider swordCollider = null;

    //攻击目标
    private Transform attackTarget = null;
    private Transform attackTargetPoint = null;
    //攻击结束点
    private Vector3 attackEndPoint;
    //飞剑可招回
    private bool swordRecalled = false;

    private BoxCollider swordOriginCollider = null;
    private int offsetAngle = -15;

    private float attackEndDirection = 1;
    private bool dodgeFlag = false;

    private bool canController = false;      //手柄控制是否有效

    //------------------------- skill----------------------------------
    public GameObject standbyObj = null;

    protected Transform tip_nib;

    private int skillID = -1;
    private EffectComponent skillEffectComponent;
    private string handActionParm = "";

    private GameObject _effectObj = null;
    private PaintPath paintPath = null;
    private AudioSource audioSource = null;
   
    private void Start()
    {
        if (controllerHand == Hand.LEFT)
            tip_nib = VRInputManager.Instance.tip_nib_left;
        else
            tip_nib = VRInputManager.Instance.tip_nib_right;

        if (controllerHand == Hand.RIGHT)
        {
            hand = VRInputManager.Instance.handRight.transform;
            handTipForward = hand.FindChild("RightController/TipForward");
        }
        else
        {
            hand = VRInputManager.Instance.handLeft.transform;
            handTipForward = hand.FindChild("LeftController/TipForward");
        }
        head = VRInputManager.Instance.head.transform;

        CreateSwordModel();

        //SwordOrigin so = swordOrigin.gameObject.AddComponent<SwordOrigin>();
        //so.Init(this);

        //swordOriginCollider = swordOrigin.GetComponent<BoxCollider>();
        //if (swordOriginCollider)
        //    swordOriginCollider.enabled = false;

        SPELL.HoldSwordSkill skill = holdSword.gameObject.AddComponent<SPELL.HoldSwordSkill>();
        if (VRInputManager.Instance.playerComponent != null)
        {
            skill.Init(VRInputManager.Instance.playerComponent);
            skill.SetEnable(true);
        }

        //注册飞剑触发开关事件
        //GlobalEvent.register("Event_SwordOriginColliderSwitch", this, "Event_SwordOriginColliderSwitch");
        SceneManager.sceneLoaded += onSceneLoaded;

        GlobalEvent.register("Event_PlayerStatusChanged", this, "OnPlayerStatusChanged");
        GlobalEvent.register("Event_RegisterControllerEvents", this, "RegisterControllerEvents");
        GlobalEvent.register("Event_DeregisterControllerEvents", this, "DeregisterControllerEvents");

        RegisterControllerEvents();
    }

    /// <summary>
    /// 注册控制器事件
    /// </summary>
    public void RegisterControllerEvents()
    {
        if (canController)
            return;

        GlobalEvent.register("OnGripPressed", this, "OnGripPressed");
        GlobalEvent.register("OnGripReleased", this, "OnGripReleased");

        GlobalEvent.register("OnTriggerPressed", this, "OnPressed");
        GlobalEvent.register("OnTriggerReleased", this, "OnReleased");

        //GlobalEvent.register("OnTouchpadPressed", this, "OnTouchpadPressed");
        canController = true;
    }

    public void DeregisterControllerEvents()
    {
        if (!canController)
            return;

        GlobalEvent.deregister("OnGripPressed", this, "OnGripPressed");
        GlobalEvent.deregister("OnGripReleased", this, "OnGripReleased");

        GlobalEvent.deregister("OnTriggerPressed", this, "OnPressed");
        GlobalEvent.deregister("OnTriggerReleased", this, "OnReleased");

        //GlobalEvent.deregister("OnTouchpadPressed", this, "OnTouchpadPressed");
        canController = false;

        VRInputManager.Instance.handRightAnimator.SetBool("at", false);
        sword.gameObject.SetActive(false);
        ChangeStatus(SwordStatus.CLOSED);
    }

    private void Update()
    {
        //FindAvatarComponent();

        Vector3 dir = -head.forward;
        dir.y = 0;
        Vector3 pos = dir.normalized * 0.3f + head.position + new Vector3(0, -0.2f, 0);
        swordOrigin.position = pos;
        swordOrigin.forward = head.forward;

        if (VRInputManager.Instance.playerComponent.HasEffectStatus(eEffectStatus.SpellCasting))
        {
            if (currentStatus != SwordStatus.CLOSED && currentStatus != SwordStatus.CLOSING && currentStatus != SwordStatus.HOLD)
            {
                ChangeStatus(SwordStatus.IDLE);
                sword.gameObject.SetActive(false);
            }
            return;
        }

        if (currentStatus == SwordStatus.IDLE)
        {
            if (!VRInputManager.Instance.playerComponent.HasEffectStatus(eEffectStatus.SpellCasting))
            {
                ChangeStatus(SwordStatus.FOLLOWING);
                sword.gameObject.SetActive(true);
                VRInputManager.Instance.handRightAnimator.SetBool("at", true);
            }
        }
        else if (currentStatus == SwordStatus.CLOSED || currentStatus == SwordStatus.HOLD)
        {

        }
        else if (currentStatus == SwordStatus.CLOSING)
        {
            //Vector3 originPoint = sword.position;
            //Vector3 targetPoint = handTipForward.position - handTipForward.forward * 5f;

            //sword.LookAt(targetPoint);
            //sword.position = Vector3.MoveTowards(originPoint, targetPoint, Time.deltaTime * 15);

            //if (Vector3.Distance(originPoint, targetPoint) < 1.0f)
            //{
            //    sword.gameObject.SetActive(false);
            //    ChangeStatus(SwordStatus.CLOSED);
            //}

            Vector3 targetPoint = handTipForward.position - handTipForward.forward * 2f;
            sword.LookAt(targetPoint);

            float angle = Mathf.Min(1, Vector3.Distance(sword.position, targetPoint) / 15) * 120;
            sword.rotation = sword.rotation * Quaternion.Euler(Mathf.Clamp(-angle, -42, 42), 0, 0);

            float currentDist = Vector3.Distance(sword.position, targetPoint);
            sword.Translate(Vector3.forward * Mathf.Min(30.0f * Time.deltaTime, currentDist));

            if (Vector3.Distance(sword.position, targetPoint) < 1.0f)
            {
                sword.gameObject.SetActive(false);
                ChangeStatus(SwordStatus.CLOSED);
            }

        }
        else
        {
            RaycastHit hitInfo;
            Vector3 velocity = VRInputManager.Instance.GetControllerEvents(controllerHand).GetVelocity();
            if (velocity.magnitude > mixRecalledVelocity)
            {
                swordRecalled = true;
            }

            if (showLine)
            {
                line.SetVertexCount(2);
                line.SetPosition(0, handTipForward.position);
                line.SetPosition(1, handTipForward.position + handTipForward.forward * 20f);
            }
            line.enabled = showLine;

            switch (currentStatus)
            {
                //------------------------------------------------
                case SwordStatus.OPENED:
                    ChangeStatus(SwordStatus.FOLLOWING);
                    break;
                //------------------------------------------------
                case SwordStatus.ATTACKING:
                    swordRecalled = false;
                    if (attackTarget != null && VRInputSelectTarget.Instance.AvatarTarget != null)
                    {
                        //当前飞行方向
                        Vector3 currentDir = (attackTargetPoint.position - sword.position).normalized;
                        //结束点偏移
                        Vector3 endPointOffset = currentDir * 5f;
                        //新的坐标
                        //Vector3 newPosition = Vector3.Lerp(sword.position, attackTargetPoint.position + endPointOffset, Time.deltaTime * attackSpeed);
                        Vector3 newPosition = Vector3.MoveTowards(sword.position, attackTargetPoint.position + endPointOffset, Time.deltaTime * 30);
                        //新的朝向
                        Vector3 newDir = (attackTargetPoint.position - newPosition).normalized;

                        if (dodgeFlag)
                        {
                            if (Vector3.Distance(sword.position, attackTargetPoint.position) < 5.0f)
                            {
                                VRInputSelectTarget.Instance.AvatarTarget.eventObj.fire("Event_OnCastFeiJian", new object[] { VRInputManager.Instance.playerComponent });
                                dodgeFlag = false;
                            }
                        }
                        
                        //没到达目标点，用前后方向角度进行判断
                        if (Vector3.Dot(currentDir, newDir) > 0)
                        {
                            swordCollider.enabled = true;
                            sword.position = newPosition;
                            sword.LookAt(attackEndPoint);
                            //攻击结束点
                            attackEndPoint = attackTargetPoint.position + endPointOffset;
                        }
                        else
                        {
                            swordCollider.enabled = false;
                            attackTarget = null;

                            //射线检测地面
                            if (Physics.Raycast(sword.position, attackEndPoint - sword.position, out hitInfo, 10, followRaycastLayer))
                            {
                                Debug.DrawLine(sword.position, hitInfo.point, Color.blue);
                                attackEndPoint = hitInfo.point;
                            }
                            ChangeStatus(SwordStatus.POST_ATTACK);
                        }
                    }
                    else
                    {
                        swordCollider.enabled = false;
                        ChangeStatus(SwordStatus.POST_ATTACK);
                    }
                    break;
                //------------------------------------------------
                case SwordStatus.POST_ATTACK:
                    if (Vector3.Distance(sword.position, attackEndPoint) > 0.1f)
                    {
                        sword.position = Vector3.MoveTowards(sword.position, attackEndPoint, Time.deltaTime * 30);
                        sword.LookAt(attackEndPoint);
                    }
                    else
                    {
                        attackEndDirection *= -1;
                        ChangeStatus(SwordStatus.END_ATTACK);
                        //VRInputManager.Instance.playerComponent.effectManager.AddEffect("VR_Sword_Hit_Ground", attackEndPoint);
                    }
                    break;
                //------------------------------------------------
                case SwordStatus.END_ATTACK:
                    Vector3 endPoint = -handTipForward.forward * 2.0f + handTipForward.position;
                    sword.LookAt(endPoint);

                    float angle = Mathf.Min(1, Vector3.Distance(sword.position, endPoint) / 15) * 60;
                    sword.rotation = sword.rotation * Quaternion.Euler(Mathf.Clamp(-angle, -30, 30), Mathf.Clamp(attackEndDirection * angle, -42, 42), 0);

                    float currentDist = Vector3.Distance(sword.position, endPoint);
                    sword.Translate(Vector3.forward * Mathf.Min(30.0f * Time.deltaTime, currentDist));

                    if (Vector3.Distance(sword.position, endPoint) < 0.5f)
                    {
                        ChangeStatus(SwordStatus.FOLLOWING);
                    }
                    break;
                //------------------------------------------------
                case SwordStatus.FOLLOWING:
                    Vector3 originPoint = sword.position;
                    Vector3 targetPoint = handTipForward.forward * 5f + handTipForward.position;
                    //射线检测地面
                    if (Physics.Raycast(handTipForward.position, handTipForward.forward, out hitInfo, 5, followRaycastLayer))
                    {
                        //Debug.DrawLine(handTipForward.position, hitInfo.point, Color.blue);
                        targetPoint = hitInfo.point + Vector3.up * 0.1f;
                    }

                    sword.position = Vector3.Lerp(originPoint, targetPoint, Time.deltaTime * followSpeed);

                    //小位移忽略转向
                    if (Vector3.Distance(originPoint, targetPoint) > 0.1f)
                    {
                        sword.LookAt(targetPoint);
                    }

                    if (VRInputSelectTarget.Instance.AvatarTarget != null)
                    {
                        if (VRInputManager.Instance.GetControllerEvents(controllerHand).GetVelocity().magnitude > 3.5f)
                        {
                            attackTarget = VRInputSelectTarget.Instance.AvatarTarget.transform;
                            attackTargetPoint = attackTarget.transform.FindChild("Root/hit001");
                            if (attackTargetPoint == null) attackTargetPoint = attackTarget;
                            swordRecalled = false;
                            ChangeStatus(SwordStatus.ATTACKING);
                            dodgeFlag = true;
                        }
                    }

                    //射线检测攻击目标
                    //if (Physics.Raycast(handTipForward.position, handTipForward.forward, out hitInfo, 30, attackRaycastLayer))
                    //{
                    //    Debug.DrawLine(handTipForward.position, hitInfo.point, Color.blue);
                    //    if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Entity"))
                    //    {
                    //        AvatarComponent component = hitInfo.transform.gameObject.GetComponent<AvatarComponent>();
                    //        if (component != null)
                    //        {
                    //            if (component != avatarTarget)
                    //                return;

                    //            //在视野角度内可攻击
                    //            if (angle(head.forward, hitInfo.point - head.position) < 30f)
                    //            {
                    //                attackTarget = hitInfo.transform;
                    //                attackTargetPoint = attackTarget.FindChild("Root/Bip001");
                    //                if (attackTargetPoint == null) attackTargetPoint = attackTarget;
                    //                swordRecalled = false;
                    //                ChangeStatus(SwordStatus.ATTACKING);
                    //                //component.eventObj.fire("Event_OnCastFeiJian", new object[] { VRInputManager.Instance.playerComponent });
                    //            }
                    //        }
                    //    }
                    //    else if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("broken"))
                    //    {
                    //        if (angle(head.forward, hitInfo.point - head.position) < 30f)
                    //        {
                    //            attackTarget = hitInfo.transform;
                    //            attackTargetPoint = attackTarget.FindChild("Root/Bip001");
                    //            if (attackTargetPoint == null) attackTargetPoint = attackTarget;
                    //            swordRecalled = false;
                    //            ChangeStatus(SwordStatus.ATTACKING);
                    //        }
                    //    }
                    //}
                    break;
            }
        }
    }

    private void ChangeStatus(SwordStatus status = SwordStatus.NULL)
    {
        if (status != SwordStatus.NULL)
            nextStatus = status;
        if (nextStatus != SwordStatus.NULL)
            currentStatus = nextStatus;
        //Debug.Log(currentStatus);
        nextStatus = SwordStatus.NULL;
    }

    private float angle(Vector3 from_, Vector3 to_)
    {
        return Vector3.Angle(from_, to_);
    }

    private float time = 0;
    private float maxTime = 0.5f;
    private bool isStay = false;

    public void SwordOriginStay()
    {
        //if (!isStay)
        //{
        //    if (time < maxTime)
        //        time += Time.deltaTime;
        //    else
        //    {
        //        time = 0f;
        //        //启动
        //        if (currentStatus == SwordStatus.CLOSED)
        //        {
        //            Debug.Log("启动飞剑");
        //            sword.gameObject.SetActive(true);
        //            VRInputManager.Instance.Shake(controllerHand, 1500, 0.1f, 0.01f);
        //            VRInputManager.Instance.handRightAnimator.SetBool("at", true);
        //            sword.position = hand.position + hand.forward;
        //            ChangeStatus(SwordStatus.OPENED);

        //            GlobalEvent.fire("GuideEvent",GuideEvent.StartFlySword);
        //        }
        //        //关闭
        //        else
        //        {
        //            Debug.Log("关闭飞剑");
        //            VRInputManager.Instance.Shake(controllerHand, 1500, 0.1f, 0.01f);
        //            VRInputManager.Instance.handRightAnimator.SetBool("at", false);
        //            ChangeStatus(SwordStatus.CLOSING);

        //            GlobalEvent.fire("GuideEvent", GuideEvent.EndFlySword);
        //        }
        //        isStay = true;
        //    }
        //}
    }
    public void SwordOriginExit()
    {
        time = 0;
        isStay = false;
    }

    public void Event_SwordOriginColliderSwitch(bool value)
    {
        if (swordOriginCollider)
            swordOriginCollider.enabled = value;
    }

    public void onSceneLoaded(Scene scene, LoadSceneMode model)
    {
        CreateSwordModel();

        if (swordOriginCollider)
            swordOriginCollider.enabled = true;

        VRInputManager.Instance.handRightAnimator.SetBool("at", false);
        currentStatus = SwordStatus.CLOSED;
    }

    private void CreateSwordModel()
    {
        GameObject go = Instantiate(swordPrefab) as GameObject;
        sword = go.transform;
        swordCollider = sword.GetComponentInChildren<BoxCollider>();
        swordCollider.enabled = false;

        SPELL.FlyingSwordSkill skill = sword.gameObject.AddComponent<SPELL.FlyingSwordSkill>();
        if (VRInputManager.Instance.playerComponent != null)
        {
            skill.Init(VRInputManager.Instance.playerComponent);
            skill.SetEnable(true);
        }

        line = sword.gameObject.AddComponent<LineRenderer>();
        line.SetWidth(0.005f, 0f);

        sword.gameObject.SetActive(false);
    }

    public void OnPressed(VRControllerEventArgs e)
    {
        if (VRInputManager.Instance.playerComponent.HasEffectStatus(eEffectStatus.SpellCasting))
        {
            return;
        }

        if (e.hand == controllerHand)
        {
            if (currentStatus == SwordStatus.HOLD)
                return;

            if (currentStatus == SwordStatus.CLOSED)
            {
                sword.gameObject.SetActive(true);
                VRInputManager.Instance.Shake(controllerHand, 1500, 0.05f, 0.01f);
                VRInputManager.Instance.handRightAnimator.SetBool("at", true);
                sword.position = hand.position + hand.forward;
                ChangeStatus(SwordStatus.OPENED);

                GlobalEvent.fire("GuideEvent",GuideEvent.StartFlySword);
            }
            else
            {
                VRInputManager.Instance.Shake(controllerHand, 1500, 0.05f, 0.01f);
                VRInputManager.Instance.handRightAnimator.SetBool("at", false);
                ChangeStatus(SwordStatus.CLOSING);

                GlobalEvent.fire("GuideEvent",GuideEvent.EndFlySword);
            }
        }
    }

    public void OnReleased(VRControllerEventArgs e)
    {
        if (e.hand == controllerHand)
        {
        }
    }

    public void OnGripPressed(VRControllerEventArgs e)
    {
        if (VRInputManager.Instance.playerComponent.HasEffectStatus(eEffectStatus.SpellCasting))
            return;

        if (e.hand == controllerHand)
        {
            if (holdSword != null && currentStatus == SwordStatus.FOLLOWING)
            {
                holdSword.SetActive(true);
                sword.gameObject.SetActive(false);
                VRInputManager.Instance.handRightAnimator.SetBool("at", false);
                ChangeStatus(SwordStatus.HOLD);
                VRInputManager.Instance.handRightModel.SetActive(false);
                VRInputManager.Instance.Shake(controllerHand, 2500, 0.2f, 0.01f);
                GlobalEvent.fire("GuideEvent",GuideEvent.HoldFlySword);

                VRInputManager.Instance.playerComponent.EffectStatusCounterIncr((int)eEffectStatus.SpellCasting);
            }
        }
    }

    public void OnGripReleased(VRControllerEventArgs e)
    {
        if (e.hand == controllerHand)
        {
            if (holdSword != null && currentStatus == SwordStatus.HOLD)
            {
                holdSword.SetActive(false);

                sword.gameObject.SetActive(true);
                VRInputManager.Instance.handRightModel.SetActive(true);
                VRInputManager.Instance.Shake(controllerHand, 1500, 0.05f, 0.01f);
                VRInputManager.Instance.handRightAnimator.SetBool("at", true);
                sword.position = hand.position + hand.forward;
                ChangeStatus(SwordStatus.OPENED);

                VRInputManager.Instance.playerComponent.EffectStatusCounterDecr((int)eEffectStatus.SpellCasting);
            }
        }
    }

    public void OnPlayerStatusChanged(eEntityStatus newStatus, eEntityStatus oldStatus)
    {
        if (newStatus == eEntityStatus.Death)
        {
            if (currentStatus != SwordStatus.CLOSED)
            {
                holdSword.SetActive(false);
                sword.gameObject.SetActive(false);
                VRInputManager.Instance.handRightModel.SetActive(true);
                VRInputManager.Instance.handRightAnimator.SetBool("at", false);
                ChangeStatus(SwordStatus.CLOSED);
            }

            DeregisterControllerEvents();
        }
        else if (newStatus == eEntityStatus.Idle && oldStatus == eEntityStatus.Death)
        {
            RegisterControllerEvents();
        }
    }

    /// <summary>
    /// 点到线的距离
    /// </summary>
    /// <param name="point"></param>
    /// <param name="linePoint1"></param>
    /// <param name="linePoint2"></param>
    /// <returns></returns>
    //public static float DisPoint2Line(Vector3 point, Vector3 linePoint1, Vector3 linePoint2)
    //{
    //    Vector3 vec1 = point - linePoint1;
    //    Vector3 vec2 = linePoint2 - linePoint1;
    //    Vector3 vecProj = Vector3.Project(vec1, vec2);
    //    float dis = Mathf.Sqrt(Mathf.Pow(Vector3.Magnitude(vec1), 2) - Mathf.Pow(Vector3.Magnitude(vecProj), 2));
    //    return dis;
    //}

    //public List<AvatarComponent> Find(AvatarComponent src, float radius, float angle)
    //{
    //    List<AvatarComponent> objs = AvatarComponent.AvatarInRange(radius, src, Vector3.zero);
    //    List<AvatarComponent> result = new List<AvatarComponent>();

    //    foreach (AvatarComponent obj in objs)
    //    {
    //        Vector3 desDir = obj.gameObject.transform.position - src.gameObject.transform.position;
    //        desDir.y = 0;
    //        desDir.Normalize();

    //        float an = Vector3.Dot(src.gameObject.transform.forward, desDir);

    //        if (an < -1)
    //            an = -1;
    //        if (an > 1)
    //            an = 1;

    //        int angleTemp = (int)(Mathf.Acos(an) / Mathf.PI * 180);
    //        if (angleTemp <= angle / 2.0)
    //        {
    //            result.Add(obj);
    //        }
    //    }
    //    return result;
    //}

    //public void FindAvatarComponent()
    //{
    //    AvatarComponent tempComponent = null;
    //    List<AvatarComponent> objs = Find(VRInputManager.Instance.playerComponent, 30, 60);
    //    foreach (AvatarComponent obj in objs)
    //    {
    //        if (obj.status == eEntityStatus.Death || obj.status == eEntityStatus.Pending)
    //            continue;

    //        if (tempComponent == null)
    //            tempComponent = obj;
    //        else if (DisPoint2Line(obj.transform.position, head.transform.position, head.transform.position + head.transform.forward) <
    //            DisPoint2Line(tempComponent.transform.position, head.transform.position, head.transform.position + head.transform.forward))
    //        {
    //            tempComponent = obj;
    //        }
    //    }

    //    if (tempComponent != avatarTarget)
    //    {
    //        if (tempComponent != null)
    //        {
    //            if (VRInputManager.Instance.head.GetComponent<HighlightingSystem.HighlightingRenderer>() == null)
    //            {
    //                VRInputManager.Instance.head.AddComponent<HighlightingSystem.HighlightingRenderer>();
    //            }
    //            HighlightingSystem.Highlighter lighter = tempComponent.GetComponent<HighlightingSystem.Highlighter>();
    //            if (lighter == null)
    //                lighter = tempComponent.gameObject.AddComponent<HighlightingSystem.Highlighter>();
    //            lighter.ConstantOn();
    //        }
    //        if (avatarTarget != null)
    //        {
    //            HighlightingSystem.Highlighter lighter = avatarTarget.GetComponent<HighlightingSystem.Highlighter>();
    //            if (lighter != null)
    //            {
    //                lighter.ConstantOff();
    //            }
    //        }

    //        avatarTarget = tempComponent;
    //    }
    //}

    //public void OnTouchpadPressed(VRControllerEventArgs e)
    //{
    //    if (e.hand != controllerHand || skillID != -1)
    //        return;

    //    if (currentStatus == SwordStatus.HOLD)
    //        return;

    //    float angle = e.touchpadAngle;
    //    if (angle < 45 || angle >= 315)
    //    {
    //        //Debug.LogError("Up");
    //        if (VRInputManager.Instance.playerComponent.CheckMagicSparCount(3))
    //        {
    //            if (skillEffectComponent == null)
    //            {
    //                handActionParm = "launch";
    //                VRInputManager.Instance.handRightAnimator.SetBool(handActionParm, true);
    //                skillEffectComponent = VRInputManager.Instance.playerComponent.effectManager.AddEffect("penhuo", VRInputManager.Instance.handRight.transform);
    //                SPELL.FireJetSkill fj = skillEffectComponent.gameObject.AddComponent<SPELL.FireJetSkill>();
    //                fj.Init(VRInputManager.Instance.playerComponent);
    //                StartCoroutine(SkillTimer(1,5));

    //                VRInputManager.Instance.playerComponent.MagicSparCountChange(-3);
    //            }
    //        }
    //    }
    //    else if (45 <= angle && angle < 135)
    //    {
    //        //Debug.LogError("Right");
    //        if (VRInputManager.Instance.playerComponent.CheckMagicSparCount(3))
    //        {
    //            if (avatarTarget != null)
    //            {
    //                Vector3 pos = avatarTarget.transform.position;
    //                skillEffectComponent = VRInputManager.Instance.playerComponent.effectManager.AddEffect("Meteor", pos);
    //                StartCoroutine(SkillTimer(3, 1));

    //                VRInputManager.Instance.playerComponent.MagicSparCountChange(-3);
    //            }
    //        }
    //    }
    //    else if (135 <= angle && angle < 225)
    //    {
    //        //Debug.LogError("Down");
    //        if (VRInputManager.Instance.playerComponent.CheckMagicSparCount(3))
    //        {
    //            handActionParm = "launch";
    //            VRInputManager.Instance.handRightAnimator.SetBool(handActionParm, true);
    //            skillEffectComponent = VRInputManager.Instance.playerComponent.effectManager.AddEffect("frostJet", VRInputManager.Instance.handRight.transform);
    //            SPELL.FrostJetSkill fj = skillEffectComponent.gameObject.AddComponent<SPELL.FrostJetSkill>();
    //            fj.Init(VRInputManager.Instance.playerComponent);
    //            StartCoroutine(SkillTimer(2,3));

    //            VRInputManager.Instance.playerComponent.MagicSparCountChange(-3);
    //        }
    //    }
    //    else
    //    {
    //        //Debug.LogError("Left");
    //        if (VRInputManager.Instance.playerComponent.CheckMagicSparCount(10))
    //        {
    //            if (_effectObj == null)
    //            {
    //                handActionParm = "at";
    //                VRInputManager.Instance.handRightAnimator.SetBool(handActionParm, true);

    //                _effectObj = Instantiate(standbyObj) as GameObject;
    //                _effectObj.transform.position = tip_nib.position;
    //                Vector3 forward = VRInputManager.Instance.handRight.transform.forward;
    //                forward.y = 0;
    //                _effectObj.transform.forward = forward;
    //                paintPath = _effectObj.GetComponentInChildren<PaintPath>();
    //                paintPath.onPaintCompleted += () => OnPaintCompleted();

    //                audioSource = AudioManager.Instance.SoundPlay("能量球-蓄力", 1, 0, true, _effectObj);
    //                StartCoroutine(SkillTimer(4,5));

    //                VRInputManager.Instance.playerComponent.MagicSparCountChange(-10);
    //            }
    //        }
    //    }
    //}

    //private IEnumerator SkillTimer(int id, float stayTime = 0)
    //{
    //    GlobalEvent.fire("GuideEvent", GuideEvent.CastSkill);

    //    skillID = id;
    //    if (currentStatus != SwordStatus.CLOSED && currentStatus != SwordStatus.CLOSING)
    //    {
    //        ChangeStatus(SwordStatus.IDLE);
    //        sword.gameObject.SetActive(false);
    //        if (skillID != 4)
    //            VRInputManager.Instance.handRightAnimator.SetBool("at", false);
    //    }

    //    if (stayTime > 0)
    //        yield return new WaitForSeconds(stayTime);

    //    SkillEnd();
    //}

    ///// <summary>
    ///// 玩家不同技能可能需要做不同的结束处理
    ///// </summary>
    //private void SkillEnd()
    //{
    //    if (handActionParm != "")
    //    {
    //        VRInputManager.Instance.handRightAnimator.SetBool(handActionParm, false);
    //        handActionParm = "";
    //    }

    //    if (_effectObj != null)
    //        Destroy(_effectObj);

    //    if (skillID == 1 || skillID == 2)
    //    {
    //        if (skillEffectComponent != null)
    //            VRInputManager.Instance.playerComponent.effectManager.RemoveEffect(skillEffectComponent);
    //    }
    //    else if(skillID == 4)
    //    {
    //        if(skillEffectComponent != null)
    //            VRInputManager.Instance.playerComponent.effectManager.RemoveEffect(skillEffectComponent);
    //    }

    //    skillEffectComponent = null;
    //    skillID = -1;

    //    if (currentStatus == SwordStatus.IDLE)
    //    {
    //        ChangeStatus(SwordStatus.FOLLOWING);
    //        sword.gameObject.SetActive(true);
    //        VRInputManager.Instance.handRightAnimator.SetBool("at", true);
    //    }
    //}

    /// <summary>
    /// 喷射火焰和冰雾特效的延迟销毁
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    //private IEnumerator SkillComponentDelayRemove(float delay)
    //{
    //    if (skillEffectComponent != null)
    //    {
    //        foreach (Transform child in skillEffectComponent.gameObject.transform)
    //        {
    //            ParticleSystem ps = child.GetComponent<ParticleSystem>();
    //            if (ps != null)
    //            {
    //                var emission = ps.emission;
    //                emission.rateOverTime = 0;
    //            }
    //        }
    //    }
    //    yield return new WaitForSeconds(delay);
    //    VRInputManager.Instance.playerComponent.effectManager.RemoveEffect(skillEffectComponent);
    //}

    ///// <summary>
    ///// 次元门技能绘制结束回调
    ///// </summary>
    //private void OnPaintCompleted()
    //{
    //    if (_effectObj != null)
    //        Destroy(_effectObj);
    //    if (audioSource != null)
    //        Destroy(audioSource.gameObject);
    //    SPELL.SpellTargetData data = new SPELL.SpellTargetData();
    //    data.pos = paintPath.mirrorTransform.position;
    //    VRInputManager.Instance.playerComponent.CastSpell(1000003, data);
    //    SkillEnd();
    //}

}