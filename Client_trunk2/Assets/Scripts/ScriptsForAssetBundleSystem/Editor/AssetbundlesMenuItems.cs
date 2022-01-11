using System.Collections;
using UnityEditor;
using UnityEngine;

public class AssetbundlesMenuItems
{
    private const string kSimulateAssetBundlesMenu = "AssetBundles/Simulate AssetBundles";

    //[MenuItem(kSimulateAssetBundlesMenu)]
    //public static void ToggleSimulateAssetBundle ()
    //{
    //    AssetBundleManager.SimulateAssetBundleInEditor = !AssetBundleManager.SimulateAssetBundleInEditor;
    //}

    //[MenuItem(kSimulateAssetBundlesMenu, true)]
    //public static bool ToggleSimulateAssetBundleValidate ()
    //{
    //    Menu.SetChecked(kSimulateAssetBundlesMenu, AssetBundleManager.SimulateAssetBundleInEditor);
    //    return true;
    //}

    [MenuItem("AssetBundles/Build AssetBundles")]
    static public void BuildAssetBundles()
    {
        if (EditorUtility.DisplayDialog("提示", "是否开始Build AssetBundles？", "确认", "取消"))
        {
            Debug.Log("开始Build AssetBundles");
            BuildScript.BuildAssetBundles();
        }
    }

    [MenuItem("AssetBundles/Build Player")]
    static void BuildPlayer()
    {
        if (EditorUtility.DisplayDialog("提示", "是否开始Build Player？", "确认", "取消"))
        {
            Debug.Log("开始Build Player");
            BuildScript.BuildPlayer();
        }
    }
}