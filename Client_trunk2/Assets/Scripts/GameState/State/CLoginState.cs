using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CLoginState : CBaseState
{
    public override void Enter()
    {
        GlobalEvent.register("onLoginSuccessfully", this, "onLoginSuccessfully");

        CLoading.instance.ShowUI();

        CGameObject.instance.StartCoroutine(LoadSceneAsync("001_suo_ling_ta_01"));    //切换到Login场景
    }

    public IEnumerator LoadSceneAsync(string scene)
    {
        /*
        AsyncOperation m_sceneLoader = SceneManager.LoadSceneAsync(scene);

        //场景加载完成
        while (!m_sceneLoader.isDone)
        {
            yield return new WaitForFixedUpdate();
        }
        */
        yield return 0;
        //开始初始化登录UI界面
        //<todo:tangcaoyuan>
        CLoading.instance.HideUI();
        CLoginScene.instance.ShowUI();
        GlobalEvent.fire("CLoginState_Enter");
        //模拟登录<todo:yelei> 后面要移到界面层
        //Startup.instance.Login(SystemInfo.deviceUniqueIdentifier, "test1");
    }

    public override void Leave()
    {
        GlobalEvent.deregister(this);
        CLoginScene.instance.HideUI();
        GlobalEvent.fire("CLoginState_Leave");
    }

    public void onLoginSuccessfully(UInt64 rndUUID, Int32 eid)
    {
        CGameState.Instance().ChangeState(GameStateEnum.LoadingSceneState);
    }
}