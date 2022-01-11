using UnityEngine;
using System.Collections;

public class ModelSequence : MonoBehaviour
{
    public int uvAnimationTileX = 1;
    public int uvAnimationTileY = 1;
    public float framesPerSecond = 10.0f;
    public int playCount = 1;
    public float delayTime = 0.0f;
    public Gradient gradient = new Gradient();
    public bool useGradient = false;
    public bool loop = false;
    public bool play = true;
    private int index;
    private float offsettime = 0.0f;
    public bool Hidewhenstopplaying;
    private int count = 0;
    float firstFrameTime = 0.0f;
    float _time = 0.0f;
    MeshRenderer meshRender;

    void Start()
    {
        meshRender = gameObject.GetComponent<MeshRenderer>();
        if (meshRender != null)
        {
            meshRender.enabled = false;
        }

        firstFrameTime = Time.time;
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
        float _dtime = Time.time - firstFrameTime;
        if (_dtime >= delayTime)
        {
            if (meshRender.enabled == false && play)
            {
                meshRender.enabled = true;
            }
            PlayQueue();
        }
    }

    void PlayQueue()
    {

        index = (int)((Time.time - offsettime) * framesPerSecond);
        if (play)
        {
            index = index % (uvAnimationTileX * uvAnimationTileY);
            var size = new Vector2(1.0f / (float)uvAnimationTileX, 1.0f / (float)uvAnimationTileY);
            var uIndex = index % uvAnimationTileX;
            var vIndex = index / uvAnimationTileX;
            var offset = new Vector2((float)(uIndex * size.x), (float)(1.0f - size.y - vIndex * size.y));

            GetComponent<Renderer>().material.SetTextureOffset("_MainTex", offset);
            GetComponent<Renderer>().material.SetTextureScale("_MainTex", size);

            if (useGradient)
            {
                float t = (float)(index / ((uvAnimationTileX * uvAnimationTileY) - 1f));
                Color color = gradient.Evaluate(t);
                GetComponent<Renderer>().material.SetColor("_TintColor", color);
            }
            
        }

        if (!loop)
        {
            if (index >= (uvAnimationTileX * uvAnimationTileY) - 1)
            {
                count++;
                offsettime = Time.time;
                if (count == playCount)
                {
                    meshRender.enabled = false;
                    play = false;
                    if (Hidewhenstopplaying)
                    {
                        GetComponent<Renderer>().enabled = false;
                    }
                }
            }

        }
    }
}
