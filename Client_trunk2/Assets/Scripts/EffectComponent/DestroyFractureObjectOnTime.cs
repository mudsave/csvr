using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyFractureObjectOnTime : MonoBehaviour {

    public float time = 0.5f;

    void OnEnable()
    {
        StartCoroutine(Shoot());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator Shoot()
    {
        float currentTime = 0.0f;
        while (currentTime < time)
        {
            currentTime += Time.deltaTime;
            yield return 0;
        }

        //Recycle this pooled bullet instance
        gameObject.Recycle();
    }
}
