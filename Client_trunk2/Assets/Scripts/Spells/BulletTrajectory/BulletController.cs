using UnityEngine;
using System.Collections;
using System;


namespace SPELL
{
    /// <summary>
    /// 子弹控制类
    /// </summary>
    public class BulletController : MonoBehaviour
    {
        public AvatarComponent owner;
        public Alias.BuffDataType buffData;
        public int[] relation;
        public SpellEffect[] collisionEffect;
        public CollisionTrigger[] collisionTriggers;

        public float moveSpeed = 8.0f;
        public bool isStart = false;
        public Vector3 dir = Vector3.zero;
        private Vector3 startPos;

        void OnTriggerEnter(Collider other)
        {
            //临时写法
            if (other.gameObject.layer == (int)eLayers.Broken)
            {
                FracturedObject fracturedObject = other.gameObject.GetComponent<FracturedObject>();
                if (fracturedObject)
                    fracturedObject.CheckDetachNonSupportedChunks(true);

                return;
            }

            //临时写法
            if (other.gameObject.layer == (int)eLayers.Diban)
            {

                OnHit(other);
                return;
            }

            AvatarComponent dst = other.gameObject.GetComponent<AvatarComponent>();

            if (dst != null)
            {
                for (int i = 0; i < relation.Length; i++)
                {
                    if (owner.CheckRelationship(dst) == (eTargetRelationship)relation[i] && dst.status != eEntityStatus.Death)
                    {
                        foreach (SpellEffect effect in collisionEffect)
                        {
                            effect.Cast(owner, dst, null, null);
                        }
                        OnHit(other);
                        break;
                    }
                }
            }

        }

        void OnHit(Collider other)
        {
            //临时写法
            Vector3 closestPoint = other.ClosestPointOnBounds(gameObject.transform.position);
            owner.effectManager.AddEffect("baozha", closestPoint);
            AudioManager.Instance.SoundPlay("火球-炸裂");
            BuffBullet buff = (BuffBullet)buffData.GetBuff();
            buff.Detach(owner, buffData);
        }

        void Start()
        {
            //记录开始位置
            startPos = gameObject.transform.position;
        }

        void Update()
        {
            if (isStart)
            {
                //飞行到了最大距离
                if (Vector3.Distance(startPos, gameObject.transform.position) > 6.0f)
                {
                    owner.effectManager.AddEffect("baozha", gameObject.transform.position);
                    AudioManager.Instance.SoundPlay("火球-炸裂");
                    BuffBullet buff = (BuffBullet)buffData.GetBuff();
                    buff.Detach(owner, buffData);
                    return;
                }
                gameObject.transform.Translate(dir * Time.deltaTime * moveSpeed, Space.World);
            }
        }

    }
}
