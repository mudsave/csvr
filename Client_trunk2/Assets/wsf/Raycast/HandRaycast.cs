using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandRaycast : MonoBehaviour 
{
    public Transform m_target;

    private GameObject m_pointer_holder;
    private GameObject m_pointer_beam;
    public Color m_beamColor;

    void Awake()
    {
        InitPointer();
        ChangeMaterialColor(m_pointer_beam, m_beamColor);
    }

    protected void InitPointer()
    {
        m_pointer_holder = new GameObject("Pointer_Holder");
        m_pointer_holder.transform.localPosition = new Vector3(0, 3f, 0);

        m_pointer_beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        m_pointer_beam.name = "Pointer_Beam";
        m_pointer_beam.transform.SetParent(m_pointer_holder.transform);
        m_pointer_beam.GetComponent<Collider>().isTrigger = true;
        m_pointer_beam.AddComponent<Rigidbody>().isKinematic = true;

        m_pointer_beam.transform.localScale = new Vector3(0.002f, 0.002f, 50f);
        m_pointer_beam.transform.localPosition = new Vector3(0, 0, 25);

        DontDestroyOnLoad(m_pointer_holder);
    }

	// Update is called once per frame
	void Update() 
    {
        Debug.DrawLine(transform.position, m_target.position, Color.red);
        RaycastHit hitInfo;
        Vector3 myPos = transform.position;
        Vector3 dir = (m_target.position - myPos).normalized;

        int layer = LayerMask.GetMask("UI");
        if (Physics.Raycast(myPos, dir, out hitInfo, Mathf.Infinity, layer))
        {
            Debug.DrawLine(myPos, hitInfo.point, Color.blue);
        }
        else
        {
            Debug.LogError("can not raycast...");
        }
	}

    public static float NumberPercent(float value, float percent)
    {
        percent = Mathf.Clamp(percent, 0f, 100f);
        return (percent == 0f ? value : (value - (percent / 100f)));
    }

    protected virtual void ChangeMaterialColor(GameObject obj, Color color)
    {
        foreach (Renderer mr in obj.GetComponentsInChildren<Renderer>())
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
                    //int percent = 50;
                    //Color tempColor = new Color(NumberPercent(color.r, percent), NumberPercent(color.g, percent), NumberPercent(color.b, percent), color.a);
                    mr.material.SetColor("_EmissionColor", color);
                }
            }
        }
    }
}
