using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


public class MyAtlasDumpFixer : EditorWindow
{
    private string _sourceAtlasFilePath = "";

    private string _owningAtlasFilePath = "";

    [MenuItem("Tools/07 Auto Fix Atlas Dump", priority = 7)]
    public static void ShowWindow()
    {
        GetWindow<MyAtlasDumpFixer>("Auto Fix Atlas Dump");
    }

    private void OnGUI()
    {
        GUIStyle style = new(GUI.skin.label)
        {
            wordWrap = true
        };

        _owningAtlasFilePath = DragAndDropFileField("My Atlas Dump*\n我的图集导出*", _owningAtlasFilePath, "json");
        _sourceAtlasFilePath = DragAndDropFileField("Source Atlas Dump*\n源图集导出*", _sourceAtlasFilePath, "json");
        GUILayout.Space(5);

        GUI.enabled = File.Exists(_sourceAtlasFilePath) && File.Exists(_owningAtlasFilePath);

        if (GUILayout.Button("Run (运行)"))
        {
            // ReplacePackedSpritesPathID();
            SortNameIDToSourceOrder();
            ReplaceRenderDataMapPathID();
        }

        GUI.enabled = true;

        GUILayout.Space(35);

        string messageEn = "Usage: Fix all Path_ID in my Altas Dump file using the Source, " +
                            "including m_PackedSprites/Array/m_PathID " +
                            "and m_RenderDataMap/Array/second/texture/m_PathID.";
        string messageZh = "用途：使用源图集导出修复我的图集导出文件中的路径ID。" +
                            "包括m_PackedSprites/Array/m_PathID和" +
                            "m_RenderDataMap/Array/second/texture/m_PathID。";

        GUILayout.Label(messageEn, style);
        GUILayout.Label(messageZh, style);
    }

    private string DragAndDropFileField(string label, string path, string requiredExtension = null)
    {
        GUILayout.Label(label);
        Rect dropArea = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, string.IsNullOrEmpty(path) ? "Drag file here or browse... \n拖放文件或搜索..." : path);

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
        if (GUILayout.Button("Browse (搜索)"))
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
        JObject _sourceAtlasFileJson = JObject.Parse(File.ReadAllText(_sourceAtlasFilePath));
        JObject _owningAtlasFileJson = JObject.Parse(File.ReadAllText(_owningAtlasFilePath));

        var packedSpritesSource = _sourceAtlasFileJson["m_PackedSprites"]["Array"];
        var packedSpriteNameToIndexSource = _sourceAtlasFileJson["m_PackedSpriteNamesToIndex"]["Array"];

        var packedSpritesOwning = _owningAtlasFileJson["m_PackedSprites"]["Array"];
        var packedSpriteNameToIndexOwning = _owningAtlasFileJson["m_PackedSpriteNamesToIndex"]["Array"];

        // source name & source id
        Dictionary<string, List<long>> sourceNamePathID = new();
        // New: copy file id
        Dictionary<string, List<int>> sourceNameFileID = new();

        for (int i = 0; i < packedSpritesSource.Count(); i++)
        {
            var packedSprite = packedSpritesSource[i];
            var pathID = long.Parse(packedSprite["m_PathID"].ToString());
            var fileID = int.Parse(packedSprite["m_FileID"].ToString());
            var name = packedSpriteNameToIndexSource[i].ToString();
            if (!sourceNamePathID.ContainsKey(name))
            {
                sourceNamePathID.Add(name, new() { pathID });
            }
            else
            {
                sourceNamePathID[name].Add(pathID);
                Debug.LogWarning($"{name} is duplicated, current stored Path IDs [{string.Join(", ", sourceNamePathID[name])}].");
            }
            if (!sourceNameFileID.ContainsKey(name))
            {
                sourceNameFileID.Add(name, new() { fileID });
            }
            else
            {
                sourceNameFileID[name].Add(fileID);
                Debug.LogWarning($"{name} is duplicated, current stored file IDs [{string.Join(", ", sourceNameFileID[name])}].");
            }
        }

        for (int i = 0; i < packedSpritesOwning.Count(); i++)
        {
            var name = packedSpriteNameToIndexOwning[i].ToString();
            try
            {
                var pathID = sourceNamePathID[name][0];
                var fileID = sourceNameFileID[name][0];
                sourceNamePathID[name].RemoveAt(0);
                sourceNameFileID[name].RemoveAt(0);
                packedSpritesOwning[i]["m_PathID"] = pathID;
                packedSpritesOwning[i]["m_FileID"] = fileID;
            }
            catch
            {
                string oldName = name;
                name = name.Split(" #")[0];
                var pathID = sourceNamePathID[name][0];
                var fileID = sourceNameFileID[name][0];
                sourceNamePathID[name].RemoveAt(0);
                sourceNameFileID[name].RemoveAt(0);
                packedSpritesOwning[i]["m_PathID"] = pathID;
                packedSpritesOwning[i]["m_FileID"] = fileID;
                Debug.LogWarning($"{oldName} is not in the source atlas dump, assign Path ID {pathID} and File ID {fileID}.");
            }
        }

        _owningAtlasFileJson["m_PackedSpriteNamesToIndex"]["Array"] = packedSpriteNameToIndexOwning;
        File.WriteAllText(_owningAtlasFilePath, JsonConvert.SerializeObject(_owningAtlasFileJson, Formatting.Indented));
    }

    private void SortNameIDToSourceOrder()
    {
        JObject _sourceAtlasFileJson = JObject.Parse(File.ReadAllText(_sourceAtlasFilePath));
        JObject _owningAtlasFileJson = JObject.Parse(File.ReadAllText(_owningAtlasFilePath));
        _owningAtlasFileJson["m_PackedSprites"] = _sourceAtlasFileJson["m_PackedSprites"];
        _owningAtlasFileJson["m_PackedSpriteNamesToIndex"] = _sourceAtlasFileJson["m_PackedSpriteNamesToIndex"];
        File.WriteAllText(_owningAtlasFilePath, JsonConvert.SerializeObject(_owningAtlasFileJson, Formatting.Indented));
    }

    private void ReplaceRenderDataMapPathID()
    {
        JObject _sourceAtlasFileJson = JObject.Parse(File.ReadAllText(_sourceAtlasFilePath));
        JObject _owningAtlasFileJson = JObject.Parse(File.ReadAllText(_owningAtlasFilePath));

        var renderDataMapSource = _sourceAtlasFileJson["m_RenderDataMap"]["Array"];
        var pathIDToChange = renderDataMapSource[0]["second"]["texture"]["m_PathID"];

        var renderDataMapOwning = _owningAtlasFileJson["m_RenderDataMap"]["Array"];
        for (int i = 0; i < renderDataMapOwning.Count(); i++)
        {
            renderDataMapOwning[i]["second"]["texture"]["m_PathID"] = pathIDToChange;
        }

        _owningAtlasFileJson["m_RenderDataMap"]["Array"] = renderDataMapOwning;
        File.WriteAllText(_owningAtlasFilePath, JsonConvert.SerializeObject(_owningAtlasFileJson, Formatting.Indented));
    }
}
