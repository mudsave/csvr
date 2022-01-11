using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using System.IO;
using System.Reflection;
using System;

public partial  class CConfigClass
{

    private static bool m_initializedBefore = false;
    private static bool m_initializedAfter = false;

    public static bool initializedBefore
    {
        get { return m_initializedBefore; }
    }

    public static bool initializedAfter
    {
        get { return m_initializedAfter; }
    }

    //地图配置
    public static Dictionary<string, CMapConfig> mapConfig = new Dictionary<string, CMapConfig>();

    //公共地图配置
    public static Dictionary<string, CCommonMapConfig> commonMapConfig = new Dictionary<string, CCommonMapConfig>();

    //副本地图配置
    public static Dictionary<string, CDuplicateMapConfig> duplicateMapConfig = new Dictionary<string, CDuplicateMapConfig>();

    //角色基本属性配置
    public static Dictionary<int, CHeroConfig> heroConfig = new Dictionary<int, CHeroConfig>(); 

    //提示和错误信息配置表
    public static Dictionary<int, CErrorConfig> messageConfig = new Dictionary<int, CErrorConfig>();

    //模型配置表
    public static Dictionary<string, CModelConfig> modelConfig = new Dictionary<string, CModelConfig>();

    public static Dictionary<string, CModelConfig> heroModelConfig = new Dictionary<string, CModelConfig>();

    //entity配置表
    public static Dictionary<int, CEntityConfig> entityConfig = new Dictionary<int, CEntityConfig>();

    //怪物基本属性配置
    public static Dictionary<int, CMonsterConfig> monsterConfig = new Dictionary<int, CMonsterConfig>();

    //NPC配置
    public static Dictionary<int, CNPCConfig> npcConfig = new Dictionary<int, CNPCConfig>();

    //触发器entity
    public static Dictionary<int, CTriggerEConfig> triggerEConfig = new Dictionary<int, CTriggerEConfig>();

    //效果配置
    public static Dictionary<string, CActionEffectConfig> actionEffectConfig = new Dictionary<string, CActionEffectConfig>();

    //特效配置
    public static Dictionary<string, CEffectConfig> effectConfig = new Dictionary<string, CEffectConfig>();
   
    //任务配置表
    public static Dictionary<int, CQuestConfig> questConfig = new Dictionary<int, CQuestConfig>();

    //NPC配置
    public static Dictionary<int, CNPCEConfig> npcEConfig = new Dictionary<int, CNPCEConfig>();

    //功能性NPC
    public static Dictionary<int, CActNPCEConfig> actNPCEConfig = new Dictionary<int, CActNPCEConfig>();

    //对话配置表
    public static Dictionary<int, CDialogConfig> dialogConfig = new Dictionary<int, CDialogConfig>();

    public static IEnumerator InitBeforeLogin()
    {
        if (m_initializedBefore)
        {
            yield break;
        }
 
        CGameObject obj = CGameObject.instance;
    
        //yield return obj.StartCoroutine(JsonToDictionaryInt<CErrorConfig>("MessageConfig.json", messageConfig));
        //yield return obj.StartCoroutine(JsonToDictionaryString<CModelConfig>("ModelConfig.json", modelConfig));
        //yield return obj.StartCoroutine(JsonToDictionaryString<CModelConfig>("HeroModelConfig.json", heroModelConfig));
        //yield return obj.StartCoroutine(JsonToDictionaryString<CActionEffectConfig>("ActionEffectConfig.json", actionEffectConfig));
        yield return obj.StartCoroutine(JsonToDictionaryString<CEffectConfig>("EffectConfig.json", effectConfig));
        m_initializedBefore = true;
    }

    static void ClearConfig()
    { 

    }

    /// <summary>
    /// 加载固定的对象池
    /// </summary>
    public static void LoadPools()
    {
        //玩家头顶信息资源
        ResourceManager.IncreaseAddPool(CPrefabPaths.MonsterHurtMessage, "UIPool", 10, typeof(CPoolTag));
    }

    /// <summary>
    /// 加载固定的资源
    /// </summary>
    /// <returns></returns>
    public static IEnumerator LoadModelRes()
    {
        var enumerator = heroModelConfig.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var element = enumerator.Current;
            yield return CGameObject.instance.StartCoroutine(ResourceManager.LoadModelResSync(element.Key, true));
        }

