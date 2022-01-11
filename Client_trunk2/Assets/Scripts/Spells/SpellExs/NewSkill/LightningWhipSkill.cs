using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPELL
{
    /// <summary>
    /// 鞭子技能
    /// </summary>
    public class LightningWhipSkill : MonoBehaviour
    {
        private AvatarComponent caster;          //技能施法者

        public int[] relation;                 //碰撞关系
        public SpellEffect[] triggerEffects;   //触发效果

        private float waveSpeed = 0.0f;
        private Vector3 lastPositon;

        void Start()
        {
            caster = VRInputManager.Instance.playerComponent;

            relation = new int[] { (int)eTargetRelationship.HostileMonster };
            var _triggerEffects = new List<SpellEffect>();
            _triggerEffects.Add(SpellLoader.instance.GetEffect(3001002));
            //_triggerEffects.Add(SpellLoader.instance.GetEffect(1001001));
            triggerEffects = _triggerEffects.ToArray();

            lastPositon = gameObject.transform.position;

            GlobalEvent.register("Event_OnTriggerEnter", this, "Event_OnTriggerEnter");
        }

        public void Event_OnTriggerEnter(GameObject obj)
        {
            if (waveSpeed < 5.0f)
                return;

                AvatarComponent dst = obj.gameObject.GetComponent<AvatarComponent>();
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
                        break;
                    }
                }
            }

            //Debug.LogError("LightningWhipSkill::Event_OnTriggerEnter,The GameObject name:" + obj.name);
        }

        void Update()
        {
            waveSpeed = Vector3.Distance(gameObject.transform.position, lastPositon) / Time.deltaTime;
            lastPositon = gameObject.transform.position;
        }

    }
}
