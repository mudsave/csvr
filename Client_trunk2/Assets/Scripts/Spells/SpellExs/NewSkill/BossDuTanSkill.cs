using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPELL
{
    public class BossDuTanSkill : MonoBehaviour
    {
        
        public int[] relation;                    //碰撞关系
        public SpellEffect[] triggerEffects;      //触发效果

        public float intervalTime = 1.0f;

        private bool isEnable = false;
        private AvatarComponent _caster;          //技能施法者

        public AvatarComponent caster
        {
            get { return _caster; }
        }

        public void Init(AvatarComponent component)
        {
            _caster = component;

            //初始化敌对关系
            relation = new int[] { (int)eTargetRelationship.HostilePlayers };

            //初始化效果
            var _triggerEffects = new List<SpellEffect>();
            _triggerEffects.Add(SpellLoader.instance.GetEffect(1000001));
            triggerEffects = _triggerEffects.ToArray();

            isEnable = true;
        }

        private void OnTriggerStay(Collider other)
        {
            if (!isEnable)
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
                        isEnable = false;
                    }
                }
            }
        }

        void Update()
        {
            intervalTime -= Time.deltaTime;
            if (intervalTime <= 0)
            {
                isEnable = true;
                intervalTime = 1.0f;
            }
        }

    }
}
