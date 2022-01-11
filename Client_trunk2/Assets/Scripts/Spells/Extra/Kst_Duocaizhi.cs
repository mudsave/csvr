using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class Kst_Duocaizhi : MonoBehaviour
{
    Renderer ren;
    public Material[] materials;

	// Use this for initialization
	void Start () 
    {
        ren = gameObject.GetComponent<Renderer>();
	}

    void Update()
    {
        if (Application.isPlaying)
        {
            return;
        }

        if (ren != null && materials.Length != 0)
        {
            ren.materials = materials;
        }      
    }
}
