using UnityEngine;

public abstract class ScriptableAsset<T> : ScriptableSingleton<T> where T : ScriptableObject
{
    #region Constants


    private const string PathToResources = "Assets/Resources/";
    private const string AssetExtension = "asset";


    #endregion


    #region Protected


    protected static T LoadFromResources(string assetName)
    {
        T asset = Resources.Load(assetName) as T;

        Debug.Assert(asset != null, asset);

        return asset;
    }


    #endregion


    #if UNITY_EDITOR
    private static string CreateAssetPath()
    {
        var assetPath = PathToResources;

        if (!System.IO.Directory.Exists(PathToResources))
        {
            System.IO.Directory.CreateDirectory(PathToResources);
        }

        if (!System.IO.Directory.Exists(assetPath))
        {
            System.IO.Directory.CreateDirectory(assetPath);
        }

        assetPath = System.IO.Path.Combine(assetPath, typeof(T).Name);
        assetPath = System.IO.Path.ChangeExtension(assetPath, AssetExtension);

        return assetPath;
    }

    protected static void EDITOR_CreateOrResetAsset()
    {
        var obj = ScriptableObject.CreateInstance<T>();

        Editor_SaveAsset(obj, false);
    }

    protected static T Editor_LoadAsset()
    {
        var assetPath = CreateAssetPath();

        return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
    }

    protected static void Editor_SaveAsset(T asset, bool silent)
    {
        var assetPath = CreateAssetPath();

        var existingAsset = Editor_LoadAsset();

        if (existingAsset != asset)
        {
            UnityEditor.AssetDatabase.DeleteAsset(assetPath);
            UnityEditor.AssetDatabase.CreateAsset(asset, assetPath);
        }

        UnityEditor.EditorUtility.SetDirty(asset);
        UnityEditor.AssetDatabase.SaveAssets();

        if (!silent)
        {
	        UnityEditor.Selection.activeObject = asset;

	        Debug.LogFormat(asset, "Saved game data asset stored under \"{0}\"", assetPath);
        }
    }

    protected static void EDITOR_InspectAsset()
    {
        var asset = Editor_LoadAsset();

        if (asset != null)
        {
            UnityEditor.Selection.activeObject = asset;
        }
    }
    #endif
}
