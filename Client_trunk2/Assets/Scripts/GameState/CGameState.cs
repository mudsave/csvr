using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GameStateEnum
{
	Null, //空状态
    Update, //更新状态
    LoadingLoginState,  //加载登录状态
	Login, //登录状态
    LoadingSceneState, //初次登录场景状态 
    GameSceneState, //游戏场景状态
    ChangeSceneState, //切换场景状态
	Quit, //退出状态 
};

public class CGameState  {
	private static CGameState m_inst = null;
	private Dictionary<GameStateEnum , CBaseState> m_stateMap = new Dictionary<GameStateEnum , CBaseState>();
	private GameStateEnum m_currentState = GameStateEnum.Null;

	public GameStateEnum currentState {
		get {
			return m_currentState;
		}
	}

	//单例
	public static CGameState Instance()
	{
		if (m_inst == null)
		{
			m_inst = new CGameState();
			m_inst.Initialization();
		}

		return m_inst;
	}

	//初始化
	void Initialization()
	{
        m_stateMap[GameStateEnum.Update] = new CUpdateState();
        m_stateMap[GameStateEnum.LoadingLoginState] = new CLoadingLoginState();
		m_stateMap[GameStateEnum.Login] = new CLoginState();
        m_stateMap[GameStateEnum.LoadingSceneState] = new LoadingSceneState();
        m_stateMap[GameStateEnum.GameSceneState] = new CGameSceneState();
        m_stateMap[GameStateEnum.ChangeSceneState] = new CChangeSceneState();
		m_stateMap[GameStateEnum.Quit] = new CQuitState();
	}

    public void Init()
    { }

	public void ChangeState(GameStateEnum state)
	{
		//相同状态，无需切换
        //if (m_currentState == state)
        //    return;

		if (m_currentState != GameStateEnum.Null)
			m_stateMap[m_currentState].Leave();

		m_currentState = state;

		m_stateMap[m_currentState].Enter();
	}

}
