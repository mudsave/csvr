using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class VRInputModule : BaseInputModule
{
    private static VRInputModule m_instance;

    private GameObject m_submitObject;

    public static VRInputModule Instance
    {
        get
        {
            if (m_instance == null)
                m_instance = GameObject.FindObjectOfType<VRInputModule>();

            return m_instance;
        }
    }

    public override bool ShouldActivateModule()
    {
        if (!base.ShouldActivateModule())
            return false;

        return m_submitObject != null;
    }

    public void HoverBegin(GameObject p_gameObject)
    {
        PointerEventData eventData = new PointerEventData(eventSystem);
        ExecuteEvents.Execute(p_gameObject, eventData, ExecuteEvents.pointerEnterHandler);
    }

    public void HoverEnd(GameObject p_gameObject)
    {
        PointerEventData eventData = new PointerEventData(eventSystem);
        ExecuteEvents.Execute(p_gameObject, eventData, ExecuteEvents.pointerExitHandler);
    }

    public void Submit(GameObject p_gameObject)
    {
        Debug.Log("Submit(GameObject p_gameObject)");
        m_submitObject = p_gameObject;
    }

    public override void Process()
    {
        if(m_submitObject)
        {
            BaseEventData data = GetBaseEventData();
            data.selectedObject = m_submitObject;
            ExecuteEvents.Execute(m_submitObject, data, ExecuteEvents.submitHandler);

            m_submitObject = null;
        }
    }
}
