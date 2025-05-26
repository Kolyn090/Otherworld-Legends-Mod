using UnityEditor;
using UnityEngine;
using System.IO;

public class ReplacePathID38 : EditorWindow
{
    private DefaultAsset originalFolder;
    private DefaultAsset mineFolder;

    [MenuItem("Tools/Replace Line 38 From Folder")]
    public static void ShowWindow()
    {
        GetWindow<ReplacePathID38>("Line 38 Replacer");
    }

    void OnGUI()
    {
        GUILayout.Label("Replace Line 38", EditorStyles.boldLabel);
        GUILayout.Space(5);

        originalFolder = (DefaultAsset)EditorGUILayout.ObjectField("Source", originalFolder, typeof(DefaultAsset), false);
        mineFolder = (DefaultAsset)EditorGUILayout.ObjectField("My Bundle", mineFolder, typeof(DefaultAsset), false);

        GUILayout.Space(10);

        if (GUILayout.Button("Replace Line 38"))
        {
            if (originalFolder == null || mineFolder == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign both folders first.", "OK");
                return;
            }

            string originalPath = AssetDatabase.GetAssetPath(originalFolder);
            string minePath = AssetDatabase.GetAssetPath(mineFolder);

            DoReplace(originalPath, minePath);
        }
    }

    void DoReplace(string originalPath, string minePath)
    {
        string fullOriginalPath = Path.Combine(Application.dataPath, originalPath.Substring("Assets/".Length));
        string fullMinePath = Path.Combine(Application.dataPath, minePath.Substring("Assets/".Length));

        string[] originalFiles = Directory.GetFiles(fullOriginalPath, "*.txt");

        if (originalFiles.Length == 0)
        {
            Debug.LogWarning($"No text assets found in {fullOriginalPath}.");
        }

        string[] mineFiles = Directory.GetFiles(fullMinePath, "*.txt");

        if (originalFiles[0].Length < 38)
        {
            Debug.LogWarning($"{Path.GetFileName(originalFiles[0])} is not a valid UABEA text asset.");
        }

        string replaceLine = File.ReadAllLines(originalFiles[0])[37];


        for (int i = 0; i < mineFiles.Length; i++)
        {
            string[] linesMine = File.ReadAllLines(mineFiles[i]);

            if (linesMine.Length > 38)
            {
                linesMine[37] = replaceLine;
                File.WriteAllLines(mineFiles[i], linesMine);
                Debug.Log($"Updated: {Path.GetFileName(mineFiles[i])}");
            }
            else
            {
                Debug.LogWarning($"Skipped {Path.GetFileName(mineFiles[i])}: not enough lines.");
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Done", "Line 38 replacement complete.", "OK");
    }
}
