using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TimedDictionary.WebExample.Endpoints
{
    public class TaskTimeoutExample
    {
        private TimedTaskDictionary<string, string> Dictionary = new TimedTaskDictionary<string, string>(expectedDuration: 5000);

        public async Task<IResult> HandleAsync([FromRoute]string key)
        {
            bool fromDictionary = true;

            var task = Dictionary.GetOrAddIfNewAsync(key, () => 
            {
                fromDictionary = false;
                return ExecuteInnerFunctionAsync(key);
            });

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