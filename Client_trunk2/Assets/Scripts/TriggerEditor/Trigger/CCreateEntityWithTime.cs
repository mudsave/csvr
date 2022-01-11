using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class CCreateEntityWithTime : CTriggerBase
{

    public float interval = 0;
    public int times = -1;

    // Use this for initialization
    public override void Start()
    {
        base.Start();
    }

    public override JsonData WriteJson()
    {
        JsonData datas = new JsonData();

        datas["type"] = this.GetType().FullName;
        datas["interval"] = interval;
        datas["times"] = times;
        datas["birthPoint"] = new JsonData();
        datas["birthPoint"].SetJsonType(JsonType.Array);

        exportChildEntitiyConfig(datas["birthPoint"]);

        return datas;
    }
}
