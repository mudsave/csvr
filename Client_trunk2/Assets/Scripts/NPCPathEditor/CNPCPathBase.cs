using UnityEngine;
using System.Collections;
using LitJson;

public class CNPCPathBase : MonoBehaviour
{
    //public static string sceneName = "";

    public virtual void Start()
    {
        Destroy(this.gameObject);
    }

    public virtual JsonData WriteJson()
    {
        return new JsonData();
    }

    public virtual void ReadJson(JsonData data)
    {
    }

    public virtual void SetNPCPath(Transform parent, Vector3 position)
    {

    }

}
