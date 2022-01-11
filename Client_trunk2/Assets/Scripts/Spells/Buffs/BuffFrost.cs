using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SPELL
{
    /// <summary>
    /// 冰霜减速buff
    /// </summary>
    public class BuffFrost : BuffSimple
    {
        public Int32 buffFrozenID = 3004002;
        public SpellEffect buffFrozen = null;

        public override void Init(DataSection.DataSection dataSection)
        {
            base.Init(dataSection);
            // 战斗表现模块
            //DataSection.DataSection combatPerformance = dataSection["combatPerformance"];
            buffFrozen = SpellLoader.instance.GetEffect(buffFrozenID);
        }

        public override SpellStatus OnCast(AvatarComponent src, AvatarComponent dst, SpellEx spell)
        {
            foreach (var buff in dst.buffs.Values)
            {
                if (buff.buffID == buffFrozenID)
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
                    if (level >= 2)
                    {
                        buff.temp["level"] = 1;
                        buffFrozen.Cast(src, dst, null, null);
                    }
                    return SpellStatus.BUFF_EFFECT_SUPERPOSITION;
                }

            }
            return SpellStatus.OK;
        }

        protected override void OnAttach(AvatarComponent owner, Alias.BuffDataType buffData)
        {
            buffData.temp["level"] = 1;

            owner.animator.SetFloat("moveActionSpeed", 0.3f);
            owner.animator.SetFloat("attackSpeed", 0.3f);
            owner.eventObj.fire("Event_OnChangeSpeed", -2.5f);
        }
        protected override void OnDetach(AvatarComponent owner, Alias.BuffDataType buffData)
        {
            owner.animator.SetFloat("moveActionSpeed", 1f);
            owner.animator.SetFloat("attackSpeed", 0.7f);
            owner.eventObj.fire("Event_OnChangeSpeed", 2.5f);
        }
    }
}
