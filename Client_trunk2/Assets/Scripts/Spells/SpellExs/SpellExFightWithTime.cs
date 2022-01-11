using UnityEngine;
using System.Collections;

namespace SPELL
{
    /// <summary>
    /// 使用时间间隔做为技能效果触发方式的类，主要针对VR玩家无施法动作的技能类型
    /// </summary>
    [AddComponentMenu("Spell/SpellExFightWithTime")]
    public class SpellExFightWithTime : SpellExFightWithCurve
    {
        public override bool FireStart(AvatarComponent caster, SpellTargetData targetData)
        {
            _CurveBaseDatas cDatas = new _CurveBaseDatas();
            cDatas.currTrigger = CurveIndex2Value(0);
            cDatas.totalTime = intervalTimeTotal();
            caster.SetMapping("CurveBaseDatas", cDatas);
            return true;
        }

        public override bool FireUpdate(AvatarComponent caster, SpellTargetData targetData)
        {
            var curveDatas = (_CurveBaseDatas)caster.QueryMapping("CurveBaseDatas");
            curveDatas.waitingTime += Time.deltaTime;

            //到了指定时间，触发效果
            if (curveDatas.waitingTime >= IntervalTimeIndexValue(curveDatas.curveIndex))
            {
                OnHit(caster, targetData, curveDatas.curveIndex);
                curveDatas.waitingTime = 0.0f;
                ++curveDatas.curveIndex;
                if (curveDatas.curveIndex >= GetCurveLength())
                {
                    return false;
                }
            }
            //DoLightEffect(caster, curveDatas);
            return true;
        }

        protected  float IntervalTimeIndexValue(int index)
        {
            if (index >= triggers.Length)
                return -1.0f;
            return triggers[index].intervalTime;
        }

    }
}
