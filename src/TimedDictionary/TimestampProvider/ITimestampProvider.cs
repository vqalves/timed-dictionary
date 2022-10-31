using System;

namespace TimedDictionary.TimestampProvider
{
    internal interface ITimestampProvider
    {
        DateTime StartUtc { get; }
        Timestamp CurrentTimestamp { get; }
    }
}