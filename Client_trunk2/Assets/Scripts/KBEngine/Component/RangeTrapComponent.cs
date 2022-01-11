using UnityEngine;
using System.Collections;

public class RangeTrapComponent : GameObjComponent
{


    public override void OnShow()
    {
        base.OnShow();
        gameObject.SetActive(true);
    }

    public override void OnHide()
    {
        base.OnHide();
        gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider collision)
    {
    }

    void OnTriggerExit(Collider collision)
    {
    }

}
