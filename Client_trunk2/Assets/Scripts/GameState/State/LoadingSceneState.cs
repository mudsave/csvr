using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingSceneState : CBaseState
{
    private bool isHavePlayerData = false; //是否已经有玩家数据
    private bool isUILoadComplete = false; //是否UI资源加载完成

    public override void Enter()
    {
        GlobalEvent.register("playerEnterSpace", this, "OnEnterSpace");
        GlobalEvent.register("EVENT_OnSceneLoading", this, "OnEVENT_OnSceneLoading");

        //<todo:tangcaoyuan> 加载loading
        CLoading.instance.ShowUI();
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

        //异步加载UI系统
        CGameObject.instance.StartCoroutine(LoadUIResourceAsync());

        //异步检测是否各个条件是否加载完毕
        CGameObject.instance.StartCoroutine(CheckLoading());
    }

    private IEnumerator OnSceneLoaderProgress(AsyncOperation m_sceneLoader)
    {
        yield break;
    }

    //异步加载UI系统资源
    public IEnumerator LoadUIResourceAsync()
    {
        isUILoadComplete = true;
        yield break;

        ////加载UI主节点
        //GameObject root = ResourceManager.InstantiateAssetBundleResource(CPrefabPaths.SystemWinRoot);

        ////加载错误弹窗
        //yield return ResourceManager.LoadAsyncObject(CPrefabPaths.ErrorTip);
        //CUISystemWinRoot.instance.AddUI(ResourceManager.GetGameObjectFromPath(CPrefabPaths.ErrorTip), false);

        ////加载错误信息提示
        //yield return ResourceManager.LoadAsyncObject(CPrefabPaths.ErrorMessage);
        //CUISystemWinRoot.instance.AddUI(ResourceManager.GetGameObjectFromPath(CPrefabPaths.ErrorMessage), false);

        ////加载手势指引
        //yield return ResourceManager.LoadAsyncObject(CPrefabPaths.GuideSkillGesture);
        //CUISystemWinRoot.instance.AddUI(ResourceManager.GetGameObjectFromPath(CPrefabPaths.GuideSkillGesture), false);

        ////传送门界面系统
        //yield return ResourceManager.LoadAsyncObject(CPrefabPaths.UIGateSystem);
        //CUISystemWinRoot.instance.AddUI(ResourceManager.GetGameObjectFromPath(CPrefabPaths.UIGateSystem), false);

        ////复活界面系统
        //yield return ResourceManager.LoadAsyncObject(CPrefabPaths.Resurrection);
        //CUISystemWinRoot.instance.AddUI(ResourceManager.GetGameObjectFromPath(CPrefabPaths.Resurrection), false);

    }

    //检测加载情况
    public IEnumerator CheckLoading()
    {
        while (!isHavePlayerData || !isUILoadComplete)
        {
            yield return new WaitForFixedUpdate();
        }

        //初始化界面
        var enumerator = CUIBaseWin.winlist.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var element = enumerator.Current;
            element.Key.Initialize();
            if (element.Value)
            {
                element.Key.ShowUI();
            }
            else
            {
                element.Key.HideUI();
            }
        }
			
        //初始化界面
        foreach (KeyValuePair<CUIBaseWin, bool> kv in CUIBaseWin.winlist)
        {
            kv.Key.OnInitialize();
        }

        CGameState.Instance().ChangeState(GameStateEnum.GameSceneState);
    }

    public void OnEnterSpace()
    {
        isHavePlayerData = true;
    }
}