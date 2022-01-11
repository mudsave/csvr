using UnityEngine;
using System.Collections;

public class SkillMenuOrigin : MonoBehaviour
{
    private VRInputSkillMenu skillMenu = null;
    private string triggerName;

    public void Init(VRInputSkillMenu skillMenu)
    {
        this.skillMenu = skillMenu;
        if (skillMenu.controllerHand == Hand.RIGHT)
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
            skillMenu.SkillMenuOriginStay();
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.name == triggerName)
            skillMenu.SkillMenuOriginExit();
    }
}