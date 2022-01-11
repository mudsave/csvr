using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LitJson;

#if MAP_EDITOR
[CustomEditor(typeof(Transform))]
#endif
public class CSceneTriggerEditor : Editor
{

    [SerializeField]
    Transform mTrans;

    Rect sliderRect = new Rect(Screen.width - 150, 30, 30, 200);
    float yProgress = 0;

    void OnSceneGUI()
    {
        mTrans = target as Transform;
        yProgress = mTrans.position.y;
        sliderRect = new Rect(Screen.width - 150, 30, 30, 200);

        Handles.BeginGUI();
        if (GUILayout.Button("导出场景配置", GUILayout.Width(150)))
        {

            //string[] pathlist = EditorApplication.currentScene.Split(new char[] { '/' });
            //string sceneName = pathlist[pathlist.Length - 1].Split(new char[] { '.' })[0];
            //CTriggerBase.sceneName = sceneName;
            CTriggerRoot triggerRoot = Selection.activeTransform.gameObject.GetComponent<CTriggerRoot>();
            CTriggerBase.sceneName = triggerRoot.mapID;
            string sceneName = triggerRoot.mapID;

            string serverconfig = System.IO.Path.Combine(Application.dataPath + "/../../Server_trunk/res/configs/Map", string.Format("{0}Trigger.json", sceneName));
            string clientConfig = System.IO.Path.Combine(Application.dataPath + "/Resources/Configs/Map", string.Format("{0}Trigger.json", sceneName));

            //List<string> pythonDatas = new List<string>();
            JsonData data = new JsonData();

            JsonData childDatas = new JsonData();
            childDatas.SetJsonType(JsonType.Array);

            data[sceneName] = childDatas;

            CalcMonstersCount(Selection.activeTransform, childDatas);

            JsonWriter writer = new JsonWriter();
            writer.PrettyPrint = true;
            data.ToJson(writer);
            string vd = writer.ToString();

            FileStream fs = File.Open(serverconfig, FileMode.Create);
            byte[] info = new UTF8Encoding(true).GetBytes(vd);
            fs.Write(info, 0, info.Length);
            fs.Close();

            FileStream fsclient = File.Open(clientConfig, FileMode.Create);
            byte[] infoclient = new UTF8Encoding(true).GetBytes(vd);
            fsclient.Write(infoclient, 0, infoclient.Length);
            fsclient.Close();

            EditorUtility.DisplayDialog("导出完毕！！", "导出完毕！！", "确定", "关闭");
        }
        Handles.EndGUI();

        if (GUI.changed)
            EditorUtility.SetDirty(target);
    }


    private void CalcMonstersCount(Transform t, JsonData datas)
    {
        if (t.childCount == 0)
        {
            return;
        }
       
        for (int i = 0; i < t.childCount; i++)
        {
            Transform tc = t.GetChild(i);
            if (tc.gameObject.activeSelf)
            {
                datas.Add(tc.GetComponent<CTriggerBase>().WriteJson());
            }
        }

    }
}
