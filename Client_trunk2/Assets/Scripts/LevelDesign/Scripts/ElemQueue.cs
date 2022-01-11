using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LevelDesign
{
	public class ListElement: IComparer<ListElement>   
	{
		int m_index = 0;
		LevelElement m_element = null;
		
		public int Index
		{
			get { return m_index; }
			set { m_index = value; }
		}
		
		public LevelElement Element
		{
			get { return m_element; }
			set { m_element = value; }
		}
		
		public int Compare(ListElement listA, ListElement listB)
		{
			if (listA.Index > listB.Index)
				return 1;
			if (listA.Index < listB.Index)
				return -1;
			return 0;
		}
		
		public int CompareTo(ListElement element)
		{
			return Compare(this, element);
		}
	}
	
	/// <summary>
    /// 维护多个对象，对象会按顺序激活，同一时间仅会激活一个对象
    /// </summary>
    [AddComponentMenu("LevelDesign/ElemQueue")]
    public class ElemQueue : LevelElement
    {
        /// <summary>
        /// 用于设定每个关卡切換时的延迟时间。
        /// 即当关卡1完成时，延迟多少时间进入关卡2.
        /// </summary>
        public float switchDelay = 0.0f;

        int m_levelIndex = -1;  // 记录当前处于关卡的哪个位置
        LevelElement m_currElement = null;

		List<ListElement> m_elements = new List<ListElement>();

		// Use this for initialization
		public override void Init ()
		{
            int index = 0;

			foreach (Transform child in transform)
			{
                //int id = child.name.Contains("-") ? (int.Parse(child.name.Split('-')[0]) >= 0 ? int.Parse(child.name.Split('-')[0]) : 0) : 0;
                //LevelElement elemChild = child.GetComponent<LevelElement>();
                //if (elemChild)
                //{
                //    elemChild.parent = this;
                //    m_elements.Add(new ListElement { Index = id, Element = elemChild });
                //}
                LevelElement elemChild = child.GetComponent<LevelElement>();
                if (elemChild)
                {
                    elemChild.parent = this;
                    m_elements.Add(new ListElement { Index = index, Element = elemChild });
                    ++index;
                }
            }
			//m_elements.Sort(new ListElement());

            foreach (var e in m_elements)
                e.Element.Init();
        }
	
		public List<ListElement> elements
		{
			get { return m_elements; }
		}
		
		public override void OnActive()
        {
            m_levelIndex = 0;
            if (elements.Count > 0)
            {
                m_currElement = elements[m_levelIndex].Element;
                m_currElement.Active();
            }
            else
            {
                LevelPass();
            }
        }

        public override void ChildLevelPassed(LevelElement child)
        {
            if (child != m_currElement)
            {
                throw new System.Exception("child not current element, may be somewhere has logic error.");
            }

            ++m_levelIndex;
            if (elements.Count > m_levelIndex)
            {
                m_currElement = elements[m_levelIndex].Element;
                if (switchDelay > 0.0f)
                {
                    StartCoroutine(_ActivCurrentElement());
                }
                else
                {
                    m_currElement.Active();
                }
            }
            else
            {
                LevelPass();
            }
        }

        private IEnumerator _ActivCurrentElement()
        {
            yield return new WaitForSeconds(switchDelay);
            m_currElement.Active();
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

