using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TimedDictionary.ActionScheduler;

namespace TimedDictionary
{
    internal class DictionaryEntry<T, K>
    {
        private readonly object Lock;
        internal readonly T Key;
        public readonly K Value;
        internal readonly TimedDictionary<T,K>.OnRemovedDelegate OnRemoved;

        private readonly TimedDictionary<T, K> ParentDictionary;
        private bool WasRemoved;

        private IActionScheduler TimeoutScheduler;
        private readonly EntryLifetime Lifetime;

        public DictionaryEntry(T key, K value, TimedDictionary<T,K>.OnRemovedDelegate onRemoved, TimedDictionary<T, K> parentDictionary, EntryLifetime lifetime)
        {
            this.Lock = new object();

            this.Key = key;
            this.Value = value;
            this.OnRemoved = onRemoved;

            this.ParentDictionary = parentDictionary;
            this.WasRemoved = false;

            this.Lifetime = lifetime;

            this.TimeoutScheduler = null;
        }

        internal void AttachTimeoutScheduler(IActionScheduler timeoutScheduler)
        {
            this.TimeoutScheduler = timeoutScheduler;
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

        public int CurrentLifetimeInMilliseconds()
        {
            return Lifetime.CurrentLifetimeInMs();
        }
    }
}