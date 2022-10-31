using System;
using TimedDictionary.ActionScheduler;
using TimedDictionary.ShardedDictionaryStructure;
using TimedDictionary.TimestampProvider;

namespace TimedDictionary
{
    internal class DictionaryEntry<TKey, TValue>
    {
        private readonly object Lock;

        internal readonly TKey Key;
        public readonly TValue Value;

        private readonly ShardedDictionary<TKey, TValue> ParentDictionary;
        internal readonly OnValueRemovedDelegate<TValue> OnRemovedCallback;
        private bool WasRemoved;

        internal readonly IActionScheduler TimeoutScheduler;
        private readonly EntryLifetime Lifetime;

        public DictionaryEntry(TKey key, TValue value, OnValueRemovedDelegate<TValue> onRemovedCallback, ShardedDictionary<TKey, TValue> parentDictionary, EntryLifetime lifetime, ITimestampProvider timestampProvider)
        {
            this.Lock = new object();

            this.Key = key;
            this.Value = value;
            this.OnRemovedCallback = onRemovedCallback;

            this.ParentDictionary = parentDictionary;
            this.WasRemoved = false;

            this.Lifetime = lifetime;

            this.TimeoutScheduler = ActionSchedulerFactory.CreateUnstarted(timestampProvider, RemoveItselfFromDictionary, (int?)lifetime.MillisecondsUntilLimit());
        }

        public void RefreshCleanUpDuration()
        {
            var newDuration = Lifetime.ExtendCurrentLimitTime();
            
            if(newDuration.HasValue)
                TimeoutScheduler.RescheduleTo(newDuration.Value);
        }

        public void RemoveItselfFromDictionary()
        {
            lock(Lock)
            {
                if(!WasRemoved)
                {
                    WasRemoved = true;
                    ParentDictionary.Remove(this);
                }
                else
                {
                    Console.WriteLine("Double hit");
                }
            }
        }

        public long CurrentLifetimeInMilliseconds()
        {
            return Lifetime.CurrentLifetimeInMilliseconds();
        }
    }
}