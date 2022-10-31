using System;
using TimedDictionary.TimestampProvider;

namespace TimedDictionary
{
    internal class EntryLifetime
    {
        private readonly ITimestampProvider TimestampProvider;
        private readonly ExtendTimeConfiguration ExtendTimeConfiguration;

        private readonly Timestamp CreationTime;
        public Timestamp? CurrentLimitTime { get; private set; }

        /// <summary>Expected time + Extension limit</summary>
        private readonly Timestamp? MaximumAcceptableTime;

        public EntryLifetime(int? expectedDuration, ITimestampProvider timestampProvider, ExtendTimeConfiguration extendTimeConfiguration)
        {
            this.TimestampProvider = timestampProvider;
            this.ExtendTimeConfiguration = extendTimeConfiguration;

            this.CreationTime = timestampProvider.CurrentTimestamp;

            if (expectedDuration.HasValue)
            {
                this.CurrentLimitTime = CreationTime.AddMilliseconds(expectedDuration.Value);
                
                if(extendTimeConfiguration.Limit.HasValue)
                    this.MaximumAcceptableTime = CurrentLimitTime.Value.AddMilliseconds(extendTimeConfiguration.Limit.Value);
            }
        }

        /// <summary>Check how many millisecods must pass to reach the limit</summary>
        /// <returns>If it's reached, returns 0. If there's not limit, returns null</returns>
        public long? MillisecondsUntilLimit()
        {
            TimeSpan? result = null;

            if(CurrentLimitTime.HasValue)
                result = CurrentLimitTime.Value - TimestampProvider.CurrentTimestamp;

            if(result.HasValue && result.Value.TotalMilliseconds < 0)
                return 0;

            return (long?)result?.TotalMilliseconds;
        }

        /// <summary>Try to extend the current time limit for the entry</summary>
        /// <returns>What's the new current time limit for the entry. If there's not limit, returns null</returns>
        public Timestamp? ExtendCurrentLimitTime()
        {
            // Does not have a default limit
            if(!CurrentLimitTime.HasValue)
                return null;
            
            // Does not contain a extend time configuration
            if(!ExtendTimeConfiguration.Duration.HasValue)
                return CurrentLimitTime;

            var now = TimestampProvider.CurrentTimestamp;

            // Present time already exceeds the maximum acceptable, so it's not possible to extend the current limit
            if (MaximumAcceptableTime.HasValue && now > MaximumAcceptableTime.Value)
            {
                if(CurrentLimitTime != MaximumAcceptableTime)
                    CurrentLimitTime = MaximumAcceptableTime;

                return MaximumAcceptableTime;
            }

            var newTime = now.AddMilliseconds(ExtendTimeConfiguration.Duration.Value);

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
            var timespan = TimestampProvider.CurrentTimestamp - CreationTime;
            return (long)timespan.TotalMilliseconds;
        }
    }
}