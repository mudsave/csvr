using UnityEngine;
using System.Collections;

/// <summary>
/// 扩展函数
/// </summary>
public static class CStaticExtension
{
    public static Color SetAlpha(this Color c, float alpha)
    {
        Color color = new Color(c.r, c.g, c.b, alpha);
        return color;
    }
}
