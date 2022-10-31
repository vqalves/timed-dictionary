namespace TimedDictionary.LockStrategy
{
    internal interface ILockStrategyFactory
    {
        ILockStrategy CreateNew();
    }
}