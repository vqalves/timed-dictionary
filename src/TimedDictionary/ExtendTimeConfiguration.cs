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

        public static ExtendTimeConfiguration None() => new ExtendTimeConfiguration(duration: null, limit: null);
        internal static ExtendTimeConfiguration None(IDateTimeProvider dateTimeProvider) => new ExtendTimeConfiguration(dateTimeProvider: dateTimeProvider, duration: null, limit: null);
    }
}