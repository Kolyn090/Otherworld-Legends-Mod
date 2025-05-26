using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class TextAssetRenamerWindow : EditorWindow
{
    private string newBaseName = "new_sprite_name";
    private List<TextAsset> selectedAssets = new List<TextAsset>();
    private Vector2 scroll;

    [MenuItem("Tools/Rename TextAssets Window")]
    public static void ShowWindow()
    {
        GetWindow<TextAssetRenamerWindow>("Rename TextAssets");
    }

    void OnGUI()
    {
        GUILayout.Label("Rename TextAssets", EditorStyles.boldLabel);
        newBaseName = EditorGUILayout.TextField("New Base Name", newBaseName);

        EditorGUILayout.Space();
        GUILayout.Label("Drag and drop TextAssets below:", EditorStyles.label);

        Rect dropArea = GUILayoutUtility.GetRect(0, 100, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drop TextAssets here", EditorStyles.helpBox);
        HandleDragAndDrop(dropArea);

        EditorGUILayout.Space();
        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (var asset in selectedAssets)
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

    void HandleDragAndDrop(Rect dropArea)
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
                        if (dragged is TextAsset textAsset && !selectedAssets.Contains(textAsset))
                        {
                            selectedAssets.Add(textAsset);
                        }
                    }
                    evt.Use();
                }
            }
        }
    }

    void RenameFiles()
    {
        foreach (var asset in selectedAssets)
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
            string newName = $"{newBaseName}_{digits}";

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
