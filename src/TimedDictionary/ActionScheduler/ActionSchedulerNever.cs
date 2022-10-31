using TimedDictionary.TimestampProvider;

namespace TimedDictionary.ActionScheduler
{
    internal class ActionSchedulerNever : IActionScheduler
    {
        public static readonly ActionSchedulerNever Instance = new ActionSchedulerNever();

        private ActionSchedulerNever() { }

        public void RescheduleTo(Timestamp newNextExecution)
        {
            // Do nothing
        }

        public void StartSchedule()
        {
            // Do nothing
        }
    }
}