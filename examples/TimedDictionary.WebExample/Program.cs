using Microsoft.AspNetCore.Mvc;
using TimedDictionary;
using TimedDictionary.WebExample.Endpoints;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var syncTimeoutHandler = new SynchronousTimeoutEndpoint();
var taskTimeoutHandler = new TaskTimeoutExample();
var taskBundlingHandler = new TaskBundlingExample();

app.MapGet("/timeout-sync/{key}", syncTimeoutHandler.Handle);
app.MapGet("/timeout-task/{key}", taskTimeoutHandler.HandleAsync);
app.MapGet("/bundle-task/{key}", taskBundlingHandler.HandleAsync);

app.Run();
