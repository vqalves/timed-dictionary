using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TimedDictionary.LockStrategy
{
    internal class LockObjectStrategy<Key> : ILockStrategy<Key>
    {
        private readonly object Lock;

        public LockObjectStrategy()
        {
            this.Lock = new object();
        }

        public void WithLock(Key key, Action action)
        {
            lock (Lock)
            {
                action.Invoke();
            }
        }

        public T WithLock<T>(Key key, Func<T> function)
        {
            lock (Lock)
            {
                return function.Invoke();
            }
        }
    }
}