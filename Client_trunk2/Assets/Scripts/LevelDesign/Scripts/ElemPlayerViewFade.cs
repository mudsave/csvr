using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelDesign
{
    public class ElemPlayerViewFade : LevelElement
    {
        public float fadeInTime = 2f;     //淡入时间
        public float fadeOutTime = 2f;    //淡出时间
        public float stagnationTime = 1f; //停滞时间

        public Vector3 position = new Vector3(0, 0.9f, 0);  //重置玩家的位置

        void Start()
        {
            if (VRInputManager.Instance.playerComponent != null)
            {
                VRInputManager.Instance.playerComponent.CreatViewFade(fadeInTime, fadeOutTime, stagnationTime);
                StartCoroutine(DelayLevelPass());
            }
        }

        public IEnumerator DelayLevelPass()
        {
            yield return new WaitForSeconds(fadeInTime);
            if (VRInputManager.Instance.playerComponent != null)
            {
                //重置玩家的位置和朝向
                VRInputManager.Instance.playerComponent.navMeshAgent.Warp(position);
                VRInputManager.Instance.playerComponent.transform.LookAt(gameObject.transform);
            }

            yield return new WaitForSeconds(stagnationTime + fadeOutTime);
            if (isActive)
                LevelPass();
        }

    }
}
