using UnityEngine;
using System.Collections;

public class LineTrack : TrackBace
{
    public float moveSpeed = 0.0f;

    public override void Init(DataSection.DataSection dataSection)
    {
        moveSpeed = dataSection.readFloat("speed");
    }

    public override IEnumerator Operation() 
    {
        while (true)
        {
            gameObject.transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed);
            yield return new WaitForEndOfFrame();
        }
    }
}
