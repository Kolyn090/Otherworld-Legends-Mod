using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;


public class TextAssetRenamerWindow : EditorWindow
{
    /*
        Rename all loaded TextAsset (sprite dumps) files. After renaming, it will use
        number of suffix. Notice that the base name in TextAsset will also be changed.

        为所有载入的TextAsset文件（sprite dump）重新命名。修改名字后的文件会用数字作为后缀。
        文件里的base name也会被更改。
    */
    private string _newBaseName = "new_sprite_name";
    private List<TextAsset> _selectedAssets = new();
    private Vector2 _scroll;

    [MenuItem("Tools/Rename TextAssets Window")]
    public static void ShowWindow()
    {
        GetWindow<TextAssetRenamerWindow>("Rename TextAssets");
    }

    private void OnGUI()
    {
        GUILayout.Label("Rename TextAssets", EditorStyles.boldLabel);
        _newBaseName = EditorGUILayout.TextField("New Base Name", _newBaseName);

        EditorGUILayout.Space();
        GUILayout.Label("Drag and drop TextAssets below:", EditorStyles.label);

        Rect dropArea = GUILayoutUtility.GetRect(0, 100, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drop TextAssets here", EditorStyles.helpBox);
        HandleDragAndDrop(dropArea);

        EditorGUILayout.Space();
        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        foreach (var asset in _selectedAssets)
        {
            EditorGUILayout.ObjectField(asset, typeof(TextAsset), false);
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        if (GUILayout.Button("Rename Files and Update Contents"))
        {
            RenameFiles();
        }
    }

    private void HandleDragAndDrop(Rect dropArea)
    {
        Event evt = Event.current;
        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            if (dropArea.Contains(evt.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (Object dragged in DragAndDrop.objectReferences)
                    {
                        if (dragged is TextAsset textAsset && !_selectedAssets.Contains(textAsset))
                        {
                            _selectedAssets.Add(textAsset);
                        }
                    }
                    evt.Use();
                }
            }
        }
    }

    private void RenameFiles()
    {
        foreach (var asset in _selectedAssets)
        {
            string path = AssetDatabase.GetAssetPath(asset);
            string filename = Path.GetFileNameWithoutExtension(path);

            Match match = Regex.Match(filename, @"(\d+)$");
            if (!match.Success)
            {
                Debug.LogWarning($"Skipped (no trailing digits): {filename}");
                continue;
            }

            string digits = match.Groups[1].Value;
            string newName = $"{_newBaseName}_{digits}";

            // Read and modify file content
            string fullPath = Path.Combine(Application.dataPath, path.Substring("Assets/".Length));
            string content = File.ReadAllText(fullPath);
            content = Regex.Replace(content, @"string m_Name = "".*?""", $"string m_Name = \"{newName}\"");
            File.WriteAllText(fullPath, content);

            // Rename file
            AssetDatabase.RenameAsset(path, newName);
            Debug.Log($"Renamed: {filename} → {newName}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
