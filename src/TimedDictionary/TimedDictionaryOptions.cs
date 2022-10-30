using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimedDictionary.DateTimeProvider;
using TimedDictionary.LockStrategy;

namespace TimedDictionary
{
    internal class TimedDictionaryOptions<Key>
    {
        public IDateTimeProvider DateTimeProvider { get; set; }
        public ILockStrategy<Key> LockStrategy { get; set; }

        public TimedDictionaryOptions()
        {
            this.DateTimeProvider = DefaultDateTimeProvider.Instance;
            this.LockStrategy = new LockBucketStrategy<Key>();
        }
    }
}