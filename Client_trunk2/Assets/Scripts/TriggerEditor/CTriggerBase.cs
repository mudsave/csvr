using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class CTriggerBase : MonoBehaviour {

    public static string sceneName = "";

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

    public void exportChildEntitiyConfig(JsonData datas)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform tc = transform.GetChild(i);
            if (tc.gameObject.activeSelf)
            {

                datas.Add(tc.GetComponent<CTriggerBase>().WriteJson());
            }
        }
    }

}
