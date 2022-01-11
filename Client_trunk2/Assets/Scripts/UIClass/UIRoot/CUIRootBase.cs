using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CUIRootBase : MonoBehaviour
{
    private GameObject root;

    public virtual void Awake()
    {
        root = gameObject;
        DontDestroyOnLoad(gameObject);
    }

    public virtual GameObject AddUI(GameObject child, bool isShow)
    {
        child.transform.SetParent(root.transform);
        child.transform.localPosition = Vector3.zero;
        child.transform.localScale = Vector3.one;
        child.transform.localRotation = Quaternion.Euler(Vector3.zero);
        child.layer = root.layer;

        child.SendMessage("InitUI", new object[] { this, isShow }, SendMessageOptions.DontRequireReceiver);
        return child;
    }

    public virtual void OnShowWin(eUIShowType type, CUIBaseWin win)
    {
    }

    public virtual void OnHideWin(eUIHideType type, CUIBaseWin win)
    {
    }

    public virtual void OnClearWin()
    {
    }

    /// <summary>
    /// 整体隐藏
    /// </summary>
    public virtual void HideRoot()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 整体显示
    /// </summary>
    public virtual void ShowRoot()
    {
        gameObject.SetActive(true);
    }

    public void Destroy()
    {
        Object.Destroy(gameObject);
    }
}