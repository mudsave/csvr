using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class TrackBace : MonoBehaviour
{
    private static Dictionary<int, System.Type> s_trackMap = new Dictionary<int, System.Type>()
	{
		{1, typeof(LineTrack)},       //直线轨迹
	};

    public static System.Type GetTrackType(int type)
    {
        if (!s_trackMap.ContainsKey(type))
            return null;

        return s_trackMap[type];
    }

    public virtual void Init(DataSection.DataSection dataSection)
    {

    }

    public void StartOperation()
    {

        StartCoroutine(Operation());
    }

    public void EndOperation() 
    {
        StopAllCoroutines();
    }

    public virtual IEnumerator Operation() 
    {
        yield return new WaitForEndOfFrame();
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

}
