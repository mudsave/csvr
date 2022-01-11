using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIWindow : MonoBehaviour 
{
    protected bool m_isInited = false;

    public void Init()
    {
        if (!m_isInited)
        {
            OnInit();
            m_isInited = true;
        }
    }

    public void Open()
    {
        if (!gameObject.activeSelf) 
            gameObject.SetActive(true);
        OnOpen();
    }

    public void Close()
    {
        OnClose();
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }

    protected virtual void OnInit()
    {}
    protected virtual void OnClose()
    {}
    protected virtual void OnOpen()
    {}
}
