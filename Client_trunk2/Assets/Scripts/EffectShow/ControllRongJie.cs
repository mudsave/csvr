using UnityEngine;
using System.Collections;

public class ControllRongJie : MonoBehaviour {

    public float shineTime = 3.5f; //溶解效果时间
    public float delayTime = 0.0f; //延迟时间
    public bool reverseFlag = false; //是否是反向溶解
    float currentTime = 0.0f;
    float lastTime = 0.0f;
    float durationTime = 0.0f;
    Material currentMat = null;
    bool fFlag = true;

    // Use this for initialization
    void Start()
    {
        lastTime = 0.0f;
        currentTime = Time.time;
        currentMat = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if (fFlag)
        {
            float _dTime = Time.time - currentTime;
            if (_dTime > delayTime)
            {
                if (currentMat != null)
                {
                    durationTime += Time.deltaTime;

                    float f = durationTime / shineTime;
                    if (reverseFlag)
                    {
                        currentMat.SetFloat("_Amount", 1.0f - f);
                    }
                    else
                    {
                        currentMat.SetFloat("_Amount", f);
                    }

                    if (durationTime > shineTime)
                    {
                        fFlag = false;
                    }
                }
            }
        }
    }
}
