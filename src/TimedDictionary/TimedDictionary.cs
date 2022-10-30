using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TimedDictionary.ActionScheduler;
using TimedDictionary.DateTimeProvider;
using TimedDictionary.LockStrategy;
using TimedDictionary.ShardedDictionaryStructure;

namespace TimedDictionary
{
    public class TimedDictionary<Key, Value>
    {
        public delegate void OnRemovedDelegate(Value removedValue);

        private readonly ShardedDictionaries<Key, Value> Shards;
        private readonly ExtendTimeConfiguration ExtendTimeConfiguration;
        private readonly TimedDictionaryOptions Options;
        private readonly int? ExpectedDuration;
        private readonly int? MaximumSize;
        private readonly OnRemovedDelegate OnRemoved;

        /// <summary>Amount of items currently in the dictionary</summary>
        public int Count => Shards.EntryCount;

        /// <summary>Time-based self-cleaning dictionary structure. The entries are automatically removed from the structure after the specified time</summary>
        /// <param name="expectedDuration">How many milliseconds each value should be kept in the dictionary. If null, the structure will keep all records until they are manually removed.</param>
        /// <param name="maximumSize">Maximum amount of keys allowed at a time. When the limit is reached, no new keys will be added and new keys will always execute the evaluation function. If null, there will be no limits to the dictionary size.</param>
        /// <param name="extendTimeConfiguration">Allows the increase of each object lifetime inside the dictionary by X milliseconds, up to Y milliseconds, everytime the value is retrieved. If null, the object lifetime will obey the `expectedDuration` parameter.</param>
        /// <param name="onRemoved">Callback called whenever the value is removed from the object, either by timeout or manually. Called only once per value. Example: (valueRemoved) => { }</param>
        public TimedDictionary(int? expectedDuration = null, int? maximumSize = null, ExtendTimeConfiguration extendTimeConfiguration = null, OnRemovedDelegate onRemoved = null) : this
        (
            changeOptions: null,

            expectedDuration: expectedDuration,
            maximumSize: maximumSize,
            extendTimeConfiguration: extendTimeConfiguration,
            onRemoved: onRemoved
        )
        {
            
        }

        internal TimedDictionary(Action<TimedDictionaryOptions> changeOptions, int? expectedDuration = null, int? maximumSize = null, ExtendTimeConfiguration extendTimeConfiguration = null, OnRemovedDelegate onRemoved = null)
        {
            this.Options = new TimedDictionaryOptions();
            changeOptions?.Invoke(Options);

            this.ExtendTimeConfiguration = extendTimeConfiguration ?? ExtendTimeConfiguration.None(Options.DateTimeProvider);

            this.Shards = new ShardedDictionaries<Key, Value>(Options.LockStrategyFactory);
            this.ExpectedDuration = expectedDuration;
            this.MaximumSize = maximumSize;
            this.OnRemoved = onRemoved;
        }

        /// <summary>Get the current value associated with the key</summary>
        /// <param name="key">Key which the value was stored</param>
        public Value GetValueOrDefault(Key key)
        {
            var shard = Shards.GetShard(key);

            if(shard.TryGetValue(key, out var entry))
                return entry.Value;

            return default(Value);
        }

        public bool ContainsKey(Key key) 
        {
            return Shards.GetShard(key).ContainsKey(key);
        }

        private bool TryAddUnsafe(ShardedDictionary<Key, Value> shard, Key key, Value value, out DictionaryEntry<Key, Value> currentEntry, OnRemovedDelegate onRemoved = null)
        {
            if(!shard.TryGetValue(key, out currentEntry))
            {
                // If onRemoved is not specified by parameter, use the instance configuration
                onRemoved = onRemoved ?? this.OnRemoved;

                var lifetime = new EntryLifetime(ExpectedDuration, Options.DateTimeProvider, ExtendTimeConfiguration);

                currentEntry = new DictionaryEntry<Key, Value>(key, value, onRemoved, shard, lifetime, Options.DateTimeProvider);
                shard.Dictionary.Add(key, currentEntry);

                currentEntry.TimeoutScheduler.StartSchedule();

                return true;
            }

            return false;
        }

        public bool TryAdd(Key key, Value value, OnRemovedDelegate onRemoved = null)
        {
            var shard = Shards.GetShard(key);

            return shard.LockStrategy.WithLock(() => 
            {
                return TryAddUnsafe(shard, key, value, out var entry, onRemoved);
            });
        }

        /// <summary>Get the current value associated with the key. If the key is not found, generate a new value and associate with key</summary>
        /// <param name="key">Key which the value was stored</param>
        /// <param name="notFound">Generate a new value for the key. This function locks the object, be mindful when using parallel processing.</param>
        /// <param name="onRemoved">Overrides onRemoved function specified on the dictionary constructor. Callback called whenever the value is removed from the object, either by timeout or manually. Called only once per value. Example: (valueRemoved) => { }</param>
        public Value GetOrAddIfNew(Key key, Func<Value> notFound, OnRemovedDelegate onRemoved = null)
        {
            return GetOrAddIfNew(key: key, notFound: notFound, onNewEntry: null, onRemoved: onRemoved);
        }

        internal Value GetOrAddIfNew(Key key, Func<Value> notFound, Action<DictionaryEntry<Key, Value>> onNewEntry, OnRemovedDelegate onRemoved)
        {
            var shard = Shards.GetShard(key);

            if(!shard.TryGetValue(key, out var entry))
            {
                if(MaximumSize.HasValue && Count >= MaximumSize.Value)
                    return notFound.Invoke();
                    
                shard.LockStrategy.WithLock(() => 
                {
                    var value = notFound.Invoke();
                    var wasCreated = TryAddUnsafe(shard, key, value, out entry, onRemoved);

                    if(wasCreated)
                        onNewEntry?.Invoke(entry);
                });
            }

            return entry.Value;
        }

        public bool Remove(Key key)
        {
            var shard = Shards.GetShard(key);

            return shard.LockStrategy.WithLock(() => 
            {
                return shard.RemoveUnsafe(key);
            });
        }
    }
}
