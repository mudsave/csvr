using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SPELL
{
    /// <summary>
    /// 非主动跟随飞剑类型
    /// </summary>
    public class PlayerSkillFlySword : PlayerSkillBase
    {
        /// <summary>
        /// 飞剑状态
        /// </summary>
        public enum FlySwordStatus
        {
            CLOSING,            //关闭中
            CLOSED,             //关闭
            OPENED,             //开启
            ATTACKING,          //攻击中
            POST_ATTACK,        //攻击后
            END_ATTACK,         //攻击结束
            FOLLOWING,          //跟随
            HOLD,               //手持
        }

        [Header("技能表现模块")]
        [Tooltip("飞剑 [ 预制体 ]")]
        public GameObject swordPrefab;

        [Tooltip("手持剑 [ 预制体 ]")]
        public GameObject holdSword;      //手持状态飞剑的预制体

        [Tooltip("飞剑跟随 [ 速度 ]")]
        public float followSpeed = 2;

        [Tooltip("飞剑攻击 [ 速度 ]")]
        public float attackSpeed = 30;

        [Header("技能战斗相关模块")]
        [Tooltip("目标 [ 关系 ]")]
        public eTargetRelationship[] relation;

        [Tooltip("飞剑击中技能 [ 效果ID ]")]
        public int[] flySwordEffectsID;

        [Tooltip("手持剑击中技能 [ 效果ID ]")]
        public int[] holdSwordEffectsID;

        private Transform swordTransform = null;
        private Transform holdSwordTransform = null;
        private Transform handTipForward = null;
        private Transform head = null;
        private BoxCollider swordCollider = null;

        //攻击目标
        private AvatarComponent attackTarget = null;
        private Transform attackTransform = null;

        private SpellEffect[] flySwordEffects;
        private SpellEffect[] holdSwordEffects;

        private bool attackFlag = false;
        private bool dodgeFlag = false;
        private bool followHand = false;
        private Vector3 followPosition;
        private Vector3 lookAtPosition;
        private Vector3 attackEndPosition;
        private float attackEndDirection;

        private FlySwordStatus status = FlySwordStatus.CLOSED;
        private LayerMask attackRaycastLayer = 1 << (int)eLayers.Entity;
        private LayerMask followRaycastLayer = 1 << (int)eLayers.Diban;

        private Animator m_animator = null;

        public override void Init()
        {
            base.Init();

            SceneManager.sceneLoaded += onSceneLoaded;
            if (castHand == Hand.RIGHT)
            {
                handTipForward = castHandTransform.FindChild("RightController/TipForward");
            }
            else
            {
                handTipForward = castHandTransform.FindChild("LeftController/TipForward");
            }
            head = VRInputManager.Instance.head.transform;

            var _flySwordEffects = new List<SpellEffect>();
            foreach (int id in flySwordEffectsID)
            {
                _flySwordEffects.Add(SpellLoader.instance.GetEffect(id));
            }
            flySwordEffects = _flySwordEffects.ToArray();

            var _holdSwordEffects = new List<SpellEffect>();
            foreach (int id in holdSwordEffectsID)
            {
                _holdSwordEffects.Add(SpellLoader.instance.GetEffect(id));
            }
            holdSwordEffects = _holdSwordEffects.ToArray();

            CreateFlySwordModel();
            CreateHoldSwordModel();
            OpenFlySword();
        }

        private void CreateFlySwordModel()
        {
            if (swordTransform != null)
                return;

            //实例化飞剑模型
            GameObject sword = Instantiate(swordPrefab) as GameObject;
            swordTransform = sword.transform;
            swordCollider = swordTransform.GetComponentInChildren<BoxCollider>();
            swordCollider.enabled = false;
            swordTransform.gameObject.SetActive(false);

            m_animator = swordTransform.GetComponent<Animator>();
            ColliderDelegate swordColliderDelegate = sword.gameObject.AddComponent<ColliderDelegate>();
            swordColliderDelegate.TriggerEnterEvent += OnSkillEnter;
            DontDestroyOnLoad(sword);
        }

        private void CreateHoldSwordModel()
        {
            if (holdSwordTransform != null)
                return;

            //实例化手持剑模型
            GameObject holdsword = Instantiate(holdSword) as GameObject;
            holdSwordTransform = holdsword.transform;
            Transform bindTransform = castHandTransform.FindChild("RightController/Weapon/SwordHold");
            holdSwordTransform.SetParent(bindTransform, false);
            holdSwordTransform.gameObject.SetActive(false);

            ColliderDelegate swordColliderDelegate = holdsword.gameObject.AddComponent<ColliderDelegate>();
            swordColliderDelegate.TriggerEnterEvent += OnSkillEnter;
        }

        public void OpenFlySword()
        {
            if (status == FlySwordStatus.CLOSED)
            {
                ChangeStatus(FlySwordStatus.OPENED);
            }
        }

        public void CloseFlySword()
        {
            if (status != FlySwordStatus.CLOSED)
            {
                //如果是握持状态需要做一些特殊处理
                if (status == FlySwordStatus.HOLD)
                {
                    //隐藏剑模型，显示手部模型
                    holdSwordTransform.gameObject.SetActive(false);
                    VRInputManager.Instance.handRightModel.SetActive(true);
                    //清除施法状态
                    VRInputManager.Instance.playerComponent.EffectStatusCounterDecr((int)eEffectStatus.SpellCasting);
                }
                castHandAnimator.SetBool(handAnimation, false);
                swordTransform.gameObject.SetActive(false);
                ChangeStatus(FlySwordStatus.CLOSED);
            }
        }

        /// <summary>
        /// 开启握持状态
        /// </summary>
        public void OpenHoldStatus()
        {
            if (holdSwordTransform == null)
                return;

            if (status == FlySwordStatus.FOLLOWING)
            {
                ChangeStatus(FlySwordStatus.HOLD);
                holdSwordTransform.gameObject.SetActive(true);
                swordTransform.gameObject.SetActive(false);
                //castHandAnimator.SetBool(handAnimation, false);
                VRInputManager.Instance.handRightModel.SetActive(false);
                VRInputManager.Instance.Shake(castHand, 2500, 0.2f, 0.01f);
                GlobalEvent.fire("GuideEvent", GuideEvent.HoldFlySword);
                VRInputManager.Instance.playerComponent.EffectStatusCounterIncr((int)eEffectStatus.SpellCasting);
            }
        }

        /// <summary>
        /// 关闭握持状态
        /// </summary>
        public void CloseHoldStatus()
        {
            if (holdSwordTransform == null)
                return;

            if (status == FlySwordStatus.HOLD)
            {
                ChangeStatus(FlySwordStatus.OPENED);
                holdSwordTransform.gameObject.SetActive(false);
                VRInputManager.Instance.handRightModel.SetActive(true);
                VRInputManager.Instance.Shake(castHand, 1500, 0.05f, 0.01f);
                //castHandAnimator.SetBool(handAnimation, true);
                VRInputManager.Instance.playerComponent.EffectStatusCounterDecr((int)eEffectStatus.SpellCasting);
            }
        }

        public void SetFollowHand(bool value)
        {
            followHand = value;
        }

        /// <summary>
        /// 攻击指令
        /// </summary>
        public void SimulationAttackOlder()
        {
            if (status == FlySwordStatus.FOLLOWING)
            {
                attackFlag = true;
            }
        }

        /// <summary>
        /// 改变飞剑状态
        /// </summary>
        /// <param name="nextStatus"></param>
        private void ChangeStatus(FlySwordStatus nextStatus)
        {
            if (status != nextStatus)
            {
                status = nextStatus;
            }
        }

        /// <summary>
        /// 切换场景完毕回调
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="model"></param>
        public void onSceneLoaded(Scene scene, LoadSceneMode model)
        {
            ResetStatus();
        }

        private void Update()
        {
            if (status == FlySwordStatus.CLOSED)
                return;

            attackTarget = VRInputSelectTarget.Instance.AvatarTarget;

            switch (status)
            {
                //----------------------------------------------------------------------------------------------------------------------------------------------
                case FlySwordStatus.OPENED:
                    if (swordTransform == null)
                    {
                        CreateFlySwordModel();
                    }
                    swordTransform.gameObject.SetActive(true);
                    swordTransform.position = castHandTransform.position + castHandTransform.forward;

                    ChangeStatus(FlySwordStatus.FOLLOWING);
                    break;
                //----------------------------------------------------------------------------------------------------------------------------------------------
                case FlySwordStatus.FOLLOWING:
                    if (followHand)
                    {
                        followPosition = handTipForward.position + handTipForward.forward * 5.0f;

                        RaycastHit hitInfo;
                        if (Physics.Raycast(handTipForward.position, handTipForward.forward, out hitInfo, 5, followRaycastLayer))
                        {
                            followPosition = hitInfo.point + Vector3.up * 0.1f;
                        }

                        lookAtPosition = followPosition;
                    }
                    else
                    {
                        Quaternion right = head.transform.rotation * Quaternion.AngleAxis(25, Vector3.up);
                        followPosition = head.transform.position + (right * Vector3.forward) * 3f;

                        RaycastHit hitInfo;
                        if (Physics.Raycast(head.transform.position, right * Vector3.forward, out hitInfo, 5, followRaycastLayer))
                        {
                            followPosition = hitInfo.point + Vector3.up;
                        }

                        lookAtPosition = followPosition + head.transform.forward;
                    }
                    swordTransform.position = Vector3.Lerp(swordTransform.position, followPosition, Time.deltaTime * followSpeed);

                    //小位移忽略转向
                    if (Vector3.Distance(swordTransform.position, followPosition) > 0.1f)
                    {
                        swordTransform.LookAt(lookAtPosition);
                    }

                    if (attackTarget != null && (attackFlag || VRInputManager.Instance.GetControllerEvents(castHand).GetVelocity().magnitude > 3.5f)
                            && !player.HasEffectStatus(eEffectStatus.SpellCasting))
                    {
                        attackTransform = attackTarget.transform.FindChild("Root/hit001");
                        if (attackTransform == null) attackTransform = attackTarget.transform;
                        ChangeStatus(FlySwordStatus.ATTACKING);
                        if (m_animator)
                        {
                            m_animator.SetBool("attack",true);
                        }
                        dodgeFlag = true;
                    }
                    break;
                //----------------------------------------------------------------------------------------------------------------------------------------------
                case FlySwordStatus.ATTACKING:
                    if (attackTarget != null)
                    {
                        castHandAnimator.SetBool(handAnimation, true);
                        swordCollider.enabled = true;
                        Vector3 attackDir = (attackTransform.position - swordTransform.position).normalized;

                        swordTransform.position = Vector3.MoveTowards(swordTransform.position, attackTransform.position, Time.deltaTime * attackSpeed);
                        swordTransform.LookAt(attackTransform.position);

                        if (dodgeFlag)
                        {
                            if (Vector3.Distance(swordTransform.position, attackTransform.position) < 5.0f)
                            {
                                attackTarget.eventObj.fire("Event_OnCastFeiJian", new object[] { player });
                                dodgeFlag = false;
                            }
                        }

                        if (Vector3.Distance(swordTransform.position, attackTransform.position) < 0.1f)
                        {
                            swordCollider.enabled = false;
                            attackTarget = null;

                            attackEndPosition = swordTransform.position + swordTransform.forward * 5.0f;

                            RaycastHit hitInfo;
                            if (Physics.Raycast(swordTransform.position, attackDir, out hitInfo, 5, followRaycastLayer))
                            {
                                attackEndPosition = hitInfo.point;
                            }
                            ChangeStatus(FlySwordStatus.POST_ATTACK);
                        }
                    }
                    else
                    {
                        swordCollider.enabled = false;

                        attackEndPosition = swordTransform.position + swordTransform.forward * 5.0f;

                        RaycastHit hitInfo;
                        if (Physics.Raycast(swordTransform.position, swordTransform.forward, out hitInfo, 5, followRaycastLayer))
                        {
                            attackEndPosition = hitInfo.point;
                        }
                        ChangeStatus(FlySwordStatus.POST_ATTACK);
                    }
                    break;
                //----------------------------------------------------------------------------------------------------------------------------------------------
                case FlySwordStatus.POST_ATTACK:
                    if (m_animator)
                    {
                        m_animator.SetBool("attack", false);
                    }
                    attackFlag = false;
                    swordTransform.position = Vector3.MoveTowards(swordTransform.position, attackEndPosition, Time.deltaTime * attackSpeed);
                    if (Vector3.Distance(swordTransform.position, attackEndPosition) < 0.1f)
                    {
                        attackEndDirection = Random.Range(-1.0f, 1.0f);
                        ChangeStatus(FlySwordStatus.END_ATTACK);
                    }
                    break;
                //----------------------------------------------------------------------------------------------------------------------------------------------
                case FlySwordStatus.END_ATTACK:
                    Vector3 endPoint = head.position - head.forward * 2.0f;
                    swordTransform.LookAt(endPoint);

                    float angle = Mathf.Min(1, Vector3.Distance(swordTransform.position, endPoint) / 15) * 60;
                    swordTransform.rotation = swordTransform.rotation * Quaternion.Euler(Mathf.Clamp(-angle, -30, 30), Mathf.Clamp(attackEndDirection * angle, -42, 42), 0);

                    float currentDist = Vector3.Distance(swordTransform.position, endPoint);
                    swordTransform.Translate(Vector3.forward * Mathf.Min(30.0f * Time.deltaTime, currentDist));

                    if (Vector3.Distance(swordTransform.position, endPoint) < 0.5f)
                    {
                        castHandAnimator.SetBool(handAnimation, false);
                        ChangeStatus(FlySwordStatus.FOLLOWING);
                    }
                    break;
                //----------------------------------------------------------------------------------------------------------------------------------------------
                case FlySwordStatus.CLOSING:
                    Vector3 targetPoint = handTipForward.position - handTipForward.forward * 2.0f;
                    swordTransform.LookAt(targetPoint);

                    float angle2 = Mathf.Min(1, Vector3.Distance(swordTransform.position, targetPoint) / 15) * 120;
                    swordTransform.rotation = swordTransform.rotation * Quaternion.Euler(Mathf.Clamp(-angle2, -42, 42), 0, 0);

                    float currentDist2 = Vector3.Distance(swordTransform.position, targetPoint);
                    swordTransform.Translate(Vector3.forward * Mathf.Min(30.0f * Time.deltaTime, currentDist2));

                    if (Vector3.Distance(swordTransform.position, targetPoint) < 0.5f)
                    {
                        swordTransform.gameObject.SetActive(false);
                        ChangeStatus(FlySwordStatus.CLOSED);
                    }
                    break;
            }
        }

        private void OnSkillEnter(Collider other)
        {
            AvatarComponent dst = other.gameObject.GetComponent<AvatarComponent>();
            if (dst != null)
            {
                for (int i = 0; i < relation.Length; i++)
                {
                    if (player.CheckRelationship(dst) == relation[i] && dst.status != eEntityStatus.Death)
                    {
                        if (status == FlySwordStatus.HOLD)
                        {
                            if (VRInputManager.Instance.GetControllerEvents(castHand).GetVelocity().magnitude < 2.0f)
                                return;

                            foreach (SpellEffect effect in holdSwordEffects)
                            {
                                effect.Cast(player, dst, null, null);
                            }

                            Vector3 pos = dst.transform.FindChild("Root/hit001").position;
                            dst.effectManager.AddEffect("feijian_Hand_Hit", pos);
                        }
                        else
                        {
                            foreach (SpellEffect effect in flySwordEffects)
                            {
                                effect.Cast(player, dst, null, null);
                            }

                            Vector3 pos = dst.transform.FindChild("Root/hit001").position;
                            dst.effectManager.AddEffect("feijianHit", pos);
                        }
                        break;
                    }
                }
            }
        }

        //重置飞剑状态
        public void ResetStatus()
        {
            //如果是握持状态需要做一些特殊处理
            if (status == FlySwordStatus.HOLD)
            {
                //隐藏剑模型，显示手部模型
                holdSwordTransform.gameObject.SetActive(false);
                VRInputManager.Instance.handRightModel.SetActive(true);
                //清除施法状态
                VRInputManager.Instance.playerComponent.EffectStatusCounterDecr((int)eEffectStatus.SpellCasting);
            }

            castHandAnimator.SetBool(handAnimation, false);
            //swordTransform.gameObject.SetActive(false);
            ChangeStatus(FlySwordStatus.OPENED);
        }
    }
}

