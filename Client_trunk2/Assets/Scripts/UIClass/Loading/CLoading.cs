using System.Collections;
using UnityEngine;

/// <summary>
/// 加载界面
/// </summary>
public class CLoading : MonoBehaviour
{
    public static CLoading instance;

	public GameObject vrCamera;

    void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    void Start()
    {
    }

    public void InitUI(object[] obj)
    {
        gameObject.SetActive((bool)obj[1]);
    }

    public  void ShowUI()
    {
        vrCamera.SetActive(true);
        gameObject.SetActive(true);
    }

    public  void HideUI()
    {
        vrCamera.SetActive(false);
        gameObject.SetActive(false);
    }
}