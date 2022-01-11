using LitJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[ExecuteInEditMode]
public class CNPCPathPoint : CNPCPathBase
{
    public int id;
    public CNPCPathPoint next;

    private CNPCPathGroup pathParent;

    void Start()
    {
        pathParent = gameObject.GetComponentInParent<CNPCPathGroup>();
    }

    public override JsonData WriteJson()
    {
        JsonData jsonData = new JsonData();
        jsonData["id"] = id;
        jsonData["position"] = new JsonData();
        {
            jsonData["position"].Add((double)transform.position.x);
            jsonData["position"].Add((double)transform.position.y);
            jsonData["position"].Add((double)transform.position.z);
        }
        jsonData["next"] = -1;
        if (next != null)
        {
            //TODO:ZMZ 暂时不需要方向
            //Vector3 direction = (next.transform.position - transform.position).normalized;
            //jsonData["direction"] = new JsonData();
            //jsonData["direction"].Add((double)0);
            //jsonData["direction"].Add((double)0);
            //jsonData["direction"].Add((double)direction.y / 360 * 6.283185307179586);

            jsonData["next"] = next.id;
        }

        return jsonData;
    }

    void OnDrawGizmos()
    {
        if (pathParent == null)
            return;
        Gizmos.color = pathParent.pointColor;
        Gizmos.DrawWireSphere(transform.position, pathParent.pointRadius);
    }
}
