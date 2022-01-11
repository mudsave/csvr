using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class CEntityGroupRandom: CTriggerBase
{

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
