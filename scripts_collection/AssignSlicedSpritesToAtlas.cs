using UnityEngine;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine.U2D;
using System.Collections.Generic;


public class AssignSlicedSpritesToAtlas : EditorWindow
{
    private SpriteAtlas _spriteAtlas;
    private Texture2D _slicedTexture;
    private bool _removeExisting;

    // Persistent setting throughout editor sessions
    private const string RemoveExistingKey = "AssignSlicedSpritesToAtlas_RemoveExisting";

    [MenuItem("Tools/04 Assign Sprites to SpriteAtlas", priority = 4)]
    public static void ShowWindow()
    {
        GetWindow<AssignSlicedSpritesToAtlas>("Assign Sliced Sprites");
    }

    private void OnEnable()
    {
        _removeExisting = EditorPrefs.GetBool(RemoveExistingKey, true);
    }

    private void OnGUI()
    {
        GUIStyle style = new(GUI.skin.label)
        {
            wordWrap = true
        };

        GUILayout.Label("Remove existing items from Atlas.");
        _removeExisting = EditorGUILayout.Toggle("移除现有图集内容", _removeExisting);
        GUILayout.Space(35);

        GUILayout.Label("Assign the Texture2D with Sprites to Sprite Atlas");
        GUILayout.Label("放入有切割过图像的材质文件到图集文件");
        GUILayout.Space(5);

        GUILayout.Label("Sprite Atlas*");
        _spriteAtlas = (SpriteAtlas)EditorGUILayout.ObjectField("图集文件*", _spriteAtlas, typeof(SpriteAtlas), false);
        GUILayout.Label("Texture2D*");
        _slicedTexture = (Texture2D)EditorGUILayout.ObjectField("材质文件*", _slicedTexture, typeof(Texture2D), false);
        GUILayout.Space(5);

        GUI.enabled = _spriteAtlas != null && _slicedTexture != null;
        if (GUILayout.Button("Run (运行)"))
        {
            if (_spriteAtlas == null || _slicedTexture == null)
            {
                Debug.LogWarning("Please assign both a SpriteAtlas and a Texture2D.");
                return;
            }

            AssignSprites();
        }

        GUI.enabled = true;

        GUILayout.Space(35);

        string messageEn = "Usage: Removes all currently packed packables in the Atlas (by default) and load all sliced" + 
                            "Sprites in Spritesheet to it.";
        string messageZh = "用途：移除所选择的图集中所有的资源（初始设置），并从材质文件载入所有切割过的图像。";

        
        GUILayout.Label(messageEn, style);
        GUILayout.Label(messageZh, style);
    }

    private void AssignSprites()
    {
        string texturePath = AssetDatabase.GetAssetPath(_slicedTexture);
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);

        List<Object> slicedSprites = new();
        foreach (Object asset in assets)
        {
            if (asset is Sprite && asset != _slicedTexture)
            {
                slicedSprites.Add(asset);
            }
        }

        if (slicedSprites.Count == 0)
        {
            Debug.LogWarning("No sliced sprites found in the texture.");
            return;
        }

        if (_removeExisting)
        {
            _spriteAtlas.Remove(_spriteAtlas.GetPackables());
        }
        _spriteAtlas.Add(slicedSprites.ToArray());

        EditorUtility.SetDirty(_spriteAtlas);
        AssetDatabase.SaveAssets();
        Debug.Log($"Assigned {slicedSprites.Count} sliced sprites from {_slicedTexture.name} to {_spriteAtlas.name}");
    }

    private void OnDisable()
    {
        EditorPrefs.SetBool(RemoveExistingKey, _removeExisting);
    }
}
