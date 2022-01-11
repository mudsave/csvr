using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class CDynamicBirthPointTrigger : CTriggerBase
{
    public override JsonData WriteJson()
    {
        JsonData jsonData = new JsonData();
        jsonData["type"] = this.GetType().FullName;
        jsonData["dynamic"] = new JsonData();
        jsonData["dynamic"].SetJsonType(JsonType.Array);
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform tc = transform.GetChild(i);
            if (tc.gameObject.activeSelf)
            {
                JsonData data = tc.GetComponent<CTriggerBase>().WriteJson();
                jsonData["dynamic"].Add(data);
            }
        }
        return jsonData;
    }
}