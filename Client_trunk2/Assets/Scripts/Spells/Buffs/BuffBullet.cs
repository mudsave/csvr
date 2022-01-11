using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SPELL
{
    public struct CollisionTrigger
    {
        public int[] relation;
        public ConditionBase[] targetConditions;
        public FinderBase finder;
        public SpellEffect[] casterEffects;  // 触发时给施法者放的技能效果
        public SpellEffect[] spellEffects;   // 触发时给符合条件的受术者放的技能效果
    }

    /// <summary>
    /// 直线弹道Buff
    /// </summary>
    public class BuffBullet : SpellBuff
    {
        //public float stayTime = 0.0f;
        public string bulletBindingObjectPath = "";  // 子弹出生绑定点
        public string bulletEffect = "";             // 子弹光效名称
        public bool bulletBindEffect = false;        // 子弹光效是否绑定目标
        public string bulletSound = "";              // 子弹音效名称
        public int[] relation;                       // 碰撞目标关系
        public SpellEffect[] collisionEffect;        // 碰撞效果
        public CollisionTrigger[] collisionTriggers; // 碰撞触发点
        //public int type;                           // 运动轨迹类型

        //public TrackBace track;
        //public DataSection.DataSection trackSection;

        public override void Init(DataSection.DataSection dataSection)
        {
            base.Init( dataSection );

            DataSection.DataSection generalFunction = dataSection["generalFunction"];
            //stayTime = generalFunction.readFloat("stayTime");
            relation = generalFunction.readIntArray("collisionRelation", ',');                   

            DataSection.DataSection combatPerformance = dataSection["combatPerformance"];
            bulletBindingObjectPath = combatPerformance.readString("bulletBindingObjectPath");
            bulletEffect = combatPerformance.readString("bulletEffect");                        
            bulletSound = combatPerformance.readString("bulletSound");
            bulletBindEffect = combatPerformance.readBool("bulletBindEffect");

            var SPELLLOADER = SpellLoader.instance;

            var _collisionEffect = new List<SpellEffect>();
            foreach (var effectID in generalFunction["collisionEffect"].readInts("item"))
            {
                _collisionEffect.Add(SPELLLOADER.GetEffect(effectID));
            }
            collisionEffect = _collisionEffect.ToArray();

            var _triggers = new List<CollisionTrigger>();
            foreach (var section in dataSection["combatFunction"]["collisionTriggers"].values())
            {
                var trigger = new CollisionTrigger();
                trigger.relation = section.readIntArray("relation", ',');

                //将判断目标关系的条件判断默认添加到目标条件判断中
                var _targetConditions = new List<ConditionBase>();
                RelationCondition relationCondition = new RelationCondition();
                relationCondition.Init(section);
                _targetConditions.Add(relationCondition);

                if (section.has_key("targetConditions"))
                {
                    foreach (var cond in section["targetConditions"].values())
                    {
                        var targetCondition = ConditionBase.CreateCondition(cond.asInt);
                        if (targetCondition != null)
                        {
                            targetCondition.Init(cond);
                            _targetConditions.Add(targetCondition);
                        }
                    }
                }
                trigger.targetConditions = _targetConditions.ToArray();

                trigger.finder = FinderBase.CreateFinder(section.readInt("targetFinder"));
                if (trigger.finder != null)
                    trigger.finder.Init(section["targetFinder"]);

                var _casterEffects = new List<SpellEffect>();
                foreach (var _castEffectID in section["casterEffects"].readInts("item"))
                {
                    _casterEffects.Add(SPELLLOADER.GetEffect(_castEffectID));
                }
                trigger.casterEffects = _casterEffects.ToArray();

                var _spellEffects = new List<SpellEffect>();
                foreach (var _spellEffectID in section["spellEffects"].readInts("item"))
                {
                    _spellEffects.Add(SPELLLOADER.GetEffect(_spellEffectID));
                }
                trigger.spellEffects = _spellEffects.ToArray();

                _triggers.Add(trigger);
            }
            collisionTriggers = _triggers.ToArray();

            //trackSection = combatPerformance["trackType"];
            //try
            //{
            //    type = int.Parse(trackSection.value);
            //}
            //catch (System.Exception e) 
            //{
            //    Debug.Log(this + "::Init(),  trackType Exception: \n" + e);
            //    type = 0;
            //}
        }

        protected override void OnAttach(AvatarComponent owner, Alias.BuffDataType buffData)
        {
            //没有配置子弹特效
            if (bulletEffect == "")
            {
                return;
            }

            EffectComponent eComponent = null;
            SpellTargetData targetData = null;
            if (bulletBindingObjectPath != "")
            {
                Transform transf = owner.myTransform;
                transf = owner.myTransform.FindChild(bulletBindingObjectPath);
                if (!transf)
                    transf = owner.myTransform;

                if (bulletBindEffect)
                {
                    eComponent = owner.effectManager.AddEffect(bulletEffect, transf);
                }
                else
                {
                    eComponent = owner.effectManager.AddEffect(bulletEffect, transf.position);
                }
            }
            else
            {
                Vector3 pos = owner.myTransform.position;
                //无论如何都以服务器位置为准
                if (buffData.temp.ContainsKey("SpellTargetData"))
                {
                    targetData = (SpellTargetData)buffData.temp["SpellTargetData"];
                    pos = targetData.pos;
                }
                eComponent = owner.effectManager.AddEffect(bulletEffect, pos);

            }

            if (eComponent.gameObject != null)
            {
                eComponent.gameObject.layer = (int)eLayers.Bullet;

                BulletController bc = eComponent.gameObject.AddComponent<BulletController>();
                bc.owner = owner;
                bc.buffData = buffData;
                bc.relation = relation;
                bc.collisionEffect = collisionEffect;
                bc.collisionTriggers = collisionTriggers;
                
                if (!bulletBindEffect)
                {
                    bc.dir = owner.myTransform.forward;
                    if (targetData != null)
                    {
                        bc.dir = targetData.dir;
                    }
                    bc.isStart = true;
                }

                // 记录光效实例
                buffData.localBuffData["bulletEffect"] = eComponent;
            }

            if (bulletSound != "" && AudioManager.Instance != null)
            {
                buffData.localBuffData["audio"] = AudioManager.Instance.SoundPlay(bulletSound, 1, 0.0f, true);
            }
        }

        /// <summary>
        /// 把buff从owner身上取下来
        /// </summary>
        /// <param name="owner">拥有这个buff的人</param>
        /// <param name="buffData">存储的buff数据</param>
        protected override void OnDetach(AvatarComponent owner, Alias.BuffDataType buffData)
        {
            //光效销毁
            if (buffData.localBuffData.ContainsKey("bulletEffect"))
            {
                owner.effectManager.RemoveEffect((EffectComponent)buffData.localBuffData["bulletEffect"]);
            }

            if (buffData.localBuffData.ContainsKey("audio"))
            {
                UnityEngine.Object.Destroy(((AudioSource)buffData.localBuffData["audio"]).gameObject);
            }
        }
    }
}
