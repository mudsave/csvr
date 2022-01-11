using UnityEngine;
using System.Collections;
public class AE_GroupBloom : MonoBehaviour
{
    public Material m_groupBloomMaterial;
    public LayerMask dissloveLayers = 1 << 17;
    private Camera mCamera;

    void Start()
    {
        mCamera = GetComponent<Camera>();
        if (mCamera)
        {
            mCamera.cullingMask = mCamera.cullingMask & (~(dissloveLayers));
        }
    }

    void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
    {
        //Copies source texture into destination render texture with a shader.
        Graphics.Blit(sourceTexture, destTexture, m_groupBloomMaterial);
    }
}