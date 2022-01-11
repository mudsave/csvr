using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CErrorTipsWin : CUIBaseWin
{
    public GameObject errorTipsWin;
    public Text tips;   //提示信息
    public Button confirm;  //确认按钮

    private static CErrorTipsWin s_instance;
    public static CErrorTipsWin instance
    {
        get { return s_instance; }
    }

    void Awake()
    {
        if (s_instance == null)
        {
            s_instance = this;
        }

        GlobalEvent.register("OnTouchpadPressed", this, "OnTouchpadPressed");
        confirm.onClick.AddListener(OnConfirmClick);
    }

    void OnDestroy()
    {
        GlobalEvent.deregister(this);
    }

    private void OnConfirmClick()
    { 
        HideUI();
    }

    public void OnTouchpadPressed(VRControllerEventArgs e)
    {
        if (e.hand == Hand.RIGHT)
        {
            OnConfirmClick();
        }
    }

    /// <summary>
    /// 初始化函数
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
        GlobalEvent.register("ShowErrorTipsWin", this, "ShowMessage");
    }

    public void ShowMessage(string message)
    {
        if (message != "")
        {
            ShowUI();

            if (VRInputManager.Instance.camera == null) return;

            Transform cameraTransform = VRInputManager.Instance.camera.gameObject.transform;
            if (transform.parent != cameraTransform)
            {
                transform.parent = cameraTransform;
                transform.localPosition = Vector3.zero;
                transform.rotation = cameraTransform.rotation;
            }
            tips.text = message;
        }
    }

    protected override void OnShowUI(params object[] args)
    {
        base.OnShowUI(args);

        if (!errorTipsWin.activeSelf)
        {
            errorTipsWin.SetActive(true);
        }
    }

    protected override void OnHideUI()
    {
        base.OnHideUI();
        
        if (errorTipsWin.activeSelf)
        {
            errorTipsWin.SetActive(false);
        }
    }
}
