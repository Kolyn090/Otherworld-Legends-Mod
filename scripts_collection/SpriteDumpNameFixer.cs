using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;


public class SpriteDumpNameFixer : EditorWindow
{
    private string _owningAtlasFilePath = ""; // Only assign this if SpriteAtlas is used

    private DefaultAsset _owningSpriteDumpsFolder;

    private DefaultAsset _sourceSpriteDumpsFolder;

    [MenuItem("Tools/06 Fix Dump Names", priority = 6)]
    public static void ShowWindow()
    {
        GetWindow<SpriteDumpNameFixer>("Fix names in My Sprite & SpriteAtlas Dumps.");
    }

    private void OnGUI()
    {
        GUIStyle style = new(GUI.skin.label)
        {
            wordWrap = true,
        };
        GUIStyle boldStyle = new(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
        };

        GUILayout.Label("Fix the names in My SpriteAtlas Dump. (Json)", boldStyle);
        GUILayout.Label("修复我的图集导出中的名称（Json）");
        GUILayout.Label("My SpriteAtlas Dump File*");
        _owningAtlasFilePath = DragAndDropFileField("我的图集导出文件*", _owningAtlasFilePath, "json");
        GUILayout.Space(35);

        GUILayout.Label("Fix the names in My Sprite Dumps (Json).", boldStyle);
        GUILayout.Label("修复我的图像导出中的名称（Json）");
        GUILayout.Label("My Sprite Dumps Folder*");
        _owningSpriteDumpsFolder = (DefaultAsset)EditorGUILayout.ObjectField("我的图像导出文件夹*", _owningSpriteDumpsFolder, typeof(DefaultAsset), false);
        GUILayout.Space(5);
        GUILayout.Label("Source Sprite Dumps Folder*");
        _sourceSpriteDumpsFolder = (DefaultAsset)EditorGUILayout.ObjectField("源图像导出文件夹*", _sourceSpriteDumpsFolder, typeof(DefaultAsset), false);
        GUILayout.Space(5);

        GUI.enabled = !string.IsNullOrWhiteSpace(_owningAtlasFilePath) || (_owningSpriteDumpsFolder != null && _sourceSpriteDumpsFolder != null);

        if (GUILayout.Button("Run (运行)"))
        {
            FixNamesOfSpriteDumps();
            FixNamesOfSpritesInAtlas();
        }

        GUI.enabled = true;

        GUILayout.Space(35);

        string messageEn = "Usage: fix names of Sprites/SpriteAtlas Dumps. During one run you can just " +
                            "fix either one of them. Detail: 1. for each Sprite's file name, the hashtag" +
                            "part will be removed, and its Bundle Id will be fixed, as well as its Path Id." +
                            "In the end, the tool fixes its 'm_Name' value. 2. for SpriteAtlas dump, each value in " +
                            "m_PackedSpriteNamesToIndex/Array will be removed the hashtag part.";
        string messageZh = "用途：修复图像/图集导出中的名称。一次运行你可以只修复图像或者图集。具体修复内容：1. " +
                            "每个图像的文件名的' #'部分会被移除，而且资源包的ID也会被修复，其次是路径ID。最后" +
                            "是文件中的'm_Name'值。2. 在图集导出文件里，每个在m_PackedSpriteNamesToIndex/Array中的值都会被移除" +
                            "' #'部分。";

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

    private void FixNamesOfSpriteDumps()
    {
        if (_owningSpriteDumpsFolder == null)
        {
            Debug.LogWarning("Could not find My Sprite Dumps Folder or Source Sprite Dumps Folder, skip calling FixNamesOfSpriteDumps().");
            return;
        }

        // The owning and source CAB parts
        string owningCabPart = "";
        string sourceCabPart = "";

        // Get source Sprite Dump Names to Sprite Dump Path ID dictionary for later
        Dictionary<string, List<string>> sourceNameID = new();
        string sourceSpriteDumpFolderPath = AssetDatabase.GetAssetPath(_sourceSpriteDumpsFolder);
        string[] sourceJsonFiles = Directory.GetFiles(sourceSpriteDumpFolderPath, "*.json", SearchOption.TopDirectoryOnly);
        foreach (string jsonFile in sourceJsonFiles)
        {
            JObject spriteJson = JObject.Parse(File.ReadAllText(jsonFile));
            if (spriteJson.ContainsKey("m_Rect")) // Make sure it's a Sprite Dump File
            {
                string fileName = Path.GetFileName(jsonFile);
                var name = fileName.Split("-CAB-")[0]; // The actual name without hashtag part (might conflict with others but that's ok)
                Match match = Regex.Match(fileName, @"-(\-?\d+)(?=\.\w+$)");
                long pathID = 0; // The Path ID (the last section stored in dump file name)
                if (match.Success && long.TryParse(match.Groups[1].Value, out long matchedID))
                {
                    pathID = matchedID;
                }
                else
                {
                    Debug.LogError($"No Path ID found in the name of {fileName}.");
                }

                if (sourceCabPart == "")
                {
                    match = Regex.Match(fileName, @"CAB-[0-9a-fA-F]{32}");
                    if (match.Success)
                    {
                        sourceCabPart = match.Value;
                    }
                }

                if (!sourceNameID.ContainsKey(name))
                {
                    sourceNameID.Add(name, new() { pathID.ToString() });
                }
                else
                {
                    sourceNameID[name].Add(pathID.ToString());
                    Debug.LogWarning($"{name} is duplicated, current stored IDs [{string.Join(", ", sourceNameID[name])}].");
                }
            }
        }

        string owningSpriteDumpFolderPath = AssetDatabase.GetAssetPath(_owningSpriteDumpsFolder);
        string[] owningJsonFiles = Directory.GetFiles(owningSpriteDumpFolderPath, "*.json", SearchOption.TopDirectoryOnly);
        foreach (string jsonFile in owningJsonFiles)
        {
            JObject spriteJson = JObject.Parse(File.ReadAllText(jsonFile));
            string name = (string)spriteJson["m_Name"];
            if (spriteJson.ContainsKey("m_Rect")) // Make sure it's a Sprite Dump File
            {
                // 1. Remove hashtag part
                string newName = name.Split(" #")[0];
                spriteJson["m_Name"] = newName;
                File.WriteAllText(jsonFile, JsonConvert.SerializeObject(spriteJson, Formatting.Indented));
                string finalName = Regex.Replace(Path.GetFileName(jsonFile), @"\s#\d+", "");

                // 2. Replace Path ID in the file name with the correct Path ID (based on source dumps)
                // The hashtag part indicates that two Dumps are almost identical.
                if (!sourceNameID.ContainsKey(newName))
                {
                    Debug.LogWarning($"{newName} is not in sourceNameID dictionary. It's probably extra.");
                    continue;
                }
                string sourcePathID = sourceNameID[newName][0];
                sourceNameID[newName].RemoveAt(0);
                Match match = Regex.Match(finalName, @"-(\-?\d+)(?=\.\w+$)");
                long owningPathID = 0; // The Path ID (the last section stored in dump file name)
                if (match.Success && long.TryParse(match.Groups[1].Value, out long matchedID))
                {
                    owningPathID = matchedID;
                }
                else
                {
                    Debug.LogError($"No Path ID found in the name of {finalName}.");
                }
                finalName = finalName.Replace(owningPathID.ToString(), sourcePathID.ToString());

                if (owningCabPart == "")
                {
                    match = Regex.Match(finalName, @"CAB-[0-9a-fA-F]{32}");
                    if (match.Success)
                    {
                        owningCabPart = match.Value;
                    }
                }

                // 3. Replace the CAB part to source CAB part
                finalName = finalName.Replace(owningCabPart, sourceCabPart);

                AssetDatabase.RenameAsset(jsonFile, finalName);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void FixNamesOfSpritesInAtlas()
    {
        if (_owningAtlasFilePath == "")
        {
            Debug.LogWarning("My SpriteAtlas Dump not found, skip calling FixNamesOfSpritesInAtlas().");
            return;
        }

        JObject _owningAtlasFileJson = JObject.Parse(File.ReadAllText(_owningAtlasFilePath));

        var packedSpriteNameToIndexOwning = _owningAtlasFileJson["m_PackedSpriteNamesToIndex"]["Array"];
        for (int i = 0; i < packedSpriteNameToIndexOwning.Count(); i++)
        {
            var name = (string)packedSpriteNameToIndexOwning[i];
            string[] splitName = name.Split(" #");
            if (splitName.Length > 1)
            {
                packedSpriteNameToIndexOwning[i] = splitName[0];
            }
        }

        _owningAtlasFileJson["m_PackedSpriteNamesToIndex"]["Array"] = packedSpriteNameToIndexOwning;
        File.WriteAllText(_owningAtlasFilePath, JsonConvert.SerializeObject(_owningAtlasFileJson, Formatting.Indented));
    }
}
