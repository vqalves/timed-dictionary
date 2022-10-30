using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TimedDictionary.ActionScheduler;
using TimedDictionary.DateTimeProvider;
using TimedDictionary.ShardedDictionaryStructure;

namespace TimedDictionary
{
    internal class DictionaryEntry<T, K>
    {
        private readonly object Lock;

        internal readonly T Key;
        public readonly K Value;

        private readonly ShardedDictionary<T, K> ParentDictionary;
        internal readonly TimedDictionary<T,K>.OnRemovedDelegate OnRemovedCallback;
        private bool WasRemoved;

        internal readonly IActionScheduler TimeoutScheduler;
        private readonly EntryLifetime Lifetime;

        public DictionaryEntry(T key, K value, TimedDictionary<T,K>.OnRemovedDelegate onRemovedCallback, ShardedDictionary<T, K> parentDictionary, EntryLifetime lifetime, IDateTimeProvider dateTimeProvider)
        {
            this.Lock = new object();

            this.Key = key;
            this.Value = value;
            this.OnRemovedCallback = onRemovedCallback;

            this.ParentDictionary = parentDictionary;
            this.WasRemoved = false;

            this.Lifetime = lifetime;

            this.TimeoutScheduler = ActionSchedulerFactory.CreateUnstarted(dateTimeProvider, RemoveItselfFromDictionary, (int?)lifetime.MillisecondsUntilLimit());
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
            }
        }

        public long CurrentLifetimeInMilliseconds()
        {
            return Lifetime.CurrentLifetimeInMilliseconds();
        }
    }
}