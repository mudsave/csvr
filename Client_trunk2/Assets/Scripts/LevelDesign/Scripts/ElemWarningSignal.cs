using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelDesign
{
    public class ElemWarningSignal : LevelElement
    {
        public GameObject warningPrefab = null;
        public float lastTime = 3.0f;

        private GameObject _warningGameObject = null;

        public override void OnActive()
        {
            base.OnActive();

            if (warningPrefab != null)
            {
                _warningGameObject = Instantiate(warningPrefab) as GameObject;
                Transform cameraTransform = VRInputManager.Instance.camera.gameObject.transform;
                _warningGameObject.transform.parent = cameraTransform;
                _warningGameObject.transform.localPosition = Vector3.zero + Vector3.forward;
                _warningGameObject.transform.rotation = cameraTransform.rotation;
            }

            StartCoroutine(OnLevelPass());
        }

        private IEnumerator OnLevelPass()
        {
            yield return new WaitForSeconds(lastTime);

            if (_warningGameObject)
                Destroy(_warningGameObject);

            if (isActive)
                LevelPass();
        }
    }
}
