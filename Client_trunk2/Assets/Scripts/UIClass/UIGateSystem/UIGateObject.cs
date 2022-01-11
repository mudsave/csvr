using UnityEngine;
using System.Collections;
using VRTK;

public class UIGateObject : MonoBehaviour
{
    private Vector3 m_originPosition = Vector3.zero;
    public Vector3 originPosition
    {
        get { return m_originPosition; }
        set { m_originPosition = value; }
    }
    private VRTK_InteractableObject interactableObject;

    void Awake()
    {
        interactableObject = GetComponent<VRTK_InteractableObject>();
        interactableObject.InteractableObjectGrabbed += OnGrabbed;
        interactableObject.InteractableObjectUngrabbed += OnUngrabbed;
    }

    void Start()
    {
    }

    void Update()
    {
        if (!interactableObject.IsGrabbed() && m_originPosition != transform.position)
        {
            transform.position = m_originPosition;
        }

        if (interactableObject.IsGrabbed())
        {
            if (Vector3.Distance(VRInputManager.Instance.head.transform.position, transform.position) < 0.5f)
            {
                //Debug.Log("切换场景");
                GlobalEvent.fire("OnGotoMap");
                KBEngine.KBEngineApp.app.player().cellCall("gotoMapIndex", new object[] { 0 });
            }
        }
    }

    public void OnGrabbed(object sender, InteractableObjectEventArgs e)
    {
        Debug.Log("UIGateObject.OnGrabbed");
    }

    public void OnUngrabbed(object sender, InteractableObjectEventArgs e)
    {
        Debug.Log("UIGateObject.OnUngrabbed");
    }
}