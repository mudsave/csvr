using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveEffectComponent : EffectComponent
{
    public float shineTime = 3.5f;
    public int inverse = 1;
    private float currentTime = 0.0f;
    private List<Material> oldMat = new List<Material>();
    private List<Material> currentMat = new List<Material>();
    private Renderer[] renderers;

    private float startAmount = 0.1f;
    private float illuminate = 0.5f;
    private float tile = 1;
    private Color dissColor = new Color(0.412f, 0.043f, 0.0f, 1.0f);
    private Vector4 colorAnimate = new Vector4(1, 0, 0, 0);
    private Object dissolveObject;
    private Texture tex;
    private int k = 0;
    private int f = 0;

    public override void Init(CEffectParameter modelParameter)
    {
        base.Init(modelParameter);
        shineTime = (float)(double)modelParameter.effectConfig.args[0];
        inverse = (int)(double)modelParameter.effectConfig.args[1];
    }

    public override void Update()
    {
        if (currentMat != null)
        {
            currentTime += Time.deltaTime;

            if (currentMat != null && currentMat.Count != 0)
            {
                for (int i = 0; i < currentMat.Count; i++)
                {
                    if (inverse == 1)
                    {
                        currentMat[i].SetFloat("_Amount", currentTime / shineTime);
                    }
                    else
                    {
                        currentMat[i].SetFloat("_Amount", 1.0f - currentTime / shineTime);
                    }
                }
            }
        }
    }

    public override void StartEffect()
    {
        currentTime = 0.0f;
        Shader shader = Shader.Find("CSFS/Dissolve_TexturCoords");
        Material dissolveMat = new Material(shader);
        dissolveMat.SetFloat("_StartAmount", startAmount);
        dissolveMat.SetFloat("_Illuminate", illuminate);
        dissolveMat.SetFloat("_Tile", tile);
        if (inverse == -1)
        {
            dissColor = Color.white;
        }
        dissolveMat.SetColor("_DissColor", dissColor);
        dissolveMat.SetVector("_ColorAnimate", colorAnimate);
        dissolveObject = Resources.Load("DissolveTex");
        if (dissolveObject != null)
        {
            tex = (Texture)UnityEngine.Object.Instantiate(dissolveObject);
        }
        renderers = gameObject.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {           
            Renderer ren = renderers[i];
            if (ren != null)
            {
                Material[] mats = ren.materials;
                if (mats.Length > 0)
                {
                    for (int j = 0; j < mats.Length; j++)
                    {
                        oldMat.Add(ren.materials[j]);
                        mats[j] = dissolveMat;

                        currentMat.Add(mats[j]);
                        currentMat[k].mainTexture = oldMat[k].mainTexture;
                        currentMat[k].SetTexture("_DissolveSrc", tex);
                        if (inverse == -1)
                        {
                            currentMat[i].SetFloat("_Amount", 1.0f);
                        }
                        k++;
                    }
                }

                ren.materials = mats;
            }
            renderers[i].enabled = true;
        }
    }

    public override void EndEffect()
    {
        base.EndEffect();
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer ren = renderers[i];
            if (ren != null)
            {
                Material[] mats = ren.materials;
                if (mats.Length > 0)
                {
                    for (int j = 0; j < mats.Length; j++)
                    {
                        mats[j] = oldMat[f];
                        f++;
                    }
                }

                ren.materials = mats;
            }
        }

        f = 0;
        k = 0;
        oldMat.Clear();
        currentMat.Clear();
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
            EndEffect();
            Destroy(this);
        }
    }
}