using UnityEngine;
using System; 
using System.Collections;

using KBEngine;

/// <summary>
/// 游戏启动脚本，所有需要在启动时顺序启动或初始化的工作都应该在这里统一处理
/// </summary>
public class Startup : MonoBehaviour {
	static Startup s_instance = null;
    public static Startup instance
    {
        get { return s_instance; }
    }

    public GameObject KBEprefab;
	public bool autoConnectServer;

	KBEMain m_kbeMain;

    [HideInInspector]
    public bool disConnectToLogin = false;

	void Awake() 
	{
		DontDestroyOnLoad(transform.gameObject);
        if (s_instance)
        {
            Destroy(transform.gameObject);
            return;
        }
        else
        {
            s_instance = this;
        }

        this.gameObject.AddComponent<KBEngine.World>();  // init World
	}
	
	// Use this for initialization
	void Start ()
	{
		if (autoConnectServer)
			StartCoroutine( LoginServer() );
        
        CGameState.Instance().ChangeState(GameStateEnum.Update);
	}

    public void Login(string account, string password, string sceneName = "Scenes/Demo")
    {
        World.instance.enterWorld(sceneName);
    }

    IEnumerator LoginServer(string account, string password)
    {
        while (true)
        {
            if (m_kbeMain.gameapp != null)
            {
				m_kbeMain.gameapp.login(account, password, new byte[0]);
                yield break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void CreateAccount(string account, string password)
    {
        StartCoroutine(CreateAccountServer(account, password));
    }

    IEnumerator CreateAccountServer(string account, string password)
    {
        while (true)
        {
            if (m_kbeMain.gameapp != null)
            {
				m_kbeMain.gameapp.createAccount(account, password, new byte[0]);
                yield break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }


	IEnumerator LoginServer()
	{
		while (true)
		{
			if ( m_kbeMain.gameapp != null )
			{
				m_kbeMain.gameapp.login(SystemInfo.deviceUniqueIdentifier, "test1", new byte[0] );
				yield break;
			}
			yield return new WaitForSeconds( 0.1f );
		}
	}

    /// <summary>
    /// 切换账号和切换角色调用的接口
    /// </summary>
    public void OnClearThing()
    {
    }

	void InstallKBEEvents()
	{
		KBEngine.Event.registerOut("onDisableConnect", this, "onDisableConnect");
		KBEngine.Event.registerOut("onConnectStatus", this, "onConnectStatus");

		// login
		KBEngine.Event.registerOut("onCreateAccountResult", this, "onCreateAccountResult");
		KBEngine.Event.registerOut("onLoginFailed", this, "onLoginFailed");
		KBEngine.Event.registerOut("onVersionNotMatch", this, "onVersionNotMatch");
		KBEngine.Event.registerOut("onScriptVersionNotMatch", this, "onScriptVersionNotMatch");
		KBEngine.Event.registerOut("onLoginGatewayFailed", this, "onLoginGatewayFailed");
        GlobalEvent.register("onLoginSuccessfully", this, "onLoginSuccessfully");
		KBEngine.Event.registerOut("login_baseapp", this, "login_baseapp");
	}

	public void onDisableConnect()
	{
        if (disConnectToLogin)
        {
            disConnectToLogin = false;
            GlobalEvent.fire("loginOff");
        }
        else
        {
            GlobalEvent.fire("onDisableConnect", new object[] { });
        }
	}

	public void onConnectStatus(bool success)
	{
		if(!success)
			Dbg.ERROR_MSG("Startup::onConnectStatus(), connect is error! (连接错误)");
		else
			Dbg.INFO_MSG("Startup::onConnectStatus(), connect successfully, please wait...(连接成功，请等候...)");
	}
	
	public void onCreateAccountResult(UInt16 retcode, byte[] datas)
	{
		if(retcode != 0)
		{
			Dbg.ERROR_MSG("Startup::onCreateAccountResult(), createAccount is error(注册账号错误)! err=" + KBEngineApp.app.serverErr(retcode));
			return;
		}
		
		Dbg.INFO_MSG("Startup::onCreateAccountResult(), createAccount is successfully!(注册账号成功!)");
	}

	public void onLoginFailed(UInt16 failedcode)
	{
		if(failedcode == 20)
		{
			Dbg.ERROR_MSG("Startup::onLoginFailed(), login is failed(登陆失败), err=" + KBEngineApp.app.serverErr(failedcode) + ", " + System.Text.Encoding.ASCII.GetString(KBEngineApp.app.serverdatas()));
		}
		else
		{
			Dbg.ERROR_MSG("Startup::onLoginFailed(), login is failed(登陆失败), err=" + KBEngineApp.app.serverErr(failedcode));
		}
	}

	public void onVersionNotMatch(string verInfo, string serVerInfo)
	{
		Dbg.ERROR_MSG("Startup::onVersionNotMatch(), ver info: " + verInfo + "; ser ver info: " + serVerInfo );
	}

	public void onScriptVersionNotMatch(string verInfo, string serVerInfo)
	{
		Dbg.ERROR_MSG("Startup::onScriptVersionNotMatch(), ver info: " + verInfo + "; ser ver info: " + serVerInfo );
	}

	public void onLoginGatewayFailed(UInt16 failedcode)
	{
		Dbg.ERROR_MSG("Startup::onLoginGatewayFailed(), loginGateway is failed(登陆网关失败), err=" + KBEngineApp.app.serverErr(failedcode));
	}

	public void onLoginSuccessfully(UInt64 rndUUID, Int32 eid)
	{
		Dbg.INFO_MSG("Startup::onLoginSuccessfully(), login is successfully!(登陆成功!)");
	}

	public void login_baseapp()
	{
		Dbg.INFO_MSG("Startup::login_baseapp(), connect to loginGateway, please wait...(连接到网关， 请稍后...)");
	}
}
