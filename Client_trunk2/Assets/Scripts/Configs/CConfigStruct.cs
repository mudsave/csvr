using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using System.IO;
using System.Reflection;
using System;


public interface IConfigSection
{
    void OnReadConfig();
}

//地图配置基类表
public class CMapConfig : IConfigSection
{
    public string eMetaClass;
    public string dataType;
    public string clientPath;
    public string mappingPath;
    public string name;
    public string music;
    public int canModifyPKMode;
    public int loadOnStart;
    public int maxPlayer;
    public int newLineByPlayerAmount;
    public int maxLine;
    public int initLine;
    public int enterPos;
    public int campIDForPlayerEnter;
    public List<List<int>> campMap;
    public List<int> disableReviveType;
    public int recordOnLeave;
    public int autoClose;
    public int autoCloseDelay;

    public virtual void OnReadConfig()
    {
        CConfigClass.mapConfig[eMetaClass] = this;
    }
}

//公共地图
public class CCommonMapConfig : CMapConfig
{

}

//副本地图
public class CDuplicateMapConfig : CMapConfig
{
    public int loginAction;
    public Dictionary<string, JsonData> enterCondition;
    public Dictionary<string, JsonData> finishCondition;
    public List<JsonData> reward;
}

//错误配置表
public class CErrorConfig : IConfigSection
{
    public int id;
    public string errorDescription;
    public string serverVar;
    public int showType;

    public virtual void OnReadConfig() { }
}

//模型配置表
public class CModelConfig : IConfigSection
{
    public string name;
    public CModelType type;
    public string path;
    public string controller;
    public List<double> up;
    public List<double> boxSize;
    public List<double> boxPosition;
    public string showController;
    public CWeaponNode equipPoint;
    public List<double> offset;
    public List<double> modelScale;
    public List<double> direction; //模型朝向
    public Dictionary<string, string> effectBind;

    public virtual void OnReadConfig()
    {
        CConfigClass.modelConfig[name] = this;

    }
}

//entity配置表
public class CEntityConfig : IConfigSection
{
    public int id;
    public string name;
    public string actName;
    public CEntityType entityType;
    public string modelID;
    public Dictionary<string, object> property;
    public int sceneCampID;
    //public EffectOnDead.DeadEffectType deadType;
    public string icon;

    public virtual void OnReadConfig()
    {
        CConfigClass.entityConfig[id] = this;
    }

    public virtual IEnumerator LoadRes()
    {
        yield return CGameObject.instance.StartCoroutine(ResourceManager.LoadModelResSync(modelID, false));
    }
}

//角色属性配置结构体
public class CHeroConfig : CEntityConfig
{
    public eProfession profession;
    public int place;
    public string professionName;
    public int sex;
    public string weaponID;
    public double moveSpeed;
    public double damageVol;
    public List<int> skilllist;
    public int baseSkill;
    public string describe;

    public override IEnumerator LoadRes()
    {
        yield return CGameObject.instance.StartCoroutine(base.LoadRes());

        yield return CGameObject.instance.StartCoroutine(ResourceManager.LoadModelResSync(weaponID, false));
    }
}

public class CMonsterConfig : CEntityConfig
{
    public int fightID;
    public string weaponID;
    public int ai;
    public string birthActionFX;
    public int dropID;

    public override IEnumerator LoadRes()
    {
        yield return CGameObject.instance.StartCoroutine(base.LoadRes());

        yield return CGameObject.instance.StartCoroutine(ResourceManager.LoadModelResSync(weaponID, false));
    }
}

//功能性NPC
public class CNPCConfig : CEntityConfig
{
    public int fightID;
    public string weaponID;

    public override IEnumerator LoadRes()
    {
        yield return CGameObject.instance.StartCoroutine(base.LoadRes());

        yield return CGameObject.instance.StartCoroutine(ResourceManager.LoadModelResSync(weaponID, false));
    }
}

//触发器entity
public class CTriggerEConfig : CEntityConfig
{
    public Dictionary<string, JsonData> enterEvent;
}

