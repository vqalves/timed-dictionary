using TimedDictionary.Tests.Mock;
using Xunit;

namespace TimedDictionary.Tests.Test.TimedDictionaryTests
{
    public class TimedDictionaryTests_GetOrAddIfNew
    {
        [Fact]
        public void TimedDictionary_GetOrAddIfNew()
        {
            int key = 1;
            string value = "Test";

            TimedDictionary<int, string> dictionary = new TimedDictionary<int, string>(expectedDuration: null);
            var result = dictionary.GetOrAddIfNew(key, () => value);

            Assert.Equal(value, result);
        }

        [Fact]
        public void TimedDictionary_GetOrAddIfNew_OnOverflow()
        {
            int key = 1;
            string value = "Test";

            TimedDictionary<int, string> dictionary = new TimedDictionary<int, string>(expectedDuration: null, maximumSize: 0);
            var result = dictionary.GetOrAddIfNew(key, () => value);

            Assert.Equal(value, result);
        }
    }
}