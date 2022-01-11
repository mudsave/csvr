using UnityEngine;
using System.Collections;
using LitJson;

public class CNPCPathGroup : CNPCPathBase
{
    public CNPCPathPoint begin;
    public Color lineColor = Color.red;
    public Color pointColor = Color.yellow;
    public float pointRadius = 0.2f;

    private CNPCPathPoint end;
    private int pointID = 0;

    public override JsonData WriteJson()
    {
        JsonData jsonData = new JsonData();
        jsonData["begin"] = begin.id;

        JsonData pathData = new JsonData();
        jsonData["pathGroup"] = pathData;

        for (int i = 0; i < transform.childCount; i++)
        {
            CNPCPathPoint child = transform.GetChild(i).GetComponent<CNPCPathPoint>();

            if (child == null)
                continue;
            pathData[child.id.ToString()] = child.WriteJson();
        }

        return jsonData;
    }

    public override void SetNPCPath(Transform parent, Vector3 position)
    {
        CNPCPathPoint npcPathPoint = TraversePoint(parent, position);
        if (npcPathPoint == null)
        {
            pointID++;
            GameObject obj = new GameObject();
            obj.name = pointID.ToString();
            obj.transform.parent = parent;
            obj.transform.position = position;

            npcPathPoint = obj.AddComponent<CNPCPathPoint>();
            npcPathPoint.id = pointID;
        }

        if (begin == null)
            begin = npcPathPoint;

        if (end != null)
            end.next = npcPathPoint;
        end = npcPathPoint;
    }

    private CNPCPathPoint GetEndPoint(CNPCPathPoint next)
    {
        CNPCPathPoint npcPathPoint = next;

        while (npcPathPoint != null)
        {
            if (npcPathPoint.next == null || npcPathPoint.next == npcPathPoint || npcPathPoint.next == next)
                return npcPathPoint;
            npcPathPoint = npcPathPoint.next;
        }

        return npcPathPoint;
    }

    //遍历查看是否选中已经存在的路径点
    private CNPCPathPoint TraversePoint(Transform parent, Vector3 clickPosition)
    {
        CNPCPathPoint npcPathPoint = null;

        for (int i = 0; i < parent.childCount; i++)
        {
            if (Vector3.Distance(parent.GetChild(i).position, clickPosition) < pointRadius)
            {
                npcPathPoint = parent.GetChild(i).GetComponent<CNPCPathPoint>();
                break;
            }
        }

        return npcPathPoint;
    }

    void OnDrawGizmos()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            CNPCPathPoint npcPathPoint = transform.GetChild(i).GetComponent<CNPCPathPoint>();
            if (npcPathPoint == null || npcPathPoint.next == null)
                continue;

            Gizmos.color = lineColor;
            Gizmos.DrawLine(npcPathPoint.transform.position, npcPathPoint.next.transform.position);
        }
    }

}
