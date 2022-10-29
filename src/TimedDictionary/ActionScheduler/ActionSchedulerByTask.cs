using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TimedDictionary.ActionScheduler
{
    internal class ActionSchedulerByTask : IActionScheduler
    {
        private readonly object Lock;
        private readonly Action Action;

        private Task ActionTask;
        private CancellationTokenSource LastCancellationTokenSource;
        
        ///<summary>Schedule a task to run after X milliseconds</summary>
        public ActionSchedulerByTask(Action action, int millisecondsToExecute)
        {
            this.Lock = new Object();
            this.Action = action;

            this.RescheduleTo(millisecondsToExecute);
        }

        private bool TryExecuteUnsafe(CancellationToken? token)
        {
            if(token?.IsCancellationRequested == true)
                return false;
            
            Action.Invoke();
            return true;
        }

        private void Execute(CancellationToken? token)
        {
            lock(Lock)
            {
                TryExecuteUnsafe(token);
            }
        }

        public void RescheduleTo(int duration)
        {
            lock(Lock)
            {
                // Non-significative durations can execute the task immediately
                if(duration <= 0)
                {
                    TryExecuteUnsafe(LastCancellationTokenSource?.Token);
                    
                    LastCancellationTokenSource?.Cancel();
                    return;
                }

                // Cancel the last token and create a new task
                LastCancellationTokenSource?.Cancel();
                LastCancellationTokenSource = new CancellationTokenSource();
                var token = LastCancellationTokenSource.Token;

                ActionTask = Task.Run(async () => 
                {
                    await Task.Delay(duration, token);
                    Execute(token);
                }, token);
            }
        }
    }
}