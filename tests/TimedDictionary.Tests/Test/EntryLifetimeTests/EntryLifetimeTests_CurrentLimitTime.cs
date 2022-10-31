using TimedDictionary.Tests.Mock;
using Xunit;

namespace TimedDictionary.Tests.Test.EntryLifetimeTests
{
    public class EntryLifetimeTests_CurrentLimitTime
    {
        [Fact]
        public void EntryLifetime_CurrentLimitTime_UnlimitedTime()
        {
            var provider = new TimestampProvider_Fixed();
            var extendConfig = ExtendTimeConfiguration.None;

            var config = new EntryLifetime(expectedDuration: null, provider, extendConfig);

            Assert.Null(config.CurrentLimitTime);
        }

        [Fact]
        public void EntryLifetime_CurrentLimitTime_LimitedTime()
        {
            var provider = new TimestampProvider_Fixed();
            var extendConfig = ExtendTimeConfiguration.None;

            var config = new EntryLifetime(expectedDuration: 1000, provider, extendConfig);

            Assert.NotNull(config.CurrentLimitTime);
        }
    }
}