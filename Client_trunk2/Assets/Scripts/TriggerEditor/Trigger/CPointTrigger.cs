using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class CPointTrigger : CTriggerBase
{

    // Use this for initialization
    public override void Start()
    {
        base.Start();
    }

    public override JsonData WriteJson()
    {
        JsonData jsonData = new JsonData();
        jsonData["type"] = this.GetType().FullName;

        jsonData["birthPoint"] = new JsonData();
        jsonData["birthPoint"].SetJsonType(JsonType.Array);

        exportChildEntitiyConfig(jsonData["birthPoint"]);

        return jsonData;
    }
}
