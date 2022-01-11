using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FrozenEffectMethod : MonoBehaviour
{

    Renderer[] renderers;
    private Object frozenObject;

    struct ModelMaterial
    {
        public GameObject go;
        public Material mat;
    }
    List<ModelMaterial> currentMaterialList = new List<ModelMaterial>();

    void Awake()
    {
        renderers = gameObject.GetComponentsInChildren<Renderer>();
        for (int i = 0, imax = renderers.Length; i < imax; ++i)
        {
            Renderer render = renderers[i];
            if (render != null && render.GetComponent<ParticleSystem>() == null)
            {
                render.enabled = true;
                ModelMaterial mm = new ModelMaterial();
                mm.go = render.gameObject;
                mm.mat = render.material;
                currentMaterialList.Add(mm);

                frozenObject = ResourceManager.LoadAssetBundleResource("Effects/materials/frozen");
                Material mat = Instantiate(frozenObject) as Material;
                if (mat != null)
                {
                    Texture mainTexture = render.material.GetTexture("_MainTex");
                    if (mainTexture != null)
                    {
                        mat.SetTexture("_colorTexture", mainTexture);
                    }                  
                    render.material = mat;
                }
            }
        }
    }

	public void OnCompleteNull()
	{
        for (int i = 0, imax = renderers.Length; i < imax; ++i)
        {
            Renderer render = renderers[i];
            if (render != null && render.GetComponent<ParticleSystem>() == null)
            {
                for (int f = 0; f < currentMaterialList.Count; f++)
                {
                    ModelMaterial ff = currentMaterialList[f];
                    if (ff.go != null && ff.go == render.gameObject)
                    {
                        render.material = ff.mat;
                    }
                }
            }           
        }

		Destroy(this);
	}
}
