using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SPELL
{
    public class BlackHoleSkill : MonoBehaviour
    {

        private AvatarComponent _caster;
        public int[] relation;
        public SpellEffect[] triggerEffects;    //触发效果
        private List<AvatarComponent> objList;  //打击过的目标
        private float intervalTime;
        private float accumulateTime = 0.0f;

        private float totalTime = 0.0f;      //持续总时间

        private const float maxTime = 8.0f;
        private const float AbsorbTime = 3f; //目标最大被吸收时间

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
            //_triggerEffects.Add(SpellLoader.instance.GetEffect(3002001));
            triggerEffects = _triggerEffects.ToArray();

            objList = new List<AvatarComponent>();
            intervalTime = 0.5f;

        }

        void Start()
        {

        }

        void Update()
        {
            if (totalTime > maxTime)
                return;

            accumulateTime += Time.deltaTime;
            totalTime += Time.deltaTime;
            if (accumulateTime >= intervalTime)
            {
                accumulateTime = 0.0f;
                List<AvatarComponent> objs = AvatarComponent.AvatarInRange(5.0f, caster, gameObject.transform.position);
                foreach (AvatarComponent dst in objs)
                {
                    bool flag = true;
                    foreach (var obj in objList)
                    {
                        if (dst.id == obj.id)
                        {
                            flag = false;
                            continue;
                        }   
                    }

                    for (int i = 0; i < relation.Length; i++)
                    {
                        if (flag && caster.CheckRelationship(dst) == (eTargetRelationship)relation[i] && dst.status != eEntityStatus.Death)
                        {
                            objList.Add(dst);
                            foreach (SpellEffect effect in triggerEffects)
                            {
                                effect.Cast(caster, dst, null, null);
                            }
                            float time = AbsorbTime;
                            if (maxTime - totalTime < AbsorbTime)
                                time = maxTime - totalTime;
                            StartCoroutine(_action(dst,time));
                            continue;
                        }
                    }

                }
            }
        }

        public IEnumerator _action(AvatarComponent component, float time)
        {
            component.animator.SetBool("struggle",true);
            component.EffectStatusCounterIncr((int)eEffectStatus.HitBy);

            float speed_y = 0f; //Y方向的速度
            float speed_xz = 2; //XZ平面初速度
            float a_xz = 0;     //加速度
            float scale = 0.0f;

            int rotate_x = Random.Range(-60, 60);  //X轴旋转量
            int rotate_y = Random.Range(-60, 60);  //Y轴旋转量
            int rotate_z = Random.Range(-60, 60);  //Z轴旋转量

            Vector3 startPoint = component.transform.position;  //起始点
            Vector3 endPoint = gameObject.transform.position;   //目标点

            Vector3 startScale = component.transform.localScale;
            Vector3 endScale = component.transform.localScale * scale;

            speed_y = (endPoint.y - component.transform.position.y) / time;

            float len = Vector2.Distance(new Vector2(gameObject.transform.position.x, gameObject.transform.position.z), new Vector2(component.transform.position.x, component.transform.position.z));
            a_xz = 2  * (len - speed_xz * time) / (time * time) ;

            Vector2 dir = new Vector2(gameObject.transform.position.x, gameObject.transform.position.z) - new Vector2(component.transform.position.x, component.transform.position.z);

            float lenStart = Vector3.Distance(component.transform.position, endPoint);

            while (Vector3.Distance(component.transform.position, endPoint) > 0.5f)
            {
                float y = component.transform.position.y + speed_y * Time.deltaTime;
                Vector2 xz = new Vector2(component.transform.position.x, component.transform.position.z) + dir.normalized * speed_xz * Time.deltaTime;

                speed_xz += a_xz * Time.deltaTime;

                component.transform.position = new Vector3(xz.x, y, xz.y);
                component.transform.Rotate(new Vector3(Time.deltaTime * rotate_x, Time.deltaTime * rotate_y, Time.deltaTime * rotate_z));
                //component.transform.up = Vector3.Lerp(component.transform.up, (endPoint - component.transform.position)* 0.5f, Time.deltaTime);

                component.transform.localScale = (scale + (Vector3.Distance(component.transform.position, endPoint) - 0.5f) / (lenStart - 0.5f)) * startScale;
                yield return new WaitForEndOfFrame();
            }
            component.receiveDamage(caster, 100000, CDeadType.Normal);
            yield break;
        }
    }
}