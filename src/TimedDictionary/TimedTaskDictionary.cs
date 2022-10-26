using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimedDictionary.DateTimeProvider;
using TimedDictionary.LockStrategy;

namespace TimedDictionary
{
    public class TimedTaskDictionary<Key, Value>
    {
        private TimedDictionary<Key, Task<Value>> TimedDictionary;

        public TimedTaskDictionary(int? expectedDuration = null, int? maximumSize = null, ExtendTimeConfiguration extendTimeConfiguration = null, IDateTimeProvider dateTimeProvider = null, ILockStrategy lockStrategy = null)
        {
            this.TimedDictionary = new TimedDictionary<Key, Task<Value>>
            (
                expectedDuration: expectedDuration,
                maximumSize: maximumSize,
                extendTimeConfiguration: extendTimeConfiguration,
                dateTimeProvider: dateTimeProvider,
                lockStrategy: lockStrategy
            );
        }

        public int Count => TimedDictionary.Count;

        public Task<Value> GetValueOrDefaultAsync(Key key) => TimedDictionary.GetValueOrDefault(key);
        public bool ContainsKey(Key key) => TimedDictionary.ContainsKey(key);

        public bool TryAdd(Key key, Value value) => TimedDictionary.TryAdd(key, Task.FromResult(value));
        public bool TryAdd(Key key, Task<Value> task) => TimedDictionary.TryAdd(key, task);

        public Task<Value> GetOrAddIfNewAsync(Key key, Func<Task<Value>> notFound, AfterTaskCompletion afterTaskCompletion = null)
        {
            afterTaskCompletion = afterTaskCompletion ?? AfterTaskCompletion.DoNothing;

            return TimedDictionary.GetOrAddIfNew(key, notFound, onNewEntry: (entry) => 
            {
                afterTaskCompletion.Handle(entry);
            });
        }

        public Task<Value> GetOrAddIfNewAsync(Key key, Func<Value> notFound)
        {
            Func<Task<Value>> task = () => Task.FromResult(notFound.Invoke());
            return GetOrAddIfNewAsync(key, task);
        }

        public bool Remove(Key key) => TimedDictionary.Remove(key);
    }
}
