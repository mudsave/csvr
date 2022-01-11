using UnityEngine;
using System.Collections;

/// <summary>
/// 伤害显示基类
/// </summary>
public class CHurtDisplay
{
    private static GameObject dynamicMessageNode;   //伤害UI的父节点

    /// <summary>
    /// 获取动态UI父节点
    /// </summary>
    /// <returns></returns>
    protected GameObject GetDynamicMessageRoot()
    {
        if (dynamicMessageNode == null)
        {
            dynamicMessageNode = GameObject.Find("Dynamic Object");
            if (dynamicMessageNode == null)
            {
                dynamicMessageNode = new GameObject("Dynamic Object");
            }
        }
        return dynamicMessageNode;
    }
}
