using TimedDictionary.Tests.Mock;
using Xunit;

namespace TimedDictionary.Tests.Test.EntryLifetimeTests
{
    public class EntryLifetimeTests_ExtendCurrentLimitTime
    {
        [Fact]
        public void EntryLifetime_ExtendCurrentLimitTime_ExtensionBelowDuration()
        {
            var duration = 1000;
            var durationExtension = 500;

            var provider = new TimestampProvider_Fixed();
            var extendConfig = new ExtendTimeConfiguration(duration: durationExtension);

            var config = new EntryLifetime(expectedDuration: duration, provider, extendConfig);
            var nextSchedulerTime = config.ExtendCurrentLimitTime()!.Value;

            var nextExecutionInMs = nextSchedulerTime - provider.CurrentTimestamp;
            Assert.Equal(duration, nextExecutionInMs.TotalMilliseconds);
        }

        [Fact]
        public void EntryLifetime_ExtendCurrentLimitTime_ExtensionOverridesDurationBecauseIsBigger()
        {
            var duration = 500;
            var durationExtension = 1500;

            var provider = new TimestampProvider_Fixed();
            var extendConfig = new ExtendTimeConfiguration(duration: durationExtension);

            var config = new EntryLifetime(expectedDuration: duration, provider, extendConfig);
            var nextSchedulerTime = config.ExtendCurrentLimitTime()!.Value;

            var nextExecutionInMs = nextSchedulerTime - provider.CurrentTimestamp;
            Assert.Equal(durationExtension, nextExecutionInMs.TotalMilliseconds);
        }

        [Fact]
        public void EntryLifetime_ExtendCurrentLimitTime_PartialExtensionBecauseLimit()
        {
            var duration = 0;
            var durationExtension = 1000;
            var extensionLimit = 500;

            var provider = new TimestampProvider_Fixed();
            var extendConfig = new ExtendTimeConfiguration(duration: durationExtension, limit: extensionLimit);

            var config = new EntryLifetime(expectedDuration: duration, provider, extendConfig);
            var nextSchedulerTime = config.ExtendCurrentLimitTime()!.Value;

            var nextExecutionInMs = nextSchedulerTime - provider.CurrentTimestamp;
            Assert.Equal(extensionLimit, nextExecutionInMs.TotalMilliseconds);
        }

        [Fact]
        public void EntryLifetime_ExtendCurrentLimitTime_TotalExtension()
        {
            var duration = 0;
            var durationExtension = 1000;

            var provider = new TimestampProvider_Fixed();
            var extendConfig = new ExtendTimeConfiguration(duration: durationExtension);

            var config = new EntryLifetime(expectedDuration: duration, provider, extendConfig);
            var nextSchedulerTime = config.ExtendCurrentLimitTime()!.Value;

            var totalExtension = nextSchedulerTime - provider.CurrentTimestamp;
            Assert.Equal(durationExtension, totalExtension.TotalMilliseconds);
        }
    }
}