using System;

namespace TimedDictionary.TimestampProvider
{
    internal struct Timestamp
    {
        private ITimestampProvider TimestampProvider;
        private long Value;

        public Timestamp(ITimestampProvider timestampProvider, long value)
        {
            this.TimestampProvider = timestampProvider;
            this.Value = value;
        }

        public Timestamp AddMilliseconds(int ms)
        {
            return new Timestamp(TimestampProvider, Value + ms);
        }

        public DateTime ToDateTimeUtc()
        {
            return TimestampProvider.StartUtc.AddMilliseconds(Value);
        }

        public static bool operator >=(Timestamp d, Timestamp t) => d.Value >= t.Value;
        public static bool operator <=(Timestamp d, Timestamp t) => d.Value <= t.Value;

        public static bool operator >(Timestamp d, Timestamp t) => d.Value > t.Value;
        public static bool operator <(Timestamp d, Timestamp t) => d.Value < t.Value;

        public static bool operator !=(Timestamp d, Timestamp t) => d.Value != t.Value;
        public static bool operator ==(Timestamp d, Timestamp t) => d.Value == t.Value;

        public static TimeSpan operator -(Timestamp d, Timestamp t) => TimeSpan.FromMilliseconds(d.Value - t.Value);

        public override int GetHashCode() => (int)Value;
        public override bool Equals(object obj)
        {
            if(obj is Timestamp)
            {
                var converted = (Timestamp)obj;
                return converted.TimestampProvider == TimestampProvider && converted.Value == Value;
            }

            return false;
        }
    }
}