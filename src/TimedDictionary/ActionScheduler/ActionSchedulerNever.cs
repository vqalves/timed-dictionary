using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TimedDictionary.ActionScheduler
{
    internal class ActionSchedulerNever : IActionScheduler
    {
        public static readonly ActionSchedulerNever Instance = new ActionSchedulerNever();

        private ActionSchedulerNever() { }

        public void RescheduleTo(long newNextExecution)
        {
            // Do nothing
        }

        public void StartSchedule()
        {
            // Do nothing
        }
    }
}