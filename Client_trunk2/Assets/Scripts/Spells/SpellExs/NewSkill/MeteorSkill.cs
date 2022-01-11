using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPELL
{
    public class MeteorSkill : MonoBehaviour
    {
        private AvatarComponent _caster;       //技能施法者
        public int[] relation;                 //碰撞关系
        public SpellEffect[] triggerEffects;   //触发效果

        private bool isEnable = false;            //是否生效

        // Use this for initialization
        void Start()
        {
            _caster = VRInputManager.Instance.playerComponent;

            //初始化敌对关系
            relation = new int[] { (int)eTargetRelationship.HostileMonster };

            //初始化效果
            var _triggerEffects = new List<SpellEffect>();
            _triggerEffects.Add(SpellLoader.instance.GetEffect(1000001));
            _triggerEffects.Add(SpellLoader.instance.GetEffect(3005001));
            triggerEffects = _triggerEffects.ToArray();

            isEnable = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isEnable)
                return;

            AvatarComponent dst = other.gameObject.GetComponent<AvatarComponent>();
            if (dst != null)
            {
                for (int i = 0; i < relation.Length; i++)
                {
                    if (_caster.CheckRelationship(dst) == (eTargetRelationship)relation[i] && dst.status != eEntityStatus.Death)
                    {
                        foreach (SpellEffect effect in triggerEffects)
                        {
                            effect.Cast(_caster, dst, null, null);
                        }
                        break;
                    }
                }
            }
        }

    }
}
