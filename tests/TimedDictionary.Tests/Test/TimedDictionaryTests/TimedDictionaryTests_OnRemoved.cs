using TimedDictionary.Tests.Mock;
using Xunit;

namespace TimedDictionary.Tests.Test.TimedDictionaryTests
{
    public class TimedDictionaryTests_OnRemoved
    {
        
        [Fact]
        public void TimedDictionaryTests_OnRemoved_Null()
        {
            int key = 1;
            string value = "Test";

            TimedDictionary<int, string> dictionary = new TimedDictionary<int, string>(expectedDuration: null);
            dictionary.GetOrAddIfNew(key, () => value, onRemoved: null);

            var result = dictionary.Remove(key);
            Assert.True(result);
        }

        [Fact]
        public void TimedDictionaryTests_OnRemoved_ManualRemoval()
        {
            int key = 1;
            string value = "Test";
            bool removed = false;

            TimedDictionary<int, string> dictionary = new TimedDictionary<int, string>(expectedDuration: null);
            dictionary.GetOrAddIfNew(key, () => value, onRemoved: (value) => removed = true);
            dictionary.Remove(key);

            Assert.True(removed);
        }

        [Fact]
        public void TimedDictionaryTests_OnRemovedConstructor_ManualRemoval()
        {
            int key = 1;
            string value = "Test";
            bool removed = false;

            TimedDictionary<int, string> dictionary = new TimedDictionary<int, string>(expectedDuration: null, onRemoved: (value) => removed = true);
            dictionary.GetOrAddIfNew(key, () => value);
            dictionary.Remove(key);

            Assert.True(removed);
        }

        [Fact]
        public async Task TimedDictionaryTests_OnRemoved_TimeoutRemoval()
        {
            int key = 1;
            string value = "Test";
            bool removed = false;

            TimedDictionary<int, string> dictionary = new TimedDictionary<int, string>(expectedDuration: 0);
            dictionary.GetOrAddIfNew(key, () => value, onRemoved: (value) => removed = true);
            
            await Task.Delay(50);

            Assert.True(removed);
        }
    }
}