using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPELL
{
    /// <summary>
    /// 依据受术者效果状态判断施法条件
    /// </summary>
    public class EffectStatusCondition : ConditionBase
    {
        public int type;
        public int effectStatus;

        public override void Init(DataSection.DataSection dataSection)
        {
            type = dataSection.readInt("type");
            effectStatus = dataSection.readInt("effectStatus");
        }

        /// <summary>
        /// 判断施法者或受术者自身是否满足条件
        /// </summary>
        /// <param name="src">caster or receiver</param>
        public override SpellStatus Verify(AvatarComponent src, AvatarComponent dst)
        {
            if (dst.HasEffectStatus((eEffectStatus)effectStatus))
            {
                if (type == 0)
                    return SpellStatus.INVALID_TARGET_TYPE;
                else
                    return SpellStatus.OK;
            }
            else
            {
                if (type == 0)
                    return SpellStatus.OK;
                else
                    return SpellStatus.INVALID_TARGET_TYPE;
            }
        }
    }
}
