using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveControl : MonoBehaviour {

    public Object currentObj;
    public Material mat;
    public GameObject[] dissolveObjectList;
    public Vector4 dissolvePoint;
    public float intensity;
    public float noiseFreq;
    public float border;
    public Color borderColor;
    public Color borderEmission;
    public float inverse;
    public float delayTime;
    private GameObject cameraObject;
    bool beginFlag = false;
    Camera mainCamera;
    float lastTime;
    bool delayFlag = false;

	
	// Update is called once per frame
    void Update()
    {
        mainCamera = Camera.main;
        if (mainCamera != null && !beginFlag)
        {
            if(!delayFlag)
            {
                delayFlag = true;
                lastTime = Time.time;
            }

            float druationTime = Time.time - lastTime;
            if(druationTime > delayTime)
            {
                beginFlag = true;
                cameraObject = mainCamera.gameObject;
                AE_GroupBloom groupBloom = cameraObject.AddComponent<AE_GroupBloom>();
                groupBloom.m_groupBloomMaterial = mat;
                GameObject _go = Instantiate(currentObj, Vector3.zero, Quaternion.identity) as GameObject;

                for (int i = 0; i < dissolveObjectList.Length; i++)
                {
                    int lastLayer = dissolveObjectList[i].layer;
                    dissolveObjectList[i].layer = LayerMask.NameToLayer("Dislove");
                    SceneDissolveControl sceneDissolve = dissolveObjectList[i].AddComponent<SceneDissolveControl>();
                    sceneDissolve.MaxDistance = 35;
                    sceneDissolve.disolveSpeed = 4;
                    sceneDissolve._dissolvePoint = dissolvePoint;
                    sceneDissolve._intensity = intensity;
                    sceneDissolve._noiseFreq = noiseFreq;
                    sceneDissolve._border = border;
                    sceneDissolve._borderColor = borderColor;
                    sceneDissolve._borderEmission = borderEmission;
                    sceneDissolve._inverse = inverse;
                    sceneDissolve._lastLayer = lastLayer;
                    sceneDissolve.rendererCam = _go;
                }  
            }                      
        }
    }
}
