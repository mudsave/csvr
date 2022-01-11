using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SPELL
{
    /// <summary>
    /// 被动技能的基类。
    /// 对于客户端来说，可能大多数的被动技能都属于这种，
    /// 它们存在的目的更多的是为了表现的需要
    /// </summary>
    [System.Serializable]
    public class PassiveSkill : SpellEffect
    {
        public int level = 0;         // effect uid
        public string icon = "";
        public string memo = "";

        public override void Init(DataSection.DataSection dataSection)
        {
            id = dataSection.readInt("id");
            // 常规表现模块
            DataSection.DataSection generalPerformance = dataSection["generalPerformance"];
            name = generalPerformance.readString("name");
            level = generalPerformance.readInt("level");
            memo = generalPerformance.readString("memo");
            description = generalPerformance.readString("description");
        }

        private static Dictionary<string, System.Type> s_classTypeMap = new Dictionary<string, System.Type>()
        {
            {"__default__", typeof(PassiveSkill)},
            {"PassiveSkillSimple", typeof(PassiveSkillSimple)},
        };

        public new static System.Type GetClassType(string type)
        {
            if (!s_classTypeMap.ContainsKey(type))
                return null;

            return s_classTypeMap[type];
        }

        public static PassiveSkill CreatePassiveSkill(string type)
        {
            if (!s_classTypeMap.ContainsKey(type))
            {
                // 没有相同类型存在的时候，就使用默认的类型
                type = "__default__";
            }

            var obj = System.Activator.CreateInstance(s_classTypeMap[type]) as PassiveSkill;
            return obj;
        }

        /// <summary>
        /// 添加被动技能
        /// </summary>
        /// <param name="src">被动技能的添加者</param>
        /// <param name="dst">被动技能携带者</param>
        /// <param name="spell">产生这个事件的技能实例</param>
        public override void Cast(AvatarComponent src, AvatarComponent dst, SpellEx spell, SpellTargetData targetData)
        {

        }

        /// <summary>
        /// 触发被动技能效果（这里负责只表现）
        /// </summary>
        /// <param name="owner">被动技能携带者</param>
        public virtual void triggerEffect(AvatarComponent owner)
        {

        }
    }
}
