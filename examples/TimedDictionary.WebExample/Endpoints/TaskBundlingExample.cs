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
        private TimedTaskDictionary<string, IResult> Dictionary;
        private Func<string, Task<IResult>> ProcessKey;
        private int FunctionsExecuted;

        public TaskBundlingExample(Func<string, Task<IResult>> processKey)
        {
            this.Dictionary = new TimedTaskDictionary<string, IResult>(expectedDuration: null);
            this.ProcessKey = processKey;
        }

        public async Task<IResult> HandleAsync([FromRoute]string key)
        {
            var task = Dictionary.GetOrAddIfNewAsync
            (
                key, () => 
                {
                    FunctionsExecuted++;
                    // if(FunctionsExecuted % 100 == 0)
                    //     Console.WriteLine($"TaskBundlingExample: {FunctionsExecuted}");

                    return ProcessKey(key);
                }, 
                AfterTaskCompletion.RemoveFromDictionary
            );

            return await task;
        }
    }
}