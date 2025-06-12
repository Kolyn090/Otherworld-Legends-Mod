using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


public class AutoSpriteSheetSlicerFromFolder : EditorWindow
{
    private struct PixelOffsetColor
    {
        public int x, y;
        public Color color;
    }

    private Texture2D _textureToSlice;
    private DefaultAsset _spriteFolder;
    private float _colorTolerance = 0.01f;

    [MenuItem("Tools/01 Import Spritesheet from Sprites", priority = 1)]
    public static void ShowWindow()
    {
        GetWindow<AutoSpriteSheetSlicerFromFolder>("Import Spritesheet from Sprites");
    }

    private void OnGUI()
    {
        GUIStyle style = new(GUI.skin.label)
        {
            wordWrap = true
        };

        GUILayout.Label("Target Texture*");
        _textureToSlice = (Texture2D)EditorGUILayout.ObjectField("材质文件*", _textureToSlice, typeof(Texture2D), false);
        GUILayout.Label("Sprite Assets Folder*");
        _spriteFolder = (DefaultAsset)EditorGUILayout.ObjectField("图像资源文件夹*", _spriteFolder, typeof(DefaultAsset), false);
        _colorTolerance = EditorGUILayout.Slider("Color Tolerance (颜色容差)", _colorTolerance, 0f, 0.1f);
        GUILayout.Space(5);

        if (GUILayout.Button("Run (运行)"))
        {
            if (_textureToSlice == null || _spriteFolder == null)
            {
                Debug.LogWarning("Assign both texture and sprite folder.");
                return;
            }

            string texturePath = AssetDatabase.GetAssetPath(_textureToSlice);
            TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (!importer.isReadable)
            {
                importer.isReadable = true;
                AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
            }

            // SliceTextureByMatching();
            SliceTexture();
        }

        GUI.enabled = true;

        GUILayout.Space(35);

        string messageEn = "Usage: Reverse-Engineer a Spritesheet. Get the Sprites through tools like AssetStudio. " +
                            "Name and size of each sprite will be copied " +
                            "and will be automatically positioned. Normally, you don't have to adjust Color " +
                            "Tolerance. There is a chance that the tool not able to find the target Sprite if " +
                            "the value is too small and it could cause the Sprite to be mispositioned if the " +
                            "value is too large.";
        string messageZh = "用途：逆向图像切割，用图像资源切割材质。图像资源可从AssetStudio等工具取得。" +
                            "所有图像的名称，尺寸都会被复制，并且位置会被自动对齐。颜色容差一般情况可以不用调，" +
                            "值过小可能会造成找不到目标图像，值过大可能会造成错位。";

        GUILayout.Label(messageEn, style);
        GUILayout.Label(messageZh, style);
    }

    private void SliceTexture()
    {
        string texturePath = AssetDatabase.GetAssetPath(_textureToSlice);
        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(texturePath);

        if (importer == null)
        {
            Debug.LogError("Selected texture is not a valid importable asset.");
            return;
        }

        // Read existing spritesheet
        SpriteMetaData[] existingMeta = importer.spritesheet;

        string folderPath = AssetDatabase.GetAssetPath(_spriteFolder);
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });

        Dictionary<string, SpriteMetaData> newMetaDict = new Dictionary<string, SpriteMetaData>();

        List<Sprite> sprites = new();

        foreach (string guid in guids)
        {
            string spritePath = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite == null) continue;
            sprites.Add(sprite);
        }

        List<SpriteMetaData> metas = new();
        foreach (var sprite in sprites)
        {
            Texture2D spriteTex = GetSpriteTexture(sprite);
            if (spriteTex == null) continue;

            Rect? foundRect = FindSpriteInTexture(_textureToSlice, spriteTex, _colorTolerance);

            if (foundRect.HasValue)
            {
                SpriteMetaData metaData = new SpriteMetaData
                {
                    alignment = 9,
                    border = sprite.border,
                    name = sprite.name,
                    pivot = new Vector2(0.5f, 0.5f),
                    rect = foundRect.Value
                };
                // Debug.Log(sprite);
                metas.Add(metaData);
                newMetaDict[sprite.name] = metaData;
            }
        }

        if (metas.Count == 0)
        {
            Debug.LogError("No valid sprites found in the folder.");
            return;
        }

        List<SpriteMetaData> finalMeta = new List<SpriteMetaData>();
        // Keep existing sprites that are not replaced
        foreach (var meta in existingMeta)
        {
            if (!newMetaDict.ContainsKey(meta.name))
            {
                finalMeta.Add(meta);
            }
        }

        // Add new/updated sprites
        finalMeta.AddRange(newMetaDict.Values);

        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritesheet = finalMeta.ToArray();

        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();

        Debug.Log($"✅ Sliced texture into {metas.Count} sprites.");
        // foreach (var meta in metas)
        //     Debug.Log($" - {meta.name} @ {meta.rect}");
    }

    // Extracts a readable portion of the original texture
    private Texture2D GetSpriteTexture(Sprite sprite)
    {
        Texture2D srcTex = sprite.texture;
        Rect rect = sprite.rect;

        Texture2D newTex = new((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
        newTex.SetPixels(srcTex.GetPixels(
            (int)rect.x,
            (int)(srcTex.height - rect.y - rect.height),
            (int)rect.width,
            (int)rect.height));
        newTex.Apply();
        return newTex;
    }

    private Rect? FindSpriteInTexture(Texture2D largeTexture, Texture2D smallSprite, float tolerance)
    {
        int W = largeTexture.width;
        int H = largeTexture.height;
        int w = smallSprite.width;
        int h = smallSprite.height;

        var spritePixels = GetVisiblePixels(smallSprite);

        for (int y = 0; y <= H - h; y++)
        {
            for (int x = 0; x <= W - w; x++)
            {
                if (MatchAt(largeTexture, spritePixels, x, y, tolerance))
                {
                    return new Rect(x, y, w, h);
                }
            }
        }
        return null;
    }

    private List<PixelOffsetColor> GetVisiblePixels(Texture2D tex, float alphaThreshold = 0.1f)
    {
        List<PixelOffsetColor> pixels = new();
        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                Color c = tex.GetPixel(x, y);
                if (c.a >= alphaThreshold)
                {
                    pixels.Add(new PixelOffsetColor { x = x, y = y, color = c });
                }
            }
        }
        return pixels;
    }

    private bool MatchAt(Texture2D big, List<PixelOffsetColor> spritePixels, int offsetX, int offsetY, float tolerance)
    {
        foreach (var px in spritePixels)
        {
            Color a = big.GetPixel(offsetX + px.x, offsetY + px.y);
            if (!ColorsSimilar(a, px.color, tolerance))
                return false;
        }
        return true;
    }

    private bool ColorsSimilar(Color a, Color b, float threshold)
    {
        return Mathf.Abs(a.r - b.r) < threshold &&
                Mathf.Abs(a.g - b.g) < threshold &&
                Mathf.Abs(a.b - b.b) < threshold &&
                Mathf.Abs(a.a - b.a) < threshold;
    }
}
