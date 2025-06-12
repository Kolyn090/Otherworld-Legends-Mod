using UnityEngine;
using UnityEditor;
using System.IO;
using System;


public class GameModApplier : EditorWindow
{
    private string _platformFolderPath = "";
    private string _modFolderPath = "";

    [MenuItem("Tools/10 Apply Mod", priority = 10)]
    public static void ShowWindow()
    {
        GetWindow<GameModApplier>("Apply Mod");
    }

    private void OnGUI()
    {
        GUIStyle style = new(GUI.skin.label)
        {
            wordWrap = true
        };

        _platformFolderPath = DragAndDropFolderField("Platform folder Path (such as StandaloneWindows64)\n对象平台路径", _platformFolderPath);
        GUILayout.Space(5);

        _modFolderPath = DragAndDropFolderField("Mod Folder Path\n 模组文件夹路径", _modFolderPath);
        GUILayout.Space(10);

        GUI.enabled = Directory.Exists(_platformFolderPath) && Directory.Exists(_modFolderPath);

        if (GUILayout.Button("Run (运行)"))
        {
            RunModApplier();
        }

        GUI.enabled = true;

        GUILayout.Space(35);

        string messageEn = "Usage: load the mod into the game. The mod must be" +
                            "constructed in a specific way - each modified .bun file must be put in the " +
                            "location referring to its location in the game folder. Run this script to do" +
                            "this for you. After the replacement, remember to do AddrTool. You will also " +
                            "need to keep your mod up to date (File location change & name change...)";
        string messageZh = "用途：载入模组到游戏。所有模组中的bun文件需要存放在一个模拟游戏文件夹位置的文件夹中。" +
                            "运行该工具以替换游戏资源。然后记得运行AddrTool。你还需要保证模组更新。";

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

    private void RunModApplier()
    {
        ReplaceFiles(_platformFolderPath, _modFolderPath);
    }

    public static void ReplaceFiles(string folderA, string folderB)
    {
        var allFilesB = Directory.GetFiles(folderB, "*", SearchOption.AllDirectories);

        foreach (var fileB in allFilesB)
        {
            // Get relative path of the file from folder B
            var relPath = Path.GetRelativePath(folderB, fileB);

            // Combine it with folder A to find the matching file
            var fileA = Path.Combine(folderA, relPath);

            if (File.Exists(fileA))
            {
                Console.WriteLine($"Replacing: {fileA}");
                File.Copy(fileB, fileA, overwrite: true);
            }
            else
            {
                Console.WriteLine($"Skipped (no match in A): {relPath}");
            }
        }
    }
}
