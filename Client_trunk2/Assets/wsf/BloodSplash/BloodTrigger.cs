using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TestTrigger : MonoBehaviour 
{
    void OnTriggerStay(Collider collider)
    {
        AvatarComponent component = collider.GetComponent<AvatarComponent>();
		if (component == null)
			return;

        if(component.HP > component.m_maxHP*0.1)
			component.HP = Convert.ToInt32( component.HP * (1 - 0.009) );
    }
}
