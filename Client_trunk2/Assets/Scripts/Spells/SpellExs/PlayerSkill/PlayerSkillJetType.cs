using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace SPELL
{
    /// <summary>
    /// 玩家喷射类技能
    /// </summary>
    public class PlayerSkillJetType : PlayerSkillBase
    {
        [Header("技能表现模块")]
        [Tooltip("喷射特效 [ 特效名 ]")]
        public string jetEffect = "";

        [Tooltip("喷射击中目标特效 [ 特效名 ]")]
        public string hitEffect = "";

        [Tooltip("射击中地面特效 [ 特效名 ]")]
        public string OnGroundEffect = "";

        [Header("技能战斗相关模块")]
        [Tooltip("目标 [ 关系 ]")]
        public eTargetRelationship[] relation;

        [Tooltip("技能 [ 效果ID ]")]
        public int[] triggerEffectsID;

        [Tooltip("触发效果的 [ 间隔时长 ]")]
        public float intervalTime = 0.5f;

        [Tooltip("地面技能 [ 效果ID ]")]
        public int[] groundEffectsID;

        [Tooltip("触发效果的 [ 间隔时长 ]")]
        public float groundIntervalTime = 0.5f;

        [Tooltip("地面效果的 [ 层级 ]")]
        public LayerMask groundEffectLayerMask;

        RaycastHit hit;
        LayerMask maskDiban;

        private float accumulateTime = 0.0f;
        private SpellEffect[] triggerEffects;
        private SpellEffect[] onGroundEffects;

        EffectComponent eComponent;

        private List<AvatarComponent> attackTargets;

        private GameObject groundManagerObj;
        private SkillOnGroundManager skillOnGroundManager;

        public override void Init()
        {
            base.Init();

            var _triggerEffects = new List<SpellEffect>();
            foreach (int id in triggerEffectsID)
            {
                _triggerEffects.Add(SpellLoader.instance.GetEffect(id));
            }

            triggerEffects = _triggerEffects.ToArray();

            var _onGroundEffects = new List<SpellEffect>();
            foreach (int id in groundEffectsID)
            {
                _onGroundEffects.Add(SpellLoader.instance.GetEffect(id));
            }

            onGroundEffects = _onGroundEffects.ToArray();

            maskDiban = 1 << (int)eLayers.Diban;

            attackTargets = new List<AvatarComponent>();
        }

        protected override void FireStart()
        {
            base.FireStart();

            eComponent = player.effectManager.AddEffect(jetEffect, castHandTransform);
            ColliderDelegate cd = eComponent.gameObject.AddComponent<ColliderDelegate>();
            cd.TriggerStayEvent += OnSkillStay;
            cd.TriggerEnterEvent += OnSkillEnter;
            cd.TriggerExitEvent += OnSkillExit;

            //创建地面效果管理器
            groundManagerObj = new GameObject();
            groundManagerObj.name = "OnGroundManager";
            skillOnGroundManager = groundManagerObj.AddComponent<SkillOnGroundManager>();
            skillOnGroundManager.Init(player, relation, onGroundEffects, groundIntervalTime);
        }

        protected override bool FireUpdate()
        {
            accumulateTime += Time.fixedDeltaTime;
            if (accumulateTime > intervalTime)
            {
                accumulateTime = 0.0f;
                foreach (AvatarComponent target in attackTargets)
                {
                    if (target == null)
                        continue;

                    for (int i = 0; i < relation.Length; i++)
                    {
                        if (player.CheckRelationship(target) == relation[i] && target.status != eEntityStatus.Death)
                        {
                            Transform transf = target.transform.FindChild("Root/hit001");
                            target.effectManager.AddEffect(hitEffect, transf);

                            foreach (SpellEffect effect in triggerEffects)
                            {
                                effect.Cast(player, target, null, null);
                            }
                            break;
                        }
                    }
                }
            }
            return true;
        }

        protected override void FireEnd()
        {
            base.FireEnd();

            if (eComponent != null)
            {
                player.effectManager.RemoveEffect(eComponent);
            }

            accumulateTime = 0.0f;
            //清空目标列表
            attackTargets.Clear();
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
                        attackTargets.Add(dst);
                    }
                }
            }
        }

        private void OnSkillStay(Collider other)
        {
            //地面伤害效果
            if (other.gameObject.layer == (int)eLayers.Diban)
            {
                if (Physics.Raycast(eComponent.transform.position, eComponent.transform.forward, out hit, 4.0f, maskDiban))
                {
                    if (!Physics.Raycast(eComponent.transform.position, eComponent.transform.forward, out hit, 4.0f, groundEffectLayerMask))
                    {
                        Vector3 pos = hit.point;
                        EffectComponent groundEffectComponent = player.effectManager.AddEffect(OnGroundEffect, pos);
                        groundEffectComponent.transform.SetParent(groundManagerObj.transform);
                        SkillOnGroundCollider skillOnGroundCollider = groundEffectComponent.gameObject.AddComponent<SkillOnGroundCollider>();
                        //注册到地面效果管理器
                        skillOnGroundManager.RegisterCollider(skillOnGroundCollider);
                    }
                }
            }
        }

        private void OnSkillExit(Collider other)
        {
            AvatarComponent dst = other.gameObject.GetComponent<AvatarComponent>();
            if (dst != null)
            {
                attackTargets.Remove(dst);
            }
        }
    }
}
