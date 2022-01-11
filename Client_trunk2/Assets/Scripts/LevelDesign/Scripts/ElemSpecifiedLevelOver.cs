using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LevelDesign
{
    /// <summary>
    /// 一个关卡组，子对象会一次性全部激活。
    /// 拖动子对象到达specialLevel，加入特殊管理。
    /// 当特殊关卡全部完成时，整个关卡组会通关。
    /// </summary>
    [AddComponentMenu("LevelDesign/ElemSpecifiedLevelOver")]
	public class ElemSpecifiedLevelOver : ElemGroup
    {
        public LevelElement[] specialLevel;       //将分组下的特殊关卡拖入，以加入特殊监控

        private int m_passedCount = 0;          // 已通关计数

        public override void OnActive()
        {
            m_passedCount = 0;
            if (elements.Count > 0)
            {
                foreach (var elem in elements)
                {
                    elem.Active();
                }
            }
            else
            {
                LevelPass();
            }
        }

        public override void ChildLevelPassed(LevelElement child)
        {
            for (int i = 0; i < specialLevel.Length; ++i)
            {
                if (specialLevel[i].Equals(child))
                {
                    
                    if (++m_passedCount >= specialLevel.Length)
                    {
                        LevelPass();
                    }
                }
            }
        }
    }
}
