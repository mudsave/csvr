using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SPELL
{
    public abstract class ConditionBase
    {
        /// <summary>
        /// 类型映射表
        /// 注意：此表需要与服务器端的保持一致
        /// </summary>
        private static Dictionary<int, System.Type> s_conditionMap = new Dictionary<int, System.Type>()
        {
            //{1, typeof(NotEntityActionFlags)},
            {4, typeof(EffectStatusCondition)}
        };

        public static ConditionBase CreateCondition(int type)
        {
            if (!s_conditionMap.ContainsKey(type))
                return null;

            var cond = (ConditionBase)System.Activator.CreateInstance(s_conditionMap[type]);
            return cond;
        }

        public virtual void Init(DataSection.DataSection dataSection)
        {

        }

        /// <summary>
        /// 判断施法者或受术者自身是否满足条件
        /// </summary>
        /// <param name="src">caster or receiver</param>
        public virtual SpellStatus Verify(AvatarComponent src, AvatarComponent dst)
        {
            return SpellStatus.OK;
        }
    }
}
