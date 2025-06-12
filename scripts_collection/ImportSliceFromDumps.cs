using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System;


public class ImportSliceFromDumps : EditorWindow
{
    private readonly struct RenderDataKey
    {
        readonly uint firstData0;
        readonly uint firstData1;
        readonly uint firstData2;
        readonly uint firstData3;
        readonly long second;
        public RenderDataKey(uint firstData0, uint firstData1, uint firstData2, uint firstData3, long second)
        {
            this.firstData0 = firstData0;
            this.firstData1 = firstData1;
            this.firstData2 = firstData2;
            this.firstData3 = firstData3;
            this.second = second;
        }

        public bool Compare(RenderDataKey other)
        {
            return firstData0 == other.firstData0 &&
                firstData1 == other.firstData1 &&
                firstData2 == other.firstData2 &&
                firstData3 == other.firstData3 &&
                second == other.second;
        }

        public override string ToString()
        {
            return $"\"first\"[{firstData0}, {firstData1}, {firstData2}, {firstData3}], \"second\": {second}";
        }
    }

    private string _spriteDumpsFolderPath;
    private Texture2D _targetTexture;
    private string _sourceAtlasFilePath = ""; // if used

    [MenuItem("Tools/02 Import Spritesheet from Dumps", priority = 2)]
    public static void ShowWindow()
    {
        GetWindow<ImportSliceFromDumps>("Import Sprite Slices from Dumps");
    }

