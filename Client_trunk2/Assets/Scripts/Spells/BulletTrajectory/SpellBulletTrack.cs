using UnityEngine;
using System.Collections;

public class SpellBulletTrack : MonoBehaviour {

    public float moveSpeed = 10.0f;
    public bool isStart = false;
    public Vector3 dir = Vector3.zero;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        if (isStart)
        {
            gameObject.transform.Translate(dir * Time.deltaTime * moveSpeed, Space.World);
        }
    }
}
