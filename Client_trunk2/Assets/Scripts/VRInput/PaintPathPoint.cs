using System.Collections;
using UnityEngine;

public class PaintPathPoint : MonoBehaviour
{
    public PaintPath path = null;

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.name == "RightHandCollider")
            path.CheckNext(transform);
    }

    private void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.name == "RightHandCollider")
            path.CheckNext(transform);
    }
}