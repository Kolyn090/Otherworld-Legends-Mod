using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System;


public class ReplacePathIDAtlas : EditorWindow
{
    /*
        ⚠️ This is not the same as ReplacePathIDAtlas36.cs. This tool replaces the path ids
        in the SpriteAtlas file's dump (a single txt file). Requires the source sprite atlas
        dump file (for correct path ids in game).

        ⚠️ 此工具和‘ReplacePathIDAtlas36.cs’使用目的不同。该工具替换SpriteAtlas文件（是一个单一的
        txt文件）中的（多个）Path ID。另外，该工具需要源SpriteAtlas的dump文件。
    */
    private string _sourceAtlasFilePath = "";

    private string _myAtlasFilePath = "";

    [MenuItem("Tools/Replace Path ID Atlas")]
    static void ShowWindow()
    {
        GetWindow<ReplacePathIDAtlas>("Replace Path ID Atlas");
    }

    private void OnGUI()
    {
        GUILayout.Label("Replace path id of all sprites in atlas with the provided source atlas file.");

        _myAtlasFilePath = DragAndDropFileField("My Atlas File", _myAtlasFilePath, "txt");
        GUILayout.Space(5);

        _sourceAtlasFilePath = DragAndDropFileField("Source Atlas File", _sourceAtlasFilePath, "txt");
        GUILayout.Space(5);

        GUI.enabled = File.Exists(_sourceAtlasFilePath) && File.Exists(_myAtlasFilePath);

        if (GUILayout.Button("Replace"))
        {
            ReplacePackedSpritesPathID();
            ReplaceRenderDataMapPathID();
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

    private void ReplacePackedSpritesPathID()
    {
        string[] sourceLines = File.ReadAllLines(_sourceAtlasFilePath);
        Regex indexPattern = new(@"^\s*\[(\d+)\]");
        Regex nameLinePattern = new("\"(.*?)\"");
        Regex pathIdPattern = new(@"SInt64 m_PathID = (-?\d+)");

        int totalNumOfSprites = 0;
        // Get the total count of index (# of sprites)
        foreach (var line in sourceLines)
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
        foreach (var line in sourceLines.Skip(2).ToArray())
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

        string[] myLines = File.ReadAllLines(_myAtlasFilePath);

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

        File.WriteAllLines(_myAtlasFilePath, resultLines);
    }

    private void ReplaceRenderDataMapPathID()
    {
        static int FindLineIndexStartWith(string prefix, string[] lines)
        {
            int lineNumber = 0;
            foreach (var line in lines)
            {
                if (line.StartsWith(prefix))
                {
                    return lineNumber;
                }
                lineNumber++;
            }
            return -1;
        }

        const string PathIDPrefix = "       0 SInt64 m_PathID = ";
        string[] sourceLines = File.ReadAllLines(_sourceAtlasFilePath);
        int pathIDLineIndex = FindLineIndexStartWith(PathIDPrefix, sourceLines);
        if (pathIDLineIndex == -1)
        {
            Debug.Log("Path ID not found or Text Asset file format does not match.");
        }

        Int64 sourcePathID = long.Parse(sourceLines[pathIDLineIndex].Replace(PathIDPrefix, ""));

        // Start replacement
        string[] myLines = File.ReadAllLines(_myAtlasFilePath);
        List<string> resultLines = new();
        bool IsPPtrTexture = false;
        foreach (var line in myLines)
        {
            if (line == "      0 PPtr<Texture2D> texture")
            {
                IsPPtrTexture = true;
            }
            else if (line == "      0 PPtr<Texture2D> alphaTexture")
            {
                IsPPtrTexture = false;
            }
            if (line.StartsWith(PathIDPrefix) && IsPPtrTexture)
            {
                resultLines.Add(PathIDPrefix + sourcePathID);
            }
            else
            {
                resultLines.Add(line);
            }
        }

        File.WriteAllLines(_myAtlasFilePath, resultLines);
    }
}
