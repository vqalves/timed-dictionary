using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimedDictionary.DateTimeProvider;
using TimedDictionary.LockStrategy;

namespace TimedDictionary
{
    internal class TimedDictionaryOptions
    {
        public IDateTimeProvider DateTimeProvider { get; set; }
        public ILockStrategy LockStrategy { get; set; }

        public TimedDictionaryOptions()
        {
            this.DateTimeProvider = DefaultDateTimeProvider.Instance;
            this.LockStrategy = new LockObjectStrategy();
        }
    }
}