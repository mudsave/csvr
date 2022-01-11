using UnityEngine;
using System.Collections;

public class ModelLuminousEffectComponent : EffectComponent
{
    Material currentMaterial;
    Color _mainColor;
    Color _rimColor;
    float _rimWidth = 0.0f;
    Texture mainTexture;
    Renderer[] renderers;

    public override void Init(CEffectParameter modelParameter)
    {
        base.Init(modelParameter);
        var arg1 = modelParameter.effectConfig.args[0];
        var arg2 = modelParameter.effectConfig.args[1];
        _mainColor = new Color((float)(double)arg1[0], (float)(double)arg1[1], (float)(double)arg1[2], (float)(double)arg1[3]);
        _rimColor = new Color((float)(double)arg2[0], (float)(double)arg2[1], (float)(double)arg2[2], (float)(double)arg2[3]);
        _rimWidth = (float)(double)modelParameter.effectConfig.args[2];
    }

    public override void StartEffect()
    {
        base.StartEffect();
        renderers = gameObject.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer render = renderers[i];
            if (render != null && render.GetComponent<ParticleSystem>() == null)
            {
                Material m = render.sharedMaterial;
                currentMaterial = m;
                mainTexture = m.GetTexture("_MainTex");
            }
        }

        Shader shader = Shader.Find("KST/MoXingFaGuang");
        Material mat = new Material(shader);
        mat.SetTexture("_MainTex", mainTexture);
        mat.SetColor("_Color", _mainColor);
        mat.SetColor("_RimColor", _rimColor);
        mat.SetFloat("_RimWidth", _rimWidth);

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer render = renderers[i];
            if (render != null && render.GetComponent<ParticleSystem>() == null)
            {
                render.sharedMaterial = mat;
            }
        }
    }

    public override void EndEffect()
    {
        base.EndEffect();
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer render = renderers[i];
            if (render != null)
            {
                render.sharedMaterial = currentMaterial;
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
