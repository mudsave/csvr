using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class CGameObject : MonoBehaviour {

    private static CGameObject m_inst = null;

    bool firstload = true;
    public ResourceUpdate resupdate;
    public ResourceLoader loader;

    #region vr contraller
    public bool isConnected
    {
        get
        {
            return Immerseum.VRSimulator.HMDSimulator.isHMDConnected;
        }
    }
    #endregion vr contraller

    [HideInInspector]
    public bool isCloseUnity = false;

    //游戏的FPS，可在属性窗口中修改
    public int targetFrameRate = 0;

    private CTimerManager m_timerManager = null;

    public static CGameObject instance
    {
        get { return m_inst; }
    }

    public CTimerManager TimerManager
    {
        get { return m_timerManager; }
    }

    void Awake()
    {
      
        //修改当前的FPS
        if (targetFrameRate != 0)
            Application.targetFrameRate = targetFrameRate;

        if (!m_inst)
        {
            m_inst = this;
            m_timerManager = new CTimerManager();
        }
        else
        {
            Destroy(this.gameObject);
        }
      
    }

  

    public IEnumerator  Init()
    {
        GameObject poolManager = Instantiate(Resources.Load("UI/Extra/PoolManager")) as GameObject;
        poolManager.transform.parent = transform;
        poolManager.transform.localPosition = Vector3.zero;
        poolManager.transform.rotation = Quaternion.identity;
        poolManager.transform.localScale = Vector3.one;
        DontDestroyOnLoad(gameObject);
        yield return null;

    }
    void Update()
    {
        if (m_timerManager != null)
            m_timerManager.Update();

    }

    void OnApplicationQuit()
    {
        isCloseUnity = true;
    }

   
}
