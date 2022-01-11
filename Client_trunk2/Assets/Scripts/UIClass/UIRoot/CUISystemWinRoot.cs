using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CUISystemWinRoot : CUIRootBase
{
    public delegate void UIShowHandler(eUIShowType type, CUIBaseWin win);
    public delegate void UIHideHandler(eUIHideType type, CUIBaseWin win);

    private static CUISystemWinRoot s_instance;
    public static CUISystemWinRoot instance
    {
        get { return s_instance; }
    }

    public Dictionary<eUIShowType, UIShowHandler> uiShowTypeFun = new Dictionary<eUIShowType, UIShowHandler>();
    public Dictionary<eUIHideType, UIHideHandler> uiHideTypeFun = new Dictionary<eUIHideType, UIHideHandler>();

    private List<CUIBaseWin> openWinList = new List<CUIBaseWin>();

    public override void Awake()
    {
        base.Awake();

        if (s_instance == null)
            s_instance = this;

        uiShowTypeFun[eUIShowType.common] = show_common;
        uiShowTypeFun[eUIShowType.reChained] = show_reChained;
        uiShowTypeFun[eUIShowType.addChained] = show_addChained;
        uiShowTypeFun[eUIShowType.endChained] = show_endChained;

        uiHideTypeFun[eUIHideType.common] = hide_common;
        uiHideTypeFun[eUIHideType.endChained] = hide_endChained;
        uiHideTypeFun[eUIHideType.subChained] = hide_subChained;
        uiHideTypeFun[eUIHideType.disChained] = hide_disChained;
    }

    public override void OnShowWin(eUIShowType type, CUIBaseWin win)
    {
        uiShowTypeFun[type](type, win);
    }

    public override void OnHideWin(eUIHideType type, CUIBaseWin win)
    {
        uiHideTypeFun[type](type, win);
    }

    public override void OnClearWin()
    {
        if (openWinList.Count > 0)
        {
            CUIBaseWin win = openWinList[openWinList.Count - 1];
            openWinList.Clear();
            win.HideUI();   //关闭UI会默认打开上一个UI，因此需要先清除WinList在关闭当前UI
        }
    }

    #region show ui

    public void show_common(eUIShowType type, CUIBaseWin win)
    {
        return;
    }

    public void show_reChained(eUIShowType type, CUIBaseWin win)
    {
        openWinList.Clear();
        show_addChained(type, win);
    }

    public void show_addChained(eUIShowType type, CUIBaseWin win)
    {
        if (openWinList.Count > 0 && openWinList[openWinList.Count - 1] == win)
            return;

        openWinList.Add(win);
    }

    public void show_endChained(eUIShowType type, CUIBaseWin win)
    {
        OnClearWin();
    }

    #endregion show ui

    #region hide ui

    public void hide_disChained(eUIHideType type, CUIBaseWin win)
    {
    }

    public void hide_common(eUIHideType type, CUIBaseWin win)
    {
        if (openWinList.Count == 0)
            return;

        if (openWinList[openWinList.Count - 1] != win) //普通关闭模式，并且顶底信息不是界面，无需自动移除
            return;

        hide_subChained(type, win);
    }

    public void hide_endChained(eUIHideType type, CUIBaseWin win)
    {
        OnClearWin();
    }

    public void hide_subChained(eUIHideType type, CUIBaseWin win)
    {
        if (openWinList.Count == 0)
            return;

        if (!openWinList.Contains(win))
        {
            return;
        }

        openWinList.Remove(win);

        if (openWinList.Count > 0 && openWinList[openWinList.Count - 1].isHide == eUIActivateType.hide)
        {
            openWinList[openWinList.Count - 1].ShowUI();
        }
    }

    #endregion hide ui
}