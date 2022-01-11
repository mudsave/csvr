using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 玩家伤害信息
/// </summary>
public class CPlayerHurtMessage : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    // Use this for initialization
    void Start()
    {
    }

    /// <summary>
    /// 显示伤害
    /// </summary>
    public void Display()
    {
        StartCoroutine(ConroutineDisplay(spriteRenderer));
    }

    public IEnumerator ConroutineDisplay(SpriteRenderer sprite)
    {
        //初始化
        sprite.material.color = sprite.material.color.SetAlpha(0f);
        sprite.gameObject.SetActive(true);
        iTween.FadeTo(sprite.gameObject, 1f, 0.3f);

        yield return new WaitForSeconds(0.2f);

        iTween.FadeTo(sprite.gameObject, iTween.Hash(
            "alpha", 0f,
            "time", 0.5f,
            "oncompletetarget", gameObject,
            "oncomplete", "OnFinishedTween"
            ));
    }

    /// <summary>
    /// 结束动画
    /// </summary>
    public void OnFinishedTween()
    {
        spriteRenderer.gameObject.SetActive(false);
    }
}
