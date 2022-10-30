using TimedDictionary.Tests.Mock;
using Xunit;

namespace TimedDictionary.Tests.Test.TimedTaskDictionaryTests
{
    public class TimedTaskDictionaryTests_TaskValue
    {
        [Fact]
        public async Task TimedTaskDictionary_TaskValue_CorrectEvaluation()
        {
            int key = 1;
            string value = "Test";

            TimedTaskDictionary<int, string> dictionary = new TimedTaskDictionary<int, string>(expectedDuration: null);
            var result = await dictionary.GetOrAddIfNewAsync(key, () => value);

            Assert.Equal(value, result);
        }

        [Fact]
        public async Task TimedTaskDictionary_TaskValue_CleanAfterResult_FromResult()
        {
            int key = 1;
            string value = "Test";

            TimedTaskDictionary<int, string> dictionary = new TimedTaskDictionary<int, string>(expectedDuration: null);
            var result = await dictionary.GetOrAddIfNewAsync(key, () => Task.FromResult(value), AfterTaskCompletion.RemoveFromDictionary);

            await Task.Delay(100); // Give time to the self cleaning task to trigger
            Assert.Equal(0, dictionary.Count);
        }

        [Fact]
        public async Task TimedTaskDictionary_TaskValue_CleanAfterResult_FromDelay()
        {
            int key = 1;
            string value = "Test";

            TimedTaskDictionary<int, string> dictionary = new TimedTaskDictionary<int, string>(expectedDuration: null);
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

            TimedTaskDictionary<int, string> dictionary = new TimedTaskDictionary<int, string>(expectedDuration: null);
            var result = await dictionary.GetOrAddIfNewAsync(key, () => Task.FromResult(value), AfterTaskCompletion.DoNothing);

            Assert.Equal(1, dictionary.Count);
        }
    }
}