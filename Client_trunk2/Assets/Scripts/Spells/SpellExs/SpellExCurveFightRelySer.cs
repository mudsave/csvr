using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SPELL
{
    [System.Serializable]
    public struct AnimationCurve4Behaviour2
    {
        public float curve;
        public float intervalTime;
        public int[] relation;
        public SpellEffect[] casterEffects;  // 触发时给施法者放的技能效果
        public SpellEffect[] spellEffects;   // 触发时给符合条件的受术者放的技能效果
    }


    /// <summary>
    /// 使用Animation Curve做为技能效果触发方式的类
    /// 每一次触发都使用独立的目标搜索器
    /// </summary>
    [AddComponentMenu("Spell/SpellExCurveFightRelySer")]
    public class SpellExCurveFightRelySer : SpellExCurveBaseRelySer
    {
        public AnimationCurve4Behaviour2[] triggers;

        public override void Init(DataSection.DataSection dataSection)
        {
            base.Init(dataSection);
            var SPELLLOADER = SpellLoader.instance;

            var _triggers = new List<AnimationCurve4Behaviour2>();
            foreach (var section in dataSection["combatFunction"]["curveTriggers"].values())
            {
                var ac4b = new AnimationCurve4Behaviour2();
                ac4b.curve = section.readFloat("curve");
                ac4b.intervalTime = section.readFloat("intervalTime");
                ac4b.relation = section.readIntArray("relation", ',');

                var _casterEffects = new List<SpellEffect>();
                foreach (var _castEffectID in section["casterEffects"].readInts("item"))
                {
                    _casterEffects.Add(SPELLLOADER.GetEffect(_castEffectID));
                }
                ac4b.casterEffects = _casterEffects.ToArray();

                var _spellEffects = new List<SpellEffect>();
                foreach (var _spellEffectID in section["spellEffects"].readInts("item"))
                {
                    _spellEffects.Add(SPELLLOADER.GetEffect(_spellEffectID));
                }
                ac4b.spellEffects = _spellEffects.ToArray();

                _triggers.Add(ac4b);
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

        protected override float intervalTimeTotal()
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

        }
    }
}
