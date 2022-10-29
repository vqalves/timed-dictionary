using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimedDictionary.DateTimeProvider;

namespace TimedDictionary
{
    public class ExtendTimeConfiguration
    {
        private readonly IDateTimeProvider DateTimeProvider;

        public readonly int? Duration;
        public readonly int? Limit;

        /// <summary>Used to configure how each entry of the dictionary should extend it's lifetime</summary>
        /// <param name="duration">Each hit of the entry will extend the lifetime up to this value. In milisseconds</param>
        /// <param name="limit">Total duration available to extend, in milisseconds</param>
        public ExtendTimeConfiguration(int? duration = null, int? limit = null) : this(duration: duration, limit: limit, dateTimeProvider: DefaultDateTimeProvider.Instance)
        {
            
        }

        internal ExtendTimeConfiguration(IDateTimeProvider dateTimeProvider, int? duration = null, int? limit = null)
        {
            DateTimeProvider = dateTimeProvider;

            Duration = duration;
            Limit = limit;
        }

        public int? CalculateExtendedTime(DateTime extensionStart)
        {
            // Configured to not extend
            if(!Duration.HasValue)
                return null;

            // Check if the extensionStart informed justify extending the time
            var extendedFromNow = DateTimeProvider.Now.AddMilliseconds(Duration.Value);
            if(extensionStart > extendedFromNow)
                return null;

            // Verify the remaining possible extend time, considering the configured limit
            if(Limit.HasValue)
            {
                var totalExtendedTime = (int)DateTimeProvider.Now.Subtract(extensionStart).TotalMilliseconds;

                if(totalExtendedTime + Duration.Value > Limit.Value)
                {
                    var availableExtendedTime = Limit.Value - totalExtendedTime;

                    if(availableExtendedTime > 0) 
                        return availableExtendedTime;
                    
                    return null;
                } 
            }

            return Duration.Value;
        }

        public static ExtendTimeConfiguration None() => new ExtendTimeConfiguration(duration: null, limit: null);
        internal static ExtendTimeConfiguration None(IDateTimeProvider dateTimeProvider) => new ExtendTimeConfiguration(dateTimeProvider: dateTimeProvider, duration: null, limit: null);
    }
}