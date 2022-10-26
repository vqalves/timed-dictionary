using TimedDictionary.Tests.Mock;
using Xunit;

namespace TimedDictionary.Tests.Test.TimedDictionaryTests
{
    public class TimedDictionaryTests_Count
    {
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
    }
}