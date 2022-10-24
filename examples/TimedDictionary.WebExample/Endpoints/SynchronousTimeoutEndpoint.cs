using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TimedDictionary.WebExample.Endpoints
{
    public class SynchronousTimeoutEndpoint
    {
        private TimedDictionary<string, string> Dictionary = new TimedDictionary<string, string>(expectedDuration: 2000);

        public IResult Handle([FromRoute]string key)
        {
            bool fromDictionary = true;

            var result = Dictionary.GetOrAddIfNew(key, () => 
            {
                fromDictionary = false;
                return ExecuteInnerFunction(key);
            });

            Console.WriteLine($"From dictionary: {fromDictionary}");
            return Results.Text(result);
        }

        private string ExecuteInnerFunction(string key)
        {
            return string.Join(string.Empty, key.Reverse());
        }
    }
}