using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SPELL
{
    /// <summary>
    /// 火焰喷射
    /// </summary>
    public class FireJetSkill : MonoBehaviour
    {
        private AvatarComponent _caster;          //技能施法者

        public int[] relation;                 //碰撞关系
        public SpellEffect[] triggerEffects;   //触发效果

        private float intervalTime = 0.5f;
        private float accumulateTime = 0.0f;

        EffectComponent groundEffectComponent;
        RaycastHit hit;
        LayerMask maskDiban;
        LayerMask maskFire;

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
            _triggerEffects.Add(SpellLoader.instance.GetEffect(1000006));
            //_triggerEffects.Add(SpellLoader.instance.GetEffect(1001001));
            triggerEffects = _triggerEffects.ToArray();

            maskDiban = 1 << (int)eLayers.Diban;
            maskFire = 1 << (int)eLayers.Fire;
        }

        private void OnTriggerStay(Collider other)
        {
            accumulateTime += Time.deltaTime;

            if (accumulateTime < intervalTime)
                return;

            if (other.gameObject.layer == (int)eLayers.Diban)
            {
                if (Physics.Raycast(transform.position, transform.forward, out hit, 4.0f, maskDiban))
                {
                    if (!Physics.Raycast(transform.position, transform.forward, out hit, 4.0f, maskFire))
                    {
                        Vector3 pos = hit.point;
                        groundEffectComponent = caster.effectManager.AddEffect("penhuodimian", pos);
                        FireJetOnGround fj= groundEffectComponent.gameObject.AddComponent<FireJetOnGround>();
                        fj.Init(_caster);
                    }
                }
                accumulateTime = 0.0f;
                return;
            }


            AvatarComponent dst = other.gameObject.GetComponent<AvatarComponent>();
            if (dst != null)
            {
                for (int i = 0; i < relation.Length; i++)
                {
                    if (caster.CheckRelationship(dst) == (eTargetRelationship)relation[i] && dst.status != eEntityStatus.Death)
                    {
                        Transform transf = dst.transform.FindChild("Root/hit001");
                        dst.effectManager.AddEffect("penhuo_hit", transf);

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
