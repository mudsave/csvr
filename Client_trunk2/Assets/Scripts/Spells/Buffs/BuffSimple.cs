using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Alias;

namespace SPELL
{
    public class BuffSimple : SpellBuff
    {
        //public SpellAnimation casterAnimation;
        public SpellEffect[] attachEffect;          //附上时执行的效果
        public SpellEffect[] detachEffect;          //buff正常卸下（非中断结束）时执行的效果
        public SpellEffect[] tickEffect;            //每心跳一次执行一次的效果
        public SpellEffect[] interruptEffect;       //uff被中断（非正常卸下）时执行的效果

        public override void Init(DataSection.DataSection dataSection)
        {
            base.Init(dataSection);
            DataSection.DataSection combatFunction = dataSection["combatFunction"];
            var SPELLLOADER = SpellLoader.instance;

            int[] attachEffectIDs = combatFunction["attachEffect"].readInts("item");
            var _attachEffect = new List<SpellEffect>();
            foreach (int id in attachEffectIDs)
            {
                _attachEffect.Add(SPELLLOADER.GetEffect(id));
            }
            attachEffect = _attachEffect.ToArray();

            int[] detachEffectIDs = combatFunction["detachEffect"].readInts("item");
            var _detachEffect = new List<SpellEffect>();
            foreach (int id in detachEffectIDs)
            {
                _detachEffect.Add(SPELLLOADER.GetEffect(id));
            }
            detachEffect = _detachEffect.ToArray();

            int[] tickEffectIDs = combatFunction["tickEffect"].readInts("item");
            var _tickEffect = new List<SpellEffect>();
            foreach (int id in tickEffectIDs)
            {
                _tickEffect.Add(SPELLLOADER.GetEffect(id));
            }
            tickEffect = _tickEffect.ToArray();

            int[] interruptEffectIDs = combatFunction["interruptEffect"].readInts("item");
            var _interruptEffect = new List<SpellEffect>();
            foreach (int id in interruptEffectIDs)
            {
                _interruptEffect.Add(SPELLLOADER.GetEffect(id));
            }
            interruptEffect = _interruptEffect.ToArray();
        }

        protected override void OnAttach(AvatarComponent owner, Alias.BuffDataType buffData)
        {
            AvatarComponent src = AvatarComponent.GetAvatar(buffData.casterID);
            foreach (var effect in attachEffect)
            {
                effect.Cast(src, owner, null, null);
            }
        }

        protected override bool OnTick(AvatarComponent owner, Alias.BuffDataType buffData)
        {
            return true;
        }
 
        protected override void OnDetach(AvatarComponent owner, Alias.BuffDataType buffData)
        {
            AvatarComponent src = AvatarComponent.GetAvatar(buffData.casterID);
            foreach (var effect in detachEffect)
            {
                effect.Cast(src, owner, null, null);
            }
        }

        protected override void OnInterrupt(AvatarComponent owner, BuffDataType buffData)
        {
            AvatarComponent src = AvatarComponent.GetAvatar(buffData.casterID);
            foreach (var effect in interruptEffect)
            {
                effect.Cast(src, owner, null, null);
            }
        }
    }
}
