using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class CGateEditor : CEntityEditor
{
    public int teleportEntityID = 0;
    public float triggerRadius = 1.0f;

    public override JsonData WriteJson()
    {
        JsonData datas = base.WriteJson();

        datas["triggerRadius"] = triggerRadius;
        datas["teleportID"] = teleportEntityID;

        return datas;
    }

}
