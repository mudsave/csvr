using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIItem : MonoBehaviour 
{
    protected UIWindow m_parentWin;
    public virtual void Init(UIWindow p_win)
    {
        m_parentWin = p_win;
        OnInit();
    }

    protected abstract void OnInit();
}
