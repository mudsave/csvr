using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SPELL
{
    public class PlayerSkillFollowFlySword : PlayerSkillBase
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
            IDLE,               //闲置
        }

        [Header("技能表现模块")]
        [Tooltip("飞剑 [ 预制体 ]")]
        public GameObject swordPrefab;

        [Tooltip("手持剑 [ 预制体 ]")]
        public GameObject holdSword;      //手持状态飞剑的预制体

        [Tooltip("飞剑跟随 [ 速度 ]")]
        public float followSpeed = 3;

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
        private Vector3 attackEndPosition;
        private float attackEndDirection;

        private FlySwordStatus status = FlySwordStatus.CLOSED;
        private LayerMask attackRaycastLayer = 1 << (int)eLayers.Entity;
        private LayerMask followRaycastLayer = 1 << (int)eLayers.Diban;

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
        }

        private void CreateFlySwordModel()
        {
            if(swordTransform != null)
                return;

            //实例化飞剑模型
            GameObject sword = Instantiate(swordPrefab) as GameObject;
            swordTransform = sword.transform;
            swordCollider = swordTransform.GetComponentInChildren<BoxCollider>();
            swordCollider.enabled = false;
            swordTransform.gameObject.SetActive(false);

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

        /// <summary>
        /// 飞剑开关
        /// </summary>
        public void FlySwordSwitch()
        {
            if (player.HasEffectStatus(eEffectStatus.SpellCasting))
            {
                return;
            }

            //如果飞剑处于CLOSED状态，则开启飞剑
            if (status == FlySwordStatus.CLOSED)
            {
                //手柄震动
                VRInputManager.Instance.Shake(castHand, 1500, 0.05f, 0.01f);
                castHandAnimator.SetBool(handAnimation, true);
                ChangeStatus(FlySwordStatus.OPENED);
                GlobalEvent.fire("GuideEvent", GuideEvent.StartFlySword);
            }
            //只有飞剑处于FOLLOWING状态下才允许关闭
            else if (status == FlySwordStatus.FOLLOWING)
            {
                //手柄震动
                VRInputManager.Instance.Shake(castHand, 1500, 0.05f, 0.01f);
                castHandAnimator.SetBool(handAnimation, false);
                ChangeStatus(FlySwordStatus.CLOSING);
                GlobalEvent.fire("GuideEvent", GuideEvent.EndFlySword);
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
                castHandAnimator.SetBool(handAnimation, false);
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
                castHandAnimator.SetBool(handAnimation, true);
                VRInputManager.Instance.playerComponent.EffectStatusCounterDecr((int)eEffectStatus.SpellCasting);
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

            if (player.HasEffectStatus(eEffectStatus.SpellCasting))
            {
                if (status != FlySwordStatus.CLOSING && status != FlySwordStatus.HOLD)
                {
                    ChangeStatus(FlySwordStatus.IDLE);
                    swordTransform.gameObject.SetActive(false);
                    castHandAnimator.SetBool(handAnimation, false);
                }
                return;
            }

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
                    Vector3 originPosition = swordTransform.position;
                    Vector3 targetPosition = handTipForward.position + handTipForward.forward * 5.0f;

                    RaycastHit hitInfo;
                    if (Physics.Raycast(handTipForward.position, handTipForward.forward, out hitInfo, 5, followRaycastLayer))
                    {
                        targetPosition = hitInfo.point + Vector3.up * 0.1f;
                    }

                    swordTransform.position = Vector3.Lerp(originPosition, targetPosition, Time.deltaTime * followSpeed);

                    //小位移忽略转向
                    if (Vector3.Distance(originPosition, targetPosition) > 0.1f)
                    {
                        swordTransform.LookAt(targetPosition);
                    }

                    if (attackTarget != null && (attackFlag || VRInputManager.Instance.GetControllerEvents(castHand).GetVelocity().magnitude > 3.5f))
                    {
                        attackTransform = attackTarget.transform.FindChild("Root/hit001");
                        if (attackTransform == null) attackTransform = attackTarget.transform;
                        ChangeStatus(FlySwordStatus.ATTACKING);
                    }
                    break;
                //----------------------------------------------------------------------------------------------------------------------------------------------
                case FlySwordStatus.ATTACKING:
                    if (attackTarget != null)
                    {
                        swordCollider.enabled = true;
                        Vector3 attackDir = (attackTransform.position - swordTransform.position).normalized;

                        swordTransform.position = Vector3.MoveTowards(swordTransform.position, attackTransform.position, Time.deltaTime * attackSpeed);
                        swordTransform.LookAt(attackTransform.position);

                        if (Vector3.Distance(swordTransform.position, attackTransform.position) < 0.1f)
                        {
                            swordCollider.enabled = false;
                            attackTarget = null;

                            attackEndPosition = swordTransform.position + swordTransform.forward * 5.0f;
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
                        if (Physics.Raycast(swordTransform.position, swordTransform.forward, out hitInfo, 5, followRaycastLayer))
                        {
                            attackEndPosition = hitInfo.point;
                        }
                        ChangeStatus(FlySwordStatus.POST_ATTACK);
                    }
                    break;
                //----------------------------------------------------------------------------------------------------------------------------------------------
                case FlySwordStatus.POST_ATTACK:
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
                //----------------------------------------------------------------------------------------------------------------------------------------------
                case FlySwordStatus.IDLE:
                    if (!player.HasEffectStatus(eEffectStatus.SpellCasting))
                    {
                        ChangeStatus(FlySwordStatus.FOLLOWING);
                        swordTransform.gameObject.SetActive(true);
                        castHandAnimator.SetBool(handAnimation, true);
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
            swordTransform.gameObject.SetActive(false);
            ChangeStatus(FlySwordStatus.CLOSED);
        }
    }
}

