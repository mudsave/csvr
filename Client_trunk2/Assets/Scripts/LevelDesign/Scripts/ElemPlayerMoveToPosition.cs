using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家移动到指定位置才算通过关
/// </summary>
namespace LevelDesign
{
    public class ElemPlayerMoveToPosition : LevelElement
    {
        public GameObject positionEffect = null;
        private GameObject obj;

        public override void OnActive()
        {
            base.OnActive();
            obj = Instantiate(positionEffect, transform.position, transform.rotation) as GameObject;
        }


        private void OnTriggerEnter(Collider other)
        {
            AvatarComponent component = other.GetComponent<AvatarComponent>();
            if (component)
            {
                if (component.objectType == CEntityType.Player)
                {
                    if (isActive)
                        LevelPass();

                    Destroy(obj);
                }
            }
        }

    }
}
