using System.Collections;
using UnityEngine;

public class SkillMenuItem : MonoBehaviour
{
    public delegate void OnTriggerEnterHandle(SkillMenuItem item);

    public int skillID = 0;
    public Color highlightColor = Color.yellow;
    private Hand controllerHand = Hand.LEFT;
    private string triggerName;

    public bool colliderEnable
    {
        get { if (collider != null)return collider.enabled; else return false; }
        set { if (collider != null)collider.enabled = value; }
    }

    public event OnTriggerEnterHandle onTriggerEnter;

    private BoxCollider collider = null;

    private void Start()
    {
        //collider = GetComponent<BoxCollider>();
        //collider.enabled = false;
        if (controllerHand == Hand.RIGHT)
            triggerName = "RightHandCollider";
        else
            triggerName = "LeftHandCollider";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == triggerName)
        {
            if (onTriggerEnter != null)
            {
                onTriggerEnter(this);
            }
        }
    }
}