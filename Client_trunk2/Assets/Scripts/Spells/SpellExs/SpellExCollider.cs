using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace SPELL
{
    [AddComponentMenu("Spell/SpellExCollider")]
    public class SpellExCollider : SpellEx
    {
        public SpellEffect[] colliderEffects;
        public int attackTimes;

        public override void Init(DataSection.DataSection dataSection)
        {
            base.Init(dataSection);

            attackTimes = dataSection["combatFunction"].readInt("attackTimes");

            var _spellEffects = new List<SpellEffect>();
            foreach (var _spellEffectID in dataSection["combatFunction"]["colliderEffects"].readInts("item"))
            {
                _spellEffects.Add(SpellLoader.instance.GetEffect(_spellEffectID));
            }
            colliderEffects = _spellEffects.ToArray();
        }

        public override bool FireStart(AvatarComponent caster, SpellTargetData targetData)
        {
            if (caster.rightWeapon)
            {
                ColliderComponent cComponent = caster.rightWeapon.AddComponent<ColliderComponent>();
                cComponent.Init(caster, colliderEffects, attackTimes);
                caster.SetMapping("rightWeapon", cComponent);
            }

            if (caster.leftWeapon)
            {

                ColliderComponent cComponent = caster.leftWeapon.AddComponent<ColliderComponent>();
                cComponent.Init(caster, colliderEffects, attackTimes);
                caster.SetMapping("leftWeapon", cComponent);
            }
            return true;
        }

        public override bool FireUpdate(AvatarComponent caster, SpellTargetData targetData)
        {
            if (!caster.animator.GetCurrentAnimatorStateInfo((int)AnimatorLayer.Default).IsName(casterAnimation.animation))
            {
                return false;
            }
            return true;
        }

        protected override void OnOver(AvatarComponent caster)
        {
            UnityEngine.Object.Destroy((ColliderComponent)caster.PopMapping("rightWeapon"));
            UnityEngine.Object.Destroy((ColliderComponent)caster.PopMapping("leftWeapon"));
        }

    }
}