using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TimedDictionary.DateTimeProvider;

namespace TimedDictionary.ActionScheduler
{
    internal class ActionSchedulerByTask : IActionScheduler
    {
        private readonly object Lock;
        private readonly IDateTimeProvider DateTimeProvider;
        private readonly Action Action;

        private Task ActionTask;
        private long NextExecutionScheduled;
        
        ///<summary>Schedule a task to run after X milliseconds</summary>
        public ActionSchedulerByTask(IDateTimeProvider dateTimeProvider, Action action, int millisecondsToExecute)
        {
            this.Lock = new object();
            this.DateTimeProvider = dateTimeProvider;
            this.Action = action;
            this.NextExecutionScheduled = dateTimeProvider.CurrentMilliseconds + millisecondsToExecute;
        }

        public void StartSchedule()
        {
            TryExecuteUnsafe();
        }

        private void TryExecute()
        {
            lock(Lock)
            {
                TryExecuteUnsafe();
            }
        }

        private void TryExecuteUnsafe()
        {
            var now = DateTimeProvider.CurrentMilliseconds;

            if (now >= NextExecutionScheduled)
            {
                Action.Invoke();
            }
            else
            {
                var delay = NextExecutionScheduled - now;
                var delayInt = delay > int.MaxValue ? int.MaxValue : (int)delay;

                ActionTask = Task.Run(async () =>
                {
                    await Task.Delay(delayInt);
                    TryExecute();
                });
            }
        }

        /// <summary>Forward-only rescheduler. If the new execution time is later than the current, the execution is rescheduled, otherwise it's ignored</summary>
        /// <param name="newNextExecution">New next execution, based on the IDateTimeProvider milliseconds</param>
        public void RescheduleTo(long newNextExecution)
        {
            if (newNextExecution > NextExecutionScheduled)
                lock (Lock)
                    if (newNextExecution > NextExecutionScheduled)
                        NextExecutionScheduled = newNextExecution;
        }
    }
}