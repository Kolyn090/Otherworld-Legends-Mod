using UnityEngine;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine.U2D;
using System.Collections.Generic;


public class AssignSlicedSpritesToAtlas : EditorWindow
{
    /*
        Removes all currently packed packables in the atlas (by default) and load all sliced 
        sprites in spritesheet to it. Use this tool to greatly speed up loading sprites into atlas 
        because Unity 2021.3 doesn't seem to offer batch import but this script can do it.

        移除所选择的atlas中所有的资源（初始设置），并从spritesheet载入所有切割过的sprite。该工具属于提升工作
        效率类。Unity 2021.3 似乎并不提供多文件载入到atlas但是你可以使用该工具来快速完成。
    */
    private SpriteAtlas _spriteAtlas;
    private Texture2D _slicedTexture;
    private bool _removeExisting;

    // Persistent setting throughout editor sessions
    private const string RemoveExistingKey = "AssignSlicedSpritesToAtlas_RemoveExisting";

    [MenuItem("Tools/Assign Sliced Sprites to Atlas")]
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
        _removeExisting = EditorGUILayout.Toggle("Remove existing from atlas.", _removeExisting);

        GUILayout.Label("Assign Sliced Sprites (Multi-Sprite) to Sprite Atlas", EditorStyles.boldLabel);

        _spriteAtlas = (SpriteAtlas)EditorGUILayout.ObjectField("Sprite Atlas", _spriteAtlas, typeof(SpriteAtlas), false);
        _slicedTexture = (Texture2D)EditorGUILayout.ObjectField("Sliced Texture2D", _slicedTexture, typeof(Texture2D), false);

        if (GUILayout.Button("Assign Sprites"))
        {
            if (_spriteAtlas == null || _slicedTexture == null)
            {
                Debug.LogWarning("Please assign both a SpriteAtlas and a Texture2D.");
                return;
            }

            AssignSprites();
        }
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
