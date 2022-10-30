using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TimedDictionary.DateTimeProvider;

namespace TimedDictionary
{
    internal class EntryLifetime
    {
        private readonly IDateTimeProvider DateTimeProvider;
        private readonly ExtendTimeConfiguration ExtendTimeConfiguration;

        private readonly long CreationTime;
        public long? CurrentLimitTime { get; private set; }

        /// <summary>Expected time + Extension limit</summary>
        private readonly long? MaximumAcceptableTime;

        public EntryLifetime(int? expectedDuration, IDateTimeProvider dateTimeProvider, ExtendTimeConfiguration extendTimeConfiguration)
        {
            this.DateTimeProvider = dateTimeProvider;
            this.ExtendTimeConfiguration = extendTimeConfiguration;

            this.CreationTime = dateTimeProvider.CurrentMilliseconds;

            if (expectedDuration.HasValue)
            {
                this.CurrentLimitTime = CreationTime + expectedDuration.Value;
                
                if(extendTimeConfiguration.Limit.HasValue)
                    this.MaximumAcceptableTime = CurrentLimitTime.Value + extendTimeConfiguration.Limit.Value;
            }
        }

        /// <summary>Check how many millisecods must pass to reach the limit</summary>
        /// <returns>If it's reached, returns 0. If there's not limit, returns null</returns>
        public long? MillisecondsUntilLimit()
        {
            long? result = null;

            if(CurrentLimitTime.HasValue)
                result = CurrentLimitTime.Value - DateTimeProvider.CurrentMilliseconds;

            if(result.HasValue && result.Value < 0)
                return 0;

            return result;
        }

        /// <summary>Try to extend the current time limit for the entry</summary>
        /// <returns>What's the new current time limit for the entry. If there's not limit, returns null</returns>
        public long? ExtendCurrentLimitTime()
        {
            // Does not have a default limit
            if(!CurrentLimitTime.HasValue)
                return null;
            
            // Does not contain a extend time configuration
            if(!ExtendTimeConfiguration.Duration.HasValue)
                return CurrentLimitTime;

            var now = DateTimeProvider.CurrentMilliseconds;

            // Present time already exceeds the maximum acceptable, so it's not possible to extend the current limit
            if (MaximumAcceptableTime.HasValue && now > MaximumAcceptableTime.Value)
            {
                if(CurrentLimitTime != MaximumAcceptableTime)
                    CurrentLimitTime = MaximumAcceptableTime;

                return MaximumAcceptableTime;
            }

            var newTime = now + ExtendTimeConfiguration.Duration.Value;

            // Don't reschedule if the current time limit is already bigger than the new calculated time
            if(CurrentLimitTime > newTime)
                return CurrentLimitTime;

            // Make sure the new time limit does not exceed the maximum acceptable time
            if(MaximumAcceptableTime.HasValue && newTime > MaximumAcceptableTime.Value)
                CurrentLimitTime = MaximumAcceptableTime;
            else
                CurrentLimitTime = newTime;

            return CurrentLimitTime;
        }

        public long CurrentLifetimeInMilliseconds()
        {
            return DateTimeProvider.CurrentMilliseconds - CreationTime;
        }
    }
}