using System;
using System.Collections.Generic;
using System.Linq;
using TimedDictionary.DateTimeProvider;
using TimedDictionary.LockStrategy;

namespace TimedDictionary
{
    public class TimedDictionary<Key, Value>
    {
        private readonly ILockStrategy LockStrategy;
        private readonly Dictionary<Key, DictionaryEntry<Key, Value>> Dictionary;
        private readonly ExtendTimeConfiguration ExtendTimeConfiguration;
        private readonly IDateTimeProvider DateTimeProvider;
        private readonly int? ExpectedDuration;
        private readonly int? MaximumSize;

        public int Count => Dictionary.Count;

        public TimedDictionary(int? expectedDuration = null, int? maximumSize = null, ExtendTimeConfiguration extendTimeConfiguration = null, IDateTimeProvider dateTimeProvider = null, ILockStrategy lockStrategy = null)
        {
            this.DateTimeProvider = dateTimeProvider ?? DefaultDateTimeProvider.Instance;
            this.ExtendTimeConfiguration = extendTimeConfiguration ?? ExtendTimeConfiguration.None(DateTimeProvider);
            this.LockStrategy = lockStrategy ?? new LockObjectStrategy();
            
            this.Dictionary = new Dictionary<Key, DictionaryEntry<Key, Value>>();
            this.ExpectedDuration = expectedDuration;
            this.MaximumSize = maximumSize;
        }

        private bool TryGetValue(Key key, out DictionaryEntry<Key, Value> entry)
        {
            if(Dictionary.TryGetValue(key, out entry))
            {
                entry.RefreshCleanUpDuration();
                return true;
            }

            return false;
        }

        public Value GetValueOrDefault(Key key)
        {
            if(TryGetValue(key, out var entry))
                return entry.Value;

            return default(Value);
        }

        public bool ContainsKey(Key key) 
        {
            return Dictionary.ContainsKey(key);
        }

        private bool TryAddUnsafe(Key key, Value value, out DictionaryEntry<Key, Value> currentEntry, Action<Value> onRemoved = null)
        {
            if(!Dictionary.TryGetValue(key, out currentEntry))
            {
                currentEntry = new DictionaryEntry<Key, Value>(key, value, onRemoved, this, ExpectedDuration, ExtendTimeConfiguration, DateTimeProvider);
                Dictionary.Add(key, currentEntry);
                return true;
            }

            return false;
        }

        public bool TryAdd(Key key, Value value, Action<Value> onRemoved = null)
        {
            return LockStrategy.WithLock(() => 
            {
                return TryAddUnsafe(key, value, out var entry, onRemoved);
            });
        }

        public Value GetOrAddIfNew(Key key, Func<Value> notFound, Action<Value> onRemoved = null)
        {
            return GetOrAddIfNew(key, notFound, onNewEntry: null, onRemoved);
        }

        internal Value GetOrAddIfNew(Key key, Func<Value> notFound, Action<DictionaryEntry<Key, Value>> onNewEntry = null, Action<Value> onRemoved = null)
        {
            if(!TryGetValue(key, out var entry))
            {
                if(MaximumSize.HasValue && Count >= MaximumSize.Value)
                    return notFound.Invoke();
                    
                LockStrategy.WithLock(() => 
                {
                    var value = notFound.Invoke();
                    var wasCreated = TryAddUnsafe(key, value, out entry, onRemoved);

                    if(wasCreated)
                        onNewEntry?.Invoke(entry);
                });
            }

            return entry.Value;
        }

        private bool RemoveUnsafe(DictionaryEntry<Key, Value> removedEntry)
        {
            if(Dictionary.TryGetValue(removedEntry.Key, out var entry))
            {
                if(entry == removedEntry)
                {
                    Dictionary.Remove(removedEntry.Key);
                    removedEntry.OnRemoved?.Invoke(removedEntry.Value);
                    return true;
                }
            }

            return false;
        }

        private bool RemoveUnsafe(Key key)
        {
            if(Dictionary.TryGetValue(key, out var entry))
            {
                Dictionary.Remove(entry.Key);
                entry.OnRemoved?.Invoke(entry.Value);
                return true;
            }

            return false;
        }

        public bool Remove(Key key)
        {
            return LockStrategy.WithLock(() => 
            {
                return RemoveUnsafe(key);
            });
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
