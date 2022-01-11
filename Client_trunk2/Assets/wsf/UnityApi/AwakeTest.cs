using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AwakeTest : MonoBehaviour {

    void Awake()
    {
        Debug.Log("AwakeTest::Awake.." + this.gameObject.name);
    }

	// Use this for initialization
	void Start () {
        Debug.Log("AwakeTest::Start.." + this.gameObject.name);
	}

    void OnEnable()
    {
        Debug.Log("AwakeTest::OnEnable.." + this.gameObject.name);
    }

	
	// Update is called once per frame
	void Update () {
		
	}
}
