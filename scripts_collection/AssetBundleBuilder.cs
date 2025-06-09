using UnityEditor;
using UnityEngine;
using System.IO;


public class AssetBundleBuilder : EditorWindow
{
    /*
        Pack **all** AssetBundles in the project. The best practice is to tag only
        AssetBundles that are necessary. In this way you can save some time and
        resources. The output will be stored in 'Assets/AssetBundles'.

        为项目中所有AssetBundle打包。请只标注（tag）你目前需要的AssetBundle，这样可以
        节省时间和资源。生成文件会被放在‘Assets/AssetBundles’。
    */

    private BuildTarget _buildPlatform = BuildTarget.StandaloneWindows64;

    [MenuItem("Tools/Build Bundles")]
    public static void ShowWindow()
    {
        GetWindow<AssetBundleBuilder>("Build Bundles");
    }

    private void OnGUI()
    {
        GUILayout.Label("AssetBundle Build Settings", EditorStyles.boldLabel);

        // Dropdown to change build target
        _buildPlatform = (BuildTarget)EditorGUILayout.EnumPopup("Build Target", _buildPlatform);

        if (GUILayout.Button("Build Bundles"))
        {
            BuildAllAssetBundles(_buildPlatform);
        }
    }

    public static void BuildAllAssetBundles(BuildTarget target)
    {
        string path = "Assets/AssetBundles";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, target);

        Debug.Log("Please don't forget to assign bundle name.");
    }
}
