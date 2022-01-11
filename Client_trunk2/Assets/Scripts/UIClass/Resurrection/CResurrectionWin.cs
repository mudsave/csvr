using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class CResurrectionWin : CUIBaseWin
{
    private List<int> m_reviveType = new List<int>();
    private VRTK_UIPointer uiPointer;
    private VRTK_SimplePointer simplePointer;

    public Button autochthonousRevival;
    public Button cityRevival;

    public static CResurrectionWin instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        GlobalEvent.register("ReviveTypeDisable", this, "OnReviveTypeDisable");

        autochthonousRevival.onClick.AddListener(OnClickAutochthonousRevival);
        cityRevival.onClick.AddListener(OnClickCityRevival);
    }

    void Start()
    {
    }

    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        GlobalEvent.fire("ReviveTypeDisable");
    //    }
    //}

    void OnDestroy()
    {
        GlobalEvent.deregister(this);
    }

    public void OnReviveTypeDisable()
    {
        //string mapConfigID = KBEngine.World.instance.mapConfigID;
        //if (CConfigClass.mapConfig[mapConfigID].disableReviveType.Count <= 0)
        //    return;

        //m_reviveType = CConfigClass.mapConfig[mapConfigID].disableReviveType;
        //ShowUI();

        //if (VRInputManager.Instance.camera == null) return;

        //Canvas canvas = GetComponentInChildren<Canvas>();
        //canvas.worldCamera = VRInputManager.Instance.camera;

        //Transform target = VRInputManager.Instance.camera.transform;
        //transform.position = target.position;
        //Vector3 direction = target.rotation.eulerAngles;
        //direction.x = direction.z = 0;
        //transform.rotation = Quaternion.Euler(direction);

        ////添加射线
        //GameObject rightHandler = VRInputManager.Instance.handRight;
        //if (uiPointer == null)
        //{
        //    uiPointer = rightHandler.AddComponent<VRTK_UIPointer>();
        //}
        //uiPointer.enabled = true;

        //if (simplePointer == null)
        //{
        //    simplePointer = rightHandler.AddComponent<VRTK_SimplePointer>();
        //}
        //simplePointer.enabled = true;
    }

    /// <summary>
    /// 数据完整后，调用的初始化接口
    /// </summary>
    public override void Initialize()
    {
    }

    /// <summary>
    /// 所有系统调用Initialize调用完毕后，开始调用此接口
    /// </summary>
    public override void OnInitialize()
    {
        //if (GameObjComponent.GetPlayer().entity.status == (int)eEntityStatus.Death)
        //{
        //    OnReviveTypeDisable();
        //}
    }

    public void OnClickAutochthonousRevival()
    {
        var entity = KBEngine.KBEngineApp.app.player();
        entity.cellCall("requestResurrection");
        HideUI();
    }

    public void OnClickCityRevival()
    {
        var entity = KBEngine.KBEngineApp.app.player();
        entity.cellCall("requestReturnMainCity");
        HideUI();
    }

    protected override void OnShowUI(params object[] args)
    {
        this.gameObject.SetActive(true);
    }

    protected override void OnHideUI()
    {
        if (uiPointer != null && simplePointer != null)
        {
            uiPointer.enabled = false;
            simplePointer.enabled = false;
        }

        this.gameObject.SetActive(false);
        m_reviveType = null;
    }
}