using TimedDictionary.Tests.Mock;
using Xunit;

namespace TimedDictionary.Tests.Test
{
    public class TimedDictionaryTests
    {
        [Fact]
        public void TimedDictionary_GetOrAddIfNew()
        {
            int key = 1;
            string value = "Test";

            TimedDictionary<int, string> dictionary = new TimedDictionary<int, string>();
            var result = dictionary.GetOrAddIfNew(key, () => value);

            Assert.Equal(value, result);
        }

        [Fact]
        public void TimedDictionary_GetOrAddIfNew_OnOverflow()
        {
            int key = 1;
            string value = "Test";

            TimedDictionary<int, string> dictionary = new TimedDictionary<int, string>(maximumSize: 0);
            var result = dictionary.GetOrAddIfNew(key, () => value);

            Assert.Equal(value, result);
        }

        [Fact]
        public void TimedDictionary_GetValueOrDefault_RetrieveNoDuration()
        {
            int key = 1;
            string value = "Test";

            TimedDictionary<int, string> dictionary = new TimedDictionary<int, string>();
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
            int duration = 50;
            var config = new ExtendTimeConfiguration(duration: 50);

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

        [Fact]
        public void TimedDictionary_Count_NoLimit()
        {
            int key = 1;
            string value = "Test";

            TimedDictionary<int, string> dictionary = new TimedDictionary<int, string>();
            dictionary.GetOrAddIfNew(key, () => value);

            Assert.Equal(1, dictionary.Count);
        }

        [Fact]
        public void TimedDictionary_Count_Overflow()
        {
            int key = 1;
            string value = "Test";

            TimedDictionary<int, string> dictionary = new TimedDictionary<int, string>(maximumSize: 0);
            dictionary.GetOrAddIfNew(key, () => value);

            Assert.Equal(0, dictionary.Count);
        }

        [Fact]
        public async Task TimedTaskDictionary_TaskValue_CorrectEvaluation()
        {
            int key = 1;
            string value = "Test";

            TimedTaskDictionary<int, string> dictionary = new TimedTaskDictionary<int, string>();
            var result = await dictionary.GetOrAddIfNewAsync(key, () => value);

            Assert.Equal(value, result);
        }

        [Fact]
        public async Task TimedTaskDictionary_TaskValue_CleanAfterResult_FromResult()
        {
            int key = 1;
            string value = "Test";

            TimedTaskDictionary<int, string> dictionary = new TimedTaskDictionary<int, string>();
            var result = await dictionary.GetOrAddIfNewAsync(key, () => Task.FromResult(value), AfterTaskCompletion.RemoveFromDictionary);

            await Task.Delay(100); // Give time to the self cleaning task to trigger
            Assert.Equal(0, dictionary.Count);
        }

        [Fact]
        public async Task TimedTaskDictionary_TaskValue_CleanAfterResult_FromDelay()
        {
            int key = 1;
            string value = "Test";

            TimedTaskDictionary<int, string> dictionary = new TimedTaskDictionary<int, string>();
            var result = await dictionary.GetOrAddIfNewAsync
            (
                key, 
                async () =>
                { 
                    await Task.Delay(100); 
                    return value; 
                }, 
                AfterTaskCompletion.RemoveFromDictionary
            );

            await Task.Delay(100); // Give time to the self cleaning task to trigger
            Assert.Equal(0, dictionary.Count);
        }

        [Fact]
        public async Task TimedTaskDictionary_TaskValue_DontCleanAfterResult()
        {
            int key = 1;
            string value = "Test";

            TimedTaskDictionary<int, string> dictionary = new TimedTaskDictionary<int, string>();
            var result = await dictionary.GetOrAddIfNewAsync(key, () => Task.FromResult(value), AfterTaskCompletion.DoNothing);

            Assert.Equal(1, dictionary.Count);
        }
    }
}