using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SPELL
{
    public abstract class FinderBase
    {
        private static Dictionary<int, System.Type> s_finderMap = new Dictionary<int, System.Type>()
        {
            {2,typeof(RoundFinder)},
            {3,typeof(SectorFinder)},
            {4,typeof(OffsetRoundFinder)},
        };

        public static FinderBase CreateFinder(int type)
        {
            if (!s_finderMap.ContainsKey(type))
                return null;

            var finder = (FinderBase)System.Activator.CreateInstance(s_finderMap[type]);
            return finder;
        }

        public virtual void Init(DataSection.DataSection dataSection)
        {

        }

        public virtual List<AvatarComponent> Find(AvatarComponent src)
        {
            return null;
        }
    }
}
