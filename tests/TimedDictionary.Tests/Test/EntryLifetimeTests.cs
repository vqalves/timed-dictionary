using TimedDictionary.Tests.Mock;
using Xunit;

namespace TimedDictionary.Tests.Test
{
    public class EntryLifetimeTests
    {
        [Fact]
        public void EntryLifetime_CurrentLimitTime_UnlimitedTime()
        {
            var provider = new DateTimeProvider_Fixed(DateTime.Now);
            var extendConfig = ExtendTimeConfiguration.None();

            var config = new EntryLifetime(expectedDuration: null, provider, extendConfig);

            Assert.Null(config.CurrentLimitTime);            
        }

        [Fact]
        public void EntryLifetime_CurrentLimitTime_LimitedTime()
        {
            var provider = new DateTimeProvider_Fixed(DateTime.Now);
            var extendConfig = ExtendTimeConfiguration.None();

            var config = new EntryLifetime(expectedDuration: 1000, provider, extendConfig);

            Assert.NotNull(config.CurrentLimitTime);
        }

        [Fact]
        public void EntryLifetime_MillisecondsUntilLimit_AsExpectedDuration()
        {
            var duration = 1000;

            var provider = new DateTimeProvider_Fixed(DateTime.Now);
            var extendConfig = ExtendTimeConfiguration.None();
            
            var config = new EntryLifetime(expectedDuration: duration, provider, extendConfig);
            var msUntilLimit = config.MillisecondsUntilLimit();

            Assert.Equal(duration, msUntilLimit);
        }

        [Fact]
        public void EntryLifetime_MillisecondsUntilLimit_Unlimited()
        {
            var provider = new DateTimeProvider_Fixed(DateTime.Now);
            var extendConfig = ExtendTimeConfiguration.None();
            
            var config = new EntryLifetime(expectedDuration: null, provider, extendConfig);
            var msUntilLimit = config.MillisecondsUntilLimit();

            Assert.Null(msUntilLimit);
        }

        [Fact]
        public void EntryLifetime_ExtendCurrentLimitTime_ExtensionBelowDuration()
        {
            var duration = 1000;
            var durationExtension = 500;

            var provider = new DateTimeProvider_Fixed(DateTime.Now);
            var extendConfig = new ExtendTimeConfiguration(duration: durationExtension);
            
            var config = new EntryLifetime(expectedDuration: duration, provider, extendConfig);
            var totalExtension = config.ExtendCurrentLimitTime();

            Assert.Null(totalExtension);
        }

        [Fact]
        public void EntryLifetime_ExtendCurrentLimitTime_PartialExtensionBecauseCurrentDuration()
        {
            var duration = 500;
            var durationExtension = 1500;

            var provider = new DateTimeProvider_Fixed(DateTime.Now);
            var extendConfig = new ExtendTimeConfiguration(duration: durationExtension);
            
            var config = new EntryLifetime(expectedDuration: duration, provider, extendConfig);
            var totalExtension = config.ExtendCurrentLimitTime();

            Assert.Equal(1000, totalExtension);
        }

        [Fact]
        public void EntryLifetime_ExtendCurrentLimitTime_PartialExtensionBecauseLimit()
        {
            var duration = 0;
            var durationExtension = 1000;
            var extensionLimit = 500;

            var provider = new DateTimeProvider_Fixed(DateTime.Now);
            var extendConfig = new ExtendTimeConfiguration(duration: durationExtension, limit: extensionLimit);
            
            var config = new EntryLifetime(expectedDuration: duration, provider, extendConfig);
            var totalExtension = config.ExtendCurrentLimitTime();

            Assert.Equal(extensionLimit, totalExtension);
        }

        [Fact]
        public void EntryLifetime_ExtendCurrentLimitTime_TotalExtension()
        {
            var duration = 0;
            var durationExtension = 1000;

            var provider = new DateTimeProvider_Fixed(DateTime.Now);
            var extendConfig = new ExtendTimeConfiguration(duration: durationExtension);
            
            var config = new EntryLifetime(expectedDuration: duration, provider, extendConfig);
            var totalExtension = config.ExtendCurrentLimitTime();

            Assert.Equal(durationExtension, totalExtension);
        }
    }
}