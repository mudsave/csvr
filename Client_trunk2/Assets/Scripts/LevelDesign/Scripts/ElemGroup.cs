using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LevelDesign
{
	/// <summary>
	/// 维护多个对象，对象会在ElemGroup被激活时一次性全部激活
	/// </summary>
	[AddComponentMenu("LevelDesign/ElemGroup")]
	public class ElemGroup : LevelElement {
		int m_passedCount = 0;  // 已通关计数

		List<LevelElement> m_elements = new List<LevelElement>();
		
		// Use this for initialization
		public override void Init()
		{
			foreach (Transform child in transform)
			{
				LevelElement elemChild = child.GetComponent<LevelElement>();
				if (elemChild)
				{
					elemChild.parent = this;
                    elemChild.Init();
					m_elements.Add( elemChild );
				}
			}
		}

		public List<LevelElement> elements
		{
			get { return m_elements; }
		}
		
		public override void OnActive()
		{
			m_passedCount = 0;
			if (m_elements.Count > 0)
			{
				foreach (LevelElement elem in elements)
				{
					if (!elem.isActive)
						elem.Active();
				}
			}
			else
			{
				LevelPass();
			}
		}

		public override void ChildLevelPassed( LevelElement child )
		{
			if (++m_passedCount >= m_elements.Count)
				LevelPass();
		}

        public override void OnDrawGizmos()
        {
#if UNITY_EDITOR
            base.OnDrawGizmos();

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.2f);
#endif
        }
    }
}

