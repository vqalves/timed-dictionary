using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimedDictionary.LockStrategy;

namespace TimedDictionary.ShardedDictionaryStructure
{
    internal class ShardedDictionaries<Key, Value>
    {
        private readonly ShardedDictionary<Key, Value>[] Shards;
        private readonly int ShardCount;

        public int EntryCount => Shards.Sum(x => x.Dictionary.Count);

        public ShardedDictionaries(ILockStrategyFactory lockStrategyFactory)
        {
            this.ShardCount = 255;

            this.Shards = new ShardedDictionary<Key, Value>[ShardCount + 1];
            for (var i = 0; i < Shards.Length; i++)
            {
                var lockStrategy = lockStrategyFactory.CreateNew();
                Shards[i] = new ShardedDictionary<Key, Value>(lockStrategy);
            }

            /*
            this.Comparer = null;
            if (!typeof(Key).IsValueType)
                Comparer = EqualityComparer<Key>.Default;

            if (Comparer == null)
                GenerateHashCode = (key) => key.GetHashCode();
            else
                GenerateHashCode = Comparer.GetHashCode;
            */
        }

        public ShardedDictionary<Key, Value> GetShard(Key key)
        {
            var hash = key.GetHashCode();
            var modulo = hash & ShardCount;
            return Shards[modulo];
        }
    }
}