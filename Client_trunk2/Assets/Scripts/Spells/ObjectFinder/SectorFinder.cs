using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPELL
{
    public class SectorFinder : FinderBase
    {
        float radius = 0.0f;
        float angle = 0.0f;

        public override void Init(DataSection.DataSection dataSection)
        {
            radius = dataSection.readFloat("radius");
            angle = dataSection.readFloat("angle");
        }

        public override List<AvatarComponent> Find(AvatarComponent src)
        {
            List<AvatarComponent> objs = AvatarComponent.AvatarInRange(radius, src, Vector3.zero);
            List<AvatarComponent> result = new List<AvatarComponent>();

            foreach (AvatarComponent obj in objs)
            {
                Vector3 desDir = obj.gameObject.transform.position - src.gameObject.transform.position;
                desDir.y = 0;
                desDir.Normalize();

                float an = Vector3.Dot(src.gameObject.transform.forward,desDir);

                if (an < -1)
                    an = -1;
                if (an > 1)
                    an = 1;

                int angleTemp = (int)(Mathf.Acos(an) / Mathf.PI * 180);
                if (angleTemp <= angle / 2.0)
                {
                    result.Add(obj);
                }
            }
            return result;
        }
    }
}
