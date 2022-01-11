using UnityEngine;
using System.Collections;

//场景类型
public enum CEventSceneType
{
    NULL,
    CEntityEditor = 1,
    CGateEditor = 2,
}

public class CEventScene : MonoBehaviour {

    public CEventSceneType eventSceneType = CEventSceneType.NULL;

    void Awake()
    {
        Debug.Log("start");
    }

    void OnValidate()
    {
        //if (enabled && !Application.isPlaying && NGUITools.GetActive(this))
        //    Reposition();
    }

    void Reposition()
    {
        if (eventSceneType == CEventSceneType.NULL)
        {
            return;
        }

        ////加上现在需要的组件
        //switch (eventSceneType)
        //{
        //    case CEventSceneType.CEntityEditor:
        //        gameObject.AddComponent<CNPCEditor>();
        //        //CEffectOnBirth obj = gameObject.GetComponent<CEffectOnBirth>();
        //        //Editor.Destroy(obj);
        //        break;
        //    case CEventSceneType.CGateEditor:
        //        gameObject.AddComponent<CGateEditor>();
        //        //CEffectOnBirth obj = gameObject.GetComponent<CEffectOnBirth>();
        //        //Editor.Destroy(obj);
        //        break;
        //    default:
        //        break;
        //}

        eventSceneType = CEventSceneType.NULL;
    }
}
