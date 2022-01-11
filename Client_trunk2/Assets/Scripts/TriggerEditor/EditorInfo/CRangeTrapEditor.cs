using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

[RequireComponent(typeof(BoxCollider))]
public class CRangeTrapEditor : CEntityEditor
{

    public override JsonData WriteJson()
    {
        JsonData datas = base.WriteJson();

        BoxCollider collider = GetComponent<BoxCollider>();

        datas["triggerSize"] = new JsonData();
        {
            datas["triggerSize"].Add((double)collider.size.x);
            datas["triggerSize"].Add((double)collider.size.y);
            datas["triggerSize"].Add((double)collider.size.z);
        }
        datas["triggerCenter"] = new JsonData();
        {
            datas["triggerCenter"].Add((double)collider.center.x);
            datas["triggerCenter"].Add((double)collider.center.y);
            datas["triggerCenter"].Add((double)collider.center.z);
        }
        return datas;
    }
}
