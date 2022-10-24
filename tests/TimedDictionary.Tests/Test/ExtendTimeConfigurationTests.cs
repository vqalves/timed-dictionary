using TimedDictionary.Tests.Mock;
using Xunit;

namespace TimedDictionary.Tests.Test
{
    public class ExtendTimeConfigurationTests
    {
        private int? CalculateExtendedTime(int startExtendOffset, int currentTimeOffset, int? extendDuration, int? extendLimit)
        {
            var now = DateTime.Now;
            var extensionStart = now.AddMilliseconds(startExtendOffset);
            var dateProvider = new DateTimeProvider_Fixed(now.AddMilliseconds(currentTimeOffset));

            var config = new ExtendTimeConfiguration(duration: extendDuration, limit: extendLimit, dateTimeProvider: dateProvider);
            
            return config.CalculateExtendedTime(extensionStart);
        }

        [Fact]
        public void ExtendTimeConfiguration_None()
        {
            var config = ExtendTimeConfiguration.None();
            Assert.Null(config.Limit);
            Assert.Null(config.Duration);
        }

        [Fact]
        public void ExtendTimeConfiguration_DontExtendWhenNull()
        {
            var result = CalculateExtendedTime(0, 0, null, null);
            Assert.Null(result);
        }

        [Fact]
        public void ExtendTimeConfiguration_ExtendFullDuration_ExactTime()
        {
            var duration = 5;

            var result = CalculateExtendedTime(0, 0, duration, null);
            Assert.Equal(duration, result);
        }

        [Fact]
        public void ExtendTimeConfiguration_ExtendFullDuration_Early()
        {
            var duration = 5;
            var currentTimeOffset = -2;
            
            var result = CalculateExtendedTime(0, currentTimeOffset, duration, null);
            Assert.Equal(duration, result);
        }

        [Fact]
        public void ExtendTimeConfiguration_ExtendPartiallyByLimit()
        {
            var duration = 5;
            var limit = 3;

            var result = CalculateExtendedTime(0, 0, duration, limit);
            Assert.Equal(limit, result);
        }
        
        [Fact]
        public void ExtendTimeConfiguration_DontRefreshOverLimit()
        {
            var duration = 5;
            var limit = 0;

            var result = CalculateExtendedTime(0, 0, duration, limit);
            Assert.Null(result);
        }
    }
}