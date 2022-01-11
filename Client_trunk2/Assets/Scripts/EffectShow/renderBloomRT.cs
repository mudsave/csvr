using UnityEngine;
using System.Collections;
public class renderBloomRT : MonoBehaviour
{
    public Camera m_mainCameraRef;
    public RenderTexture m_rt;

    // Use this for initialization
    void Start()
    {
        m_mainCameraRef = Camera.main;
        m_rt.width = Screen.width;
        m_rt.height = Screen.height;
        synchronizePosAndRotWithMainCamera();
        synchronizeProjModeAndFrustomWithMainCamera();
    }
    void synchronizePosAndRotWithMainCamera()
    {
        transform.SetParent(m_mainCameraRef.transform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
    void synchronizeProjModeAndFrustomWithMainCamera()
    {
        GetComponent<Camera>().orthographic = m_mainCameraRef.orthographic;
        GetComponent<Camera>().orthographicSize = m_mainCameraRef.orthographicSize;
        GetComponent<Camera>().nearClipPlane = m_mainCameraRef.nearClipPlane;
        GetComponent<Camera>().farClipPlane = m_mainCameraRef.farClipPlane;
        GetComponent<Camera>().fieldOfView = m_mainCameraRef.fieldOfView;
        GetComponent<Camera>().renderingPath = m_mainCameraRef.renderingPath;
    }
}