using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPELL
{
    public class EffectOnGround : SpellEffect
    {
        public string effect = "";
        public float distance;
        public float yOffset;

        public override void Init(DataSection.DataSection dataSection)
        {
            effect = dataSection.readString("param1");
            distance = dataSection.readFloat("param2");
            yOffset = dataSection.readFloat("param3");
        }

        // Update is called once per frame
        public override void Cast(AvatarComponent src, AvatarComponent dst, SpellEx spell, SpellTargetData targetData)
        {
            Vector3 pos = src.transform.position + src.transform.forward * distance;
            pos.y += yOffset;
            EffectComponent eComponent = src.effectManager.AddEffect(effect, pos);
            BossDuTanSkill skill = eComponent.gameObject.AddComponent<BossDuTanSkill>();
            skill.Init(src);
        }
    }
}
