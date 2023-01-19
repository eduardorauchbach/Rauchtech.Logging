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
- **$${\color{cyan}string}$$ key** $${\color{orange}(required)}$$: Name of the key, it will be normalized using "Snake Case". Ex: **AsTest => as_test**
- **$${\color{cyan}object}$$ value** $${\color{orange}(required)}$$: Value of the key, can be any object.
</br>

**LogCustom**: Used to add a new Log register.

- **$${\color{cyan}LogLevel}$$ logLevel** $${\color{orange}(required)}$$ - Severity of the log (Microsoft.Extensions.Logging)
- **$${\color{cyan}LogType?}$$ logType = LogType.Log** - used to define between "Log" and "Dashboard" information
- **$${\color{cyan}EventId?}$$ eventId = null** - EventId in case it's being used (Microsoft.Extensions.Logging.EventId)
- **$${\color{cyan}Exception}$$ exception = null** - Exception
- **$${\color{cyan}string}$$ message = null** - Simple message
- **$${\color{gray}string? sourceContext = null}$$** - Do not send
- **$${\color{gray}[CallerMemberName] string memberName = null}$$** - Do not send
- **$${\color{gray}[CallerLineNumber] int sourceLineNumber = 0}$$**  - Do not send
- **$${\color{cyan}params ValueTuple<string, object>[]}$$ args** - Tupple array (Name, Objet), the objects will be **serialized using Json**
``` csharp
    new (string, object)[]
    {
        ("IsNewFlow", message.IsNewFlow),
        ("HasNewFlow", message.HasNewFlow),
        ("BusinessType", message.BusinessType)
    });
```

## **Configuration**:

- **ApplicationName** $${\color{orange}(required)}$$$${\color{cyan}(string)}$$: string Sets the ApplicationName property at the Log custom column
- **LogLevel** $${\color{orange}(required)}$$$${\color{cyan}(string)}$$: Severity of the log (Microsoft.Extensions.Logging), determine the lower level accepted and ignores all the others.
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
- **EnableScopeKeys** $${\color{cyan}(bool)}$$: Sets if the Keys will be used from the usage onwards or if they will be set for the entire scope.

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
