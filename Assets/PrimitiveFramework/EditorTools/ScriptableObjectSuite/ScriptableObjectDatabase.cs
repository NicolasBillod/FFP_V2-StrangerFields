using UnityEngine;
using System.Collections.Generic;
using System.IO;
using PrimitiveFactory.Framework.PatternsAndStructures;

namespace PrimitiveFactory.Framework.EditorTools
{
    public class ScriptableObjectDatabase : BaseScriptableObjectDatabase<string, string, ScriptableObjectExtended>
    {
        public const string c_ObjectBaseDirectory = "Assets/Resources/";
        public const string c_ObjectResourcePath = "AssetDatabase";
        public static readonly string c_ObjectFullPath = string.Concat(c_ObjectBaseDirectory, c_ObjectResourcePath, ".asset");

        [SerializeField]
        private StringStringDictionary m_DbDictionary;

        public ScriptableObjectDatabase()
        {
            m_DbDictionary = StringStringDictionary.New<StringStringDictionary>();
        }

        protected override ScriptableObjectExtended InternalGetAsset(string key)
        {
#if UNITY_EDITOR
            if (key == null || !m_DbDictionary.ContainsKey(key))
            {
                Debug.LogWarning("No asset in DB with key equals to " + key);
                return null;
            }
#endif

            if (!m_DbDictionary.ContainsKey(key))
                throw new System.Exception("Asset with guid not found: " + key);
            else
            {
                string path = m_DbDictionary[key];
                if (path.StartsWith("Assets/"))
#if UNITY_EDITOR
                    return UnityEditor.AssetDatabase.LoadAssetAtPath<ScriptableObjectExtended>(path);
#else
                    throw new System.Exception("Trying to access to an editor-only asset");
#endif
                else
                    return (ScriptableObjectExtended)Resources.Load(path);
            }
        }

#if UNITY_EDITOR

        protected override void InternalUpdateEntry(string guid, string path)
        {
            m_DbDictionary[guid] = AssetToResourcePath(path);
        }

        protected override void InternalDeleteEntry(string key, string value)
        {
            m_DbDictionary.Remove(key);
        }

        protected override void InternalClearDatabase()
        {
            m_DbDictionary.Clear();
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Primitive/Scriptable Object Suite/Initialize Database")]
        public static void CreateDatabase()
        {
            if (!Directory.Exists(c_ObjectBaseDirectory))
            {
                Directory.CreateDirectory(c_ObjectBaseDirectory);
            }

            ScriptableObjectDatabase test = ScriptableObject.CreateInstance<ScriptableObjectDatabase>();
            UnityEditor.AssetDatabase.CreateAsset(test, c_ObjectFullPath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }

        [UnityEditor.MenuItem("Primitive/Scriptable Object Suite/Force Database Refresh")]
        public static void RefreshDatabase()
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ScriptableObjectExtended", new string[] { "Assets" });
            foreach (string guid in guids)
                UnityEditor.AssetDatabase.LoadAssetAtPath<ScriptableObjectExtended>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid)).GenerateGuid();
        }
#endif

        public static List<T> GetAllObjectsOfType<T>() where T:ScriptableObjectExtended
        {
            ScriptableObjectDatabase db = (ScriptableObjectDatabase)GetDatabaseEditorMode(c_ObjectFullPath);
            return db.InternalGetAllObjectsOfType<T>();
        }

        internal List<T> InternalGetAllObjectsOfType<T>() where T:ScriptableObjectExtended
        {
            List<T> res = new List<T>();

            foreach (KeyValuePair<string, string> pair in m_DbDictionary)
            {
                ScriptableObjectExtended obj = InternalGetAsset(pair.Key);
                if (typeof(T).IsAssignableFrom(obj.GetType()))
                    res.Add((T)obj);
            }

            return res;
        }
#endif
    }
}