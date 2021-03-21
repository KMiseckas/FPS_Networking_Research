using System.IO;
using UnityEditor;
using UnityEngine;

public static class EditorUtils
{
    [MenuItem("Assets/Create Asset From Script")]
    public static void CreateAsset()
    {
        foreach(Object obj in Selection.objects)
        {
            var ms = obj as MonoScript;
            if(ms == null)
            {
                Debug.LogError("You must select a C# Script");
                return;
            }

            CreateAsset(ms);
        }
    }

    public static void CreateAsset(MonoScript ms)
    {
        var asset = ScriptableObject.CreateInstance(ms.GetClass());
        var assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.GetDirectoryName(AssetDatabase.GetAssetPath(ms)) + "/" + ms.name + ".asset");

        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }
}