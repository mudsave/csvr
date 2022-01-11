using UnityEngine;
using System.Collections;

[ExecuteInEditMode]

public class WarpEffect : MonoBehaviour {

	// Update is called once per frame
	void Update () {
        Shader.SetGlobalMatrix("vTransform", transform.localToWorldMatrix);
        Shader.SetGlobalMatrix("vInvTransform", transform.localToWorldMatrix.inverse);
	}
}
