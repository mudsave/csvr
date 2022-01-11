using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;


public class SelectPlotWin : MainMemuWin
{
    [Header("新手地图")]
    public string m_easyMap;
    public Vector3 m_easySpawnPoint = new Vector3(-20, 0, 0);

    [Header("锁灵塔地图")]
    public string m_suolingtaMap;
    public Vector3 m_suolingtaSpawnPoint = new Vector3(0f, 1.5f, 0f);
    
    public void Botton_BackModeSelect()
    {
        PlayerSound(m_audioButtonClick);
        UIManager.Instance.CloseUI(CPrefabPaths.UILoginPlotList);
        UIManager.Instance.OpenUI(CPrefabPaths.UILoginModeList, true);
    }

    public void Botton_EnterSuolingta()
    {
        PlayerSound(m_audioButtonClick);

        if (CLoginScene.instance.AlreadyLogin)
            EnterSuolingtaScene();
        else
            LoginSuolingtaScene();

        CloseMainMenu();
    }

    public void Botton_EnterEasySence()
    {
        PlayerSound(m_audioButtonClick);

        if (CLoginScene.instance.AlreadyLogin)
            EnterEasyScene();
        else
            LoginEasyScene();

        CloseMainMenu();
    }

    private void EnterEasyScene()
    {
        VRInputManager.Instance.playerComponent.movementController.StopMove();

        var sceneLoader = SceneManager.LoadSceneAsync(m_easyMap);
        SceneManager.sceneLoaded += OnEnterEasyScene;
    }

    public void OnEnterEasyScene(Scene p_scene, LoadSceneMode p_mode)
    {
        VRInputManager.Instance.playerComponent.navMeshAgent.Warp(m_easySpawnPoint);
        VRInputManager.Instance.playerComponent.transform.rotation = Quaternion.Euler(Vector3.zero);
        SceneManager.sceneLoaded -= OnEnterEasyScene;
    }

    private void LoginEasyScene()
    {
        CLoginScene.instance.OnLoginClick(m_easyMap);
    }

    public void Botton_EnterSnake()
    {
        PlayerSound(m_audioButtonClick);
    }
    
    public void EnterSuolingtaScene()
    {
        VRInputManager.Instance.playerComponent.movementController.StopMove();

        var sceneLoader = SceneManager.LoadSceneAsync(m_suolingtaMap);
        SceneManager.sceneLoaded += OnEnterSoulingta;
    }

    public void OnEnterSoulingta(Scene scene, LoadSceneMode mode)
    {
        VRInputManager.Instance.playerComponent.navMeshAgent.Warp(m_suolingtaSpawnPoint);
        VRInputManager.Instance.playerComponent.transform.rotation = Quaternion.Euler(Vector3.zero);
        SceneManager.sceneLoaded -= OnEnterSoulingta;
    }

    private void LoginSuolingtaScene()
    {
        CLoginScene.instance.OnLoginClick(m_suolingtaMap);
    }
}
