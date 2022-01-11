using UnityEngine;
using System.Collections;

public class CChangeSceneState : CBaseState
{

    public override void Enter()
    {
        if (KBEngine.World.instance.isShowLoading)
            CLoading.instance.ShowUI();

        GlobalEvent.register("EVENT_OnSceneLoading", this, "OnEVENT_OnSceneLoading");
    }

    public override void Leave()
    {
        CLoading.instance.HideUI();

        GlobalEvent.deregister(this);
    }

    public void OnEVENT_OnSceneLoading(string scene, AsyncOperation m_sceneLoader)
    {
        //异步查看场景加载过程 
        CGameObject.instance.StartCoroutine(OnSceneLoaderProgress(m_sceneLoader));
    }

    private IEnumerator OnSceneLoaderProgress(AsyncOperation m_sceneLoader)
    {
        while (!m_sceneLoader.isDone)
        {
            yield return new WaitForFixedUpdate();
        }

        //检测是否所有资源都已经加载完毕
        while (!KBEngine.World.instance.ScenesIsLoaded())
        {
            yield return new WaitForFixedUpdate();
        }

        CGameState.Instance().ChangeState(GameStateEnum.GameSceneState);

        yield break;
    }

}
