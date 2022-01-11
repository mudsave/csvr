using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPELL
{
    public class HoldSwordSkill : MonoBehaviour
    {
        private AvatarComponent _caster;          //技能施法者
        private bool isEnable = false;            //是否生效

        public int[] relation;                 //碰撞关系
        public SpellEffect[] triggerEffects;   //触发效果

        public AvatarComponent caster
        {
            get { return _caster; }
        }

        public void Init(AvatarComponent component)
        {
            _caster = component;

            //初始化敌对关系
            relation = new int[] { (int)eTargetRelationship.HostileMonster };

            //初始化效果
            var _triggerEffects = new List<SpellEffect>();
            _triggerEffects.Add(SpellLoader.instance.GetEffect(1000007));
            _triggerEffects.Add(SpellLoader.instance.GetEffect(1001001));
            triggerEffects = _triggerEffects.ToArray();
        }

        public void SetEnable(bool value)
        {
            isEnable = value;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isEnable)
                return;

            if (VRInputManager.Instance.GetControllerEvents(Hand.RIGHT).GetVelocity().magnitude < 2.0f)
                return;

            AvatarComponent dst = other.gameObject.GetComponent<AvatarComponent>();
            if (dst != null)
            {
                for (int i = 0; i < relation.Length; i++)
                {
                    if (caster.CheckRelationship(dst) == (eTargetRelationship)relation[i] && dst.status != eEntityStatus.Death)
                    {
                        foreach (SpellEffect effect in triggerEffects)
                        {
                            effect.Cast(caster, dst, null, null);
                        }

                        Vector3 closestPoint = other.ClosestPointOnBounds(gameObject.transform.position);
                        dst.effectManager.AddEffect("feijian_Hand_Hit", closestPoint);
                        break;
                    }
                }
            }
        }
    }
}
