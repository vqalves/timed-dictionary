using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TimedDictionary.ActionScheduler;
using TimedDictionary.DateTimeProvider;
using TimedDictionary.LockStrategy;

namespace TimedDictionary
{
    public class TimedDictionary<Key, Value>
    {
        public delegate void OnRemovedDelegate(Value removedValue);

        private readonly Dictionary<Key, DictionaryEntry<Key, Value>> Dictionary;
        private readonly ExtendTimeConfiguration ExtendTimeConfiguration;
        private readonly TimedDictionaryOptions Options;
        private readonly int? ExpectedDuration;
        private readonly int? MaximumSize;
        private readonly OnRemovedDelegate OnRemoved;

        /// <summary>Amount of items currently in the dictionary</summary>
        public int Count => Dictionary.Count;

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

            this.Dictionary = new Dictionary<Key, DictionaryEntry<Key, Value>>();
            this.ExpectedDuration = expectedDuration;
            this.MaximumSize = maximumSize;
            this.OnRemoved = onRemoved;
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

        /// <summary>Get the current value associated with the key</summary>
        /// <param name="key">Key which the value was stored</param>
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

        private bool TryAddUnsafe(Key key, Value value, out DictionaryEntry<Key, Value> currentEntry, OnRemovedDelegate onRemoved = null)
        {
            if(!Dictionary.TryGetValue(key, out currentEntry))
            {
                // If onRemoved is not specified by parameter, use the instance configuration
                onRemoved = onRemoved ?? this.OnRemoved;

                var lifetime = new EntryLifetime(ExpectedDuration, Options.DateTimeProvider, ExtendTimeConfiguration);

                currentEntry = new DictionaryEntry<Key, Value>(key, value, onRemoved, this, lifetime);
                Dictionary.Add(key, currentEntry);

                // ActionScheduler can execute immediately, so add the entry to dictionary before instanciating the scheduler
                var timeoutScheduler = ActionSchedulerFactory.Create(currentEntry.RemoveItselfFromDictionary, ExpectedDuration);
                currentEntry.AttachTimeoutScheduler(timeoutScheduler);

                return true;
            }

            return false;
        }

        public bool TryAdd(Key key, Value value, OnRemovedDelegate onRemoved = null)
        {
            return Options.LockStrategy.WithLock(() => 
            {
                return TryAddUnsafe(key, value, out var entry, onRemoved);
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
            if(!TryGetValue(key, out var entry))
            {
                if(MaximumSize.HasValue && Count >= MaximumSize.Value)
                    return notFound.Invoke();
                    
                Options.LockStrategy.WithLock(() => 
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
            return Options.LockStrategy.WithLock(() => 
            {
                return RemoveUnsafe(key);
            });
        }

        internal void Remove(DictionaryEntry<Key, Value> removedEntry)
        {
            Options.LockStrategy.WithLock(() => 
            {
                RemoveUnsafe(removedEntry);
            });
        }
    }
}
