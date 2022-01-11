using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FrozenEffectComponent : EffectComponent
{
    private Renderer[] renderers;
    private Object frozenObject;

    struct ModelMaterial
    {
        public GameObject go;
        public Material mat;
    }

    private List<ModelMaterial> currentMaterialList = new List<ModelMaterial>();

    public override void Init(CEffectParameter modelParameter)
    {
        base.Init(modelParameter);
    }

    public override void StartEffect()
    {
        base.StartEffect();
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

    public override void EndEffect()
    {
        base.EndEffect();
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
    }

    public override void DestroyEffect()
    {
        AvatarComponent owner = AvatarComponent.GetAvatar(m_ownerID);
        if (owner && owner.effectManager != null)
        {
            owner.effectManager.RemoveModelEffect(m_effectName);
        }
        else
        {
            Destroy(this);   
        }
    }
}
