using System;
using System.Diagnostics;

namespace TimedDictionary.TimestampProvider
{
    internal class TimestampDefaultProvider : ITimestampProvider
    {
        public static TimestampDefaultProvider Instance = new TimestampDefaultProvider();

        public DateTime StartUtc { get; private set; }
        private readonly Stopwatch Stopwatch;

        private TimestampDefaultProvider() 
        {
            this.StartUtc = DateTime.UtcNow;

            this.Stopwatch = new Stopwatch();
            this.Stopwatch.Start();
        }

        public Timestamp CurrentTimestamp => new Timestamp(this, Stopwatch.ElapsedMilliseconds);
    }
}