using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MagicSparMgr
{
    private static string _effectName = "jingshi";

    /// <summary>
    /// 创建魔法晶石
    /// </summary>
    /// <param name="player"></param>
    /// <param name="target">目标</param>
    /// <param name="count">晶石个数</param>
    public static void CreateMagicSpar(AvatarComponent target, int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 startPos = target.transform.position;
            startPos.y += 1.2f;
            EffectComponent eComponent = VRInputManager.Instance.playerComponent.effectManager.AddEffect(_effectName, startPos);
            if (eComponent)
            {
                MagicSparTrack ms = eComponent.gameObject.AddComponent<MagicSparTrack>();
                ms.Init();
            }
        }   
    }
}
