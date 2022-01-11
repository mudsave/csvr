using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SPELL
{
    /// <summary>
    /// 火焰喷射地面效果
    /// </summary>
    public class FireJetOnGround : MonoBehaviour
    {

        private AvatarComponent _caster;          //技能施法者

        public int[] relation;                 //碰撞关系
        public SpellEffect[] triggerEffects;   //触发效果

        private float intervalTime = 2f;
        private float accumulateTime = 0.0f;

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
            _triggerEffects.Add(SpellLoader.instance.GetEffect(1000000));
           // _triggerEffects.Add(SpellLoader.instance.GetEffect(1001001));
            triggerEffects = _triggerEffects.ToArray();
        }

        private void OnTriggerStay(Collider other)
        {
            accumulateTime += Time.deltaTime;

            if (accumulateTime < intervalTime)
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
                        accumulateTime = 0.0f;
                        break;
                    }
                }
            }
        }
    }
}
