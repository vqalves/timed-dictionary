using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimedDictionary.TimestampProvider;

namespace TimedDictionary.Tests.Mock
{
    internal class TimestampProvider_Fixed : ITimestampProvider
    {
        internal long Milliseconds { get; set; }

        public DateTime StartUtc { get; private set; }
        public Timestamp CurrentTimestamp => new Timestamp(this, Milliseconds);

        public TimestampProvider_Fixed()
        {
            this.StartUtc = DateTime.UtcNow;
            this.Milliseconds = 0;
        }
    }
}