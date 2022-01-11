using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

namespace SPELL
{
    public class BuffShieldHit : BuffPush
    {
        public override void Init(DataSection.DataSection dataSection)
        {
            base.Init(dataSection);
        }

        protected override void OnAttach(AvatarComponent owner, Alias.BuffDataType buffData)
        {
            //base.OnAttach(owner, buffData);
            owner.EffectStatusCounterIncr((int)eEffectStatus.HitBy);
            owner.animator.SetTrigger("shield");

        }

        protected override void OnDetach(AvatarComponent owner, Alias.BuffDataType buffData)
        {
            owner.EffectStatusCounterDecr((int)eEffectStatus.HitBy);
        }

    }
}