using UnityEditor;
using UnityEngine;

namespace PrimitiveFactory.Framework.PatternsAndStructures
{
    [CustomPropertyDrawer(typeof(StringStringDictionary))]
    public class StringStringDictionaryDrawer : SerializableDictionaryDrawer<string, string>
    {
        protected override SerializableKeyValueTemplate<string, string> GetTemplate()
        {
            return GetGenericTemplate<SerializableStringStringTemplate>();
        }
    }
    internal class SerializableStringStringTemplate : SerializableKeyValueTemplate<string, string> { }

    [CustomPropertyDrawer(typeof(StringSpriteDictionary))]
    public class StringSpriteDictionaryDrawer : SerializableDictionaryDrawer<string, Sprite>
    {
        protected override SerializableKeyValueTemplate<string, Sprite> GetTemplate()
        {
            return GetGenericTemplate<SerializableStringSpriteTemplate>();
        }
    }
    internal class SerializableStringSpriteTemplate : SerializableKeyValueTemplate<string, Sprite> { }
}