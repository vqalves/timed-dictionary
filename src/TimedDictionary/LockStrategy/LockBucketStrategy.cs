using System;
using System.Collections.Generic;
using System.Text;

namespace TimedDictionary.LockStrategy
{
    internal class LockBucketStrategy<Key> : ILockStrategy<Key>
    {
        private readonly object[] Locks;
        private readonly int Size;

        public LockBucketStrategy()
        {
            this.Size = 255;
            this.Locks = new object[Size + 1];

            for (var i = 0; i < Locks.Length; i++)
                Locks[i] = new object();
        }

        private object GetLock(Key key)
        {
            var hash = key.GetHashCode();
            var modulo = hash & Size;
            return Locks[modulo];
        }

        public void WithLock(Key key, Action action)
        {
            lock (GetLock(key))
            {
                action.Invoke();
            }
        }

        public T WithLock<T>(Key key, Func<T> function)
        {
            lock (GetLock(key))
            {
                return function.Invoke();
            }
        }
    }
}
