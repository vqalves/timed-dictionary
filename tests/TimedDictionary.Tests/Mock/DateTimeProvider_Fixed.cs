using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimedDictionary.DateTimeProvider;

namespace TimedDictionary.Tests.Mock
{
    public class DateTimeProvider_Fixed : IDateTimeProvider
    {
        public DateTime Now { get; set; }

        public long CurrentMilliseconds => Now.Ticks / TimeSpan.TicksPerMillisecond;

        public DateTimeProvider_Fixed(DateTime value)
        {
            this.Now = value;
        }
    }
}