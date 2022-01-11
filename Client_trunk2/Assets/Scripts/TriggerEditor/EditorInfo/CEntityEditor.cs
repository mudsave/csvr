using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class CEntityEditor : CTriggerBase
{
    public int entityID = 0;
    public string entityName = "";
    public CNPCPathGroup npcPathGroup;

    public void Start()
    {
    }

    public override JsonData WriteJson()
    {
        JsonData datas = new JsonData();

        datas["entityID"] = entityID;
        datas["position"] = new JsonData();
        {
            datas["position"].Add((double)transform.position.x);
            datas["position"].Add((double)transform.position.y);
            datas["position"].Add((double)transform.position.z);
        }
        datas["rotation"] = new JsonData();
        {
            datas["rotation"].Add((double)0);
            datas["rotation"].Add((double)0);
            datas["rotation"].Add((double)transform.rotation.eulerAngles.y / 360 * 6.283185307179586);
        }
        datas["scene"] = sceneName;

        if (npcPathGroup != null)
            datas["patrolPath"] = npcPathGroup.WriteJson();

        return datas;
    }

    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Gizmos.DrawLine(transform.position, transform.parent.position);

        TextMesh text = transform.GetComponentInChildren<TextMesh>();  
        Camera targetCamera = UnityEditor.SceneView.lastActiveSceneView.camera;
        if (targetCamera != null && text != null)
        {
            text.transform.rotation = targetCamera.transform.rotation;

            float distance = Vector3.Distance(transform.position, targetCamera.transform.position);
            float scale = distance / 8;
            text.transform.localScale = new Vector3(scale, scale, 1);

        }
#endif
    }
}
