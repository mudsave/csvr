using UnityEngine;
using System.Collections;

namespace SPELL
{
    /* 最简单的单次吟唱，也是吟唱的基类
	 */
    public class ChantBehaviour : SpellBehaviour
    {
        public float m_chantTime = 0.0f;
        public SpellAnimation m_animation = null;

        float m_overTime = 0.0f;

        public override bool BehaviourStart(SpellEx spell, AvatarComponent caster)
        {
            m_overTime = Time.time + m_chantTime;
            m_animation.Do(caster);

            return BehaviourUpdate(spell, caster);
        }

        public override bool BehaviourUpdate(SpellEx spell, AvatarComponent caster)
        {
            return (m_overTime >= Time.time);
        }

    }
}
