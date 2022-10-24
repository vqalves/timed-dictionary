using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TimedDictionary.WebExample.Endpoints
{
    /// <summary>
    /// This example does not use timeout.
    /// Instead, all tasks are cached until the task is completed, and the entry is cleaned automatically
    /// </summary>
    public class TaskBundlingExample
    {
        private TimedTaskDictionary<string, string> Dictionary = new TimedTaskDictionary<string, string>();

        public async Task<IResult> HandleAsync([FromRoute]string key)
        {
            bool fromDictionary = true;

            var task = Dictionary.GetOrAddIfNewAsync(key, () => 
            {
                fromDictionary = false;
                return ExecuteInnerFunctionAsync(key);
            }, AfterTaskCompletion.RemoveFromDictionary);

            var result = await task;

            Console.WriteLine($"From dictionary: {fromDictionary}");
            return Results.Text(result);
        }

        private async Task<string> ExecuteInnerFunctionAsync(string key)
        {
            await Task.Delay(5000);
            return string.Join(string.Empty, key.Reverse());
        }
    }
}