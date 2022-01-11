using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  SPELL
{
    public class SkillOnGroundManager : MonoBehaviour
    {
        public Dictionary<int, int> attackTargets = new Dictionary<int, int>();

        private float intervalTime;
        private eTargetRelationship[] relation;
        private SpellEffect[] groundEffects;
        private AvatarComponent player;
        private float accumulateTime;

        public void Init(AvatarComponent player, eTargetRelationship[] relation, SpellEffect[] groundEffects, float intervalTime)
        {

            this.player = player;
            this.relation = relation;
            this.groundEffects = groundEffects;
            this.intervalTime = intervalTime;

            StartCoroutine(DelayDestroy());
        }

        protected IEnumerator DelayDestroy()
        {
            yield return new WaitForSeconds(25);
            Destroy(gameObject);
        }

        public void RegisterCollider(SkillOnGroundCollider collider)
        {
            if (collider.colliderManager == null)
            {
                collider.colliderManager = this;
                collider.relation = relation;
                collider.player = player;
            }
        }

        public void AddTarget(int id)
        {
            if (attackTargets.ContainsKey(id))
            {
                attackTargets[id]++;
            }
            else
            {
                attackTargets.Add(id,1);
            }
        }

        public void RemoveTarget(int id)
        {
            if (attackTargets.ContainsKey(id))
            {
                attackTargets[id]--;
                if (attackTargets[id] == 0)
                {
                    attackTargets.Remove(id);
                }
            }
        }

        public void OnColliderDestroy(List<int> objs)
        {
            foreach (int id in objs)
            {
                RemoveTarget(id);
            }
        }

        void Update()
        {
            accumulateTime += Time.deltaTime;
            if (accumulateTime > intervalTime)
            {
                accumulateTime = 0.0f;
                foreach (int id in attackTargets.Keys)
                {
                    AvatarComponent obj = AvatarComponent.GetAvatar(id);
                    if (obj)
                    {
                        if(obj.status == eEntityStatus.Death)
                            continue;

                        foreach (SpellEffect effect in groundEffects)
                        {
                            effect.Cast(player, obj, null, null);
                        }
                    }
                }
            }
        }
    }
}

