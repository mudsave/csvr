using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

/// <summary>
/// 登录界面
/// </summary>
public class CLoginScene : MonoBehaviour
{
    public static CLoginScene instance;

    public GameObject vrCamera;
    public SteamVR_TrackedObject leftTracked;
    public SteamVR_TrackedObject rightTracked;

    private bool canLogin = true;

    private bool m_hasLogin = false;

    private void Awake()
    {
        instance = this;
    }

    public void InitUI(object[] obj)
    {
        gameObject.SetActive((bool)obj[1]);
    }

    public void ShowUI()
    {
        gameObject.SetActive(true);
        vrCamera.SetActive(true);
    }

    public void HideUI()
    {
        vrCamera.SetActive(false);
        gameObject.SetActive(false);
    }

    public bool AlreadyLogin
    {
        get
        {
            return m_hasLogin;
        }
    }

    /// <summary>
    /// 登录按钮事件
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="e">E.</param>
    public void OnLoginClick(string sceneName = "Scenes/Demo")
    {
        if (canLogin)
        {
            canLogin = false;
            Debug.Log("OnLoginClick");
            Startup.instance.Login(SystemInfo.deviceUniqueIdentifier, "test1", sceneName);
            m_hasLogin = true;  // demo临时做法，置已经登录过的标记
            Invoke("OpenLogin", 1);
        }
    }

    private void OpenLogin()
    {
        canLogin = true;
    }

    //private void FixedUpdate()
    //{
    //    if (leftTracked.gameObject.activeSelf)
    //    {
    //        var left = SteamVR_Controller.Input((int)leftTracked.index);
    //        if (left.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
    //        {
    //            OnLoginClick();
    //        }
    //    }
    //    if (rightTracked.gameObject.activeSelf)
    //    {
    //        var right = SteamVR_Controller.Input((int)rightTracked.index);
    //        if (right.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
    //        {
    //            OnLoginClick();
    //        }
    //    }
    //}
}