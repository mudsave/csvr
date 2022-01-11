using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SPELL
{
    /// <summary>
    /// 冰冻buff
    /// </summary>
    public class BuffFrozen : BuffSimple
    {
        public override void Init(DataSection.DataSection dataSection)
        {
            base.Init(dataSection);          
            // 战斗表现模块
            //DataSection.DataSection combatPerformance = dataSection["combatPerformance"];
        }

        public override SpellStatus OnCast(AvatarComponent src, AvatarComponent dst, SpellEx spell)
        {
            foreach (var buff in dst.buffs.Values)
            {
                if (buff.buffID == id)
                {
                    return SpellStatus.BUFF_EFFECT_SUPERPOSITION;
                }
                    
            }
            return SpellStatus.OK;
        }

        protected override void OnAttach(AvatarComponent owner, Alias.BuffDataType buffData)
        {
            owner.effectManager.AddEffect("iceCrystal", owner.transform.position);
            owner.EffectStatusCounterIncr((int)eEffectStatus.Frozen);
            owner.animator.speed = 0;
        }
        protected override void OnDetach(AvatarComponent owner, Alias.BuffDataType buffData)
        {
            owner.EffectStatusCounterDecr((int)eEffectStatus.Frozen);
            owner.animator.speed = 1;
        }
    }
}
