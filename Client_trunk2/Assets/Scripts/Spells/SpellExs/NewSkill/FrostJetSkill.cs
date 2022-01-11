using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SPELL
{
    /// <summary>
    /// 冰雾喷射
    /// </summary>
    public class FrostJetSkill : MonoBehaviour
    {
        private AvatarComponent _caster;          //技能施法者

        public int[] relation;                 //碰撞关系
        public SpellEffect[] triggerEffects;   //触发效果

        private float intervalTime = 0.5f;
        private float accumulateTime = 0.0f;

        EffectComponent groundEffectComponent;
        RaycastHit hit;
        LayerMask maskDiban;
        LayerMask maskIce;

        private List<string> bindPaths = new List<string>()
        {
            "root/hips/leftUpLeg/leftLeg",
            "root/hips/leftUpLeg/cloth1",
            "root/hips/rightUpLeg/cloth2",
            "root/hips/rightUpLeg/rightLeg",
            "root/hips/spine/spine1/spine2/spine3/neck",
            "root/hips/spine/spine1/spine2/spine3/leftShoulder/leftArm",
            "root/hips/spine/spine1/spine2/spine3/rightShoulder/rightArm",
            "root/hips/spine/spine1/spine2/spine3/leftShoulder/leftArm/leftForeArm",
            "root/hips/spine/spine1/spine2/spine3/rightShoulder/rightArm/rightForeArm",
        };

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
            //_triggerEffects.Add(SpellLoader.instance.GetEffect(1000000));
            _triggerEffects.Add(SpellLoader.instance.GetEffect(3004001));
            triggerEffects = _triggerEffects.ToArray();

            maskDiban = 1 << (int)eLayers.Diban;
            maskIce = 1 << (int)eLayers.Ice;
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
                    if (!Physics.Raycast(transform.position, transform.forward, out hit, 4.0f, maskIce))
                    {
                        Vector3 pos = hit.point;
                        groundEffectComponent = caster.effectManager.AddEffect("iceGround", pos);
                        FrostJetOnGround fj = groundEffectComponent.gameObject.AddComponent<FrostJetOnGround>();
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
                        foreach (SpellEffect effect in triggerEffects)
                        {
                            effect.Cast(caster, dst, null, null);
                        }

                        for (int j = 0; j < 3; j++)
                        {
                            Transform transf = dst.transform.FindChild(bindPaths[Random.Range(0, bindPaths.Count)]);
                            dst.effectManager.AddEffect("iceTarget", transf);
                        }
                        accumulateTime = 0.0f;
                        break;
                    }
                }
            }
        }

    }
}
