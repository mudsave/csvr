using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WarpEffectComponent : EffectComponent
{

    public float warpPower = 0.0f;
    public Vector4 strengthRadius = Vector4.zero;
    public float delayZoomTime = 0.0f;
    private Renderer[] renderers;
    float currentTime = 0.0f;
    float zoomSize = 1.0f;
    float durationTime = 0.0f;
    List<Material> oldMat = new List<Material>();
    List<Material> currentMat = new List<Material>();


    public override void Init(CEffectParameter modelParameter)
    {
        base.Init(modelParameter);
        var arg1 = modelParameter.effectConfig.args[0];
        strengthRadius = new Vector4((float)(double)arg1[0], (float)(double)arg1[1], (float)(double)arg1[2], (float)(double)arg1[3]);
        warpPower = (float)(double)modelParameter.effectConfig.args[1];
        delayZoomTime = (float)(double)modelParameter.effectConfig.args[2];
        durationTime = lastTime;
    }

    public override void StartEffect()
    {
        currentTime = Time.time;
        renderers = gameObject.GetComponentsInChildren<Renderer>();
        for (int i = 0, imax = renderers.Length; i < imax; ++i)
        {
            Renderer render = renderers[i];
            Material[] mats = render.materials;
            for (int j = 0; j < mats.Length; j++)
            {
                oldMat.Add(mats[j]);
                string shaderName = mats[j].shader.name;
                if (shaderName == "Standard (Specular setup)")
                {
                    Shader shader = Shader.Find("StandardSpecularCutOffWarp");
                    Material lastMat = mats[j];
                    Material mat = new Material(shader);
                    mats[j] = mat;
                    mats[j].SetTexture("_MainTex", lastMat.GetTexture("_MainTex"));
                    mats[j].SetTexture("_SpecGlossMap", lastMat.GetTexture("_SpecGlossMap"));
                    mats[j].SetFloat("_Glossiness", lastMat.GetFloat("_Glossiness"));
                    mats[j].SetTexture("_BumpMap", lastMat.GetTexture("_BumpMap"));
                    mats[j].SetFloat("_BumpScale", lastMat.GetFloat("_BumpScale"));
                }
                else if (shaderName == "Standard")
                {
                    Shader shader = Shader.Find("StandardWarp");
                    Material lastMat = mats[j];
                    Material mat = new Material(shader);
                    mats[j] = mat;
                    mats[j].SetTexture("_MainTex", lastMat.GetTexture("_MainTex"));
                    mats[j].SetTexture("_BumpMap", lastMat.GetTexture("_BumpMap"));
                    mats[j].SetFloat("_BumpScale", lastMat.GetFloat("_BumpScale"));
                }
                else
                {
					Shader shader = Shader.Find("CSFS/VertexWarp");
					Material lastMat = mats[j];
					Material mat = new Material(shader);
					mats[j] = mat;
					mats[j].mainTexture = lastMat.mainTexture; 
                }

                currentMat.Add(mats[j]);
            }
            render.materials = mats;
        }     
    }

    public override void Update()
    {
        for (int i = 0; i < currentMat.Count; i++)
        {
            strengthRadius.y += warpPower;
            currentMat[i].SetVector("_StrengthRadius", strengthRadius);

            float time = Time.time - currentTime;
            if (time > delayZoomTime)
            {
                zoomSize = 1 - (time - delayZoomTime) / (durationTime - delayZoomTime);
                currentMat[i].SetFloat("_Size", zoomSize);
            }
        }
    }

    public override void EndEffect()
    {
        base.EndEffect();
        //for (int i = 0; i < renderers.Length; i++)
        //{
        //    Renderer ren = renderers[i];
        //    if (ren != null)
        //    {
        //        ren.material = oldMat[i];
        //        Debug.LogError(ren.material.shader.name);
        //    }
        //}

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
            Destroy(this);
        }
    }
}
