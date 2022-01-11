using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using KBEngine;

/// <summary>
/// 怪物伤害去血信息
/// </summary>
public class CMonsterHurtMessage : MonoBehaviour
{
    public Text hurtContent;    //伤害信息
    private const float MaxOffset = 0.8f;   //动画偏移最大距离

    // Use this for initialization
    void Start()
    {

    }

    /// <summary>
    /// 播放提示信息
    /// </summary>
    public void Display(string message, float aspect)
    {
        hurtContent.text = message;
        float offset = MaxOffset * aspect;
        iTween.MoveAdd(gameObject, iTween.Hash("easetype", iTween.EaseType.easeOutCirc,
            "y", offset,
            "time", 1f,
            "oncompletetarget", gameObject,
            "oncomplete", "OnFinishedTween"
            ));
    }

    /// <summary>
    /// 结束动画
    /// </summary>
    public void OnFinishedTween()
    {
        gameObject.SetActive(false);
        ResourceManager.DestroyResource(gameObject);
    }
}
