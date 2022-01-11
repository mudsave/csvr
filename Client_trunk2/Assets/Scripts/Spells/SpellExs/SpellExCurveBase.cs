using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SPELL
{
    public enum HitProgress : int
    {
        PreHit = 0,                /// 前摇
		Hiting,                /// attack now
		PostHit,               /// 后摇（也就是打击刚结束）
	}

    [System.Serializable]
    public class CasterLightEffectStruct
    {
        public string curveName = "attack_curve";
        public float curveValue;
        public SpellLightEffect[] casterLightEffect;
    }

    /// <summary>
    /// 使用Animation Curve做为技能效果触发方式的基础技能类
    /// </summary>
    [AddComponentMenu("Spell/SpellExCurveBase")]
    public abstract class SpellExCurveBase : SpellEx
    {
        int m_curveIndex = 0;

        public class _CurveBaseDatas
        {
            public int curveIndex = 0;
            public int effectCurveIndex = 0;
            public float currTrigger = 0.0f;
            public HitProgress hitProgress = HitProgress.PreHit;
            public bool forbidMove = false;
            public float waitingTime = 0.0f;
            public float totalTime = 0.0f;
        }

        public string curveName = "attack_curve";
        public CasterLightEffectStruct[] casterLightEffects;

        public override void Init(DataSection.DataSection dataSection)
        {
            base.Init(dataSection);

            //List<CasterLightEffectStruct> _casterLightEffects = new List<CasterLightEffectStruct>();
            //foreach (var section in dataSection["combatPerformance"]["casterExtraLightEffect"].values())
            //{
            //    var _casterLightEffect = new CasterLightEffectStruct();
            //    _casterLightEffect.curveName = section.readString("curveName");
            //    _casterLightEffect.curveValue = section.readFloat("curveValue");

            //    var _lightEffects = new List<SpellLightEffect>();
            //    foreach (var effSection in section["lightEffects"].values())
            //    {
            //        var _lightEffect = new SpellLightEffect();
            //        _lightEffect.Init(effSection);
            //        _lightEffects.Add(_lightEffect);
            //    }
            //    _casterLightEffect.casterLightEffect = _lightEffects.ToArray();

            //    _casterLightEffects.Add(_casterLightEffect);
            //}
            //casterLightEffects = _casterLightEffects.ToArray();
        }

        protected virtual int GetCurveLength()
        {
            return 0;
        }

        /// <summary>
        /// get the curve value which corresponding index,
        /// return -1.0f if index out of range.
        /// </summary>
        /// <returns>the curve value which corresponding index</returns>
        /// <param name="curveIndex">Curve index.</param>
        protected virtual float CurveIndex2Value(int index)
        {
            return -1.0f;
        }

        public override bool FireStart(AvatarComponent caster, SpellTargetData targetData)
        {
            m_curveIndex = 0;
            _CurveBaseDatas cDatas = new _CurveBaseDatas();
            cDatas.currTrigger = CurveIndex2Value(m_curveIndex);
            caster.SetMapping("CurveBaseDatas", cDatas);

            PreHit(caster, targetData);
            return true;
        }

        public override bool FireUpdate(AvatarComponent caster, SpellTargetData targetData)
        {
            var curveDatas = (_CurveBaseDatas)caster.QueryMapping("CurveBaseDatas");
            float curve = caster.animator.GetFloat(curveName);
            //Debug.Log( this + "::BehaviourUpdate(), " + Time.time + ", curve: " + curve + ", " + animator.GetCurrentAnimatorStateInfo((int)AnimatorLayer.Default).IsName(casterAnimation.animation) );
            if ((curveDatas.hitProgress != HitProgress.PreHit || curveDatas.waitingTime >= 0.1f) &&
                !caster.animator.GetCurrentAnimatorStateInfo((int)AnimatorLayer.Default).IsName(casterAnimation.animation))
            {
                PostHitEnd(caster, targetData);
                return false;
            }

            //DoLightEffect(caster, curveDatas);

            switch (curveDatas.hitProgress)
            {
                case HitProgress.PreHit:
                    /// 做一个前置的延迟状态处理，以避免“技能放了，但动作还没放出来时又播放了受击动作，导致技能死锁”的问题。
                    curveDatas.waitingTime += Time.deltaTime;

                    // 此处配置的技能动画第一个Curve值必须大于0.1，来保证前面的前置延迟状态处理有效。这么做能保证在低帧数下光效能正常播放,当帧数低于10FPS还是会发生问题。
                    if (curve >= curveDatas.currTrigger)
                    {
                        PreHitEnd(caster, targetData, curveDatas);
                        curveDatas.hitProgress = HitProgress.Hiting;
                        goto case HitProgress.Hiting;
                    }
                    return true;

                case HitProgress.Hiting:
                    if (curve >= curveDatas.currTrigger)
                    {
                        OnHit(caster, targetData, curveDatas.curveIndex);

                        ++curveDatas.curveIndex;
                        if (curveDatas.curveIndex < GetCurveLength())
                        {
                            curveDatas.currTrigger = CurveIndex2Value(curveDatas.curveIndex);
                        }
                        else
                        {
                            curveDatas.hitProgress = HitProgress.PostHit;
                            PostHit(caster, targetData);
                        }
                    }
                    return true;

                case HitProgress.PostHit:
                    return true;

                default:
                    return false;
            }
        }

        //public void DoLightEffect(AvatarComponent caster, _CurveBaseDatas curveData)
        //{
        //    if (curveData.effectCurveIndex < casterLightEffects.Length)
        //    {
        //        float curve = caster.animator.GetFloat(casterLightEffects[curveData.effectCurveIndex].curveName);
        //        if (curve >= casterLightEffects[curveData.effectCurveIndex].curveValue)
        //        {
        //            foreach (var v in casterLightEffects[curveData.effectCurveIndex].casterLightEffect)
        //            {
        //                AddEffectInstances(caster, v.Do(caster));
        //            }

        //            ++curveData.effectCurveIndex;
        //        }
        //    }
        //}

        /// <summary>
        /// 前摇开始
        /// </summary>
        public virtual void PreHit(AvatarComponent caster, SpellTargetData targetData)
        {

        }

        /// <summary>
        /// 前摇结束（也就是第一次打击开始）
        /// </summary>
        public virtual void PreHitEnd(AvatarComponent caster, SpellTargetData targetData, _CurveBaseDatas curveData)
        {

        }

        /// <summary>
        /// 每一次击中时 
        /// </summary>
        public virtual void OnHit(AvatarComponent caster, SpellTargetData targetData, int curveIndex)
        {

        }

        /// <summary>
        /// 后摇开始
        /// </summary>
        public virtual void PostHit(AvatarComponent caster, SpellTargetData targetData)
        {

        }

        /// <summary>
        /// 后摇结束
        /// </summary>
        public virtual void PostHitEnd(AvatarComponent caster, SpellTargetData targetData)
        {
        }

        protected override void OnOver(AvatarComponent caster)
        {
        }
    }
}
