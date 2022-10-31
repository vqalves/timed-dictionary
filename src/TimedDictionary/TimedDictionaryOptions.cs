using TimedDictionary.LockStrategy;
using TimedDictionary.TimestampProvider;

namespace TimedDictionary
{
    internal class TimedDictionaryOptions
    {
        public ITimestampProvider TimestampProvider { get; set; }
        public ILockStrategyFactory LockStrategyFactory { get; set; }

        public TimedDictionaryOptions()
        {
            this.TimestampProvider = TimestampDefaultProvider.Instance;
            this.LockStrategyFactory = new LockObjectStrategyFactory();
        }
    }
}