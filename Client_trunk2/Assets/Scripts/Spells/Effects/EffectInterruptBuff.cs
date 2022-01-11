using UnityEngine;
using System.Collections;

namespace SPELL
{
    /// <summary>
    /// 根据中断码打断buff
    /// </summary>
    public class EffectInterruptBuff : SpellEffect
    {
        public int[] interruptCodes;

        public override void Init(DataSection.DataSection dataSection)
        {
            base.Init(dataSection);
            interruptCodes = dataSection.readIntArray("param1", ',');
        }

        public override void Cast(AvatarComponent src, AvatarComponent dst, SpellEx spell, SpellTargetData targetData)
        {
            foreach (int interruptCode in interruptCodes)
            {
                dst.InterruptBuff(interruptCode);
            }
        }
    }
}