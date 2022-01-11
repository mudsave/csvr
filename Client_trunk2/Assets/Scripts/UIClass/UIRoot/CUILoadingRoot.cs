using UnityEngine;
using System.Collections;

public class CUILoadingRoot : CUIRootBase
{
    private static CUIRootBase s_instance;

    public static CUIRootBase instance
    {
        get { return s_instance; }
    }

    public override void Awake()
    {
        base.Awake();

        if (s_instance == null)
            s_instance = this;
    }
}