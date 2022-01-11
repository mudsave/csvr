using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace SPELL
{
    public abstract class SpellEffect
    {
        //效果类型
        private static Dictionary<string, System.Type> s_classTypeMap = new Dictionary<string, System.Type>()
        {
            {"None",typeof(SpellEffect)},
            {"EffectHitPose", typeof(EffectHitPose)},
            {"EffectLighting", typeof(EffectLighting)},
            {"EffectDynamicDamage", typeof(EffectDynamicDamage)},
            {"EffectInterruptBuff", typeof(EffectInterruptBuff)},
            {"EffectBossDuTan", typeof(EffectBossDuTan)},
            {"EffectOnGround", typeof(EffectOnGround)},
            {"EffectDizziness", typeof(EffectDizziness)},
            {"EffectSuperBody", typeof(EffectSuperBody)},
        };

        //Buff类型
        private static Dictionary<string, System.Type> s_classTypeBuffMap = new Dictionary<string, System.Type>()
        {
            {"BuffSimple", typeof(BuffSimple)},
            {"BuffBullet", typeof(BuffBullet)},
            {"BuffPush", typeof(BuffPush)},
            {"BuffSummon", typeof(BuffSimple)},
            {"BuffFrozen", typeof(BuffFrozen)},
            {"BuffShield", typeof(BuffShield)},
            {"BuffLightning", typeof(BuffLightning)},
            {"BuffAbsorb", typeof(BuffAbsorb)},
            {"BuffShieldHit",typeof(BuffShieldHit)},
            {"BuffFrost", typeof(BuffFrost)},
            {"BuffFrostOnGround", typeof(BuffFrostOnGround)},
        };

        public static System.Type GetClassType(string type)
        {
            if (!s_classTypeMap.ContainsKey(type))
                return s_classTypeMap["None"];

            return s_classTypeMap[type];
        }

        public static SpellEffect CreateSpellEffect(string type)
        {
            if (s_classTypeMap.ContainsKey(type))
            {
                var obj = System.Activator.CreateInstance(s_classTypeMap[type]) as SpellEffect;
                return obj;
            }
            else if(s_classTypeBuffMap.ContainsKey(type))
            {
                var obj = System.Activator.CreateInstance(s_classTypeBuffMap[type]) as SpellBuff;
                return obj;
            }
            return null;
        }

        public int id = 0;         // effect uid
        public string name = "";
        public string description = "";

        public virtual void Init(DataSection.DataSection dataSection)
        {
            id = dataSection.readInt("id");
            name = dataSection.readString("name");
            description = dataSection.readString("description");
        }

        public virtual void Cast(AvatarComponent src, AvatarComponent dst, SpellEx spell , SpellTargetData targetData)
        {
        }

    }

}
