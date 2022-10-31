using TimedDictionary.Tests.Mock;
using Xunit;

namespace TimedDictionary.Tests.Test.EntryLifetimeTests
{
    public class EntryLifetimeTests_MillisecondsUntilLimit
    {
        [Fact]
        public void EntryLifetime_MillisecondsUntilLimit_AsExpectedDuration()
        {
            var duration = 1000;

            var provider = new TimestampProvider_Fixed();
            var extendConfig = ExtendTimeConfiguration.None;

            var config = new EntryLifetime(expectedDuration: duration, provider, extendConfig);
            var msUntilLimit = config.MillisecondsUntilLimit();

            Assert.Equal(duration, msUntilLimit);
        }

        [Fact]
        public void EntryLifetime_MillisecondsUntilLimit_Unlimited()
        {
            var provider = new TimestampProvider_Fixed();
            var extendConfig = ExtendTimeConfiguration.None;

            var config = new EntryLifetime(expectedDuration: null, provider, extendConfig);
            var msUntilLimit = config.MillisecondsUntilLimit();

            Assert.Null(msUntilLimit);
        }
    }
}