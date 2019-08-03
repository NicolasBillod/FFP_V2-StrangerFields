using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace PrimitiveFactory.Framework.EditorTools
{
    public static class ScriptableObjectUtility
    {
#if UNITY_EDITOR
        #region Modifiers
        public static void CreateAsset<T>() where T : ScriptableObject { CreateAsset<T>(null); }

        /// <summary>
        ///	This makes it easy to create, name and place unique new ScriptableObject asset files.
        /// </summary>
        public static T CreateAsset<T>(string filePath) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();

            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            if (filePath == null || filePath.Length <= 0)
            {
                filePath = "Assets/" + typeof(T).ToString() + ".asset";
            }

            string assetPathAndName = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(filePath);

            if (assetPathAndName != filePath)
            {
                Debug.LogWarning("Asset file has been changed from " + filePath + " to " + assetPathAndName);
            }

            SaveAsset(asset, assetPathAndName);

            return asset;
        }

        public static void RenameAsset<T>(T asset, string newName) where T : ScriptableObject
        {
            UnityEditor.AssetDatabase.RenameAsset(UnityEditor.AssetDatabase.GetAssetPath(asset), newName);
        }

        public static void SaveAsset<T>(T asset, string path) where T : ScriptableObject
        {
            UnityEditor.AssetDatabase.CreateAsset(asset, path);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }

        public static T DuplicateAsset<T>(T asset, string newPath = null) where T : ScriptableObject
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(asset);
            if (newPath == null)
                newPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(path);

            UnityEditor.AssetDatabase.CopyAsset(path, newPath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(newPath);
        }

        public static void ReplaceAsset<T>(T asset, string path) where T : ScriptableObject
        {
            T oldAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);

            if (oldAsset == null)
            {
                SaveAsset<T>(asset, path);
            }
            else
            {
                UnityEditor.EditorUtility.CopySerialized(asset, oldAsset);
                UnityEditor.EditorUtility.SetDirty(oldAsset);
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
            }
        }

        public static void DeleteAsset<T>(T asset) where T : ScriptableObject
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(asset);

            UnityEditor.AssetDatabase.DeleteAsset(path);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }

        public static void ReloadAsset<T>(ref T asset) where T : ScriptableObject
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(asset);

            Resources.UnloadAsset(asset);
            asset = UnityEditor.AssetDatabase.LoadAssetAtPath(path, asset.GetType()) as T;
        }
        #endregion

        #region Queries
        public static bool AssetExists<T>(string path) where T : ScriptableObject
        {
            Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(T));

            return obj != null;
        }

        public static string GetAssetNameWithoutExtension<T>(T asset) where T : ScriptableObject
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(asset);

            return Path.GetFileNameWithoutExtension(path);
        }

        public static string GetAssetName<T>(T asset) where T : ScriptableObject
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(asset);

            return Path.GetFileName(path);
        }

        public static List<ObjectType> GetAllScriptableObjectsOfType<ObjectType>() where ObjectType : ScriptableObject
        {
            List<ObjectType> res = new List<ObjectType>();
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(ObjectType).FullName, new string[] { "Assets" });

            foreach (string guid in guids)
            {
                res.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<ObjectType>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid)));
            }
            return res;
        }

        public static List<string> GenerateGameDatabaseOfAllScriptableObjectsOfType<ObjectType>() where ObjectType : ScriptableObject
        {
            List<string> res = new List<string>();
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(ObjectType).FullName, new string[] { "Assets" });

            foreach (string guid in guids)
            {
                res.Add(UnityEditor.AssetDatabase.GUIDToAssetPath(guid).Replace("Assets/Resources", ""));
            }
            return res;
        }
        #endregion
#endif
    }
}