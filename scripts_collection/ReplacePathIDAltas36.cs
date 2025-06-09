using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ReplacePathIDAltas36 : EditorWindow
{
    /*
        Replace path id (of sprites TextAssest) in atlas. This is located in line 36. 
        We want to replace this line with the path id in the original game resource. 
        Warning: for non-atlas sprites, use ReplacePathID38.cs instead.

        替换atlas中的sprite TextAsset的Path ID。该数据存储在文件中的第38行。该文件主要使用目的
        为替换Path ID成源游戏资源的Path ID。注意：该工具只适用于有使用SpriteAtlas的文件，
        如果无使用atlas，则使用‘ReplacePathID38.cs’工具。
    */
    private DefaultAsset _myFolder;
    private string _replacePathID = "";

    [MenuItem("Tools/Replace Line 36 Sprite Atlas Path ID")]
    public static void ShowWindow()
    {
        GetWindow<ReplacePathIDAltas36>("Line 36 Replacer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Replace Line 36 Sprite Atlas", EditorStyles.boldLabel);
        GUILayout.Space(5);

        _myFolder = (DefaultAsset)EditorGUILayout.ObjectField("My Dump", _myFolder, typeof(DefaultAsset), false);

        GUILayout.Label("New Sprite Atlas ID");
        _replacePathID = EditorGUILayout.TextField(_replacePathID);

        GUILayout.Space(20);
        if (GUILayout.Button("Replace Line 36 in All .txt Files"))
        {
            string folderPath = AssetDatabase.GetAssetPath(_myFolder);

            if (Directory.Exists(folderPath))
            {
                ReplaceLine36InFiles(folderPath);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please select a valid folder.", "OK");
            }
        }
    }

    private void ReplaceLine36InFiles(string folder)
    {
        string[] txtFiles = Directory.GetFiles(folder, "*.txt", SearchOption.TopDirectoryOnly);
        int modifiedCount = 0;

        foreach (string file in txtFiles)
        {
            var lines = new List<string>(File.ReadAllLines(file));
            if (lines.Count >= 36)
            {
                lines[35] = $"  0 SInt64 m_PathID = {_replacePathID}";
                File.WriteAllLines(file, lines);
                modifiedCount++;
            }
        }

        EditorUtility.DisplayDialog("Done", $"Modified {modifiedCount} files.", "OK");
    }
}
