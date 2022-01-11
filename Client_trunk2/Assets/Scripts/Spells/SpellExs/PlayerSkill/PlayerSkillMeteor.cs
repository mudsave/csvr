using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPELL
{
    /// <summary>
    /// 玩家陨石技能
    /// </summary>
    public class PlayerSkillMeteor : PlayerSkillBase
    {
        [Header("技能表现模块")]
        [Tooltip("火焰喷射 [ 特效名 ]")]
        public string meteorEffect = "Meteor";

        [Header("技能战斗相关模块")]
        [Tooltip("目标 [ 关系 ]")]
        public eTargetRelationship[] relation;

        [Tooltip("技能 [ 效果ID ]")]
        public int[] triggerEffectsID;

        EffectComponent eComponent;

        private SpellEffect[] triggerEffects;
        private AvatarComponent target = null;

        public override void Init()
        {
            base.Init();

            var _triggerEffects = new List<SpellEffect>();

            foreach (int id in triggerEffectsID)
            {
                _triggerEffects.Add(SpellLoader.instance.GetEffect(id));
            }

            triggerEffects = _triggerEffects.ToArray();
        }

        public override SpellStatus CanCast()
        {
            //判断有没有攻击目标
            if(VRInputSelectTarget.Instance.AvatarTarget == null)
                return SpellStatus.NO_TARGET;

            return base.CanCast();
        }

        protected override void FireStart()
        {
            base.FireStart();

            target = VRInputSelectTarget.Instance.AvatarTarget;

            eComponent = player.effectManager.AddEffect(meteorEffect, target.transform.position);
            Transform rock = eComponent.transform.FindChild("meteor/Rock/Rock");
            ColliderDelegate cd = rock.gameObject.AddComponent<ColliderDelegate>();
            cd.TriggerEnterEvent += OnSkillEnter;
        }

        protected override void FireEnd()
        {
            base.FireEnd();

            target = null;
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
                        foreach (SpellEffect effect in triggerEffects)
                        {
                            effect.Cast(player, dst, null, null);
                        }
                        break;
                    }
                }
            }
        }
    }
}
