using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateDirection : MonoBehaviour
{
    GameObject playerGameObject;


    void Start()
    {
        playerGameObject = VRInputManager.Instance.playerComponent.gameObject;
    }


	// Update is called once per frame
	void Update () {     

        if (playerGameObject)
        {
            Vector3 dir1 = transform.position;
            dir1.y = 0.0f;
            Vector3 dir2 = playerGameObject.transform.position;
            dir2.y = 0.0f;
            Vector3 dir = dir2 - dir1;
            Vector3.Normalize(dir);
            transform.forward = dir;
        }    
	}
}
