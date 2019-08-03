// Adapted from http://wiki.unity3d.com/index.php/SerializableDictionary

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace PrimitiveFactory.Framework.PatternsAndStructures
{
    abstract public class SerializableDictionary<K, V> : ISerializationCallbackReceiver, IDictionary<K, V>
    {
        [SerializeField]
        private K[] keys;
        [SerializeField]
        private V[] values;

        private Dictionary<K, V> dictionary;

        static public T New<T>() where T : SerializableDictionary<K, V>, new()
        {
            var result = new T()
            {
                dictionary = new Dictionary<K, V>()
            };

            return result;
        }

        public Dictionary<K,V> AsDictionary { get { return dictionary; } }

        public void OnAfterDeserialize()
        {
            var c = keys.Length;
            dictionary = new Dictionary<K, V>(c);
            for (int i = 0; i < c; i++)
            {
                dictionary[keys[i]] = values[i];
            }
            keys = null;
            values = null;
        }

        public void OnBeforeSerialize()
        {
            var c = dictionary.Count;
            keys = new K[c];
            values = new V[c];
            int i = 0;
            using (var e = dictionary.GetEnumerator())
                while (e.MoveNext())
                {
                    var kvp = e.Current;
                    keys[i] = kvp.Key;
                    values[i] = kvp.Value;
                    i++;
                }
        }

        public V this[K key]
        {
            get
            {
                return dictionary[key];
            }
            set
            {
                dictionary[key] = value;
            }
        }

        public ICollection<K> Keys { get { return dictionary.Keys; } }

        public ICollection<V> Values { get { return dictionary.Values; } }

        public int Count { get { return dictionary.Count; } }

        public bool IsReadOnly { get { return false; } }

        public void Add(K key, V value) { dictionary.Add(key, value); }

        public void Add(KeyValuePair<K, V> item) { dictionary.Add(item.Key, item.Value); }

        public void Clear() { dictionary.Clear(); }

        public bool Contains(KeyValuePair<K, V> item) { return dictionary.ContainsKey(item.Key); }

        public bool ContainsKey(K key) { return dictionary.ContainsKey(key); }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator() { return dictionary.GetEnumerator(); }

        public bool Remove(K key) { return dictionary.Remove(key); }

        public bool Remove(KeyValuePair<K, V> item) { return dictionary.Remove(item.Key); }

        public bool TryGetValue(K key, out V value) { return dictionary.TryGetValue(key, out value); }

        IEnumerator IEnumerable.GetEnumerator() { return dictionary.GetEnumerator(); }
    }
}