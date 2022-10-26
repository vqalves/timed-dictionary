using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TimedDictionary.LockStrategy
{
    internal class LockObjectStrategy : ILockStrategy
    {
        private readonly object Lock;

        public LockObjectStrategy()
        {
            this.Lock = new object();
        }

        public void WithLock(Action action)
        {
            lock(Lock)
            {
                action.Invoke();
            }
        }

        public T WithLock<T>(Func<T> function)
        {
            lock(Lock)
            {
                return function.Invoke();
            }
        }
    }
}