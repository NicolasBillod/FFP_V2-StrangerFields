using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PrimitiveFactory.Framework.EditorTools
{
    public class ScriptableObjectExtended : ScriptableObject, ISerializationCallbackReceiver
    {
        public string Name;
        public string Guid;

        private const string c_GUIDRegex = @"(0-9a-f)*";

#if UNITY_EDITOR
        public void GenerateGuid()
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(this);
            Guid = UnityEditor.AssetDatabase.AssetPathToGUID(path);
            ScriptableObjectDatabase.UpdateEntry(Guid, path, ScriptableObjectDatabase.c_ObjectFullPath);
        }
#endif

        public virtual void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            foreach (FieldInfo field in GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                object[] attributes = field.GetCustomAttributes(typeof(LoadOnDemand), true);
                if (attributes.Length == 1)
                {
                    LoadOnDemand attribute = (LoadOnDemand)attributes[0];
                    FieldInfo linkedField = GetType().GetField(attribute.FieldName);

                    object value = linkedField.GetValue(this);
                    if (value != null)
                    {
                        if (typeof(ScriptableObjectExtended).IsAssignableFrom(value.GetType()))
                        {
                            ScriptableObjectExtended linkedObject = (ScriptableObjectExtended)value;
                            if (linkedObject != null)
                                field.SetValue(this, linkedObject.Guid);
                        }
                        else
                        {
                            field.SetValue(this, AssetToResourcePath(UnityEditor.AssetDatabase.GetAssetPath((Object)value)));
                        }
                    }
                }
            }
#endif
        }

        public virtual void OnAfterDeserialize()
        {
        }

        public void Load(string fieldName = null, bool async = false)
        {
            foreach (FieldInfo field in GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                object[] attributes = field.GetCustomAttributes(typeof(LoadOnDemand), true);
                if (attributes.Length == 1)
                {
                    LoadOnDemand attribute = (LoadOnDemand)attributes[0];
                    string attributeFieldName = attribute.FieldName;
                    if (fieldName == null || attributeFieldName == fieldName)
                    {
                        FieldInfo linkedField = GetType().GetField(attributeFieldName);
                        string fieldValue = (string)field.GetValue(this);
                        if (fieldValue != null)
                        {
                            if (fieldValue.Length == 32 && Regex.IsMatch(fieldValue, c_GUIDRegex))
                            {
                                // it's a guid
                                linkedField.SetValue(this, ScriptableObjectDatabase.GetAsset(fieldValue, ScriptableObjectDatabase.c_ObjectResourcePath));
                            }
                            else
                            {
                                // it's a path
                                if (async)
                                    Resources.LoadAsync(fieldValue);
                                else
                                {
                                    // Sprite special case since by default Unity loads images as textures
                                    if (typeof(Sprite) == linkedField.FieldType)
                                    {
                                        linkedField.SetValue(this, Resources.Load<Sprite>(fieldValue));
                                    }
                                    else
                                    {
                                        linkedField.SetValue(this, Resources.Load(fieldValue));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Unload(string fieldName = null)
        {
            foreach (FieldInfo field in GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                object[] attributes = field.GetCustomAttributes(typeof(LoadOnDemand), true);
                if (attributes.Length == 1)
                {
                    LoadOnDemand attribute = (LoadOnDemand)attributes[0];
                    string attributeFieldName = attribute.FieldName;
                    if (fieldName == null || attributeFieldName == fieldName)
                    {
                        FieldInfo linkedField = GetType().GetField(attributeFieldName);
                        linkedField.SetValue(this, null);
                    }
                }
            }
        }

        public static string AssetToResourcePath(string path)
        {
            string find = "Resources/";
            if (path.IndexOf(find) == -1)
                return path;

            int beginning = path.IndexOf(find) + find.Length;
            int end = path.LastIndexOf('.');
            return path.Substring(beginning, end - beginning);
        }
    }
}