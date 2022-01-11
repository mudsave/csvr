using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectModeWin : MainMemuWin
{
    [Header("无限秘境地图")]
    public string m_secretMap;
    public Vector3 secretSpawnPosition = new Vector3(2.5f, 1.0f, 70.0f);

    public void Button_BackMainMenu()
    {
        PlayerSound(m_audioButtonClick);
        UIManager.Instance.CloseUI(CPrefabPaths.UILoginModeList);
        UIManager.Instance.OpenUI(CPrefabPaths.MainMenu, true);
    }

    public void Button_SelectPlot()
    {
        PlayerSound(m_audioButtonClick);
        UIManager.Instance.CloseUI(CPrefabPaths.UILoginModeList);
        UIManager.Instance.OpenUI(CPrefabPaths.UILoginPlotList, true);
    }

    public void Button_SecretPlace()
    {
        PlayerSound(m_audioButtonClick);

        if (!CLoginScene.instance.AlreadyLogin)
            LoginSecretPlace();
        else
            EnterSecretPlace();

        CloseMainMenu();
    }

    public void Button_CraftPlace()
    {
        PlayerSound(m_audioButtonClick);
    }

    // 传送进入无限秘境
    public void EnterSecretPlace()
    {
        VRInputManager.Instance.playerComponent.movementController.StopMove();
        var sceneLoader = SceneManager.LoadSceneAsync(m_secretMap);
        SceneManager.sceneLoaded += OnEnterSeretPlace;
    }

    // 登录进入无限秘境
    public void LoginSecretPlace()
    {
        CLoginScene.instance.OnLoginClick(m_secretMap);
    }

    public void OnEnterSeretPlace(Scene scene, LoadSceneMode model)
    {
        VRInputManager.Instance.playerComponent.navMeshAgent.Warp(secretSpawnPosition);
        VRInputManager.Instance.playerComponent.transform.rotation = Quaternion.Euler(Vector3.zero);
        SceneManager.sceneLoaded -= OnEnterSeretPlace;
    }
}
