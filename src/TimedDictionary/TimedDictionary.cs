using System;
using System.Collections.Generic;
using System.Linq;
using TimedDictionary.DateTimeProvider;

namespace TimedDictionary
{
    public class TimedDictionary<Key, Value>
    {
        private readonly object Lock;
        private readonly Dictionary<Key, DictionaryEntry<Key, Value>> Dictionary;
        private readonly ExtendTimeConfiguration ExtendTimeConfiguration;
        private readonly IDateTimeProvider DateTimeProvider;
        private readonly int? ExpectedDuration;
        private readonly int? MaximumSize;

        public int Count => Dictionary.Count;

        public TimedDictionary(int? expectedDuration = null, int? maximumSize = null, ExtendTimeConfiguration extendTimeConfiguration = null, IDateTimeProvider dateTimeProvider = null)
        {
            this.DateTimeProvider = dateTimeProvider ?? DefaultDateTimeProvider.Instance;
            this.ExtendTimeConfiguration = extendTimeConfiguration ?? ExtendTimeConfiguration.None(DateTimeProvider);
            
            this.Dictionary = new Dictionary<Key, DictionaryEntry<Key, Value>>();
            this.Lock = new Object();
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

        private bool TryAddUnsafe(Key key, Value value, out DictionaryEntry<Key, Value> entry)
        {
            if(!ContainsKey(key))
            {
                entry = new DictionaryEntry<Key, Value>(key, value, this, ExpectedDuration, ExtendTimeConfiguration, DateTimeProvider);
                Dictionary.Add(key, entry);
                return true;
            }

            entry = null;
            return false;
        }

        public bool TryAdd(Key key, Value value)
        {
            lock(Lock)
            {
                return TryAddUnsafe(key, value, out var entry);
            }
        }

        public Value TryGetOrAddIfNew(Key key, Func<Value> notFound)
        {
            if(!TryGetValue(key, out var entry))
            {
                if(MaximumSize.HasValue && Count >= MaximumSize.Value)
                    return notFound.Invoke();
                    
                lock(Lock)
                {
                    var value = notFound.Invoke();
                    TryAddUnsafe(key, value, out entry);
                }
            }

            return entry.Value;
        }

        public Value GetOrAddIfNew(Key key, Func<Value> notFound)
        {
            return GetOrAddIfNew(key, notFound, onNewEntry: null);
        }

        internal Value GetOrAddIfNew(Key key, Func<Value> notFound, Action<DictionaryEntry<Key, Value>> onNewEntry = null)
        {
            if(!TryGetValue(key, out var entry))
            {
                if(MaximumSize.HasValue && Count >= MaximumSize.Value)
                    return notFound.Invoke();
                    
                lock(Lock)
                {
                    var value = notFound.Invoke();
                    TryAddUnsafe(key, value, out entry);
                    onNewEntry?.Invoke(entry);
                }
            }

            return entry.Value;
        }

        public bool Remove(Key key)
        {
            lock(Lock)
            {
                return Dictionary.Remove(key);
            }
        }

        internal void Remove(DictionaryEntry<Key, Value> removedEntry)
        {
            lock(Lock)
            {
                if(Dictionary.TryGetValue(removedEntry.Key, out var entry))
                    if(entry == removedEntry)
                        Dictionary.Remove(removedEntry.Key);
            }
        }
    }
}
