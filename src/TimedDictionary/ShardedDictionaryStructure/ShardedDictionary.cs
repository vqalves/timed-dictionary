using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimedDictionary.LockStrategy;

namespace TimedDictionary.ShardedDictionaryStructure
{
    /// <summary>Class to hold a Dictionary and it's lock strategy. Dictionaries does not allow multiple writes at the same time, even when the values are different</summary>
    internal class ShardedDictionary<Key, Value>
    {
        public readonly ILockStrategy LockStrategy;
        public readonly Dictionary<Key, DictionaryEntry<Key, Value>> Dictionary;

        public ShardedDictionary(ILockStrategy lockStrategy)
        {
            this.LockStrategy = lockStrategy;
            this.Dictionary = new Dictionary<Key, DictionaryEntry<Key, Value>>();
        }

        public bool TryGetValue(Key key, out DictionaryEntry<Key, Value> entry)
        {
            if(Dictionary.TryGetValue(key, out entry))
            {
                entry.RefreshCleanUpDuration();
                return true;
            }

            return false;
        }

        public bool ContainsKey(Key key)
        {
            return Dictionary.ContainsKey(key);
        }

        public bool RemoveUnsafe(DictionaryEntry<Key, Value> removedEntry)
        {
            if(Dictionary.TryGetValue(removedEntry.Key, out var entry))
            {
                if(entry == removedEntry)
                {
                    Dictionary.Remove(removedEntry.Key);
                    removedEntry.OnRemovedCallback?.Invoke(removedEntry.Value);
                    return true;
                }
            }

            return false;
        }

        public bool RemoveUnsafe(Key key)
        {
            if(Dictionary.TryGetValue(key, out var entry))
            {
                Dictionary.Remove(entry.Key);
                entry.OnRemovedCallback?.Invoke(entry.Value);
                return true;
            }

            return false;
        }

        internal void Remove(DictionaryEntry<Key, Value> removedEntry)
        {
            LockStrategy.WithLock(() => 
            {
                RemoveUnsafe(removedEntry);
            });
        }
    }
}