using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 界面基类
/// </summary>
public class CUIBaseWin : MonoBehaviour
{
    public static Dictionary<CUIBaseWin, bool> winlist = new Dictionary<CUIBaseWin, bool>();

    private eUIActivateType activateType = eUIActivateType.none;
    private CUIRootBase rootBase;   //ui所在层

    public eUIActivateType isHide
    {
        get
        {
            return activateType;
        }
        set
        {
            activateType = value;
        }
    }

    public virtual void InitUI(object[] obj)
    {
        rootBase = (CUIRootBase)obj[0];
        winlist[this] = (bool)obj[1]; 	//需要注册的界面
    }

    /// <summary>
    /// 数据完整后，调用的初始化接口
    /// </summary>
    public virtual void Initialize()
    {
    }

    /// <summary>
    /// 所有系统调用Initialize调用完毕后，开始调用此接口
    /// </summary>
    public virtual void OnInitialize()
    {
    }

    /// <summary>
    /// 显示界面
    /// </summary>
    public void ShowUI(eUIShowType type = eUIShowType.common)
    {
        if (activateType == eUIActivateType.show)
        {
            return;
        }

        activateType = eUIActivateType.show;
        rootBase.OnShowWin(type, this);
        OnShowUI();
    }

    /// <summary>
    /// 显示界面回调函数
    /// </summary>
    protected virtual void OnShowUI(params object[] args)
    {
    }

    /// <summary>
    /// 隐藏界面
    /// </summary>
    public void HideUI(eUIHideType type = eUIHideType.common)
    {
        if (activateType == eUIActivateType.hide)
        {
            return;
        }

        activateType = eUIActivateType.hide;
        rootBase.OnHideWin(type, this);
        OnHideUI();
    }

    /// <summary>
    /// 隐藏界面回调函数
    /// </summary>
    protected virtual void OnHideUI()
    {
    }
}