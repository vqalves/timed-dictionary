using TimedDictionary.TimestampProvider;

namespace TimedDictionary.ActionScheduler
{
    internal interface IActionScheduler
    {
        void RescheduleTo(Timestamp newNextExecution);
        void StartSchedule();
    }
}