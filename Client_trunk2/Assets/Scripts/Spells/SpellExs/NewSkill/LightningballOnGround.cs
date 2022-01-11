using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPELL
{
    /// <summary>
    /// 电球地面效果
    /// </summary>
    public class LightningballOnGround : MonoBehaviour
    {
        private AvatarComponent _caster = null;  //技能施法者

        private int[] relation;                  //碰撞关系
        private SpellEffect[] triggerEffects;    //触发效果

        public void Init(Vector3 position)
        {
            relation = new int[] { (int)eTargetRelationship.HostileMonster };

            //初始化效果
            var _triggerEffects = new List<SpellEffect>();
            _triggerEffects.Add(SpellLoader.instance.GetEffect(3001003));
            _triggerEffects.Add(SpellLoader.instance.GetEffect(1000001));
            triggerEffects = _triggerEffects.ToArray();

            _caster = VRInputManager.Instance.playerComponent;

            StartCoroutine(OnHit(position));
        }

        private IEnumerator OnHit(Vector3 position)
        {
            yield return new WaitForSeconds(1.5f);

            List<AvatarComponent> objs = AvatarComponent.AvatarInRange(8.0f, _caster, position);
            foreach (AvatarComponent dst in objs)
            {
                for (int i = 0; i < relation.Length; i++)
                {
                    if (_caster.CheckRelationship(dst) == (eTargetRelationship)relation[i] && dst.status != eEntityStatus.Death)
                    {
                        foreach (SpellEffect effect in triggerEffects)
                        {
                            effect.Cast(_caster, dst, null, null);
                        }
                    }
                }
            }
        }

    }
}
