using TimedDictionary.Tests.Mock;
using Xunit;

namespace TimedDictionary.Tests.Test.TimedDictionaryTests
{
    public class TimedDictionaryTests_GetValueOrDefault
    {
        
        [Fact]
        public void TimedDictionary_GetValueOrDefault_RetrieveNoDuration()
        {
            int key = 1;
            string value = "Test";

            TimedDictionary<int, string> dictionary = new TimedDictionary<int, string>(expectedDuration: null);
            dictionary.GetOrAddIfNew(key, () => value);

            var result = dictionary.GetValueOrDefault(key);
            Assert.Equal(value, result);
        }

        [Fact]
        public void TimedDictionary_GetValueOrDefault_RetrieveWithinDuration()
        {
            int duration = 10;

            int key = 1;
            string value = "Test";

            TimedDictionary<int, string> dictionary = new TimedDictionary<int, string>(expectedDuration: duration);
            dictionary.GetOrAddIfNew(key, () => value);

            var result = dictionary.GetValueOrDefault(key);
            Assert.Equal(value, result);
        }

        [Fact]
        public async Task TimedDictionary_GetValueOrDefault_ExpireAfterDuration()
        {
            int duration = 10;

            int key = 1;
            string value = "Test";

            TimedDictionary<int, string> dictionary = new TimedDictionary<int, string>(expectedDuration: duration);
            dictionary.GetOrAddIfNew(key, () => value);

            await Task.Delay(duration * 2);

            var result = dictionary.GetValueOrDefault(key);
            Assert.Null(result);
        }

        [Fact]
        public async Task TimedDictionary_GetValueOrDefault_WithExtendTime_ExpireByNotTriggering()
        {
            int duration = 50;
            var config = new ExtendTimeConfiguration(duration: 50);

            int key = 1;
            string value = "Test";

            TimedDictionary<int, string> dictionary = new TimedDictionary<int, string>(expectedDuration: duration, extendTimeConfiguration: config);
            dictionary.GetOrAddIfNew(key, () => value);

            await Task.Delay(duration * 2);

            var result = dictionary.GetValueOrDefault(key);
            Assert.Null(result);
        }

        [Fact]
        public async Task TimedDictionary_GetValueOrDefault_WithExtendTime_ExtendTimeOnce()
        {
            int duration = 500;
            var config = new ExtendTimeConfiguration(duration: 500);

            int key = 1;
            string value = "Test";

            TimedDictionary<int, string> dictionary = new TimedDictionary<int, string>(expectedDuration: duration, extendTimeConfiguration: config);
            dictionary.GetOrAddIfNew(key, () => value);

            // Wait and trigger fresh
            await Task.Delay(duration / 2);
            dictionary.GetValueOrDefault(key);
            await Task.Delay(duration / 2);

            var result = dictionary.GetValueOrDefault(key);
            Assert.Equal(value, result);
        }

        [Fact]
        public async Task TimedDictionary_GetValueOrDefault_WithExtendTime_ExpireByLimit()
        {
            int duration = 50;
            var config = new ExtendTimeConfiguration(duration: 50, limit: 0);

            int key = 1;
            string value = "Test";

            TimedDictionary<int, string> dictionary = new TimedDictionary<int, string>(expectedDuration: duration, extendTimeConfiguration: config);
            dictionary.GetOrAddIfNew(key, () => value);

            // Wait and trigger fresh
            var partialWait = (int)(duration * 0.8);
            await Task.Delay(partialWait);
            dictionary.GetValueOrDefault(key);
            await Task.Delay(partialWait);

            var result = dictionary.GetValueOrDefault(key);
            Assert.Null(result);
        }
    }
}