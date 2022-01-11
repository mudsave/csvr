using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SPELL
{
    /// <summary>
    /// 
    /// </summary>
    [AddComponentMenu("Spell/SpellExCurveBaseRelySer")]
    public class SpellExCurveBaseRelySer : SpellExCurveBase
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

            if (curveDatas.waitingTime >= curveDatas.totalTime)
            {
                return false;
            }

            //DoLightEffect(caster, curveDatas);

            return true;
        }

        protected virtual float intervalTimeTotal()
        {
            return -1.0f;
        }

    }
}