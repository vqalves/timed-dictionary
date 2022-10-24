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

        public TimedDictionary(int? expectedDuration = null, ExtendTimeConfiguration extendTimeConfiguration = null, IDateTimeProvider dateTimeProvider = null)
        {
            this.DateTimeProvider = dateTimeProvider ?? DefaultDateTimeProvider.Instance;
            this.ExtendTimeConfiguration = extendTimeConfiguration ?? ExtendTimeConfiguration.None(DateTimeProvider);
            
            this.Dictionary = new Dictionary<Key, DictionaryEntry<Key, Value>>();
            this.Lock = new Object();
            this.ExpectedDuration = expectedDuration;
        }

        private bool TryGetValue(Key key, out DictionaryEntry<Key, Value> refreshableValue)
        {
            if(Dictionary.TryGetValue(key, out refreshableValue))
            {
                refreshableValue.RefreshCleanUpDuration();
                return true;
            }

            return false;
        }

        public Value GetValueOrDefault(Key key)
        {
            if(TryGetValue(key, out var refreshableValue))
                return refreshableValue.Value;

            return default(Value);
        }

        public Value GetOrAddIfNew(Key key, Func<Value> notFound)
        {
            if(!TryGetValue(key, out var refreshableValue))
            {
                lock(Lock)
                {
                    if(!TryGetValue(key, out refreshableValue))
                    {
                        var newValue = notFound.Invoke();

                        refreshableValue = new DictionaryEntry<Key, Value>(key, newValue, this, ExpectedDuration, ExtendTimeConfiguration, DateTimeProvider);
                        Dictionary.Add(key, refreshableValue);
                    }
                }
            }

            return refreshableValue.Value;
        }

        public bool Remove(Key key)
        {
            lock(Lock)
                return Dictionary.Remove(key);
        }
    }
}
