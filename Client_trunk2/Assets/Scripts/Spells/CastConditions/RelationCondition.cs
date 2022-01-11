using UnityEngine;
using System.Collections;

namespace SPELL
{
    public class RelationCondition : ConditionBase
    {
        int[] relation;

        public override void Init(DataSection.DataSection dataSection)
        {
            relation = dataSection.readIntArray("relation", ',');
        }

        /// <summary>
        /// 判断施法者或受术者自身是否满足条件
        /// </summary>
        /// <param name="src">caster or receiver</param>
        public override SpellStatus Verify(AvatarComponent src, AvatarComponent dst)
        {
            for (int i = 0; i < relation.Length; i++)
            {
                if (src.CheckRelationship(dst) == (eTargetRelationship)relation[i])
                    return SpellStatus.OK; 
            }
            return SpellStatus.INVALID_TARGET_TYPE;
        }

    }
}
