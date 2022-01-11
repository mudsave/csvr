using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUIRoot : MonoBehaviour 
{
    private static PlayerUIRoot m_instance;

    public static PlayerUIRoot Instance
    {
        get
        {
            if(m_instance == null)
                Debug.LogError("PlayerUIRoot::Instance:uninitialized.");
            return PlayerUIRoot.m_instance;
        }
    }

    void Awake()
    {
        if(m_instance == null)
        {
            m_instance = this;
            Init();
        }
        else if(m_instance != this)
        {
            Destroy(this.gameObject);
            Debug.LogError("PlayerUIRoot::Awake:duplicate awake.");
        }
    }

    public void Init()
    {
        GlobalEvent.register("GameSceneState_Enter", this, "ShowPlayerStatus");
        GlobalEvent.register("GameSceneState_Leave", this, "ClosePlayerStatus");
    }

    public void ShowPlayerStatus()
    {
        UIManager.Instance.OpenUI(CPrefabPaths.PlayerStatus, true, transform);
    }

    public void ClosePlayerStatus()
    {
        UIManager.Instance.CloseUI(CPrefabPaths.PlayerStatus);
    }

    void OnDestroy()
    {
        GlobalEvent.deregister(this);
    }
}
