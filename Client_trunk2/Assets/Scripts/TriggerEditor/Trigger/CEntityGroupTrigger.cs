using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class CEntityGroupTrigger : CTriggerBase
{
    public int id = 0;
    public bool isAutoStart = true;

    public override JsonData WriteJson()
    {
        JsonData jsonData = new JsonData();
        jsonData["type"] = this.GetType().FullName;
        jsonData["id"] = id;
        jsonData["isAutoStart"] = isAutoStart;
        jsonData["list"] = new JsonData();
        jsonData["list"].SetJsonType(JsonType.Array);

        int num = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform tc = transform.GetChild(i);
            if (tc.gameObject.activeSelf)
            {
                JsonData data = tc.GetComponent<CTriggerBase>().WriteJson();
                data["groupID"] = id + (num++);

                jsonData["list"].Add(data);
            }
        }

        return jsonData;
    }
}
