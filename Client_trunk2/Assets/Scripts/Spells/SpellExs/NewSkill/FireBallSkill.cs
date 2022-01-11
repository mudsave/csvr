using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPELL
{
    /// <summary>
    /// 玩家火球技能
    /// </summary>
    public class FireBallSkill : MonoBehaviour
    {
        private AvatarComponent _caster;          //技能施法者
        private bool isEnable = false;            //是否生效
        private bool isRange = false;             //是否范围伤害

        public int[] relation;                 //碰撞关系
        public SpellEffect[] triggerEffects;   //触发效果

        public float moveSpeed = 6.0f;             //飞行速度
        public Vector3 direction = Vector3.zero;   //飞行方向
        private bool canFly = false;               //是否开始飞行
        private Vector3 startPos;                  //起飞的位置
        private float maxDistance = 8.0f;                 //飞行最大距离

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
            _triggerEffects.Add(SpellLoader.instance.GetEffect(1000001));
            _triggerEffects.Add(SpellLoader.instance.GetEffect(1001001));
            triggerEffects = _triggerEffects.ToArray();
        }

        public void StartFly(Vector3 direction)
        {
            canFly = true;
            this.direction = direction;
            startPos = gameObject.transform.position;
        }

        public void SetEnable(bool value)
        {
            isEnable = value;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isEnable)
                return;

            if (other.gameObject.layer == (int)eLayers.Diban)
            {
                Vector3 closestPoint = other.ClosestPointOnBounds(gameObject.transform.position);
                OnCollision(closestPoint);
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

                        Vector3 closestPoint = other.ClosestPointOnBounds(gameObject.transform.position);
                        OnCollision(closestPoint);
                        break;
                    }
                }
            }
        }

        private void OnCollision(Vector3 position)
        {
            caster.effectManager.AddEffect("baozha", position);
            AudioManager.Instance.SoundPlay("火球-炸裂");

            EffectComponent component = gameObject.GetComponent<EffectComponent>();
            if (component != null)
                caster.effectManager.RemoveEffect(component);
        }

        private void Update()
        {
            if (canFly)
            {
                //飞行到了最大距离
                if (Vector3.Distance(startPos, gameObject.transform.position) > maxDistance)
                {
                    OnCollision(gameObject.transform.position);
                    return;
                }
                gameObject.transform.Translate(direction * Time.deltaTime * moveSpeed, Space.World);
            }
        }
    }
}