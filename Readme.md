# **CustomLogs**
Log implementation class to facilitate the logging of complex workflows

## **Startup**
 1 - To work, it must be registered using the "RegisterCustomLog()" command.

``` csharp
    services.RegisterCustomLog();
```

2 - Using dependency injection, it works similar to the ILogger: 
``` csharp
    private readonly ICustomLog<T> _logger;

    public T(ICustomLog<T> logger)
    {
         _logger = logger;
    }
```

## **Default Use Events**
<br/>

**AddKey**: Used to add key information in the logs. The Behavior can vary following the sequence or the hole scope.
- **<span style="color:cyan">string</span> key** <span style="color:orange">(required)</span>: Name of the key, it will be normalized using "Snake Case". Ex: **AsTest => as_test**
- **<span style="color:cyan">object</span> value** <span style="color:orange">(required)</span>: Value of the key, can be any object.
</br>

**LogCustom**: Used to add a new Log register.

- **<span style="color:cyan">LogLevel</span> logLevel** <span style="color:orange">(required)</span> - Severity of the log (Microsoft.Extensions.Logging)
- **<span style="color:cyan">LogType?</span> logType = LogType.Log** - used to define between "Log" and "Dashboard" information
- **<span style="color:cyan">EventId?</span> eventId = null** - EventId in case it's being used (Microsoft.Extensions.Logging.EventId)
- **<span style="color:cyan">Exception</span> exception = null** - Exception
- **<span style="color:cyan">string</span> message = null** - Simple message
- **<span style="color:gray">string? sourceContext = null</span>** - Do not send
- **<span style="color:gray">[CallerMemberName] string memberName = null</span>** - Do not send
- **<span style="color:gray">[CallerLineNumber] int sourceLineNumber = 0</span>**  - Do not send
- **<span style="color:cyan">params ValueTuple<string, object>[]</span> args** - Tupple array (Name, Objet), the objects will be **serialized using Json**
``` csharp
    new (string, object)[]
    {
        ("IsNewFlow", message.IsNewFlow),
        ("HasNewFlow", message.HasNewFlow),
        ("BusinessType", message.BusinessType)
    });
```

## **Configuration**:

- **ApplicationName** <span style="color:orange">(required)</span><span style="color:cyan">(string)</span>: string Sets the ApplicationName property at the Log custom column
- **LogLevel** <span style="color:orange">(required)</span><span style="color:cyan">(string)</span>: Severity of the log (Microsoft.Extensions.Logging), determine the lower level accepted and ignores all the others.
``` csharp
        //
        // Summary:
        //     Logs that contain the most detailed messages. These messages may contain sensitive
        //     application data. These messages are disabled by default and should never be
        //     enabled in a production environment.
        Trace,
        //
        // Summary:
        //     Logs that are used for interactive investigation during development. These logs
        //     should primarily contain information useful for debugging and have no long-term
        //     value.
        Debug,
        //
        // Summary:
        //     Logs that track the general flow of the application. These logs should have long-term
        //     value.
        Information,
        //
        // Summary:
        //     Logs that highlight an abnormal or unexpected event in the application flow,
        //     but do not otherwise cause the application execution to stop.
        Warning,
        //
        // Summary:
        //     Logs that highlight when the current flow of execution is stopped due to a failure.
        //     These should indicate a failure in the current activity, not an application-wide
        //     failure.
        Error,
        //
        // Summary:
        //     Logs that describe an unrecoverable application or system crash, or a catastrophic
        //     failure that requires immediate attention.
        Critical,
        //
        // Summary:
        //     Not used for writing log messages. Specifies that a logging category should not
        //     write any messages.
        None
```
- **EnableScopeKeys** <span style="color:cyan">(bool)</span>: Sets if the Keys will be used from the usage onwards or if they will be set for the entire scope.

    - **Warning** : If this property is set to true, the logs will only be fired after calling "Finish()", and it's advised to put this at the finish clause of trye/cath in the Function call.

``` csharp
    try
    {
    }
    catch
    {
    }
    finish
    {
        _logger.Finish();
    }
```
## **Note**:

Use the AddKey method before any other Log in case the "EnableScopeKeys" is not true.
