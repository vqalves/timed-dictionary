using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TimedDictionary.DateTimeProvider;

namespace TimedDictionary.ActionScheduler
{
    internal class ActionSchedulerFactory
    {
        private ActionSchedulerFactory() { }
        
        public static IActionScheduler CreateUnstarted(IDateTimeProvider dateTimeProvider, Action action, int? msToExecute)
        {
            if(msToExecute == null)
                return ActionSchedulerNever.Instance;

            return new ActionSchedulerByTask(dateTimeProvider, action, msToExecute.Value);
        }
    }
}