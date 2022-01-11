using UnityEngine;
using System.Collections;

namespace SPELL
{
    public class PassiveSkillSimple : PassiveSkill
    {
        public SpellAnimation triggerAnimation;

        public override void Init(DataSection.DataSection dataSection)
        {
            base.Init(dataSection);

            // 战斗表现模块
            triggerAnimation = new SpellAnimation();
            DataSection.DataSection combatPerformance = dataSection["combatPerformance"];
            if (combatPerformance != null)
            {
                triggerAnimation.Init(combatPerformance);
            }
        }

        /// <summary>
        /// 触发被动技能效果（这里负责只表现）
        /// </summary>
        /// <param name="owner">被动技能携带者</param>
        public override void triggerEffect(AvatarComponent owner)
        {
            triggerAnimation.Do(owner);
        }
    }
}
