using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCComponent : GameObjComponent
{

    public override void Destroy()
	{
		base.Destroy();
        Destroy(gameObject);
	}

}
