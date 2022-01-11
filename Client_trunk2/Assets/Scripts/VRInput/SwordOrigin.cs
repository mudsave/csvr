using UnityEngine;
using System.Collections;

public class SwordOrigin : MonoBehaviour
{
    private VRInputFlySwordAttack1 flySwordAttack = null;
    private string triggerName;

    public void Init(VRInputFlySwordAttack1 flySwordAttack)
    {
        this.flySwordAttack = flySwordAttack;
        if (flySwordAttack.controllerHand == Hand.RIGHT)
            triggerName = "RightHandCollider";
        else
            triggerName = "LeftHandCollider";
    }

    //void OnTriggerEnter(Collider collider)
    //{
    //}

    void OnTriggerStay(Collider collider)
    {
        if (collider.name == triggerName)
            flySwordAttack.SwordOriginStay();
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.name == triggerName)
            flySwordAttack.SwordOriginExit();
    }
}