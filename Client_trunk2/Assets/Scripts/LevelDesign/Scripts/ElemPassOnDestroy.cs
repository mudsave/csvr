using UnityEngine;
using System.Collections;

namespace LevelDesign
{
    /// <summary>
    /// 如果自己死亡，则自己所代表的关卡则视为通关
    /// </summary>
    [AddComponentMenu("LevelDesign/LevelElement/ElemPassOnDestroy")]
    public class ElemPassOnDestroy : LevelElement
    {
        public void Awake()
        {
            if (this.gameObject.GetComponents<ElemPassOnDestroy>().Length > 1)
            {
                Debug.LogError(string.Format("game object '{0}' has more than one ElemPassOnDestroy instance, destroy self.", name));
                this.enabled = false;
                GameObject.Destroy(this);
            }
        }

        void OnDestroy()
        {
            if (isActive)
                LevelPass();
        }
    }
}
