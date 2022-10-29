using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimedDictionary.LockStrategy
{
    /// <summary>Defines the strategy to enqueue concurrent requests instead of running in parallel</summary>
    internal interface ILockStrategy
    {
        void WithLock(Action action);
        T WithLock<T>(Func<T> function);
    }
}