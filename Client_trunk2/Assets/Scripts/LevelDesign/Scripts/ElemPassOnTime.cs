using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelDesign
{
    /// <summary>
    /// 时间到就通关
    /// </summary>
    [AddComponentMenu("LevelDesign/LevelElement/ElemPassOnTime")]
    public class ElemPassOnTime : LevelElement
    {
        public float delyTime;
        public bool isDestroyObject = true;
        public List<GameObject> destroyObjsList = new List<GameObject>();

        public void Awake()
        {
            //if (this.gameObject.GetComponents<ElemPassOnTime>().Length > 1)
            //{
            //    Debug.LogError(string.Format("game object '{0}' has more than one ElemPassOnTime instance, destroy self.", name));
            //    this.enabled = false;
            //    GameObject.Destroy(this);
            //}
        }

        public override void OnActive()
        {
            base.OnActive();
            StartCoroutine(OnTime());
        }

        private IEnumerator OnTime()
        {
            yield return new WaitForSeconds(delyTime);

            if (isActive)
                LevelPass();

            if (isDestroyObject)
            {
                foreach (var obj in destroyObjsList)
                {
                    Destroy(obj);
                }
            }    
        }
    }
}