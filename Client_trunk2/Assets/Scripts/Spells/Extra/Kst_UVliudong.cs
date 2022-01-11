using UnityEngine;
using System.Collections;

public class Kst_UVliudong : MonoBehaviour
{

    public int materialIndex = 0;
    public Vector2 uvAnimationRate = new Vector2(1.0f, 0.0f);
    public string textureName = "_MainTex";
    public float startDelay = 0.0f;
    Vector2 uvOffset = Vector2.zero;
    float lastTime = 0.0f;
    public int playCount = 0;
    public bool Loop = false;
    public Gradient gradient = new Gradient();
    public bool useGradient = false;
    private Vector2 tilingValue = Vector2.zero;
    private Vector2 offsetUv = Vector2.zero;

    MeshRenderer meshRender;
    LineRenderer lineRender;

    int _Xcount = 1;
    int _Ycount = 1;

   
    bool fFlag = false;


    void Start()
    {
        offsetUv = GetComponent<Renderer>().materials[materialIndex].GetTextureOffset(textureName);
        tilingValue = GetComponent<Renderer>().materials[materialIndex].GetTextureScale(textureName);
        lineRender = gameObject.GetComponent<LineRenderer>();
        if (lineRender == null)
        {
            meshRender = gameObject.GetComponent<MeshRenderer>();
            if (meshRender != null)
            {
                meshRender.enabled = false;
            }
        }
        

        lastTime = Time.time;

        GradientColorKey[] colorKey = new GradientColorKey[2];
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
        colorKey[0].color = gradient.colorKeys[0].color;
        colorKey[0].time = 0.0f;
        colorKey[1].color = gradient.colorKeys[1].color;
        colorKey[1].time = 1.0f;
        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        alphaKey[0].alpha = gradient.alphaKeys[0].alpha;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = gradient.alphaKeys[1].alpha;
        alphaKey[1].time = 1.0f;

        gradient.SetKeys(colorKey, alphaKey);

    }

    void Update()
    {
        float _dTime = Time.time - lastTime;
        if (_dTime >= startDelay)
        {
            if (meshRender != null)
            {
                if (meshRender.enabled == false)
                {
                    meshRender.enabled = true;
                }
            }
            

            if (!Loop)
            {
                if (uvAnimationRate.x != 0 && uvAnimationRate.y == 0)
                {
                    if (_Xcount <= playCount)
                    {
                        UvScoll();
                    }                  
                }
                else if (uvAnimationRate.x == 0 && uvAnimationRate.y != 0)
                {
                    if (_Ycount <= playCount)
                    {
                        UvScoll();
                    }
                }
                else
                {
                    if (_Xcount <= playCount || _Ycount <= playCount)
                    {
                        UvScoll();
                    }
                }
            }
            else
            {
                UvScoll();
            }

        }
    }

    void UvScoll()
    {
        uvOffset += (uvAnimationRate * Time.deltaTime);
        if (uvOffset.x > tilingValue.x)
        {
            uvOffset.x = -tilingValue.x;
            _Xcount++;
        }

        if (uvOffset.y > tilingValue.y)
        {
            uvOffset.y = -tilingValue.y;
            _Ycount++;
        }

        if (uvOffset.x < -tilingValue.x)
        {
            uvOffset.x = tilingValue.x;
            _Xcount++;
        }

        if (uvOffset.y < -tilingValue.y)
        {
            uvOffset.y = tilingValue.y;
            _Ycount++;
        }

        if (useGradient)
        {
            if (uvOffset.x != 0 && uvOffset.y == 0)
            {
                Color color = gradient.Evaluate(1 - Mathf.Abs(uvOffset.x));
                GetComponent<Renderer>().material.SetColor("_TintColor", color);
            }
            else if (uvOffset.x == 0 && uvOffset.y != 0)
            {
                Color color = gradient.Evaluate(1 - Mathf.Abs(uvOffset.y));
                GetComponent<Renderer>().material.SetColor("_TintColor", color);
            }
        }
        

        if (!fFlag)
        {
            fFlag = true;

            uvOffset = uvOffset + offsetUv;
        }
       
        GetComponent<Renderer>().materials[materialIndex].SetTextureOffset(textureName, uvOffset);
       
    }
}
