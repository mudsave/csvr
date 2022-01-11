using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SPELL
{
    /// <summary>
    /// 冰霜地面buff
    /// </summary>
    public class BuffFrostOnGround : BuffSimple
    {
        public Int32 buffSlipID = 3004004;
        public SpellEffect buffSlip = null;

        public override void Init(DataSection.DataSection dataSection)
        {
            base.Init(dataSection);
            // 战斗表现模块
            //DataSection.DataSection combatPerformance = dataSection["combatPerformance"];
            buffSlip = SpellLoader.instance.GetEffect(buffSlipID);
        }

        public override SpellStatus OnCast(AvatarComponent src, AvatarComponent dst, SpellEx spell)
        {
            foreach (var buff in dst.buffs.Values)
            {
                if (buff.buffID == buffSlipID)
                {
                    return SpellStatus.BUFF_EFFECT_SUPERPOSITION;
                }
            }

            foreach (var buff in dst.buffs.Values)
            {
                if (buff.buffID == id)
                {
                    buff.endTime = Time.time + stayTime;
                    int level = (int)buff.temp["level"];
                    buff.temp["level"] = level + 1;
                    if (level > 15)
                    {
                        buff.temp["level"] = 1;
                        buffSlip.Cast(src, dst, null, null);
                    }
                    return SpellStatus.BUFF_EFFECT_SUPERPOSITION;
                }

            }
            return SpellStatus.OK;
        }

        protected override void OnAttach(AvatarComponent owner, Alias.BuffDataType buffData)
        {
            buffData.temp["level"] = 1;
        }

        protected override void OnDetach(AvatarComponent owner, Alias.BuffDataType buffData)
        {
        }
    }
}
