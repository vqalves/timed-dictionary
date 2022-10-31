using System;
using System.Linq;

namespace TimedDictionary.ShardedDictionaryStructure
{
    internal class ShardedDictionaries<Key, Value>
    {
        private readonly ShardedDictionary<Key, Value>[] Shards;
        private readonly int ShardCount;

        internal readonly ExtendTimeConfiguration ExtendTimeConfiguration;
        internal readonly TimedDictionaryOptions Options;
        internal readonly int? ExpectedDuration;
        internal readonly int? MaximumSize;

        internal readonly OnValueRemovedDelegate<Value> OnRemoved;

        public int EntryCount => Shards.Sum(x => x.Dictionary.Count);

        public ShardedDictionaries(ExtendTimeConfiguration extendTimeConfiguration, Action<TimedDictionaryOptions> changeOptions, int? expectedDuration, int? maximumSize, OnValueRemovedDelegate<Value> onRemoved)
        {
            this.Options = new TimedDictionaryOptions();
            changeOptions?.Invoke(Options);

            this.ExtendTimeConfiguration = extendTimeConfiguration ?? ExtendTimeConfiguration.None;

            this.ExpectedDuration = expectedDuration;
            this.MaximumSize = maximumSize;
            this.OnRemoved = onRemoved;

            this.ShardCount = 255;
            this.Shards = new ShardedDictionary<Key, Value>[ShardCount + 1];
            for (var i = 0; i < Shards.Length; i++)
                Shards[i] = new ShardedDictionary<Key, Value>(i, this);
            
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