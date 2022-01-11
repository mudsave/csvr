using UnityEngine;
using System.Collections;

/// <summary>
/// 加载场景不删除该物体
/// </summary>
public class CObjectDontDestroy : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Use this for initialization
    void Start()
    {
    }
}