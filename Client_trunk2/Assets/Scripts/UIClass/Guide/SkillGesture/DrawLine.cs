using UnityEngine;

/// <summary>
/// 画线
/// </summary>
public class DrawLine : MonoBehaviour
{
    public Transform startNode;
    public Transform endNode;

    private LineRenderer lineRenderer;

    // Use this for initialization
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.SetVertexCount(2);
        lineRenderer.SetPosition(0, startNode.position);
        lineRenderer.SetPosition(1, endNode.position);
    }
}