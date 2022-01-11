using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SPELL
{
    public class OffsetRoundFinder : FinderBase
    {
        float radius = 0.0f;
        float offsetAngle = 0.0f;
        float offsetDistance = 0.0f;

        public override void Init(DataSection.DataSection dataSection)
        {
            radius = dataSection.readFloat("radius");
            offsetAngle = (float)(dataSection.readFloat("offsetAngle") * Mathf.PI / 180.0);
            offsetDistance = dataSection.readFloat("offsetDistance");
        }

        public override List<AvatarComponent> Find(AvatarComponent src)
        {
            Vector3 dir = AvatarComponent.rotalteXZ(src.transform.forward, offsetAngle);
            Vector3 pos = src.transform.position + dir * offsetDistance;
            return AvatarComponent.AvatarInRange(radius, src, pos);
        }
    }
}

