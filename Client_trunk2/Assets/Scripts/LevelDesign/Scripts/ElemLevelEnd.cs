using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelDesign
{
    public class ElemLevelEnd : LevelElement
    {
        public float delayTime = 1.0f;

        public override void OnActive()
        {
            base.OnActive();
            StartCoroutine(OnLevelPass());
        }

        private IEnumerator OnLevelPass()
        {
            yield return new WaitForSeconds(delayTime);
            Transform playerTransform = VRInputManager.Instance.playerComponent.gameObject.transform;
            Vector3 position = playerTransform.position + playerTransform.forward * 3;
            position.y += 2;
            Vector3 direction = playerTransform.rotation.eulerAngles;
            GlobalEvent.fire("Event_EndFight", position, direction);
            GlobalEvent.fire("Event_DeregisterControllerEvents");

            VRInputManager.Instance.SelectBeam.SetActive(true);

            if (isActive)
                LevelPass();
        }
    }
}
