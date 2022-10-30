using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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