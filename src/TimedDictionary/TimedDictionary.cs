using System;
using TimedDictionary.ShardedDictionaryStructure;

namespace TimedDictionary
{
    public class TimedDictionary<TKey, TValue>
    {
        private readonly ShardedDictionaries<TKey, TValue> Shards;

        /// <summary>Amount of items currently in the dictionary</summary>
        public int Count => Shards.EntryCount;

        /// <summary>Time-based self-cleaning dictionary structure. The entries are automatically removed from the structure after the specified time</summary>
        /// <param name="expectedDuration">How many milliseconds each value should be kept in the dictionary. If null, the structure will keep all records until they are manually removed.</param>
        /// <param name="maximumSize">Maximum amount of keys allowed at a time. When the limit is reached, no new keys will be added and new keys will always execute the evaluation function. If null, there will be no limits to the dictionary size.</param>
        /// <param name="extendTimeConfiguration">Allows the increase of each object lifetime inside the dictionary by X milliseconds, up to Y milliseconds, everytime the value is retrieved. If null, the object lifetime will obey the `expectedDuration` parameter.</param>
        /// <param name="onRemoved">Callback called whenever the value is removed from the object, either by timeout or manually. Called only once per value. Example: (valueRemoved) => { }</param>
        public TimedDictionary(int? expectedDuration = null, int? maximumSize = null, ExtendTimeConfiguration extendTimeConfiguration = null, OnValueRemovedDelegate<TValue> onRemoved = null) : this
        (
            changeOptions: null,

            expectedDuration: expectedDuration,
            maximumSize: maximumSize,
            extendTimeConfiguration: extendTimeConfiguration,
            onRemoved: onRemoved
        )
        {

        }

        internal TimedDictionary(Action<TimedDictionaryOptions> changeOptions, int? expectedDuration = null, int? maximumSize = null, ExtendTimeConfiguration extendTimeConfiguration = null, OnValueRemovedDelegate<TValue> onRemoved = null)
        {
            this.Shards = new ShardedDictionaries<TKey, TValue>
            (
                extendTimeConfiguration: extendTimeConfiguration,
                changeOptions : changeOptions,
                expectedDuration: expectedDuration,
                maximumSize: maximumSize,
                onRemoved: onRemoved
            );
        }

        /// <summary>Get the current value associated with the key</summary>
        /// <param name="key">Key which the value was stored</param>
        public TValue GetValueOrDefault(TKey key) => Shards.GetShard(key).GetValueOrDefault(key);

        public bool ContainsKey(TKey key) => Shards.GetShard(key).ContainsKey(key);

        public bool TryAdd(TKey key, TValue value, OnValueRemovedDelegate<TValue> onRemoved = null) => Shards.GetShard(key).TryAdd(key, value, onRemoved);

        /// <summary>Get the current value associated with the key. If the key is not found, generate a new value and associate with key</summary>
        /// <param name="key">Key which the value was stored</param>
        /// <param name="notFound">Generate a new value for the key. This function locks the object, be mindful when using parallel processing.</param>
        /// <param name="onRemoved">Overrides onRemoved function specified on the dictionary constructor. Callback called whenever the value is removed from the object, either by timeout or manually. Called only once per value. Example: (valueRemoved) => { }</param>
        public TValue GetOrAddIfNew(TKey key, Func<TValue> notFound, OnValueRemovedDelegate<TValue> onRemoved = null) => GetOrAddIfNew(key: key, notFound: notFound, onNewEntry: null, onRemoved: onRemoved);

        internal TValue GetOrAddIfNew(TKey key, Func<TValue> notFound, Action<DictionaryEntry<TKey, TValue>> onNewEntry, OnValueRemovedDelegate<TValue> onRemoved) => Shards.GetShard(key).GetOrAddIfNew(key, notFound, onNewEntry, onRemoved);

        public bool Remove(TKey key) => Shards.GetShard(key).Remove(key);
    }
}