public class CActionEffectConfig : IConfigSection
{
    public string id;
    public double totalTime;
    public Dictionary<string, JsonData> enterEvent;
    public Dictionary<string, Dictionary<string, JsonData>> timeEvent;
    public List<float> timeEventKey;
    public Dictionary<float, Dictionary<string, JsonData>> timeEventInt;
    public Dictionary<string, JsonData> leaveEvent;

    public virtual void OnReadConfig()
    {
        timeEventInt = new Dictionary<float, Dictionary<string, JsonData>>();
        foreach (KeyValuePair<string, Dictionary<string, JsonData>> kv in timeEvent)
        {
            timeEventInt[float.Parse(kv.Key)] = kv.Value;
        }

        if (timeEvent.Keys.Count == 0)
            return;

        float[] keys = new float[timeEventInt.Keys.Count];
        timeEventInt.Keys.CopyTo(keys, 0);
        timeEventKey = new List<float>(keys);
        timeEventKey.Sort();
    }
}

public class CEffectConfig : IConfigSection
{
    public string name;           //特效名称
    public int type;              //特效类型
    public int ruleType;          //特效存放规则类型
    public string path;           //特效路径
    public double lastTime;       //持续时间
    public string alias;          //组名
    public List<JsonData> args;   //参数

    public virtual void OnReadConfig()
    {
    }
}

public class CQuestConfig : IConfigSection
{

    public int id;
    public CQuestType type;
    public string name;
    public Dictionary<string, JsonData> condition;
    public List<List<JsonData>> givenEvent;
    public List<List<JsonData>> target;
    public List<List<JsonData>> reward;
    public List<List<JsonData>> action;

    public virtual void OnReadConfig()
    {
        CConfigClass.questConfig[id] = this;
    }
}


/// <summary>
/// 主线和支线任务配置
/// </summary>
public class CMBQuestConfig : CQuestConfig
{
    public int meetNpc;
    public string meetNpcTalk;
    public int givenNpc;
    public string givenNpcTalk;
}

/// <summary>
/// 日常任务配置
/// </summary>
public class CDayQuestConfig : CQuestConfig
{
    public string subtype; //日常任务子类型
    public string describe;
    public int quality; //任务品质
}


//对话配置表
public class CDialogConfig : IConfigSection
{
    public int ID;          //对话ID
    public List<List<JsonData>> Sound;    //声音
    public JsonData Action;
    public List<int> EntityID;
    public int EntityExist; //是否运行与玩家对话的NPC不存在与游戏中（0、允许 1、不允许）
    public List<List<JsonData>> Msg;      //对话内容
    public List<Dictionary<string, JsonData>> Options;  //选项
    public List<Dictionary<string, JsonData>> Condition; //条件

    public virtual void OnReadConfig() { }
}

//NPC配置
public class CNPCEConfig : CEntityConfig
{
    public int fightID;
    public string weaponID;

    public override IEnumerator LoadRes()
    {
        yield return CGameObject.instance.StartCoroutine(base.LoadRes());

        yield return CGameObject.instance.StartCoroutine(ResourceManager.LoadModelResSync(weaponID, false));
    }
}

//功能性NPC
public class CActNPCEConfig : CEntityConfig
{
    public int fightID;
    public string weaponID;

    public override IEnumerator LoadRes()
    {
        yield return CGameObject.instance.StartCoroutine(base.LoadRes());

        yield return CGameObject.instance.StartCoroutine(ResourceManager.LoadModelResSync(weaponID, false));
    }
}

public class CBuildingDataConfig : IConfigSection
{
    public int id;
    public string type;
    public int listID;
    public List<int> area;
    public string name;
    public string sprite;
    public string description;
    public Dictionary<string, JsonData> buildRes;
    public double buildTime;
    public int isMove;
    public int isDestroy;
    public int buildPlayerLevel;
    public int goldGoin;

    public virtual void OnReadConfig()
    {
    }
}