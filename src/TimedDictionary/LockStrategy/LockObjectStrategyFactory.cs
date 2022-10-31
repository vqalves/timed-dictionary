namespace TimedDictionary.LockStrategy
{
    internal class LockObjectStrategyFactory : ILockStrategyFactory
    {
        public ILockStrategy CreateNew()
        {
            return new LockObjectStrategy();
        }
    }
}