using UnityEngine;
using UnityEditor;
using System.IO;
using System;


public class AssetReplacer : EditorWindow
{
    private string standaloneWindowsPath = "";
    private string modFolderPath = "";

    [MenuItem("Tools/Apply Mod")]
    static void ShowWindow()
    {
        GetWindow<ModApplier>("Apply Mod");
    }

    private void OnGUI()
    {
        GUILayout.Label("Drag the required folders to replace Game Assets.", EditorStyles.boldLabel);

        standaloneWindowsPath = DragAndDropFolderField("Standalone Windows Path", standaloneWindowsPath);
        GUILayout.Space(5);

        modFolderPath = DragAndDropFolderField("Mod Folder Path", modFolderPath);
        GUILayout.Space(10);

        GUI.enabled = Directory.Exists(standaloneWindowsPath) && Directory.Exists(modFolderPath);

        if (GUILayout.Button("Run Mod Applier"))
        {
            RunModApplier();
        }

        GUI.enabled = true;
    }

    private string DragAndDropFolderField(string label, string path)
    {
        GUILayout.Label(label);
        Rect dropArea = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, string.IsNullOrEmpty(path) ? "Drag folder here or browse..." : path);

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
        if (GUILayout.Button("Browse"))
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
        ReplaceFiles(standaloneWindowsPath, modFolderPath);
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
