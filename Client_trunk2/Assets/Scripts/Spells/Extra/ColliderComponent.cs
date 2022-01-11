using UnityEngine;
using System.Collections;


namespace SPELL {
    public class ColliderComponent : MonoBehaviour {

        private AvatarComponent m_caster;
        private SpellEffect[] m_effects;
        private int m_times = 0;
        private bool m_resist = false;

        public AvatarComponent caster
        {
            get { return m_caster; }
        }

        public void Init(AvatarComponent caster, SpellEffect[] effects, int attackTimes)
        {
            m_caster = caster;
            m_effects = effects;
            m_times = attackTimes;
        }

        void OnTriggerEnter(Collider other)
        {
            if (m_resist || m_times == 0)
                return;

            if (other.gameObject.layer == (int)eLayers.Shield)
            {
                AudioManager.Instance.SoundPlay("盾-命中",0.2f);
                m_resist = true;
                return;
            }

            AvatarComponent dst = other.gameObject.GetComponent<AvatarComponent>();
            if (dst)
            {
                if ((m_caster.CheckRelationship(dst) == eTargetRelationship.HostilePlayers || m_caster.CheckRelationship(dst) == eTargetRelationship.HostileMonster) && dst.status != eEntityStatus.Death)
                {
                    --m_times;
                    foreach (SpellEffect effect in m_effects)
                    {
                        effect.Cast(m_caster, dst, null, null);
                    }
                }
            }
        }

    }
}