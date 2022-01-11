using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPELL
{
    public class BulletMotionSkill : MonoBehaviour
    {

        public float Distance = 30;
        public float Speed = 1;
        public LayerMask CollidesWith = ~0;
        public bool destoryOnCollision = true;
        public GameObject[] EffectsOnCollision;
        public GameObject[] EffectsOnCollisionShield;
        public float CollisionOffset = 0;
        public float DestroyTimeDelay = 5;

        Transform t;
        private bool isActivate = false;
        private bool isCollided = false;
        private bool isOutDistance = false;
        private Vector3 startPos;
        private Vector3 oldPos;
        private const float RayCastTolerance = 0.3f;

        private Vector3 dir;
        private bool isRebound = false;

        private int[] relation;                    //碰撞关系
        private SpellEffect[] triggerEffects;      //触发效果
        private AvatarComponent _caster;          //技能施法者

        public void Init(AvatarComponent component)
        {
            _caster = component;

            //初始化敌对关系
            relation = new int[] { (int)eTargetRelationship.HostilePlayers };

            //初始化效果
            var _triggerEffects = new List<SpellEffect>();
            _triggerEffects.Add(SpellLoader.instance.GetEffect(1000001));
            triggerEffects = _triggerEffects.ToArray();

            t = transform;
            startPos = t.transform.position;
            oldPos = startPos;
            isActivate = true;

            dir = t.forward;
        }

        void Start()
        {
            //t = transform;
            //startPos = t.transform.position;
            //oldPos = startPos;

            //isActivate = true;
        }

        void Update()
        {
            if (!isActivate)
                return;

            if (!isOutDistance && !isCollided)
                UpdatePosition();
        }

        void UpdatePosition()
        {
            Vector3 frameMoveOffset = dir * Speed * Time.deltaTime;

            var currentDistance = (t.position + frameMoveOffset - startPos).magnitude;

            RaycastHit hit;
            if (!isCollided && Physics.Raycast(t.position, t.forward, out hit, 10, CollidesWith))
            {
                if (frameMoveOffset.magnitude + RayCastTolerance > hit.distance)
                {
                    OnCollisionBehaviour(hit);
                }
            }

            if (!isOutDistance && currentDistance > Distance)
            {
                isOutDistance = true;
                return;
            }

            t.position = oldPos + frameMoveOffset;
            oldPos = t.position;
        }

        void OnCollisionBehaviour(RaycastHit hit)
        {
            if (hit.collider.gameObject.layer == (int)eLayers.Shield && !isRebound)
            {
                OnCollisionPlayerShield(hit);
                return;
            }

            AvatarComponent dst = hit.collider.gameObject.GetComponent<AvatarComponent>();

            if (dst != null)
            {
                for (int i = 0; i < relation.Length; i++)
                {
                    if (_caster.CheckRelationship(dst) == (eTargetRelationship)relation[i] && dst.status != eEntityStatus.Death)
                    {
                        foreach (SpellEffect effect in triggerEffects)
                        {
                            effect.Cast(_caster, dst, null, null);
                        }

                        foreach (var effect in EffectsOnCollision)
                        {
                            var instance = Instantiate(effect, hit.point + hit.normal * CollisionOffset, new Quaternion()) as GameObject;
                            instance.transform.LookAt(hit.point + hit.normal + hit.normal * CollisionOffset);
                            Destroy(instance, DestroyTimeDelay);
                        }

                        if (destoryOnCollision)
                            Destroy(gameObject);

                        isCollided = true;

                        break;
                    }
                }
            }
        }

        void OnCollisionPlayerShield(RaycastHit hit)
        {
            if (VRInputManager.Instance.GetControllerEvents(Hand.LEFT).GetVelocity().magnitude > 1)
            {
                //玩家护盾反弹伤害
                relation = new int[] { (int)eTargetRelationship.HostileMonster };
                _caster = VRInputManager.Instance.playerComponent;

                dir = -t.forward;
                isRebound = true;
            }
            else
            {
                foreach (var effect in EffectsOnCollisionShield)
                {
                    var instance = Instantiate(effect, hit.point + hit.normal * CollisionOffset, new Quaternion()) as GameObject;
                    instance.transform.LookAt(hit.point + hit.normal + hit.normal * CollisionOffset);
                    Destroy(instance, DestroyTimeDelay);
                }

                isCollided = true;

                if (destoryOnCollision)
                    Destroy(gameObject);
            }
        }

        //void OnDrawGizmosSelected()
        //{
        //    if (Application.isPlaying)
        //        return;

        //    t = transform;
        //    Gizmos.color = Color.green;
        //    Gizmos.DrawLine(t.position, t.position + t.forward * Distance);
        //}
    }
}
