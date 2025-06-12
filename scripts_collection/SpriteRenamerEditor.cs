using UnityEditor;
using UnityEngine;

public class SpriteRenamerEditor : EditorWindow
{
    Sprite spriteToRename;
    string newSpriteName = "";

    [MenuItem("Tools/03 Rename Sprite", priority = 3)]
    public static void ShowWindow()
    {
        GetWindow<SpriteRenamerEditor>("Sprite Renamer");
    }

    private void OnGUI()
    {
        GUIStyle style = new(GUI.skin.label)
        {
            wordWrap = true
        };

        GUILayout.Label("Rename a Sprite in a Spritesheet", EditorStyles.boldLabel);
        GUILayout.Label("为图集中的一个图像重新命名", EditorStyles.boldLabel);
        GUILayout.Space(35);

        GUILayout.Label("Sprite*");
        spriteToRename = (Sprite)EditorGUILayout.ObjectField("图像*", spriteToRename, typeof(Sprite), false);
        GUILayout.Label("New Sprite Name*");
        newSpriteName = EditorGUILayout.TextField("新名称*", newSpriteName);
        GUILayout.Space(5);

        GUI.enabled = spriteToRename != null && !string.IsNullOrWhiteSpace(newSpriteName);
        if (GUILayout.Button("Run (运行)"))
        {
            RenameSpriteInSheet(spriteToRename, newSpriteName);
        }

        GUI.enabled = true;

        GUILayout.Space(35);

        string messageEn = "Usage: drag a Sprite from Spritesheet to the Sprite field and enter the new name." +
                            "The entire Spritesheet will be reimported and the selected Sprite will be renamed.";
        string messageZh = "用途：从图集中拖动一个要重新命名的图像到‘图像’区域。然后输入新的名称。" +
                            "整个图集会被先重新载入而且所选图像的名称会被修改。";

        GUILayout.Label(messageEn, style);
        GUILayout.Label(messageZh, style);
    }

    private static void RenameSpriteInSheet(Sprite sprite, string newName)
    {
        if (sprite == null)
        {
            Debug.LogError("No sprite selected.");
            return;
        }

        Texture2D texture = sprite.texture;
        string path = AssetDatabase.GetAssetPath(texture);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

        if (importer == null)
        {
            Debug.LogError("Failed to get TextureImporter for: " + path);
            return;
        }

        if (importer.spriteImportMode != SpriteImportMode.Multiple)
        {
            Debug.LogError("Sprite must come from a Texture2D in 'Multiple' mode.");
            return;
        }

        SpriteMetaData[] metas = importer.spritesheet;
        bool found = false;

        for (int i = 0; i < metas.Length; i++)
        {
            if (metas[i].name == sprite.name)
            {
                var modified = metas[i];
                modified.name = newName;
                metas[i] = modified;
                found = true;
                break;
            }
        }

        if (!found)
        {
            Debug.LogError($"Sprite '{sprite.name}' not found in spritesheet.");
            return;
        }

        importer.spritesheet = metas;
        Debug.Log($"Renamed sprite '{sprite.name}' to '{newName}' in texture: {path}");
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();
        AssetDatabase.Refresh();
    }
}
