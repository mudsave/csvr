using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPELL
{
    /// <summary>
    /// 眩晕效果
    /// </summary>
    public class EffectDizziness : SpellEffect
    {
        public bool statu;  //获得 = true，失去 = false

        public override void Init(DataSection.DataSection dataSection)
        {
            base.Init(dataSection);
            statu = dataSection.readBool("param1");
        }

        public override void Cast(AvatarComponent src, AvatarComponent dst, SpellEx spell, SpellTargetData targetData)
        {
            if (statu)
            {
                dst.EffectStatusCounterIncr((int) eEffectStatus.Dizziness);
                dst.animator.SetBool("dizzy", true);
            }
            else
            {
                dst.EffectStatusCounterDecr((int)eEffectStatus.Dizziness);
                dst.animator.SetBool("dizzy", false);
            }
        }
    }
}
