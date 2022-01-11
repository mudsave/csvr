using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SPELL
{
    public class BuffAbsorb : BuffSimple
    {
        public override void Init(DataSection.DataSection dataSection)
        {
            base.Init(dataSection);
            // 战斗表现模块
            //DataSection.DataSection combatPerformance = dataSection["combatPerformance"];
        }
        protected override void OnAttach(AvatarComponent owner, Alias.BuffDataType buffData)
        {
            owner.animator.speed = 0;
            owner.effectManager.AddModelEffect("WarpEffect");
        }
        protected override void OnDetach(AvatarComponent owner, Alias.BuffDataType buffData)
        {
            owner.effectManager.RemoveModelEffect("WarpEffect");
            AvatarComponent caster = AvatarComponent.GetAvatar(buffData.casterID);
            owner.receiveDamage(caster, 100000, CDeadType.Normal);
        }
    }
}
