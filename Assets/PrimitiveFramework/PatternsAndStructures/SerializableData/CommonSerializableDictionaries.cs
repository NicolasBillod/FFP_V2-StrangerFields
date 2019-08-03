using System;
using UnityEngine;

namespace PrimitiveFactory.Framework.PatternsAndStructures
{
    [Serializable]
    public class StringStringDictionary : SerializableDictionary<string, string> { }

    [Serializable]
    public class StringSpriteDictionary : SerializableDictionary<string, Sprite> { }
}