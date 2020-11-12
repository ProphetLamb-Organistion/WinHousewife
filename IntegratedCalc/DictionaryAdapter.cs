using System;
using System.Collections;
using System.Collections.Generic;

namespace IntegratedCalc
{
    public class DictionaryAdapter<K, V> : ICollection<KeyValuePair<K, V>>, IDictionary<K, V>, IEnumerable<ValueTuple<K, V>>
    {
        private readonly IDictionary<K, V> _collector;

        public DictionaryAdapter(IEnumerable<KeyValuePair<K, V>> source)
        {
            _collector = new Dictionary<K, V>();
            foreach (KeyValuePair<K, V> kvp in source)
            {
                _collector.Add(kvp.Key, kvp.Value);
            }
        }

        public V this[K key] { get => _collector[key]; set => _collector[key] = value; }

        public ICollection<K> Keys => _collector.Keys;

        public ICollection<V> Values => _collector.Values;

        public int Count => _collector.Count;

        public bool IsReadOnly => _collector.IsReadOnly;

        public IDictionary<K, V> GetCollector() => _collector;

        public void Add(K key, V value) => _collector.Add(key, value);
        public void Add(KeyValuePair<K, V> item) => _collector.Add(item);
        public void Clear() => _collector.Clear();
        public bool Contains(KeyValuePair<K, V> item) => _collector.Contains(item);
        public bool ContainsKey(K key) => _collector.ContainsKey(key);
        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex) => _collector.CopyTo(array, arrayIndex);
        public IEnumerator<KeyValuePair<K, V>> GetEnumerator() => _collector.GetEnumerator();
        public bool Remove(K key) => _collector.Remove(key);
        public bool Remove(KeyValuePair<K, V> item) => _collector.Remove(item);
        public bool TryGetValue(K key, out V value) => _collector.TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_collector).GetEnumerator();
        IEnumerator<ValueTuple<K, V>> IEnumerable<ValueTuple<K, V>>.GetEnumerator()
        {
            foreach (var kvp in _collector)
                yield return (kvp.Key, kvp.Value);
        }
    }
}
