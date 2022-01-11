using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGateSystemWin : CUIBaseWin
{
    private float eachAngle = 0;
    private float radius = 5;
    private Vector3 centerPosition = Vector3.zero;

    //传送门数据
    private List<string> m_gateList = null;

    //传送门物体列表
    private List<GameObject> m_gateObjList = null;

    private void Start()
    {
    }

    /// <summary>
    /// 数据完整后，调用的初始化接口
    /// </summary>
    public override void Initialize()
    {
        GlobalEvent.register("OnButtonOnePressed", this, "OnApplicationMenuPressed");
        GlobalEvent.register("OnGetGateListMsg", this, "OnGetGateListMsg");
        GlobalEvent.register("OnGotoMap", this, "OnGotoMap");

        //GlobalEvent.register
    }

    /// <summary>
    /// 监听菜单键
    /// </summary>
    /// <param name="e"></param>
    public void OnApplicationMenuPressed(VRControllerEventArgs e)
    {
        if (isHide == eUIActivateType.hide)
        {
            KBEngine.KBEngineApp.app.player().cellCall("getGateList");
        }
        else if (isHide == eUIActivateType.show)
        {
            HideUI();
        }
    }

    /// <summary>
    /// 收到传送门数据
    /// </summary>
    /// <param name="data"></param>
    public void OnGetGateListMsg(List<string> data)
    {
        m_gateList = data;
        m_gateObjList = new List<GameObject>();
        for (int i = 0; i < m_gateList.Count; i++)
        {
            Debug.Log(i + ":" + m_gateList[i]);
            m_gateObjList.Add(LoadGateObject(m_gateList[i]));
        }
        //for (int i = 0; i < 1; i++)
        //{
        //    m_gateObjList.Add(LoadGateObject(m_gateList[0]));
        //}
        ResetGateObjectPosition();

        ShowUI();
    }

    /// <summary>
    /// 加载传送门物体
    /// </summary>
    /// <param name="gateID"></param>
    /// <returns></returns>
    private GameObject LoadGateObject(string gateID)
    {
        GateConfig config = null;
        if (ClientConst.gateConfigs.TryGetValue(gateID, out config))
        {
            GameObject prefab = ResourceManager.LoadAssetBundleResource(config.resPath) as GameObject;
            GameObject obj = Instantiate(prefab, transform) as GameObject;

            return obj;
        }
        return null;
    }

    private void Update()
    {
        //ResetGateObjectPosition();
    }

    /// <summary>
    /// 调整传送门位置
    /// </summary>
    private void ResetGateObjectPosition()
    {
        if (m_gateObjList != null && m_gateObjList.Count > 0)
        {
            centerPosition = VRInputManager.Instance.head.transform.position;
            centerPosition.y = VRInputManager.Instance.transform.position.y + 1.3f;
            Vector3 headForward = VRInputManager.Instance.head.transform.forward;
            headForward.y = 0;

            for (int i = 0; i < m_gateObjList.Count; i++)
            {
                //调整位置
                m_gateObjList[i].transform.position = centerPosition + headForward.normalized * 1;
                m_gateObjList[i].GetComponent<UIGateObject>().originPosition = m_gateObjList[i].transform.position;
            }
        }
    }

    /// <summary>
    /// 显示界面回调函数
    /// </summary>
    protected override void OnShowUI(params object[] args)
    {
        Debug.Log("UIGateSystemWin.OnShowUI");
    }

    /// <summary>
    /// 隐藏界面回调函数
    /// </summary>
    protected override void OnHideUI()
    {
        Debug.Log("UIGateSystemWin.OnHideUI");
        if (m_gateObjList == null) return;
        for (int i = 0; i < m_gateObjList.Count; i++)
        {
            Destroy(m_gateObjList[i]);
        }
        m_gateObjList.Clear();
    }

    public void OnGotoMap()
    {
        HideUI();
    }
}