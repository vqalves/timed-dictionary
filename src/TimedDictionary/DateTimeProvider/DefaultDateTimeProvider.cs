using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimedDictionary.DateTimeProvider
{
    internal class DefaultDateTimeProvider : IDateTimeProvider
    {
        public static DefaultDateTimeProvider Instance = new DefaultDateTimeProvider();

        private DefaultDateTimeProvider() { }

        public DateTime Now => DateTime.Now;
    }
}