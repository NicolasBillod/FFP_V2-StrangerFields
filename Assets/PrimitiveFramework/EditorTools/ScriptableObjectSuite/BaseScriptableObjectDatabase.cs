using UnityEngine;

namespace PrimitiveFactory.Framework.EditorTools
{
    public abstract class BaseScriptableObjectDatabase<TKey, TValue, AssetType> : ScriptableObjectExtended
    {
        protected static BaseScriptableObjectDatabase<TKey, TValue, AssetType> s_assetDatabase = null;

#if UNITY_EDITOR
        protected static BaseScriptableObjectDatabase<TKey, TValue, AssetType> GetDatabaseEditorMode(string path)
        {
            BaseScriptableObjectDatabase<TKey, TValue, AssetType> db = (BaseScriptableObjectDatabase<TKey, TValue, AssetType>)UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(BaseScriptableObjectDatabase<TKey, TValue, AssetType>));
            return db;
        }

        public static void ClearDatabase(string path)
        {
            BaseScriptableObjectDatabase<TKey, TValue, AssetType> db = GetDatabaseEditorMode(path);

            db.InternalClearDatabase();

            UnityEditor.EditorUtility.SetDirty(db);
            UnityEditor.AssetDatabase.SaveAssets();
        }

        protected abstract void InternalClearDatabase();

        public static void UpdateEntry(TKey key, TValue value, string path)
        {
            BaseScriptableObjectDatabase<TKey, TValue, AssetType> db = GetDatabaseEditorMode(path);

            db.InternalUpdateEntry(key, value);

            UnityEditor.EditorUtility.SetDirty(db);
            UnityEditor.AssetDatabase.SaveAssets();
        }

        protected abstract void InternalUpdateEntry(TKey key, TValue value);

        public static void DeleteEntry(TKey key, TValue value, string path)
        {
            BaseScriptableObjectDatabase<TKey, TValue, AssetType> db = GetDatabaseEditorMode(path);

            db.InternalDeleteEntry(key, value);

            UnityEditor.EditorUtility.SetDirty(db);
            UnityEditor.AssetDatabase.SaveAssets();
        }

        protected abstract void InternalDeleteEntry(TKey key, TValue value);

#endif

        public static AssetType GetAsset(TKey key, string path)
        {
            BaseScriptableObjectDatabase<TKey, TValue, AssetType> db = GetDatabase(path);

            UnityEngine.Assertions.Assert.IsNotNull(db, "Database with path " + path + "has not been loaded.");

            return db.InternalGetAsset(key);
        }

        protected abstract AssetType InternalGetAsset(TKey key);

        protected static BaseScriptableObjectDatabase<TKey, TValue, AssetType> GetDatabase(string resourcePath)
        {
            if (s_assetDatabase == null)
                s_assetDatabase = (BaseScriptableObjectDatabase<TKey, TValue, AssetType>)Resources.Load(resourcePath);

            return s_assetDatabase;
        }
    }
}