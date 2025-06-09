using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;


public class ReplacePathID38 : EditorWindow
{
    /*
        Replace path id (of sprites TextAsset). This is located in line 38. We
        want to replace this line with the path id in the original game resource. 
        Also, this tool only work for spritesheets that do
        not involve spriteatlas. If that were that case, you must use 
        ReplacePathIDAtlas36.cs instead.

        替换sprite TextAsset的Path ID。该数据存储在文件中的第38行。该文件主要使用目的
        为替换Path ID成源游戏资源的Path ID。注意：该工具不适用于有使用SpriteAtlas的文件，
        你需要用对atlas中的文件用‘ReplacePathIDAtlas36.cs’。
    */
    private DefaultAsset _myFolder;
    private string _replacePathID = "";

    [MenuItem("Tools/Replace Line 38 From Folder")]
    public static void ShowWindow()
    {
        GetWindow<ReplacePathID38>("Line 38 Replacer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Replace Line 38", EditorStyles.boldLabel);
        GUILayout.Space(5);

        _myFolder = (DefaultAsset)EditorGUILayout.ObjectField("My Dump", _myFolder, typeof(DefaultAsset), false);

        GUILayout.Space(10);

        GUILayout.Label("New Sprite Atlas ID");
        _replacePathID = EditorGUILayout.TextField(_replacePathID);

        GUILayout.Space(20);
        if (GUILayout.Button("Replace Line 38"))
        {
            string folderPath = AssetDatabase.GetAssetPath(_myFolder);

            if (Directory.Exists(folderPath))
            {
                ReplaceLine38InFiles(folderPath);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please select a valid folder.", "OK");
            }
        }
    }

    private void ReplaceLine38InFiles(string folder)
    {
        string[] txtFiles = Directory.GetFiles(folder, "*.txt", SearchOption.TopDirectoryOnly);
        int modifiedCount = 0;

        foreach (string file in txtFiles)
        {
            var lines = new List<string>(File.ReadAllLines(file));
            if (lines.Count >= 38)
            {
                lines[37] = $"  0 SInt64 m_PathID = {_replacePathID}";
                File.WriteAllLines(file, lines);
                modifiedCount++;
            }
        }

        EditorUtility.DisplayDialog("Done", $"Modified {modifiedCount} files.", "OK");
    }
}
