using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPELL
{
    public class SkillOnGroundCollider : MonoBehaviour
    {
        public SkillOnGroundManager colliderManager = null;
        public AvatarComponent player;
        public eTargetRelationship[] relation;

        private List<int> objs = new List<int>();
        
        private void OnTriggerEnter(Collider collider)
        {
            AvatarComponent obj = collider.gameObject.GetComponent<AvatarComponent>();

            if (obj != null)
            {
                for (int i = 0; i < relation.Length; i++)
                {
                    if (player.CheckRelationship(obj) == relation[i] && obj.status != eEntityStatus.Death)
                    {
                        objs.Add(obj.id);
                        if (colliderManager != null)
                        {
                            colliderManager.AddTarget(obj.id);
                        }
                        break;
                    }
                }
            }
        }

        private void OnTriggerExit(Collider collider)
        {
            AvatarComponent obj = collider.gameObject.GetComponent<AvatarComponent>();

            if (obj != null)
            {
                objs.Remove(obj.id);
                if (colliderManager != null)
                {
                    colliderManager.RemoveTarget(obj.id);
                }
            }
        }

        void OnDestroy()
        {
            if (colliderManager != null)
            {
                colliderManager.OnColliderDestroy(objs);
            }
        }
    }

}

