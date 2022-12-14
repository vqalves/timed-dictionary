using TimedDictionary.Tests.Mock;
using Xunit;

namespace TimedDictionary.Tests.Test.TimedTaskDictionaryTests
{
    public class TimedTaskDictionaryTests_GetOrAddIfNewAsync
    {
        [Fact]
        public async void TimedTaskDictionary_GetOrAddIfNewAsync_ConcurrentDuplicatedWithAutoremoval()
        {
            var lockStrategy = new LockStrategy_Manual();
            var lockFactory = new LockStrategyFactory_Manual(lockStrategy);
            
            
            int key = 1;
            string value = "Test";

            TimedTaskDictionary<int, string> dictionary = new TimedTaskDictionary<int, string>(expectedDuration: null, changeOptions: (options) => 
            { 
                options.LockStrategyFactory = lockFactory;
            });

            var createValue = async () => 
            {
                await Task.Delay(100);
                return value;
            };

            // Try to add two elements concurrently
            var task1 = Task.Run(() => dictionary.GetOrAddIfNewAsync(key, createValue, AfterTaskCompletion.RemoveFromDictionary));
            var task2 = Task.Run(() => dictionary.GetOrAddIfNewAsync(key, createValue, AfterTaskCompletion.RemoveFromDictionary));

            // Unlock one semaphore to see if the two elements are executed orderly
            // Exception will be thrown if an error occurs
            await Task.Delay(50);
            lockStrategy.Release();

            await Task.WhenAll(task1, task2);
        }
    }
}