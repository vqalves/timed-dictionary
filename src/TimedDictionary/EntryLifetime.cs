using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimedDictionary.DateTimeProvider;

namespace TimedDictionary
{
    internal class EntryLifetime
    {
        private readonly IDateTimeProvider DateTimeProvider;
        private readonly ExtendTimeConfiguration ExtendTimeConfiguration;
        private readonly DateTime CreationTime;

        /// <summary>Expected time + Extension limit</summary>
        private readonly DateTime? MaximumAcceptableTime;

        public DateTime? CurrentLimitTime { get; private set; }
        
        public EntryLifetime(int? expectedDuration, IDateTimeProvider dateTimeProvider, ExtendTimeConfiguration extendTimeConfiguration)
        {
            this.DateTimeProvider = dateTimeProvider;
            this.ExtendTimeConfiguration = extendTimeConfiguration;

            this.CreationTime = DateTimeProvider.Now;

            if(expectedDuration.HasValue)
            {
                this.CurrentLimitTime = CreationTime.AddMilliseconds(expectedDuration.Value);
                
                if(extendTimeConfiguration.Limit.HasValue)
                    this.MaximumAcceptableTime = CurrentLimitTime.Value.AddMilliseconds(extendTimeConfiguration.Limit.Value);
            }
        }

        /// <summary>Check how many millisecods must pass to reach the limit</summary>
        /// <returns>If it's reached, returns 0. If there's not limit, returns null</returns>
        public int? MillisecondsUntilLimit()
        {
            int? result = null;

            if(CurrentLimitTime.HasValue)
                result = (int)CurrentLimitTime.Value.Subtract(DateTimeProvider.Now).TotalMilliseconds;

            if(result.HasValue && result.Value < 0)
                return 0;

            return result;
        }

        /// <summary>Extend the current time limit for the entry</summary>
        /// <returns>The amount of milliseconds the time was extended by. If the time was not extended, returns null</returns>
        public int? ExtendCurrentLimitTime()
        {
            // Does not have a default limit
            if(!CurrentLimitTime.HasValue)
                return null;
            
            // Does not contain a extend time configuration
            if(!ExtendTimeConfiguration.Duration.HasValue)
                return null;

            // Current limit cannot be extended more because it capped with the maximum acceptable
            if(MaximumAcceptableTime.HasValue && MaximumAcceptableTime.Value <= CurrentLimitTime.Value)
                return null;

            var now = DateTimeProvider.Now;

            // Present time already exceeds the maximum acceptable, so it's not possible to extend the current limit
            if(MaximumAcceptableTime.HasValue && now > MaximumAcceptableTime.Value)
                return null;

            var newTime = now.AddMilliseconds(ExtendTimeConfiguration.Duration.Value);
            
            // Extension fails because the current limit is already bigger than now + extension
            if(CurrentLimitTime.Value > newTime)
                return null;

            int extendedTime;

            // Normal extension cannot exceed the maximum acceptable
            if(MaximumAcceptableTime.HasValue && newTime > MaximumAcceptableTime.Value)
            {
                CurrentLimitTime = MaximumAcceptableTime;
                extendedTime = (int)MaximumAcceptableTime.Value.Subtract(now).TotalMilliseconds;
            }
            else
            {
                extendedTime = (int)newTime.Subtract(CurrentLimitTime.Value).TotalMilliseconds;
            }

            return extendedTime;
        }

        public int CurrentLifetimeInMs()
        {
            return (int)DateTimeProvider.Now.Subtract(CreationTime).TotalMilliseconds;
        }
    }
}