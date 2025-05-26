using UnityEditor;
using UnityEngine;
using System.IO;


public class AssetBundleBuilder {
    [MenuItem("Tools/Build Bundles")]
    static void BuildAllAssetBundles()
    {
        string path = "Assets/AssetBundles";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);

        Debug.Log("Please don't forget to assign bundle name.");
    }
}
