# TimedDictionary
The `TimedDicionary` and `TimedTaskDictionary` classes are time-based self-cleaning dictionary structures. The entries are automatically removed from the structure after the specified time, allowing easier memory management on long-lived instances.

For environments with high concurrency and long-running evaluations, it is recommended to use TimedTaskDictionary to wrap evaluations round Tasks and avoid locking the object when the evaluation function is executed.

TimedDictionary does not dispose values when they are removed - use the `onRemoved` callback to handle disposal when necessary.

## Parameters
Parameter | Description
--- | ---
expectedDuration | How many milliseconds each value should be kept in the dictionary. If null, the structure will keep all records until they are manually removed. Default: null
maximumSize | Maximum amount of keys allowed at a time. When the limit is reached, no new keys will be added and new keys will always execute the evaluation function. If null, there will be no limits to the dictionary size. Default: null
extendTimeConfiguration | Allows the increase of each object lifetime inside the dictionary by X milliseconds, up to Y milliseconds, everytime the value is retrieved. If null, the object lifetime will obey the `expectedDuration` configuration. Default: null
onRemoved | Callback called whenever a value is removed from the dictionary, either by timeout or manually. Called only once per value. Default: null

## Usage
### General example
```csharp
var dictionary = new TimedDictionary<Key, Value>();
var retrievedValue = dictionary.GetOrAddIfNew(key, () => GenerateValue());
```

### Configure to remove objects 5 seconds after they were added
```csharp
var dictionary = new TimedDictionary<Key, Value>(expectedDuration: 5000);
```

### Manually remove an entry
```csharp
var retrievedValue = dictionary.Remove(key);
```

### Create onRemoved listener per-value
This listener overrides the onRemoved provided on the dictionary constructor
```csharp
dictionary.GetOrAddIfNew(key, () => GenerateValue(), onRemoved: (removedValue) => { /* Execute */ });
```

## Recommended usage
### In-memory cache, keeping the most accessed values through refresh
```csharp
// Every time the entry is retrieved, the time is extended to another 30 seconds
// If the entry exists for 10 minutes, it's automatically removed
var config = new ExtendTimeConfiguration
(
    duration: TimeSpan.FromSeconds(30).TotalMilliseconds,
    limit: TimeSpan.FromMinutes(10).TotalMilliseconds
);

// Make the objects exist for 30 seconds by default
// Limit the cache size to 1M entries, to avoid memory overflow
var dictionary = new TimedDictionary<Key, Task<Value>>
(
    expectedDuration: TimeSpan.FromSeconds(30).TotalMilliseconds,
    extendTimeConfiguration: config,
    maximumSize: 1_000_000
);

var retrievedValue = dictionary.GetOrAddIfNew(key, () => GenerateValue());
return retrievedValue;
```

### Web - Bundle different requests into a single task
When multiple users request the same thing, instead of starting one task for each request, the TimedDictionary allows all the requests to await the same task. This helps avoiding redundant processing, like multiple database calls.

It is recommended to use TimedTaskDictionary instead of TimedDictionary, because it requires less boilerplate code to wrap around tasks.

```csharp
public async Task<IResult> GetAsync
(
    [FromService] TimedTaskDictionary<Key, Value> singletonDictionary,
    [FromRoute] Key id
)
{
    var task = singletonDictionary.GetOrAddIfNewAsync(key, () => RetrieveValueAsync(id));
    var result = await task;
    return Results.Json(result);
}
```

```mermaid
sequenceDiagram
    Requests->>TimedDictionary: Retrieve ID 1
    TimedDictionary->>Evaluation function: Start new task for ID 1
    Evaluation function->>TimedDictionary: Created task
    TimedDictionary->>TimedDictionary: Cache task for ID 1
    TimedDictionary->>Requests: Cached task
    Requests->>TimedDictionary: Retrieve ID 1
    TimedDictionary->>Requests: Cached task
    Requests->>TimedDictionary: Retrieve ID 1
    TimedDictionary->>Requests: Cached task 
    TimedDictionary->>TimedDictionary: Uncaching task after X milliseconds
```

`TimedTaskDictionary.GetOrAddIfNewAsync` also has an additional parameter for AfterTaskCompletion, which can be configured to remove the entry from the dictionary right after the task is completed.

```csharp
timedTaskDictionary.GetOrAddIfNewAsync(key, AsyncFunction, AfterTaskCompletion.RemoveFromDictionary);
```