using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class VRUIElement : MonoBehaviour, IPointerEnterHandler
{
    public UnityEvent m_onButtonClick;

    public UnityEvent m_onButtonHover;

	// Use this for initialization
	void Awake() 
    {
        Button button = gameObject.GetComponent<Button>();
        if(button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
	}

    void Start()
    {
        if (m_onButtonClick == null)
            m_onButtonClick = new UnityEvent();

        if (m_onButtonHover == null)
            m_onButtonHover = new UnityEvent();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_onButtonHover.Invoke();
    }

    private void OnRayHoverBegin(SelectRaycast p_selectRaycast)
    {
        VRInputModule.Instance.HoverBegin(gameObject);
    }

    private void OnRayHoverEnd(SelectRaycast p_selectRaycast)
    {
        VRInputModule.Instance.HoverEnd(gameObject);
    }

    private void OnRayHoverUpdate(SelectRaycast p_selectRaycast)
    {
        //Debug.Log("VRUIElement::OnRayHoverUpdate:" + p_selectRaycast.name);
        if(p_selectRaycast.GetStandardButtonDown())
        {
            VRInputModule.Instance.Submit(this.gameObject);
        }
    }

    private void OnButtonClick()
    {
        m_onButtonClick.Invoke();
    }
}
