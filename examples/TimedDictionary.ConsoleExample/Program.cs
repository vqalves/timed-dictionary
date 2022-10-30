// See https://aka.ms/new-console-template for more information

using System;
using TimedDictionary;

var timedDictionary = new TimedTaskDictionary<int, int>();

var options = new ParallelOptions();
options.MaxDegreeOfParallelism = 2000;


List<Task<int>> tasks = Enumerable.Range(0, 1_000_000).Select(x =>
{
    var key = x % 1000;

    return timedDictionary.GetOrAddIfNewAsync
    (
        key,
        notFound: async () =>
        {
            await Task.Delay(500);
            return 1;
        },
        afterTaskCompletion: AfterTaskCompletion.RemoveFromDictionary
    );
}).ToList();

await Task.WhenAll(tasks);

Console.WriteLine("Done");