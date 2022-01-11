using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneDissolveControl : MonoBehaviour {

    public float MaxDistance;
    public float disolveSpeed;
    public Vector4 _dissolvePoint;
    public float _intensity;
    public float _noiseFreq;
    public float _border;
    public Color _borderColor;
    public Color _borderEmission;
    public float _inverse;
    public int _lastLayer;
    public GameObject rendererCam;
    public List<Material> oldMat = new List<Material>();
    public List<Material> currentMat = new List<Material>();

    float _maxDistance = 0f;
    Material _material;
    Renderer[] renderers;   
    bool resumeFlag = false;
    int k = 0;
    int f = 0;


    void Start()
    {
        Shader shader = Shader.Find("Custom/Dissolve");
        Material dissolveMat = new Material(shader);
        dissolveMat.SetVector("_DissolvePoint", _dissolvePoint);
        dissolveMat.SetFloat("_MaxDistance", 0f);
        dissolveMat.SetFloat("_Intensity", _intensity);
        dissolveMat.SetFloat("_NoiseFreq", _noiseFreq);
        dissolveMat.SetFloat("_Border", _border);
        dissolveMat.SetColor("_BorderColor", _borderColor);
        dissolveMat.SetColor("_BorderEmission", _borderEmission);
        dissolveMat.SetFloat("_Inverse", _inverse);
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
                        k++;
                    }
                }

                ren.materials = mats;
            }
        }

        var render = transform.GetComponent<Renderer>();
        if (render != null)
        {
            _material = render.material;
        }
    }

    void Update()
    {
        if (_maxDistance < MaxDistance)
        {
            _maxDistance = _maxDistance + Time.fixedDeltaTime * disolveSpeed;
            _material.SetFloat("_MaxDistance", _maxDistance);
        }

        if (_maxDistance >= MaxDistance && !resumeFlag)
        {
            resumeFlag = true;
            gameObject.layer = _lastLayer;
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
            Destroy(this);
            GameObject.Destroy(rendererCam);
        }
    }
}
