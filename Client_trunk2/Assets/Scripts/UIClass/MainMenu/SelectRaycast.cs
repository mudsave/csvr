using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectRaycast : MonoBehaviour 
{
    private GameObject m_pointer_holder;
    private GameObject m_pointer_beam;
    private VRUIElement m_hoverTarget;
    private SteamVR_Controller.Device m_device;

    public Color m_beamColor;
    public float m_beamThick = 0.002f;
    public float m_beamLength = 50.0f;
    public SteamVR_TrackedObject m_tracked;

    void Awake()
    {
        InitBeam();
    }

	// Use this for initialization
	void Start () 
    {
        InitDevice();
	}

    void Update()
    {
        UpdateBeam();
        UpdateHoverTarget();
    }

    protected void UpdateHoverTarget()
    {
        if (m_hoverTarget)
            m_hoverTarget.SendMessage("OnRayHoverUpdate", this, SendMessageOptions.DontRequireReceiver);
    }

    protected void InitDevice()
    {
        m_device = SteamVR_Controller.Input((int)m_tracked.index);
    }

    protected void InitBeam()
    {
        m_pointer_holder = new GameObject("Pointer_Holder");
        m_pointer_holder.transform.SetParent(transform, false);

        m_pointer_beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        m_pointer_beam.name = "Pointer_Beam";
        m_pointer_beam.transform.SetParent(m_pointer_holder.transform, false);
        m_pointer_beam.GetComponent<Collider>().isTrigger = true;
        m_pointer_beam.AddComponent<Rigidbody>().isKinematic = true;

        m_pointer_beam.transform.localScale = new Vector3(m_beamThick, m_beamThick, m_beamLength);
        m_pointer_beam.transform.localPosition = new Vector3(0, 0, m_beamLength / 2);

        ChangeBeamColor(m_beamColor);
    }

    protected void UpdateBeam()
    {
        VRUIElement targetUI = null;

        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        RaycastHit hitInfo;
        int layer = LayerMask.GetMask("UI");
        if( Physics.Raycast(transform.position, fwd, out hitInfo, 100.0f, layer) )
        {
            Debug.DrawLine(transform.position, hitInfo.point, Color.red);
            targetUI = hitInfo.collider.gameObject.GetComponentInParent<VRUIElement>();
            
            //Debug.LogError(string.Format("UpdateBeam::Physics.Raycast name({0}), point({1})", hitInfo.collider.gameObject.name, hitInfo.point));
        }
        SetHoverTarget(targetUI);
    }

    protected virtual void ChangeBeamColor(Color color)
    {
        foreach (Renderer mr in m_pointer_holder.GetComponentsInChildren<Renderer>())
        {
            if (mr.material)
            {
                mr.material.EnableKeyword("_EMISSION");

                if (mr.material.HasProperty("_Color"))
                {
                    mr.material.color = color;
                }

                if (mr.material.HasProperty("_EmissionColor"))
                {
                    mr.material.SetColor("_EmissionColor", color);
                }
            }
        }
    }

    protected void SetHoverTarget(VRUIElement p_uiElement)
    {
        if(m_hoverTarget != p_uiElement)
        {
            if(m_hoverTarget != null)
            {
                //Debug.Log("SendMessage OnRayHoverEnd:" + this.gameObject.name);
                m_hoverTarget.SendMessage("OnRayHoverEnd", this, SendMessageOptions.DontRequireReceiver);
            }

            m_hoverTarget = p_uiElement;

            if (m_hoverTarget != null)
            {
                //Debug.Log("SendMessage OnRayHoverBegin:" + this.gameObject.name);
                m_hoverTarget.SendMessage("OnRayHoverBegin", this, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public bool GetStandardButtonDown()
    {
#if UNITY_EDITOR
        if(SteamVR.instance == null)
            return Input.GetMouseButtonDown(0);
#endif
        if (m_device != null)
        {
            return m_device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger);
        }
        
        return false;
    }
}
