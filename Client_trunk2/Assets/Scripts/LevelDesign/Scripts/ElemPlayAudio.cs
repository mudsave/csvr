using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelDesign
{
    /// <summary>
    /// 播放一段声音，目前不考虑3D音效，所以不针对某个目标做处理。
    /// </summary>
    [AddComponentMenu("LevelDesign/LevelElement/ElemPlayAudio")]
    public class ElemPlayAudio : LevelElement
    {
        public string AudioName;
        public float lastTime;

        public override void OnActive()
        {
            base.OnActive();
            AudioManager.Instance.SoundPlay(AudioName);
            StartCoroutine(OnLevelPass());
        }

        private IEnumerator OnLevelPass()
        {
            yield return new WaitForSeconds(lastTime);

            if (isActive)
                LevelPass();
        }

    }
}