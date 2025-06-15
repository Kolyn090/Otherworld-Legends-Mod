using System.IO;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class FixSpriteDumpAtlasPathID : EditorWindow
{
    private string _owningFolderPath = "";
    private string _replacePathID = "";

    [MenuItem("Tools/09 Fix Atlas Path ID", priority = 9)]
    public static void ShowWindow()
    {
        GetWindow<FixSpriteDumpAtlasPathID>("Fix Sprite Dump Atlas Path ID");
    }

    private void OnGUI()
    {
        GUIStyle style = new(GUI.skin.label)
        {
            wordWrap = true
        };
        
        GUILayout.Label("My Sprite Dump Folder (Json)*");
        _owningFolderPath = DragAndDropFolderField("我的图像导出文件夹 (Json)*", _owningFolderPath);

        GUILayout.Label("New Path ID*");
        GUILayout.Label("新的Path ID*");
        _replacePathID = EditorGUILayout.TextField(_replacePathID);
        GUILayout.Space(5);

        GUI.enabled = !string.IsNullOrWhiteSpace(_owningFolderPath) && !string.IsNullOrWhiteSpace(_replacePathID);
        if (GUILayout.Button("Run (运行)"))
        {
            Fix();
        }

        GUI.enabled = true;

        GUILayout.Space(35);

        string messageEn = "Usage: replace Sprite Atlas Path ID. The purpose of this tool is to replace " +
                            "My Sprite Path ID to Source Sprite Path ID. Warning: this tools does not fit " +
                            "Dump files that use SpriteAtlas; If used, run '08 Fix Texture Path ID'.";
        string messageZh = "用途：替换图像的图集路径ID。该工具主要使用目的为替换路径ID成源游戏的路径ID。" +
                            "注意：该工具仅适用于有使用图集的文件，如果有使用图集，运行 '08 Fix Texture Path ID'。";

        GUILayout.Label(messageEn, style);
        GUILayout.Label(messageZh, style);
    }

    private string DragAndDropFolderField(string label, string path)
    {
        GUILayout.Label(label);
        Rect dropArea = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, string.IsNullOrEmpty(path) ? "Drag folder here or browse... \n拖放文件夹或搜索..." : path);

        Event evt = Event.current;
        if ((evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform) && dropArea.Contains(evt.mousePosition))
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                foreach (var draggedPath in DragAndDrop.paths)
                {
                    if (Directory.Exists(draggedPath))
                    {
                        path = draggedPath;
                        GUI.changed = true;
                        break;
                    }
                }
            }
            evt.Use();
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Browse (搜索)"))
        {
            string selected = EditorUtility.OpenFolderPanel("Select " + label, path, "");
            if (!string.IsNullOrEmpty(selected))
                path = selected;
        }
        GUILayout.EndHorizontal();

        return path;
    }

    private void Fix()
    {
        string[] jsonFiles = Directory.GetFiles(_owningFolderPath, "*.json", SearchOption.TopDirectoryOnly);
        foreach (string jsonFile in jsonFiles)
        {
            JObject spriteJson = JObject.Parse(File.ReadAllText(jsonFile));
            if (spriteJson.ContainsKey("m_Rect")) // Make sure it's a Sprite Dump File
            {
                spriteJson["m_SpriteAtlas"]["m_PathID"] = long.Parse(_replacePathID);
            }
            File.WriteAllText(jsonFile, JsonConvert.SerializeObject(spriteJson, Formatting.Indented));
        }
    }
}
