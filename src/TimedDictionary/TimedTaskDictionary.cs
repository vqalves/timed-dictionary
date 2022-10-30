using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TimedDictionary.DateTimeProvider;
using TimedDictionary.LockStrategy;

[assembly: InternalsVisibleTo("TimedDictionary.Tests")]
namespace TimedDictionary
{
    public class TimedTaskDictionary<Key, Value>
    {
        private TimedDictionary<Key, Task<Value>> TimedDictionary;

        /// <summary>Time-based self-cleaning dictionary structure. The entries are automatically removed from the structure after the specified time</summary>
        /// <param name="expectedDuration">How many milliseconds each value should be kept in the dictionary. If null, the structure will keep all records until they are manually removed.</param>
        /// <param name="maximumSize">Maximum amount of keys allowed at a time. When the limit is reached, no new keys will be added and new keys will always execute the evaluation function. If null, there will be no limits to the dictionary size.</param>
        /// <param name="extendTimeConfiguration">Allows the increase of each object lifetime inside the dictionary by X milliseconds, up to Y milliseconds, everytime the value is retrieved. If null, the object lifetime will obey the `expectedDuration` parameter.</param>
        /// <param name="onRemoved">Callback called whenever the value is removed from the object, either by timeout or manually. Called only once per value. Example: (valueRemoved) => { }</param>
        public TimedTaskDictionary(int? expectedDuration = null, int? maximumSize = null, ExtendTimeConfiguration extendTimeConfiguration = null, TimedDictionary<Key, Task<Value>>.OnRemovedDelegate onRemoved = null) : this
        (
            expectedDuration: expectedDuration,
            changeOptions: null,
            maximumSize: maximumSize,
            extendTimeConfiguration: null,
            onRemoved: onRemoved
        )
        {
            
        }

        internal TimedTaskDictionary(Action<TimedDictionaryOptions<Key>> changeOptions, int? expectedDuration = null, int? maximumSize = null, ExtendTimeConfiguration extendTimeConfiguration = null, TimedDictionary<Key, Task<Value>>.OnRemovedDelegate onRemoved = null)
        {
            this.TimedDictionary = new TimedDictionary<Key, Task<Value>>
            (
                changeOptions: changeOptions,

                expectedDuration: expectedDuration,
                maximumSize: maximumSize,
                extendTimeConfiguration: extendTimeConfiguration,
                onRemoved: onRemoved
            );
        }

        /// <summary>Amount of items currently in the dictionary</summary>
        public int Count => TimedDictionary.Count;

        /// <summary>Get the current value associated with the key</summary>
        /// <param name="key">Key which the value was stored</param>
        public Task<Value> GetValueOrDefaultAsync(Key key) => TimedDictionary.GetValueOrDefault(key);
        public bool ContainsKey(Key key) => TimedDictionary.ContainsKey(key);

        public bool TryAdd(Key key, Value value, TimedDictionary<Key, Task<Value>>.OnRemovedDelegate onRemoved = null) => TimedDictionary.TryAdd(key, Task.FromResult(value), onRemoved);
        public bool TryAdd(Key key, Task<Value> task, TimedDictionary<Key, Task<Value>>.OnRemovedDelegate onRemoved = null) => TimedDictionary.TryAdd(key, task, onRemoved);

        /// <summary>Get the current value associated with the key. If the key is not found, generate a new value and associate with key</summary>
        /// <param name="key">Key which the value was stored</param>
        /// <param name="notFound">Generate a new value for the key. This function locks the object, be mindful when using parallel processing.</param>
        /// <param name="afterTaskCompletion">Behavior that automatically triggers inside the dictionary then the task is completed.</param>
        /// <param name="onRemoved">Overrides onRemoved function specified on the dictionary constructor. Callback called whenever the value is removed from the object, either by timeout or manually. Called only once per value. Example: (valueRemoved) => { }</param>
        public Task<Value> GetOrAddIfNewAsync(Key key, Func<Task<Value>> notFound, AfterTaskCompletion afterTaskCompletion = null, TimedDictionary<Key, Task<Value>>.OnRemovedDelegate onRemoved = null)
        {
            afterTaskCompletion = afterTaskCompletion ?? AfterTaskCompletion.DoNothing;

            return TimedDictionary.GetOrAddIfNew
            (
                key,
                notFound,
                onNewEntry: (entry) => afterTaskCompletion.Handle(entry),
                onRemoved
            );
        }

        public Task<Value> GetOrAddIfNewAsync(Key key, Func<Value> notFound, TimedDictionary<Key, Task<Value>>.OnRemovedDelegate onRemoved = null)
        {
            Func<Task<Value>> task = () => Task.FromResult(notFound.Invoke());
            return GetOrAddIfNewAsync(key, task, onRemoved: onRemoved);
        }

        public bool Remove(Key key) => TimedDictionary.Remove(key);
    }
}