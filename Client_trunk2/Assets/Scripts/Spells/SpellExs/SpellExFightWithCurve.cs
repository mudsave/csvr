using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace SPELL
{
    [System.Serializable]
    public struct SpellTrigger
    {
        public float curve;
        public float intervalTime;
        public int[] relation;
        public ConditionBase[] targetConditions;
        public FinderBase finder;
        public SpellEffect[] casterEffects;  // 触发时给施法者放的技能效果
        public SpellEffect[] spellEffects;   // 触发时给符合条件的受术者放的技能效果
    }


    /// <summary>
    /// 使用Animation Curve做为技能效果触发方式的类
    /// 每一次触发都使用独立的目标搜索器
    /// </summary>
    [AddComponentMenu("Spell/SpellExFightWithCurve")]
    public class SpellExFightWithCurve : SpellExCurveBase
    {
        public SpellTrigger[] triggers;

        public override void Init(DataSection.DataSection dataSection)
        {
            base.Init(dataSection);
            var SPELLLOADER = SpellLoader.instance;

            var _triggers = new List<SpellTrigger>();
            foreach (var section in dataSection["combatFunction"]["curveTriggers"].values())
            {
                var trigger = new SpellTrigger();
                trigger.curve = section.readFloat("curve");
                trigger.intervalTime = section.readFloat("intervalTime");
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
            triggers = _triggers.ToArray();
        }

        protected override int GetCurveLength()
        {
            return triggers.Length;
        }

        protected override float CurveIndex2Value(int index)
        {
            if (index >= triggers.Length)
                return -1.0f;
            return triggers[index].curve;
        }

        protected virtual float intervalTimeTotal()
        {
            float totalTime = 0.0f;
            for (int i = 0; i < triggers.Length; i++)
            {
                totalTime += triggers[i].intervalTime;
            }
            return totalTime;
        }

        public override void OnHit(AvatarComponent caster, SpellTargetData targetData, int curveIndex)
        {
            var trigger = triggers[curveIndex];

            //作用给施法者自己的效果
            foreach (SpellEffect effect in trigger.casterEffects)
            {
                effect.Cast(caster, caster, this, targetData);
            }

            if (trigger.finder == null)
                return;

            //根据搜索器找到指定范围内的对象
            List<AvatarComponent> objs = trigger.finder.Find(caster);

            //根据目标条件对目标进行过滤
            for (int i = objs.Count - 1; i >= 0; i--)
            {
                foreach (var cond in trigger.targetConditions)
                {
                    if (cond.Verify(caster, objs[i]) != SpellStatus.OK)
                        objs.Remove(objs[i]);
                }
            }

            //对满足条件的目标投递技能效果
            foreach (AvatarComponent obj in objs)
            {
                foreach (SpellEffect effect in trigger.spellEffects)
                {
                    effect.Cast(caster, obj, this, targetData);
                }
            }
        }
    }
}
