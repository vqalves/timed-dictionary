using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimedDictionary.LockStrategy
{
    internal interface ILockStrategyFactory
    {
        ILockStrategy CreateNew();
    }
}