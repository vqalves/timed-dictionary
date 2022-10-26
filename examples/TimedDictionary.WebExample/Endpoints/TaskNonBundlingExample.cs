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
    public class TaskNonBundlingExample
    {
        private Func<string, Task<IResult>> ProcessKey;
        private int FunctionsExecuted;

        public TaskNonBundlingExample(Func<string, Task<IResult>> processKey)
        {
            this.ProcessKey = processKey;
        }

        public async Task<IResult> HandleAsync([FromRoute]string key)
        {
            FunctionsExecuted++;
            // if(FunctionsExecuted % 100 == 0)
            //     Console.WriteLine($"TaskNonBundlingExample: {FunctionsExecuted}");

            return await ProcessKey(key);
        }
    }
}