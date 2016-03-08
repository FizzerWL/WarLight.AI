using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarLight.Shared.AI
{

    /// <summary>
    /// Syntactical sugar around a List of KeyValuePairs
    /// </summary>
    public class KeyValueList<K,V>
    {
        private List<KeyValuePair<K, V>> _list = new List<KeyValuePair<K, V>>();

        public IEnumerable<KeyValuePair<K, V>> Values()
        {
            return _list;
        }

        public void Add(K key, V value)
        {
            _list.Add(new KeyValuePair<K, V>(key, value));
        }

        public void Insert(int index, K key, V value)
        {
            _list.Insert(index, new KeyValuePair<K, V>(key, value));
        }

        public void Clear()
        {
            _list.Clear();
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        public K GetKey(int index)
        {
            return _list[index].Key;
        }
        public V GetValue(int index)
        {
            return _list[index].Value;
        }
    }
}
