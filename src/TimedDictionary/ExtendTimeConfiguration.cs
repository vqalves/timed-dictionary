namespace TimedDictionary
{
    public class ExtendTimeConfiguration
    {
        public static readonly ExtendTimeConfiguration None = new ExtendTimeConfiguration(duration: null, limit: null);

        public readonly int? Duration;
        public readonly int? Limit;

        /// <summary>Used to configure how each entry of the dictionary should extend it's lifetime</summary>
        /// <param name="duration">Each hit of the entry will extend the lifetime up to this value. In milliseconds</param>
        /// <param name="limit">Total duration available to extend, in milliseconds</param>
        public ExtendTimeConfiguration(int? duration = null, int? limit = null)
        {
            Duration = duration;
            Limit = limit;
        }
    }
}