using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


public class AutoSpriteSheetSlicerFromFolder : EditorWindow
{
    /*
        Takes in a Texture2D and a folder of sliced sprites of it (The items in the
        folder must be the result of slicing of this Texture2D), apply slicing on the
        Texture2D based on the provided Sprites (It's like reversing the process from
        sliced sprites to sliced Texture2D). Name and size of each sprite will be copied.

        逆向sprite切割，用已经切割好的sprite来切割Texture2D。提供的sprite必须是要从
        Texture2D中切割出的。所有sprite的名字，尺寸都会被复制。
    */

    struct PixelOffsetColor
    {
        public int x, y;
        public Color color;
    }

    private Texture2D _textureToSlice;
    private DefaultAsset _spriteFolder;
    private float _colorTolerance = 0.01f;

    [MenuItem("Tools/Slice Texture Based on Auto Sprite Matching")]
    public static void ShowWindow()
    {
        GetWindow<AutoSpriteSheetSlicerFromFolder>("Auto Sprite Slicer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Auto Slice Texture from Sprite Folder", EditorStyles.boldLabel);
        _textureToSlice = (Texture2D)EditorGUILayout.ObjectField("Texture To Slice", _textureToSlice, typeof(Texture2D), false);
        _spriteFolder = (DefaultAsset)EditorGUILayout.ObjectField("Folder of Sprites", _spriteFolder, typeof(DefaultAsset), false);
        _colorTolerance = EditorGUILayout.Slider("Color Tolerance", _colorTolerance, 0f, 0.1f);

        if (GUILayout.Button("Slice Based on Match"))
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
