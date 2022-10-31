using System;
using System.Threading.Tasks;
using TimedDictionary.TimestampProvider;

namespace TimedDictionary.ActionScheduler
{
    internal class ActionSchedulerByTask : IActionScheduler
    {
        private readonly object Lock;
        private readonly ITimestampProvider TimestampProvider;
        private readonly Action Action;

        private Task ActionTask;
        private Timestamp NextExecutionScheduled;
        private bool Started;
        
        ///<summary>Schedule a task to run after X milliseconds</summary>
        public ActionSchedulerByTask(ITimestampProvider timestampProvider, Action action, int millisecondsToExecute)
        {
            this.Lock = new object();
            this.TimestampProvider = timestampProvider;
            this.Action = action;
            this.Started = false;
            this.NextExecutionScheduled = timestampProvider.CurrentTimestamp.AddMilliseconds(millisecondsToExecute);
        }

        public void StartSchedule()
        {
            if (!Started)
            {
                Started = true;
                TryExecuteUnsafe();
            }
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
            var now = TimestampProvider.CurrentTimestamp;

            if (now >= NextExecutionScheduled)
            {
                Action.Invoke();
            }
            else
            {
                var difference = NextExecutionScheduled - now;
                var milliseconds = (int)difference.TotalMilliseconds;

                ActionTask = Task.Run(async () =>
                {
                    await Task.Delay(milliseconds);
                    TryExecute();
                });
            }
        }

        /// <summary>Forward-only rescheduler. If the new execution time is later than the current, the execution is rescheduled, otherwise it's ignored</summary>
        /// <param name="newNextExecution">New next execution, based on the IDateTimeProvider milliseconds</param>
        public void RescheduleTo(Timestamp newNextExecution)
        {
            if (newNextExecution > NextExecutionScheduled)
                lock (Lock)
                    if (newNextExecution > NextExecutionScheduled)
                        NextExecutionScheduled = newNextExecution;
        }
    }
}