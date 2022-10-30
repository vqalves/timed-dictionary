using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimedDictionary.LockStrategy;

namespace TimedDictionary.Tests.Mock
{
    public class LockStrategy_Manual : ILockStrategy
    {
        private SemaphoreSlim Semaphore;

        public LockStrategy_Manual()
        {
            this.Semaphore = new SemaphoreSlim(0, 1);
        }

        public void Release()
        {
            Semaphore.Release();
        }

        public void WithLock(Action action)
        {
            try
            {
                Semaphore.Wait();
                action.Invoke();
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public T WithLock<T>(Func<T> function)
        {
            try
            {
                Semaphore.Wait();
                return function.Invoke();
            }
            finally
            {
                Semaphore.Release();
            }
        }
    }
}