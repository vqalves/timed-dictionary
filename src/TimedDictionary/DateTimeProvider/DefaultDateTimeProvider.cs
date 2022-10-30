using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace TimedDictionary.DateTimeProvider
{
    internal class DefaultDateTimeProvider : IDateTimeProvider
    {
        public static DefaultDateTimeProvider Instance = new DefaultDateTimeProvider();

        private Stopwatch Stopwatch;

        private DefaultDateTimeProvider() 
        {
            this.Stopwatch = new Stopwatch();
            this.Stopwatch.Start();
        }

        public DateTime Now => DateTime.Now;
        public long CurrentMilliseconds => Stopwatch.ElapsedMilliseconds;
    }
}