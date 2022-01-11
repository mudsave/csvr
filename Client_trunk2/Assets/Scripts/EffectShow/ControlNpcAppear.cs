using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlNpcAppear : MonoBehaviour {

    public enum AppearType //出生类型
    {
        Null,
        Appear_Dissolve, //溶解出生
    }
    public AppearType appearType = AppearType.Appear_Dissolve;
    public Object dissolveObject;
    public float shineTime = 3.5f;
    private List<Material> currentMat = new List<Material>();
    private Renderer[] dissRenderers;
    Renderer[] curRenderers;
    float currentTime = 0.0f;
    GameObject dissolve;

    void Awake()
    {
        if(appearType == AppearType.Appear_Dissolve)
        {
            curRenderers = gameObject.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < curRenderers.Length; i++)
            {
                Renderer ren = curRenderers[i];
                ren.enabled = false;
            }
            dissolve = Instantiate(dissolveObject) as GameObject;
            dissolve.transform.position = transform.position;
            dissolve.transform.rotation = transform.rotation;
        }
    }

	// Use this for initialization
	void Start () {
        if(appearType == AppearType.Appear_Dissolve)
        {
            dissRenderers = dissolve.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < dissRenderers.Length; i++)
            {
                Renderer ren = dissRenderers[i];
                if (ren != null)
                {
                    Material[] mats = ren.materials;
                    if (mats.Length > 0)
                    {
                        for (int j = 0; j < mats.Length; j++)
                        {
                            currentMat.Add(mats[j]);
                        }
                    }
                }
            }
        }		
	}
	
	// Update is called once per frame
	void Update () {

        if (appearType == AppearType.Appear_Dissolve)
        {
            if (currentMat != null)
            {
                currentTime += Time.deltaTime;

                if (currentMat != null && currentMat.Count != 0)
                {
                    if(currentTime < shineTime)
                    {
                        for (int i = 0; i < currentMat.Count; i++)
                        {
                            currentMat[i].SetFloat("_Amount", 1.0f - currentTime / shineTime);
                        }
                    }
                    else
                    {                      
                        for (int i = 0; i < curRenderers.Length; i++)
                        {
                            Renderer ren = curRenderers[i];
                            ren.enabled = true;
                        }
                        GameObject.Destroy(dissolve);
                    }
                }
            }
        }
	}
}
