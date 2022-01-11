using UnityEngine;
using System.Collections;

namespace SPELL
{
    /// <summary>
    /// 伤害效果
    /// </summary>
    public class EffectDynamicDamage : SpellEffect
    {
        public int damageValue;
        public float damageVol;
        public float dropPercent;
        public int magicSparCount;
        public CDeadType deadType;

        public override void Init(DataSection.DataSection dataSection)
        {
            base.Init(dataSection);
            damageValue = dataSection.readInt("param1");
            damageVol = dataSection.readFloat("param2");
            dropPercent = dataSection.readFloat("param3");
            magicSparCount = dataSection.readInt("param4");
            deadType = (CDeadType)dataSection.readInt("param5");
        }

        public override void Cast(AvatarComponent src, AvatarComponent dst, SpellEx spell, SpellTargetData targetData)
        {
            //概率获得魔法晶石
            if (magicSparCount > 0 && Random.value < dropPercent)
            {
                MagicSparMgr.CreateMagicSpar(dst, magicSparCount);
            }

            FightSystem.Fight(src,dst, spell, damageValue, damageVol, deadType);
        }
    }
}
