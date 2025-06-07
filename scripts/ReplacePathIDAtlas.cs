using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;


public class ReplacePathIDAtlas : EditorWindow
{
    private string originalAtlasFilePath = "";

    private string myAtlasFilePath = "";

    [MenuItem("Tools/Replace Path ID Atlas")]
    static void ShowWindow()
    {
        GetWindow<ReplacePathIDAtlas>("Replace Path ID Atlas");
    }

    private void OnGUI()
    {
        GUILayout.Label("Replace path id of all sprites in atlas with the provided original atlas file.");

        originalAtlasFilePath = DragAndDropFileField("Original Atlas File", originalAtlasFilePath, "txt");
        GUILayout.Space(5);

        myAtlasFilePath = DragAndDropFileField("My Atlas File", myAtlasFilePath, "exe");
        GUILayout.Space(5);

        GUI.enabled = File.Exists(originalAtlasFilePath) && File.Exists(myAtlasFilePath);

        if (GUILayout.Button("Replace"))
        {
            Replace();
        }

        GUI.enabled = true;
    }

    private string DragAndDropFileField(string label, string path, string requiredExtension = null)
    {
        GUILayout.Label(label);
        Rect dropArea = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, string.IsNullOrEmpty(path) ? "Drag file here or browse..." : path);

        Event evt = Event.current;
        if ((evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform) && dropArea.Contains(evt.mousePosition))
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
                {
                    string draggedPath = AssetDatabase.GetAssetPath(obj);
                    string fullPath = Path.GetFullPath(draggedPath);

                    if (File.Exists(fullPath))
                    {
                        if (requiredExtension == null ||
                        fullPath.ToLower().EndsWith("." + requiredExtension))
                        {
                            path = fullPath;
                            GUI.changed = true;
                            break;
                        }
                    }
                }
            }
            evt.Use();
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Browse"))
        {
            string directory = string.IsNullOrEmpty(path) ? "" : Path.GetDirectoryName(path);
            string selected = EditorUtility.OpenFilePanel("Select " + label, directory, "");
            if (!string.IsNullOrEmpty(selected))
                path = selected;
        }
        GUILayout.EndHorizontal();

        return path;
    }

    private void Replace()
    {
        string[] originalLines = File.ReadAllLines(originalAtlasFilePath);
        Regex indexPattern = new(@"^\s*\[(\d+)\]");
        Regex nameLinePattern = new("\"(.*?)\"");
        Regex pathIdPattern = new(@"SInt64 m_PathID = (-?\d+)");

        int totalNumOfSprites = 0;
        // Get the total count of index (# of sprites)
        foreach (var line in originalLines)
        {
            var indexMatch = indexPattern.Match(line);
            if (indexMatch.Success)
            {
                int index = int.Parse(indexMatch.Groups[1].Value);
                if (index >= totalNumOfSprites)
                {
                    totalNumOfSprites++;
                }
            }
        }

        int nameCount = 0;
        int pathCount = 0;
        List<string> names = new();
        List<string> paths = new();
        foreach (var line in originalLines.Skip(2).ToArray())
        {
            var nameMatch = nameLinePattern.Match(line);
            var pathIdMatch = pathIdPattern.Match(line);

            if (nameMatch.Success && nameCount < totalNumOfSprites)
            {
                // Debug.Log($"{nameCount}: {nameMatch.Groups[1].Value}");
                names.Add(nameMatch.Groups[1].Value);
                nameCount++;
            }

            if (pathIdMatch.Success && pathCount < totalNumOfSprites)
            {
                // Debug.Log($"{pathCount}: {pathIdMatch.Groups[1].Value}");
                paths.Add(pathIdMatch.Groups[1].Value);
                pathCount++;
            }
        }

        Dictionary<string, string> name2path = new();
        for (int i = 0; i < names.Count; i++)
        {
            name2path[names[i]] = paths[i];
            // Debug.Log($"{names[i]}: {paths[i]}");
        }

        string[] myLines = File.ReadAllLines(myAtlasFilePath);

        int myNumOfSprites = 0;
        // Get the my count of index (# of sprites)
        foreach (var line in myLines)
        {
            var indexMatch = indexPattern.Match(line);
            if (indexMatch.Success)
            {
                int index = int.Parse(indexMatch.Groups[1].Value);
                if (index >= myNumOfSprites)
                {
                    myNumOfSprites++;
                }
            }
        }

        List<string> newPaths = new();
        nameCount = 0;
        foreach (var line in myLines.Skip(2).ToArray())
        {
            var nameMatch = nameLinePattern.Match(line);
            if (nameMatch.Success && nameCount < myNumOfSprites)
            {
                newPaths.Add(name2path[nameMatch.Groups[1].Value]);
                nameCount++;
            }
        }

        // foreach (var path in newPaths)
        // {
        //     Debug.Log(path);
        // }

        List<string> resultLines = new();

        int lastIndex = -1;
        pathCount = 0;
        // Debug.Log(myLines[0]);
        // Debug.Log(myLines[1]);
        resultLines.Add(myLines[0]);
        resultLines.Add(myLines[1]);
        foreach (var line in myLines.Skip(2).ToArray())
        {
            var indexMatch = indexPattern.Match(line);
            var pathIdMatch = pathIdPattern.Match(line);
            if (indexMatch.Success)
            {
                int index = int.Parse(indexMatch.Groups[1].Value);
                lastIndex = index;
            }
            if (pathIdMatch.Success && pathCount < myNumOfSprites)
            {
                resultLines.Add($"     0 SInt64 m_PathID = {newPaths[lastIndex]}");
                // Debug.Log($"     0 SInt64 m_PathID = {newPaths[lastIndex]}");
                pathCount++;
            }
            else
            {
                resultLines.Add(line);
                // Debug.Log(line);
            }
        }

        foreach (var line in resultLines)
        {
            Debug.Log(line);
        }

        File.WriteAllLines(myAtlasFilePath, resultLines);
    }
}
