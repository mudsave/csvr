using UnityEngine;
using System.Collections;

public class CUpdateState : CBaseState
{
    public override void Enter()
    {
        if (AssetBundleManager.SimulateAssetBundleInEditor == AssetBundleManager.LoadMode.AssetBundleName)
        {
            CGameObject.instance.resupdate.LoadRes(OnUpdateComplete);
        }

        else
        {
            CGameObject.instance.StartCoroutine(Execute());
        }
       
    }

    public IEnumerator Execute()
    {
        //获取AB的资源配置初始化
        if (AssetBundleManager.SimulateAssetBundleInEditor == AssetBundleManager.LoadMode.AssetBundleName)
        yield return CGameObject.instance.loader.Initialize();

        //对象池初始化
        yield return CGameObject.instance.Init();

        //加载lua
        //CLuaEnvSingleton.Instance.StartLoadLua();

        //Object.Instantiate(ResourceManager.LoadAssetBundleResource("effectManager"));

        //yield return ResourceManager.LoadAsyncObject("effectManager");
        //ResourceManager.InstantiateAssetBundleResource("effectManager");

        //加载基本必要的配置
        yield return CGameObject.instance.StartCoroutine(CConfigClass.InitBeforeLogin());

        //<todo:yelei，先临时放在这里>
        yield return CGameObject.instance.StartCoroutine(CConfigClass.InitAfterLogin());

        //改变状态
        CGameState.Instance().ChangeState(GameStateEnum.LoadingLoginState);
    }

    /// <summary>
    /// 更新完成
    /// </summary>
    public void OnUpdateComplete()
    {
        CGameObject.instance.StartCoroutine(Execute()); 
    }

    public override void Leave()
    {
    }
}
