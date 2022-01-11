using UnityEngine;
using System.Collections;

public class CLoadingLoginState : CBaseState
{
    private bool isUILoadComplete;

    public override void Enter()
    {
        LoadResources();
    }

    public override void Leave()
    {
        
    }

    private void LoadResources()
    {
        //异步加载UI系统
        CGameObject.instance.StartCoroutine(LoadUIResourceAsync());

        //异步检测是否各个条件是否加载完毕
        CGameObject.instance.StartCoroutine(CheckLoading());
    }

    private IEnumerator LoadUIResourceAsync()
    {
        //加载主节点
        UIManager.Instance.InstantiateUI(CPrefabPaths.LoadingWinRoot);

        //加载Loading
        yield return UIManager.Instance.LoadUIAsync(CPrefabPaths.Loading, (uiObject) =>
            {
                CUILoadingRoot.instance.AddUI(uiObject, true);
            }
        );

        //加载Login
        yield return UIManager.Instance.LoadUIAsync(CPrefabPaths.Login, (uiObject) =>
            {
                CUILoadingRoot.instance.AddUI(uiObject, false);
            }
        );

        //加载GM
        //yield return ResourceManager.LoadAsyncObject(CPrefabPaths.GM);
        //CUILoadingRoot.instance.AddUI(ResourceManager.GetGameObjectFromPath(CPrefabPaths.GM), false);

		//加载错误提示
        //yield return ResourceManager.LoadAsyncObject(CPrefabPaths.Error);
        //CUILoadingRoot.instance.AddUI(ResourceManager.GetGameObjectFromPath(CPrefabPaths.Error), false);

        // 加载ui
        //yield return UIManager.Instance.AsyncInitResource();

        isUILoadComplete = true;
        yield break;
    }

    private IEnumerator CheckLoading()
    {
        while (!isUILoadComplete)
        {
            yield return new WaitForFixedUpdate();
        }

        CGameState.Instance().ChangeState(GameStateEnum.Login);
        //KBEngine.World.instance.enterWorld();

        yield break;
    }
}
