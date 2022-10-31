using System;
using System.Collections.Generic;
using TimedDictionary.LockStrategy;

namespace TimedDictionary.ShardedDictionaryStructure
{
    /// <summary>Class to hold the dictionary and it's lock strategy. Dictionaries does not allow multiple writes at the same time, even when the keys are different</summary>
    internal class ShardedDictionary<TKey, TValue>
    {
        private readonly ShardedDictionaries<TKey, TValue> ShardHolder;
        private readonly int ShardID;
        public readonly ILockStrategy LockStrategy;
        public readonly Dictionary<TKey, DictionaryEntry<TKey, TValue>> Dictionary;

        public ShardedDictionary(int shardID, ShardedDictionaries<TKey, TValue> shardHolder)
        {
            this.ShardID = shardID;
            this.ShardHolder = shardHolder;
            this.LockStrategy = ShardHolder.Options.LockStrategyFactory.CreateNew();
            this.Dictionary = new Dictionary<TKey, DictionaryEntry<TKey, TValue>>();
        }

        public TValue GetValueOrDefault(TKey key)
        {
            if (TryGetValue(key, out var entry))
                return entry.Value;

            return default(TValue);
        }

        public bool TryGetValue(TKey key, out DictionaryEntry<TKey, TValue> entry)
        {
            if(Dictionary.TryGetValue(key, out entry))
            {
                entry.RefreshCleanUpDuration();
                return true;
            }

            return false;
        }

        public bool ContainsKey(TKey key)
        {
            return Dictionary.ContainsKey(key);
        }

        private bool RemoveFromDictionaryUnsafe(DictionaryEntry<TKey, TValue> removedEntry)
        {
            if(Dictionary.TryGetValue(removedEntry.Key, out var entry))
            {
                if(entry == removedEntry)
                {
                    Dictionary.Remove(removedEntry.Key);
                    return true;
                }
            }

            return false;
        }

        private bool RemoveFromDictionaryUnsafe(TKey key, out DictionaryEntry<TKey, TValue> removedEntry)
        {
            if(Dictionary.TryGetValue(key, out removedEntry))
            {
                Dictionary.Remove(removedEntry.Key);
                return true;
            }

            return false;
        }

        internal bool TryAddUnscheduledValueUnsafe(TKey key, TValue value, out DictionaryEntry<TKey, TValue> currentEntry, OnValueRemovedDelegate<TValue> onRemoved = null)
        {
            if (!TryGetValue(key, out currentEntry))
            {
                // If onRemoved is not specified by parameter, use the instance configuration
                onRemoved = onRemoved ?? ShardHolder.OnRemoved;

                var lifetime = new EntryLifetime(ShardHolder.ExpectedDuration, ShardHolder.Options.TimestampProvider, ShardHolder.ExtendTimeConfiguration);

                currentEntry = new DictionaryEntry<TKey, TValue>(key, value, onRemoved, this, lifetime, ShardHolder.Options.TimestampProvider);
                Dictionary.Add(key, currentEntry);

                return true;
            }

            return false;
        }

        internal bool Remove(DictionaryEntry<TKey, TValue> removedEntry)
        {
            bool wasRemoved = LockStrategy.WithLock(() =>
            {
                return RemoveFromDictionaryUnsafe(removedEntry);
            });

            if (wasRemoved)
                removedEntry.OnRemovedCallback?.Invoke(removedEntry.Value);

            return wasRemoved;
        }

        internal bool Remove(TKey key)
        {
            DictionaryEntry<TKey, TValue> removedEntry = null;

            bool wasRemoved = LockStrategy.WithLock(() =>
            {
                return RemoveFromDictionaryUnsafe(key, out removedEntry);
            });
            
            if(wasRemoved)
                removedEntry.OnRemovedCallback?.Invoke(removedEntry.Value);

            return wasRemoved;
        }

        internal bool TryAdd(TKey key, TValue value, OnValueRemovedDelegate<TValue> onRemoved)
        {
            DictionaryEntry<TKey, TValue> entry = null;

            var wasCreated = LockStrategy.WithLock(() =>
            {
                return TryAddUnscheduledValueUnsafe(key, value, out entry, onRemoved);
            });

            if(wasCreated)
                entry.TimeoutScheduler.StartSchedule();

            return wasCreated;
        }

        internal TValue GetOrAddIfNew(TKey key, Func<TValue> notFound, Action<DictionaryEntry<TKey, TValue>> onNewEntry, OnValueRemovedDelegate<TValue> onRemoved)
        {
            if (!TryGetValue(key, out var entry))
            {
                if (ShardHolder.MaximumSize.HasValue && ShardHolder.EntryCount >= ShardHolder.MaximumSize.Value)
                    return notFound.Invoke();

                var wasCreated = false;

                LockStrategy.WithLock(() =>
                {
                    var value = notFound.Invoke();
                    wasCreated = TryAddUnscheduledValueUnsafe(key, value, out entry, onRemoved);
                });

                if (wasCreated)
                {
                    onNewEntry?.Invoke(entry);
                    entry.TimeoutScheduler.StartSchedule();
                }
            }

            return entry.Value;
        }
    }
}