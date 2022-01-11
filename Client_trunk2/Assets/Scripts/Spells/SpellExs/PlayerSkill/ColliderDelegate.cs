using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderDelegate : MonoBehaviour
{
    public delegate void ColliderEventHandler(Collider collider);

    public event ColliderEventHandler TriggerEnterEvent;
    public event ColliderEventHandler TriggerStayEvent;
    public event ColliderEventHandler TriggerExitEvent;

    private void OnTriggerEnter(Collider collider)
    {
        if (TriggerEnterEvent != null)
        {
            TriggerEnterEvent(collider);
        }
    }

    private void OnTriggerStay(Collider collider)
    {
        if (TriggerStayEvent != null)
        {
            TriggerStayEvent(collider);
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (TriggerExitEvent != null)
        {
            TriggerExitEvent(collider);
        }
    }
}
