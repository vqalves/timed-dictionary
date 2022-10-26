using Microsoft.AspNetCore.Mvc;
using TimedDictionary;
using TimedDictionary.WebExample.Endpoints;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Manually test
var syncTimeoutHandler = new SyncTimeoutEndpoint();
var taskTimeoutHandler = new TaskTimeoutExample();

app.MapGet("/timeout-sync/{key}", syncTimeoutHandler.Handle);
app.MapGet("/timeout-task/{key}", taskTimeoutHandler.HandleAsync);

// Bundle test
var random = new Random();
Func<string, Task<IResult>> processKey = async (key) => 
{
    await Task.Delay(random.Next(500, 500));
    var result = Guid.NewGuid().ToString("n");
    return Results.Text(result);
};

var stressBundleHandler = new TaskBundlingExample(processKey);
var stressNonBundleHandler = new TaskNonBundlingExample(processKey);

app.MapGet("/stress/bundle/{key}", stressBundleHandler.HandleAsync);
app.MapGet("/stress/raw/{key}", stressNonBundleHandler.HandleAsync);

app.Run("http://localhost:5000");
