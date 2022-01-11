using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SPELL
{
    public class RoundFinder : FinderBase
    {
        float radius = 0.0f;

        public override void Init(DataSection.DataSection dataSection)
        {
            radius = dataSection.readFloat("radius");
        }

        public override List<AvatarComponent> Find(AvatarComponent src)
        {
            return AvatarComponent.AvatarInRange(radius, src, Vector3.zero);
        }
    }
}

