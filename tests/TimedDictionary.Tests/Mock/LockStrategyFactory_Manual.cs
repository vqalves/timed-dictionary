using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimedDictionary.LockStrategy;

namespace TimedDictionary.Tests.Mock
{
    internal class LockStrategyFactory_Manual : ILockStrategyFactory
    {
        internal ILockStrategy SingleLock;

        public LockStrategyFactory_Manual(LockStrategy_Manual singleLock)
        {
            this.SingleLock = singleLock;
        }

        public ILockStrategy CreateNew()
        {
            return SingleLock;
        }
    }
}