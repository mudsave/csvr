using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UIRootType
{
    world,
    player
}

/// <summary>
/// 不完善的ui管理器，目前只需要有显示信息的简单ui，后续如果有复杂ui需求再加强功能
/// </summary>
public class UIManager : MonoBehaviour {

    static private UIManager m_instance;

    static private Dictionary<string, GameObject> m_cacheUI = new Dictionary<string, GameObject>();

    private bool m_isInited = false;
    private bool IsInited
    {
        get { return m_isInited; }
        set { m_isInited = value; }
    }

    public static UIManager Instance
    {
        get 
        {
            if (UIManager.m_instance == null)
            {
                Debug.LogError("UIManager::Instance:uninitialized.");
            }
            return UIManager.m_instance; 
        }
    }

    void Awake()
    {
        if (m_instance == null)
        {
            m_instance = this;
            m_instance.Init();
        }
        else if (m_instance != this)
        {
            Destroy(this.gameObject);
            Debug.LogError("UIManager::Awake:duplicate awake.");
        }
    }

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void Init()
    {
        GlobalEvent.register("CLoginState_Enter", this, "Event_CLoginState_Enter");
        GlobalEvent.register("CLoginState_Leave", this, "Event_CLoginState_Leave");
        GlobalEvent.register("Event_EndFight", this, "Event_EndFight");
        IsInited = true;
    }

    #region Events
    public void Event_EndFight(Vector3 p_position, Vector3 p_direction)
    {
        GameObject fightResult = OpenUI(CPrefabPaths.UIFightResult, true);
        fightResult.transform.position = p_position;
        fightResult.transform.rotation = Quaternion.Euler(p_direction);
    }

    public void CloseFightResultUI()
    {
        CloseUI(CPrefabPaths.UIFightResult);
    }

    public void Event_CLoginState_Enter()
    {
        this.OpenUI(CPrefabPaths.MainMenu, true);
    }

    public void Event_CLoginState_Leave()
    {
        //CloseMainMenu();
    }

    public void CloseMainMenu()
    {
        this.CloseUI(CPrefabPaths.MainMenu);
        this.CloseUI(CPrefabPaths.UILoginModeList);
        this.CloseUI(CPrefabPaths.UILoginPlotList);
    }
    #endregion

    public IEnumerator AsyncInitResource()
    {
        if(IsInited)
            yield break;

        yield return ResourceManager.LoadAsyncObject(CPrefabPaths.PlayerStatus);
        IsInited = true;

        this.OpenUI(CPrefabPaths.PlayerStatus, false);
    }

    public GameObject InstantiateUI(string p_path)
    {
        GameObject uiPrefab = Resources.Load(p_path) as GameObject;
        if(uiPrefab == null)
        {
            Debug.LogError(string.Format("UIManager::InstantiateUI:cant find ui({0}).", p_path));
            return null;
        }

        GameObject uiObject = Instantiate(uiPrefab) as GameObject;

        return uiObject;
    }

    public IEnumerator LoadUIAsync(string p_path, Action<GameObject> p_callback = null)
    {
        ResourceRequest request = Resources.LoadAsync(p_path);
        yield return request;

        UnityEngine.Object uiPrefab = request.asset;
        if (uiPrefab == null)
        {
            Debug.LogError(string.Format("UIManager::InstantiateUI:cant find ui({0}).", p_path));
            yield break;
        }

        GameObject uiObject = Instantiate(uiPrefab) as GameObject;

        if (p_callback != null)
            p_callback(uiObject);
    }

    private GameObject GetUI(string p_uiPath)
    {
        GameObject gameobject = GetUIFromCache(p_uiPath);
        if(gameobject == null)
        {
            gameobject = LoadUI(p_uiPath);
            gameobject.SetActive(false);
        }
        return gameobject;
    }

    private GameObject GetUIFromCache(string p_uiPath)
    {
        GameObject uiObject = null;
        m_cacheUI.TryGetValue(p_uiPath, out uiObject);
        return uiObject;
    }

    private GameObject LoadUI(string p_uiPath)
    {
        GameObject uiObject = null;

        uiObject = GetUIFromCache(p_uiPath);
        if (uiObject != null)
        {
            return uiObject;
        }

        uiObject = this.InstantiateUI(p_uiPath);
        if( uiObject == null)
        {
            Debug.LogError(string.Format("CLoadingLoginState::GetUI:{0} is null.", p_uiPath));
            return null;
        }

        UIManager.m_cacheUI[p_uiPath] = uiObject;
        return uiObject;
    }

    public GameObject OpenUI(string p_uiPath, Vector3 p_position, Vector3 p_direction, bool p_isShow)
    {
        GameObject uiObject = OpenUI(p_uiPath, p_isShow);
        uiObject.transform.position = p_position;
        uiObject.transform.rotation = Quaternion.Euler(p_direction);
        return uiObject;
    }

    public GameObject OpenUI(string p_uiPath, bool p_isShow, Transform p_parent = null)
    {
        Debug.Log(string.Format("UIManager::OpenUI:{0}.", p_uiPath));
        if(!IsInited)
        {
            Debug.LogError(string.Format("UIManager::OpenUI:{0}..But UIManager has not init.", p_uiPath));
            return null;
        }

        GameObject uiObject = GetUI(p_uiPath);

        Transform uiTransform = uiObject.GetComponent<Transform>();

        if (p_parent == null)
            p_parent = transform;
        uiTransform.SetParent(p_parent, false);

        UIWindow uiwin = uiObject.GetComponent<UIWindow>();
        uiwin.Init();
        if (p_isShow)
            uiwin.Open();

        return uiObject;
    }

    public void CloseUI(string p_uiPath)
    {
        if(!m_cacheUI.ContainsKey(p_uiPath))
        {
            Debug.LogError(string.Format("UIManager::CloseUI:can not find ui({0}).", p_uiPath));
            return;
        }

        UIWindow uiWin = GetUI(p_uiPath).GetComponent<UIWindow>();
        uiWin.Close();
    }

    void OnDestroy()
    {
        GlobalEvent.deregister(this);
    }

}
