using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LevelDesign
{
    /// <summary>
    /// 一个关卡组，子对象会一次性全部激活。
    /// 当完成指定通关数量后，整个关卡组会通关。
    /// </summary>
    [AddComponentMenu("LevelDesign/ElemSpecifiedNumberLevelOver")]
	public class ElemSpecifiedNumberLevelOver : ElemGroup
    {
        public int PassNumber = 1;  //需要通关的数量
        int m_passedCount = 0;  // 已通关计数

        public override void OnActive()
        {
            m_passedCount = 0;
            if (PassNumber > elements.Count)
            {
                PassNumber = elements.Count;
            }
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
            ++m_passedCount;
            if (m_passedCount == PassNumber)
            {
                LevelPass();
            }
        }
    }
}