        yield break;
    }

    //login状态后加载配置
    public static IEnumerator InitAfterLogin()
    {
        if (m_initializedAfter)
        {
            yield break;
        }

        CGameObject obj = CGameObject.instance;

        //yield return obj.StartCoroutine(JsonToDictionaryString<CCommonMapConfig>("CommonMapConfig.json", commonMapConfig));
        //yield return obj.StartCoroutine(JsonToDictionaryString<CDuplicateMapConfig>("DuplicateMapConfig.json", duplicateMapConfig));
        //yield return obj.StartCoroutine(JsonToDictionaryInt<CMonsterConfig>("MonsterConfig.json", monsterConfig));
        //yield return obj.StartCoroutine(JsonToDictionaryInt<CNPCConfig>("NPCConfig.json", npcConfig));
        //yield return obj.StartCoroutine(JsonToDictionaryInt<CTriggerEConfig>("TriggerEConfig.json", triggerEConfig));
        
   
        //LoadPools();
        //yield return obj.StartCoroutine(LoadModelRes());
        m_initializedAfter = true;

        SpellLoader.instance.init();
    }

    //<todo:yelei>手机临时读取文件代码
    public static IEnumerator JsonToDictionaryString<T>(string path, Dictionary<string, T> m_value) where T : IConfigSection
    {
        string pathconfig = System.IO.Path.Combine(SystemClass.ConfigURL+"/config", path);
        //if (!File.Exists(pathconfig))
        //{
        //    Debug.LogError(string.Format("file not found:{0}",pathconfig));
        //    yield break;
        //}
        WWW www = new WWW(pathconfig);
        //Debug.Log("pathconfig = " + pathconfig);
        yield return www;

        if (www.error != null)
            Debug.Log("error = " + www.error);
        
        Dictionary<string, T> data = new Dictionary<string, T>();
        data = JsonMapper.ToObject<Dictionary<string, T>>(www.text);

        foreach (KeyValuePair<string, T> dic in data)
        {
            m_value[dic.Key] = dic.Value;
            dic.Value.OnReadConfig();          
        }

    }

    //<todo:yelei>手机临时读取文件代码
    public static IEnumerator JsonToDictionaryUint<T>(string path, Dictionary<uint, T> m_value) where T : IConfigSection
    {
        string pathconfig = System.IO.Path.Combine(SystemClass.ConfigURL+"/config", path);
        //if (!File.Exists(pathconfig))
        //{
        //    Debug.LogError(string.Format("file not found:{0}", pathconfig));
        //    yield break;
        //}
        WWW www = new WWW(pathconfig);
        //Debug.Log("pathconfig = " + pathconfig);
        yield return www;
        if (www.error != null)
            Debug.Log("error = " + www.error);

        //Dictionary<int, T> m_value = new Dictionary<int, T>();
        Dictionary<string, T> data = new Dictionary<string, T>();
        
        data = JsonMapper.ToObject<Dictionary<string, T>>(www.text);

        foreach (KeyValuePair<string, T> dic in data)
        {
            m_value[UInt32.Parse(dic.Key)] = dic.Value;
            dic.Value.OnReadConfig();
        }
        www.Dispose();
        yield return null;
    }

    //<todo:yelei>读取json，转化为int字典
    public static IEnumerator JsonToDictionaryInt<T>(string path, Dictionary<int, T> m_value) where T : IConfigSection
    {
        string pathconfig = System.IO.Path.Combine(SystemClass.ConfigURL + "/config", path);
        //if (!File.Exists(pathconfig))
        //{
        //    Debug.LogError(string.Format("file not found:{0}", pathconfig));
        //    yield break;
        //}
        WWW www = new WWW(pathconfig);
        //Debug.Log("pathconfig = " + pathconfig);
        yield return www;
        if (www.error != null)
            Debug.Log("error = " + www.error);

        //Dictionary<int, T> m_value = new Dictionary<int, T>();
        Dictionary<string, T> data = new Dictionary<string, T>();

        data = JsonMapper.ToObject<Dictionary<string, T>>(www.text);

        foreach (KeyValuePair<string, T> dic in data)
        {
            m_value[int.Parse(dic.Key)] = dic.Value;
            dic.Value.OnReadConfig();
        }
        www.Dispose();
        yield return null;
    }

    //<todo:yelei>读取json，转化为int字典
    public static Dictionary<int, T> JsonToDictionaryInt<T>(string path) where T : IConfigSection
    {
        Dictionary<int, T> m_value = new Dictionary<int, T>();
        string pathconfig = System.IO.Path.Combine(SystemClass.ConfigPath, path);
        StreamReader stream = new StreamReader(pathconfig);

        Dictionary<string, T> data = new Dictionary<string, T>();
        data = JsonMapper.ToObject<Dictionary<string, T>>(stream);
        stream.Close();

        foreach (KeyValuePair<string, T> dic in data)
        {
            m_value[int.Parse(dic.Key)] = dic.Value;
            dic.Value.OnReadConfig();
        }

        
        return m_value;
    }

    //<todo:yelei>读取json，转化为uint字典
    public static Dictionary<uint, T> JsonToDictionaryUint<T>(string path) where T : IConfigSection
    {
        Dictionary<UInt32, T> m_value = new Dictionary<UInt32, T>();
        string pathconfig = System.IO.Path.Combine(SystemClass.ConfigPath, path);
        StreamReader stream = new StreamReader(pathconfig);

        Dictionary<string, T> data = new Dictionary<string, T>();
        data = JsonMapper.ToObject<Dictionary<string, T>>(stream);
        stream.Close();

        foreach (KeyValuePair<string, T> dic in data)
        {
            m_value[UInt32.Parse(dic.Key)] = dic.Value;
            dic.Value.OnReadConfig();
        }

        return m_value;
    }

    //<todo:yelei>读取json，转化为string字典
    public static Dictionary<string, T> JsonToDictionaryString<T>(string path) where T : IConfigSection
    {
        Dictionary<string, T> m_value = new Dictionary<string, T>();
        string pathconfig = System.IO.Path.Combine(SystemClass.ConfigPath, path);
        StreamReader stream = new StreamReader(pathconfig);

        m_value = JsonMapper.ToObject<Dictionary<string, T>>(stream);
        stream.Close();

        foreach (KeyValuePair<string, T> dic in m_value)
        {
            dic.Value.OnReadConfig();
        }

        return m_value;
    }

}
