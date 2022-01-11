using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPlayerViewFade : MonoBehaviour {

    public SpriteRenderer spriteRenderer = null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fadeInTime">淡入时长</param>
    /// <param name="fadeOutTime">淡出时长</param>
    /// <param name="stagnationTime">停止时长</param>
    public void StartFade(float fadeInTime, float fadeOutTime, float stagnationTime)
    {
        if (spriteRenderer != null)
            StartCoroutine(ConroutineDisplay(fadeInTime, fadeOutTime, stagnationTime, spriteRenderer));
        else
            Debug.LogError("CPlayerViewFade::StartFade SpriteRenderer = NULL");
    }

    private IEnumerator ConroutineDisplay(float fadeInTime, float fadeOutTime, float stagnationTime, SpriteRenderer sprite)
    {
        //初始化
        sprite.material.color = sprite.material.color.SetAlpha(0f);
        sprite.gameObject.SetActive(true);
        iTween.FadeTo(sprite.gameObject, 1f, fadeInTime);

        yield return new WaitForSeconds(fadeInTime + stagnationTime);

        iTween.FadeTo(sprite.gameObject, iTween.Hash(
            "alpha", 0f,
            "time", fadeOutTime,
            "oncompletetarget", gameObject,
            "oncomplete", "OnFinishedTween"
            ));
    }

    /// <summary>
    /// 结束动画
    /// </summary>
    private void OnFinishedTween()
    {
        spriteRenderer.gameObject.SetActive(false);
    }
}
