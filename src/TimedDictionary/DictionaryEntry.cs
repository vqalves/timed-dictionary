using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TimedDictionary.DateTimeProvider;

namespace TimedDictionary
{
    public class DictionaryEntry<T, K>
    {
        private readonly object Lock;

        internal readonly T Key;
        public readonly K Value;

        private readonly TimedDictionary<T, K> ParentDictionary;
        private Task CleanUpTask;
        private CancellationTokenSource LastCancellationTokenSource;
        
        private readonly IDateTimeProvider DateTimeProvider;
        private readonly ExtendTimeConfiguration ExtendTimeConfiguration;

        private int? ExpectedDuration;
        private readonly DateTime CreationTime;
        private readonly DateTime? LimitTime;
        

        public DictionaryEntry(T key, K value, TimedDictionary<T, K> parentDictionary, int? expectedDuration, ExtendTimeConfiguration extendTimeConfiguration, IDateTimeProvider dateTimeProvider)
        {
            this.Lock = new Object();

            this.Key = key;
            this.Value = value;

            this.ExpectedDuration = expectedDuration;
            this.DateTimeProvider = dateTimeProvider;
            this.ExtendTimeConfiguration = extendTimeConfiguration;

            this.ParentDictionary = parentDictionary;
            this.CreationTime = DateTimeProvider.Now;

            if(ExpectedDuration.HasValue)
            {
                LimitTime = CreationTime.AddMilliseconds(ExpectedDuration.Value);
                SetCancellationToken(ExpectedDuration.Value);
            }
        }

        public void RefreshCleanUpDuration()
        {
            if(!LimitTime.HasValue)
                return;

            var newDuration = ExtendTimeConfiguration.CalculateExtendedTime(LimitTime.Value);
            if(!newDuration.HasValue)
                return;

            SetCancellationToken(newDuration.Value);
        }

        private void SetCancellationToken(int duration)
        {
            lock(Lock)
            {
                LastCancellationTokenSource?.Cancel();

                LastCancellationTokenSource = new CancellationTokenSource();
                var token = LastCancellationTokenSource.Token;

                CleanUpTask = Task.Run(async () => 
                {
                    await Task.Delay(duration, token);

                    if(!token.IsCancellationRequested)
                        ParentDictionary.Remove(this);
                }, token);
            }
        }
    }
}