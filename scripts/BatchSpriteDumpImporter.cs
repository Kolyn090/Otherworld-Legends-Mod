using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;


public class BatchSpriteDumpImporter : EditorWindow
{
    private DefaultAsset folderWithDumps;
    private Texture2D manualTexture;

    [MenuItem("Tools/Batch Import Sprites From Dumps")]
    public static void ShowWindow()
    {
        GetWindow<BatchSpriteDumpImporter>("Batch Dump Importer");
    }

    void OnGUI()
    {
        GUILayout.Label("Batch Import Sprite Slices From Folder", EditorStyles.boldLabel);
        folderWithDumps = (DefaultAsset)EditorGUILayout.ObjectField("Dump Folder", folderWithDumps, typeof(DefaultAsset), false);
        manualTexture = (Texture2D)EditorGUILayout.ObjectField("Target Texture", manualTexture, typeof(Texture2D), false);

        if (GUILayout.Button("Import All Dumps") && folderWithDumps != null)
        {
            string folderPath = AssetDatabase.GetAssetPath(folderWithDumps);
            ImportAllDumps(folderPath);
        }
    }

    void ImportAllDumps(string folderPath)
    {
        string[] txtFiles = Directory.GetFiles(folderPath, "*.txt", SearchOption.TopDirectoryOnly);
        int successCount = 0;
        int failCount = 0;
        List<SpriteMetaData> allMetas = new List<SpriteMetaData>();

        foreach (var txtFile in txtFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(txtFile);
            string texturePath = $"{folderPath}/{fileName}.png";

            TextAsset dump = AssetDatabase.LoadAssetAtPath<TextAsset>(txtFile.Replace(Application.dataPath, "Assets"));
            if (dump == null)
            {
                Debug.LogWarning($"Could not load dump asset: {txtFile}");
                failCount++;
                continue;
            }

            List<SpriteMetaData> metas = ParseDump(dump.text, manualTexture.height);
            allMetas.AddRange(metas);
        }

        ApplyToTexture(manualTexture, allMetas);

        Debug.Log($"Finished processing dumps. Success: {successCount}, Failed: {failCount}");
    }

    List<SpriteMetaData> ParseDump(string text, int textureHeight)
    {
        List<SpriteMetaData> metas = new List<SpriteMetaData>();
        string[] lines = text.Split('\n');

        string name = "";
        float rectX = 0, rectY = 0, rectW = 0, rectH = 0;
        float pivotX = 0.5f, pivotY = 0.5f;
        bool readingRect = false;
        bool readingPivot = false;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            if (line.StartsWith("string m_Name"))
            {
                if (!string.IsNullOrEmpty(name))
                {
                    metas.Add(new SpriteMetaData
                    {
                        name = name,
                        rect = new Rect(rectX, rectY, rectW, rectH),
                        pivot = new Vector2(pivotX, pivotY),
                        alignment = (int)SpriteAlignment.Custom
                    });
                }

                name = Regex.Match(line, "\"(.*?)\"").Groups[1].Value;
                readingRect = false;
                readingPivot = false;
            }

            if (line.StartsWith("Rectf m_Rect"))
            {
                readingRect = true;
                continue;
            }

            if (readingRect)
            {
                if (line.StartsWith("float x ="))
                    rectX = float.Parse(line.Split('=')[1].Trim());
                else if (line.StartsWith("float y ="))
                    rectY = float.Parse(line.Split('=')[1].Trim());
                else if (line.StartsWith("float width ="))
                    rectW = float.Parse(line.Split('=')[1].Trim());
                else if (line.StartsWith("float height ="))
                {
                    rectH = float.Parse(line.Split('=')[1].Trim());
                    readingRect = false;
                }
                continue;
            }

            if (line.StartsWith("Vector2f m_Pivot"))
            {
                readingPivot = true;
                continue;
            }

            if (readingPivot)
            {
                if (line.StartsWith("float x ="))
                    pivotX = float.Parse(line.Split('=')[1].Trim());
                else if (line.StartsWith("float y ="))
                {
                    pivotY = float.Parse(line.Split('=')[1].Trim());
                    readingPivot = false;
                }
            }
        }

        if (!string.IsNullOrEmpty(name))
        {
            metas.Add(new SpriteMetaData
            {
                name = name,
                rect = new Rect(rectX, rectY, rectW, rectH),
                pivot = new Vector2(pivotX, pivotY),
                alignment = (int)SpriteAlignment.Custom
            });
        }

        return metas;
    }

    void ApplyToTexture(Texture2D texture, List<SpriteMetaData> metas)
    {
        string assetPath = AssetDatabase.GetAssetPath(texture);
        TextureImporter ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;

        if (ti != null)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Multiple;
            ti.spritesheet = metas.ToArray();
            EditorUtility.SetDirty(ti);
            ti.SaveAndReimport();
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }
        else
        {
            Debug.LogWarning($"{assetPath} not found.");
        }
    }
}
