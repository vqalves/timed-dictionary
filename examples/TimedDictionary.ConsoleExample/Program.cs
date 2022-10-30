// See https://aka.ms/new-console-template for more information

using System;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Text.Json.Serialization;
using TimedDictionary;

var timedDictionary = new TimedTaskDictionary<int, int>();

/*
var options = new ParallelOptions();
options.MaxDegreeOfParallelism = 2000;

int created = 0;

var task = Parallel.ForEachAsync(Enumerable.Range(0, 1_000_000), options, (source, token) =>
{
    var key = source % 1000;

    return new ValueTask
    (
        timedDictionary.GetOrAddIfNewAsync
        (
            key,
            notFound: async () =>
            {
                created++;

                if (created % 100 == 0)
                    Console.WriteLine(created);

                await Task.Delay(500);
                return 1;
            }
        )
    );
});

await task;
*/

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
        }, afterTaskCompletion: AfterTaskCompletion.RemoveFromDictionary
    );
}).ToList();

await Task.WhenAll(tasks);

Console.WriteLine("Done");