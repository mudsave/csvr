using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FracturedTrigger : MonoBehaviour {

    public FracturedObject fracturedObject;
    float currentTime;
    bool triggerFlag = false;
    //EventDetachedMaxLifeTime

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("FeiJian"))
        {
            fracturedObject.CollapseChunks();
            triggerFlag = true;
            currentTime = Time.time;
        }   
    }

    void Update()
    {
        if(triggerFlag)
        {
            float fTime = Time.time - currentTime;
            if(fTime > fracturedObject.EventDetachedMaxLifeTime)
            {
                GameObject.Destroy(this.gameObject);
            }
        }
    }
}
