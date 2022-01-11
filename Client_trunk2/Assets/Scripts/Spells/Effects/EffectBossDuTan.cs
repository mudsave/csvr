using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPELL
{
    public class EffectBossDuTan : SpellEffect
    {

        public override void Init(DataSection.DataSection dataSection)
        {

        }

        // Update is called once per frame
        public override void Cast(AvatarComponent src, AvatarComponent dst, SpellEx spell, SpellTargetData targetData)
        {
            Vector3 pos = src.transform.position + src.transform.forward * Random.Range(15,25);
            pos.y += 3.7f;
            EffectComponent eComponent = src.effectManager.AddEffect("shachong_dutandimian", pos);
            BossDuTanSkill skill = eComponent.gameObject.AddComponent<BossDuTanSkill>();
            skill.Init(src);

        }
    }
}