    private void OnGUI()
    {
        GUIStyle style = new(GUI.skin.label)
        {
            wordWrap = true
        };

        GUILayout.Label("⚠️Warning: You must run tool 'Import Spritesheet from Sprites' before you run this tool!");
        GUILayout.Label("⚠️警告：在使用该工具前，请先运行Import Spritesheet from Sprites工具！");
        GUILayout.Space(35);

        GUILayout.Label("Required \n必填", EditorStyles.boldLabel);
        GUILayout.Label("Target Texture*");
        _targetTexture = (Texture2D)EditorGUILayout.ObjectField("材质文件*", _targetTexture, typeof(Texture2D), false);
        _spriteDumpsFolderPath = DragAndDropFolderField("Source Sprite Dump Folder (Json files)* \n源图像导出文件夹 (Json文件)*", _spriteDumpsFolderPath);

        GUILayout.Space(35);

        GUILayout.Label("Required if Sprite Atlas is used, assign the Source SpriteAtlas Dump Json file.", style);
        GUILayout.Label("如果使用图集则必填，放入源图集的导出Json文件。", style);
        GUILayout.Space(5);

        _sourceAtlasFilePath = DragAndDropFileField("Source SpriteAtlas File \n源图集的导出文件", _sourceAtlasFilePath, "json");

        GUI.enabled = _targetTexture != null && !string.IsNullOrWhiteSpace(_spriteDumpsFolderPath);

        if (GUILayout.Button("Run (运行)") && _spriteDumpsFolderPath != null)
        {
            // ImportSlice();

            // Dictionary<long, RenderDataKey> id2rectdata = SpritePathID2RenderDataKey();
            // foreach (var item in id2rectdata)
            // {
            //     Debug.Log($"{item.Key}: {item.Value}");
            // }
            string assetPath = AssetDatabase.GetAssetPath(_targetTexture);
            TextureImporter ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            List<SpriteMetaData> metasToImport = new();

            Dictionary<string, Vector2> identified = FindIdentifiedInfo();
            if (identified == null)
            {
                Debug.LogError("Something is wrong. You probably didn't assign the correct Sprite folder for Texture2D.");
            }
            else
            {
                metasToImport.AddRange(ImportIdentifiedSlices(identified, ti.spritesheet));
            }

            List<Rect> unidentified = FindUnidentifiedInfo();
            if (unidentified == null)
            {
                Debug.LogWarning("Source SpriteAtlas Dump not assigned, skip ImportUnidentifiedSlices().");
            }
            else
            {
                // foreach (Rect item in unidentified)
                // {
                //     Debug.Log(item);
                // }
                Debug.LogWarning("Please remember to rename the unidentified Sprites.");
                metasToImport.AddRange(ImportUnidentifiedSlices(unidentified, metasToImport));
            }

            if (ti != null)
            {
                ti.spritesheet = metasToImport.ToArray();
                EditorUtility.SetDirty(ti);
                ti.SaveAndReimport();
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                ViewSpriteSheet(_targetTexture);
            }
        }

        GUI.enabled = true;

        GUILayout.Space(35);

        string messageEn = "Usage: automatically import Sprites for Texture2D through Dump files. This tool will only add new " +
                            "Sprites (not override existing Sprites). Also, this comparison is done by names. "+
                            "The tool will load pivot data from dump files for existing Sprites in the spritesheet. " +
                            "If you are using " +
                            "Sprite Atlas, there is a chance that the SpriteAtlas Dump contains more information than Sprite " +
                            "Dump files. This program creates unnamed copy of the Sprite in spritesheet for those extra info. " +
                            "You will need to *manually* rename those Sprites otherwise you will get an error. You can find " +
                            "names of Sprites that are not assigned in the Unity Console. Pick the name of the Sprites with " +
                            "those names. (If a name is used you can't use it again) If you find two same names, then their " +
                            "Sprites probably have the same size, put ' #1' after the duplicate. It's recommended to use " +
                            "Sprite Renamer tool to rename the Sprites.";
        string messageZh = "用途：从导出文件自动载入图像。该工具仅添加图像（不会改写目前图像），并且图像对比用的是名字。" +
                            "工具会从导出文件中寻找并载入已存在图像的锚点。" +
                            "如果你使用了" +
                            "图集，那么有可能图集会拥有比图像导出更多的图像。该工具会为这些额外的图像创建拷贝，并使用临时名称，以格式" +
                            "为'0000_aaa'开头后面是一个数字。你需要人工修改这些名字。在Unity的终端你会找到这些图像可能的名字。选择一个合适" +
                            "图像的名称（如果一个名字用过了就不能再用了）。如果有两个图像的名字相同，那么它们很可能有相同的尺寸。" +
                            "找到其复制并在它的名字后面加' #1'。最后，我极力推荐使用Sprite Renamer工具修改图像名称。";
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

    private List<SpriteMetaData> ImportUnidentifiedSlices(List<Rect> rects, List<SpriteMetaData> metasToImport)
    {
        static SpriteMetaData ParseDump(Rect rect, string name)
        {
            return new SpriteMetaData
            {
                name = name,
                rect = new Rect(rect.x, rect.y, rect.width, rect.height),
                pivot = new Vector2(0.5f, 0.5f),
                alignment = (int)SpriteAlignment.Custom
            };
        }

        List<SpriteMetaData> metas = new();
        int counter = 0;
        string baseName = "0000_aaa";
        foreach (Rect rect in rects)
        {
            SpriteMetaData data = ParseDump(rect, baseName + counter.ToString());
            metas.Add(data);
            counter++;
        }

        return ApplyToTextureUnidentified(metas, metasToImport);
    }

    private List<SpriteMetaData> ApplyToTextureUnidentified(List<SpriteMetaData> addMetas,
                                                            List<SpriteMetaData> currentMetas)
    {
        int GetValidNameIndex(int addMetaNameIndex,
                                HashSet<int> aaaNamesInCurrentMetasIndices,
                                HashSet<int> processedAddMetaNamesIndices)
        {
            bool IsInAnyList(int index)
            {
                return aaaNamesInCurrentMetasIndices.Contains(index) ||
                        processedAddMetaNamesIndices.Contains(index);
            }
            if (!IsInAnyList(addMetaNameIndex))
            {
                return addMetaNameIndex;
            }
            int iterateLen = aaaNamesInCurrentMetasIndices.Count() + processedAddMetaNamesIndices.Count() + 1;
            for (int i = 0; i < iterateLen; i++)
            {
                if (!IsInAnyList(i))
                {
                    return i;
                }
            }
            return -1;  // Shouldn't be possible to get here
        }

        int ParseIndex(string aaaName)
        {
            return int.Parse(aaaName.Replace("0000_aaa", ""));
        }

        // Do not override anything
        // Is a sprite with the same name has been found, try to rename
        // i.e. If 'aaa0' is in current sprites, rename my stuff to 'aaa1'
        HashSet<int> aaaNamesInCurrentMetasIndices = new();
        foreach (SpriteMetaData currentMeta in currentMetas)
        {
            if (currentMeta.name.StartsWith("0000_aaa"))
            {
                aaaNamesInCurrentMetasIndices.Add(ParseIndex(currentMeta.name));
            }
        }

        HashSet<int> processedAddMetaNamesIndices = new();
        for (int i = 0; i < addMetas.Count(); i++)
        {
            SpriteMetaData addMeta = addMetas[i];
            if (addMeta.name.StartsWith("0000_aaa"))
            {
                int addIndex = ParseIndex(addMeta.name);
                int validIndex = GetValidNameIndex(addIndex, aaaNamesInCurrentMetasIndices, processedAddMetaNamesIndices);
                processedAddMetaNamesIndices.Add(validIndex);
                addMeta = new SpriteMetaData
                {
                    name = "0000_aaa" + validIndex.ToString(),
                    rect = addMeta.rect,
                    pivot = addMeta.pivot,
                    alignment = addMeta.alignment,
                    border = addMeta.border
                };
                addMetas[i] = addMeta;
            }
        }

        return addMetas;
    }

    private List<SpriteMetaData> ImportIdentifiedSlices(Dictionary<string, Vector2> namePivots, SpriteMetaData[] currentMetas)
    {
        List<SpriteMetaData> result = new();
        for (int i = 0; i < currentMetas.Length; i++)
        {
            string currentName = currentMetas[i].name;
            currentName = currentName.Split(" #")[0];
            if (namePivots.ContainsKey(currentName))
            {
                SpriteMetaData curr = currentMetas[i];
                curr = new SpriteMetaData
                {
                    name = curr.name,
                    rect = curr.rect,
                    pivot = namePivots[currentName],
                    alignment = curr.alignment,
                    border = curr.border
                };
                result.Add(curr);
            }
            else
            {
                Debug.LogError($"Sprite named {currentName} not found. Are you certain the Source Dump Folder is correct?");
            }
        }
        return result;
    }

    private Dictionary<string, Vector2> FindIdentifiedInfo()
    {
        /*
            Find the identified Rects (all appear in Sprite Dumps).
        */
        Dictionary<string, Vector2> result = new();
        string[] jsonFiles = Directory.GetFiles(_spriteDumpsFolderPath, "*.json", SearchOption.TopDirectoryOnly);
        foreach (string jsonFile in jsonFiles)
        {
            JObject spriteJson = JObject.Parse(File.ReadAllText(jsonFile));
            if (spriteJson.ContainsKey("m_Rect"))
            {
                var r = spriteJson["m_Rect"];
                Rect rect = new(float.Parse(r["x"].ToString()),
                                float.Parse(r["y"].ToString()),
                                float.Parse(r["width"].ToString()),
                                float.Parse(r["height"].ToString()));
                var offset = spriteJson["m_Offset"];
                float pivotX = float.Parse(offset["x"].ToString()) / rect.width;
                float pivotY = float.Parse(offset["y"].ToString()) / rect.height;
                var name = spriteJson["m_Name"].ToString();
                if (!result.ContainsKey(name))
                {
                    result.Add(name, new(pivotX, pivotY));
                }
            }
        }
        return result;
    }

    private List<Rect> FindUnidentifiedInfo()
    {
        /*
            Find the unidentified Rects (i.e. if one appears in Source SpriteAtlas Dump but not
            in the Sprite Dumps). Also print names in Source SpriteAtlas that are not found in Sprite Dumps.
        */
        long? SearchRenderDataKey(Dictionary<long, RenderDataKey> pairs, RenderDataKey target)
        {
            foreach (var pair in pairs)
            {
                if (target.Compare(pair.Value))
                {
                    return pair.Key;
                }
            }
            return null;
        }

        if (_sourceAtlasFilePath == "")
        {
            return null;
        }

        List<Rect> unidentifiedRects = new();

        JObject _sourceAtlasFileJson = JObject.Parse(File.ReadAllText(_sourceAtlasFilePath));
        var packedSpritesSource = _sourceAtlasFileJson["m_PackedSprites"]["Array"];
        var packedSpriteNameToIndexSource = _sourceAtlasFileJson["m_PackedSpriteNamesToIndex"]["Array"];

        Dictionary<long, string> sourceIDName = new();
        // source name & source id
        for (int i = 0; i < packedSpritesSource.Count(); i++)
        {
            var packedSprite = packedSpritesSource[i];
            var pathID = long.Parse(packedSprite["m_PathID"].ToString());
            var name = (string)packedSpriteNameToIndexSource[i];
            sourceIDName.Add(pathID, name);
        }

        Dictionary<long, RenderDataKey> spritePathID2RenderDataKey = SpritePathID2RenderDataKey();
        List<long> foundIDs = new();

        var renderDataMapSource = _sourceAtlasFileJson["m_RenderDataMap"]["Array"];
        for (int i = 0; i < renderDataMapSource.Count(); i++)
        {
            var renderKeyData = renderDataMapSource[i]["first"];
            RenderDataKey rdk = new(uint.Parse(renderKeyData["first"]["data[0]"].ToString()),
                                    uint.Parse(renderKeyData["first"]["data[1]"].ToString()),
                                    uint.Parse(renderKeyData["first"]["data[2]"].ToString()),
                                    uint.Parse(renderKeyData["first"]["data[3]"].ToString()),
                                    long.Parse(renderKeyData["second"].ToString()));

            long? found = SearchRenderDataKey(spritePathID2RenderDataKey, rdk);
            if (found != null) // rdk is identified if it's found in spritePathID2RenderDataKey
            {
                foundIDs.Add((long)found);
            }
            else
            {
                // Find rect
                var textureRect = renderDataMapSource[i]["second"]["textureRect"];
                Rect rect = new(float.Parse(textureRect["x"].ToString()),
                                float.Parse(textureRect["y"].ToString()),
                                float.Parse(textureRect["width"].ToString()),
                                float.Parse(textureRect["height"].ToString()));
                unidentifiedRects.Add(rect);
            }
        }

        // Debug.Log($"Found id len: {foundIDs.Count()}");
        // Debug.Log($"Source id name keys len: {sourceIDName.Keys.Count()}");

        foreach (long id in sourceIDName.Keys)
        {
            if (!foundIDs.Contains(id))
            {
                // Print names in Source SpriteAtlas that are not found in Sprite Dumps
                Debug.LogWarning($"Unidentified id: {id}, name: {sourceIDName[id]}");
            }
        }
        // Debug.Log($"Result len: {result.Count}");

        return unidentifiedRects;
    }


    private Dictionary<long, RenderDataKey> SpritePathID2RenderDataKey()
    {
        Dictionary<long, RenderDataKey> result = new();
        string[] jsonFiles = Directory.GetFiles(_spriteDumpsFolderPath, "*.json", SearchOption.TopDirectoryOnly);
        foreach (string jsonFile in jsonFiles)
        {
            JObject spriteJson = JObject.Parse(File.ReadAllText(jsonFile));
            if (spriteJson.ContainsKey("m_Rect"))
            {
                var renderKeyData = spriteJson["m_RenderDataKey"];
                string fileName = Path.GetFileName(jsonFile);
                Match match = Regex.Match(fileName, @"-(\-?\d+)(?=\.\w+$)");
                // matchedID: the last section stored in dump file name
                if (match.Success && long.TryParse(match.Groups[1].Value, out long matchedID))
                {
                    result.Add(matchedID, new(uint.Parse(renderKeyData["first"]["data[0]"].ToString()),
                                            uint.Parse(renderKeyData["first"]["data[1]"].ToString()),
                                            uint.Parse(renderKeyData["first"]["data[2]"].ToString()),
                                            uint.Parse(renderKeyData["first"]["data[3]"].ToString()),
                                            long.Parse(renderKeyData["second"].ToString())));
                }
                else
                {
                    Debug.LogError($"No Path ID found in the name of {fileName}.");
                }
            }
        }
        return result;
    }

    // Debug
    private void ViewSpriteSheet(Texture2D texture)
    {
        if (texture == null)
        {
            Debug.LogWarning("No texture provided.");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(texture);
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

        if (importer == null)
        {
            Debug.LogError($"Could not get TextureImporter for: {assetPath}");
            return;
        }

        if (importer.spriteImportMode != SpriteImportMode.Multiple)
        {
            Debug.LogWarning("Texture is not set to Sprite (Multiple). Nothing to show.");
            return;
        }

        // Get the slicing data from the importer
        SpriteMetaData[] metas = importer.spritesheet;

        Debug.Log($"Sprite sheet info for '{texture.name}' ({metas.Length} sprites):");

        string log = "";
        foreach (var meta in metas)
        {
            log += $"{meta.name}, " +
                    $"{meta.rect.x} {meta.rect.y} {meta.rect.width} {meta.rect.height}\n";
        }

        // Ensure the folder exists
        string folderPath = "Assets/Logs";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        // Full file path
        string assetFilePath = Path.Combine(folderPath, "log.txt");

        // Write to file
        File.WriteAllText(assetFilePath, log);

        // Refresh the AssetDatabase so Unity sees the new file
        AssetDatabase.ImportAsset(assetFilePath);
        AssetDatabase.Refresh();
    }
}
