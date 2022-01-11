using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using LitJson;

public class MenuTools : MonoBehaviour
{
    [@MenuItem("EditTools/SetTriggerConfig（打开布置怪物的窗口）")]
    static public void SetTriggerConfigPath()
    {
        //创建窗口
        Rect wr = new Rect(0, 0, 700, 150);
        TriggerConfigPathWin window = (TriggerConfigPathWin)EditorWindow.GetWindowWithRect(typeof(TriggerConfigPathWin), wr, false, "触发器配置界面");
    }

    [@MenuItem("EditTools/ChangeEditMode（改变布置怪物的编辑模式）")]
    static public void MapEditorMode()
    {
        if (PlayerPrefs.GetInt("EditMode", 0) == 0)
        {
            Debug.Log("EditorMode");
            PlayerPrefs.SetInt("EditMode", 1);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "MAP_EDITOR");
        }
        else if (PlayerPrefs.GetInt("EditMode") == 1)
        {
            PlayerPrefs.SetInt("EditMode", 0);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "UNMAP_EDITOR");
        }
    }
}

public class EntityChoiceEditor
{
    public List<string> item = new List<string>();
    public int index;

    public TriggerConfigPathWin triggerWin;

    public void show()
    {
        EditorGUILayout.BeginHorizontal();
        index = EditorGUILayout.Popup(index, item.ToArray());

        if (GUILayout.Button("Create"))
            triggerWin.InstantiatePrimitive(item[index]);
        
        EditorGUILayout.EndHorizontal();
    }
}

public class TriggerConfigPathWin : EditorWindow
{
    public int index = 0;
    public List<string> item = new List<string>();

    Dictionary<CEntityType, EntityChoiceEditor> entityTypeList = new Dictionary<CEntityType, EntityChoiceEditor>();
    Dictionary<string, Dictionary<CEntityType, EntityChoiceEditor>> mapEntityTypeList = new Dictionary<string, Dictionary<CEntityType, EntityChoiceEditor>>();

    //绘制窗口时调用
    void OnGUI()
    {

        EditorGUILayout.BeginHorizontal();
        index = EditorGUILayout.Popup(index, item.ToArray());

        //if (item.Count > 0)
        {
            //triggerWin.InstantiatePrimitive(item[index]);
            if (item.Count > index && mapEntityTypeList.ContainsKey(item[index]))
                entityTypeList = mapEntityTypeList[item[index]];
            else
                entityTypeList = new Dictionary<CEntityType, EntityChoiceEditor>();
        }

        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("加载EntityConfig"))
            ReLoadModelConfig();

        foreach (KeyValuePair<CEntityType, EntityChoiceEditor> kv in entityTypeList)
        {
            kv.Value.show();
        }
    }

    public void InstantiatePrimitive(string str)
    {
        string[] strlist = str.Split(new char[] { ':' }, 2);
        int id = System.Int32.Parse(strlist[0]);
        CEntityConfig config = CConfigClass.entityConfig[id];

        GameObject obj = GameObject.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Trigger/Common/sp220_lod0.prefab"));
        obj.name = config.name;
        obj.transform.parent = Selection.activeTransform;
        obj.transform.localPosition = Vector3.zero;
        TextMesh text = obj.transform.GetComponentInChildren<TextMesh>();
        if (text != null)
            text.text = config.name;

        CEntityEditor entityEditor = null;
        switch (config.entityType)
        {
            case CEntityType.Player:
                entityEditor = obj.AddComponent<CEntityEditor>();
                break;

            case CEntityType.RangeTrap:
                entityEditor = obj.AddComponent<CRangeTrapEditor>();
                break;

            case CEntityType.Gear:
                entityEditor = obj.AddComponent<CGearEditor>();
                break;

            default:
                entityEditor = obj.AddComponent<CEntityEditor>();
                break;
        }

        if (entityEditor != null)
        {
            entityEditor.entityID = config.id;
            entityEditor.entityName = config.name;
        }

    }

    void ReLoadModelConfig()
    {
        CConfigClass.entityConfig.Clear();
        entityTypeList.Clear();

        CConfigClass.monsterConfig = CConfigClass.JsonToDictionaryInt<CMonsterConfig>("MonsterConfig.json");
        CConfigClass.npcConfig = CConfigClass.JsonToDictionaryInt<CNPCConfig>("NPCConfig.json");
        CConfigClass.triggerEConfig = CConfigClass.JsonToDictionaryInt<CTriggerEConfig>("TriggerEConfig.json");

        foreach (KeyValuePair<int, CEntityConfig> kv in CConfigClass.entityConfig)
        {
            string mapID = kv.Key.ToString().Substring(1,3);

            if (!mapEntityTypeList.ContainsKey(mapID))
            {
                mapEntityTypeList[mapID] = new Dictionary<CEntityType, EntityChoiceEditor>();
            }

            Dictionary<CEntityType, EntityChoiceEditor> value = mapEntityTypeList[mapID];

            if (!value.ContainsKey(kv.Value.entityType))
            {
                value[kv.Value.entityType] = new EntityChoiceEditor();
                value[kv.Value.entityType].triggerWin = this;
            }

            value[kv.Value.entityType].item.Add(kv.Key.ToString() + ":" + kv.Value.name);
        }

        foreach (KeyValuePair<string, Dictionary<CEntityType, EntityChoiceEditor>> sv in mapEntityTypeList)
        {
            foreach (KeyValuePair<CEntityType, EntityChoiceEditor> kv in sv.Value)
            {
                kv.Value.item.Sort();
            }
        }

        string[] keys = new string[mapEntityTypeList.Keys.Count];
		mapEntityTypeList.Keys.CopyTo(keys, 0);
        item = new List<string>(keys);
        item.Sort();

        index = 0;
        if (mapEntityTypeList.Count > 0)
            entityTypeList = mapEntityTypeList[item[index]];
        else 
            entityTypeList = new Dictionary<CEntityType,EntityChoiceEditor>();

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    }

}
