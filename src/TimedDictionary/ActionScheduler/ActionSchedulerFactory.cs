using System;
using TimedDictionary.TimestampProvider;

namespace TimedDictionary.ActionScheduler
{
    internal class ActionSchedulerFactory
    {
        private ActionSchedulerFactory() { }
        
        public static IActionScheduler CreateUnstarted(ITimestampProvider timestampProvider, Action action, int? msToExecute)
        {
            if(msToExecute == null)
                return ActionSchedulerNever.Instance;

            return new ActionSchedulerByTask(timestampProvider, action, msToExecute.Value);
        }
    }
}