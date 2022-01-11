using UnityEngine;

public class DrawPointGizmos : MonoBehaviour
{
    public float radius = 0.01f;
    public Color color = Color.green;

    // Use this for initialization
    void Start()
    {
    }

    void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}