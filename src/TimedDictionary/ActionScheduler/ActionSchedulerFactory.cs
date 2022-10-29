using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TimedDictionary.ActionScheduler
{
    internal class ActionSchedulerFactory
    {
        private ActionSchedulerFactory() { }
        
        public static IActionScheduler Create(Action action, int? msToExecute)
        {
            if(msToExecute == null)
                return ActionSchedulerNever.Instance;

            return new ActionSchedulerByTask(action, msToExecute.Value);
        }
    }
}