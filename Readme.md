# **CustomLogs**
Log implementation class to facilitate the logging of complex workflows



## **Requirements**

**MethodBoundaryAspect.Fody** need to be placed in any main project that uses the **LogAspect**, otherwise the code injection will not work and the log will not work.



## **Startup**

- Add the default logging to your project or use any of the RauchTechLogging chieldren Libraries.
    - Sample 1: **Rauchtech.Logging.Azure**
    ``` csharp
    using Microsoft.Extensions.DependencyInjection;

    //config is the IConfiguration
    builder.Services.ConfigureAzureLogging(config);
    ```    

- Using dependency injection, it works similar to the ILogger: 
    ``` csharp
    private readonly ICustomLog<T> _logger;

    public T(ICustomLog<T> logger)
    {
        _logger = logger;
    }
    ```



## **Default Use Events**

**AddKey**: Used to add key information in the logs. The Behavior can vary following the sequence or the hole scope.
- **string key** (required) : Name of the key, it will be normalized using "Snake Case". Ex: **AsTest => as_test**
- **object value** (required) : Value of the key, can be any object.
>**NOTE**
>
>Use the AddKey method before any other Log in case the "EnableScopeKeys" is not true.

**Log**: Used to add a new Log register.

- **LogLevel logLevel** (required) - Severity of the log (Microsoft.Extensions.Logging)
- **LogType? logType = LogType.Log** - used to define between "Log" and "Dashboard" information
- **EventId? eventId = null** - EventId in case it's being used (Microsoft.Extensions.Logging.EventId)
- **Exception exception = null** - Exception
- **string message = null** - Simple message
- **string?sourceContext = null** - Do not send
- **[CallerMemberName] stringmemberName = null** - Do not send
- **[CallerLineNumber] intsourceLineNumber = 0**  - Do not send
- **paramsValueTuple<string, object>[] args** - Tupple array (Name, Objet), the objects will be **serialized using Json**
    ``` csharp
    new (string, object)[]
    {
        ("IsNewFlow", message.IsNewFlow),
        ("HasNewFlow", message.HasNewFlow),
        ("BusinessType", message.BusinessType)
    });
    ```
>**NOTE**
>
>This method has extesions as the Microsoft.Logging.Extensions
>- LogInformation
>- LogDebug
>- LogTrace
>- LogWarning
>- LogError



## LogAspect

With Aspects we can inject events into methods, in the case of this Library, Begin, Finish and Exception events.
To use this, we need to have a ICustomLog initialized in the class and use the decoration [LogAspect] as in the sample.
``` csharp
[LogAspect]
public void DemoMethod()
{
}
```



## Filters


As an option, you can add filters to your API, so the logs are intercepted directly from the Controllers.
In this moment, it will log all the entries, but only log the parameters if the LogLevel is Debug or higher
``` csharp
using Microsoft.Extensions.DependencyInjection;

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<CustomLogFilter>();
});
```   

>[!WARNING]]
>
>Configurate the LogFiltersKeyParameters settings, so the Ids and other key informations can be automatically used as Keys in the logs.
>Configurate the LogFiltersBannedParameters settings, so the Passwords and other sensitive informations are not logged.

## **Configuration**:

- **ApplicationName** (required) (string) : string Sets the ApplicationName property at the Log custom column
- **LogLevel** (required) (string) : Severity of the log (Microsoft.Extensions.Logging), determine the lower level accepted and ignores all the others.
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
- **EnableScopeKeys** (bool) : Sets if the Keys will be used from the usage onwards or if they will be set for the entire scope.

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
- **LogFiltersKeyParameters**: Will select as an id, any parameter from the Filters parameters logging, can be used several parameters
    - Ex: "ID;Id;Identification"
- **LogFiltersBannedParameters**: Will exclude any parameter from the Filters parameters logging, can be used several parameters
    - Ex: "Password;Senha;PalavraChave"


## **Custom Visualizations**

### Azure Application Insights Customization

At the resource's Insights, go to **Monitoring/Logs**, and in the query put this Kusto script bellow:

``` kql
let All = () {
    customEvents
    | where 
        customDimensions.Data != ""
    | union
    exceptions
        | where 
        customDimensions.Data != ""
    | project
        Timestamp = todatetime(customDimensions.Timestamp),
        CurrentStep = tolong(customDimensions.CurrentStep),
        ApplicationName = customDimensions.ApplicationName,
        LogLevel = customDimensions.LogLevel,
        LogType = customDimensions.LogType,
        Source = customDimensions.SourceContext,
        Keys = todynamic(tostring(customDimensions.Keys)),
        Data = todynamic(tostring(customDimensions.Data)),
        Exception = customDimensions.ExceptionDetail
    | order by
        Timestamp desc,
        CurrentStep desc
};
All();
```

Then save it as a function, the sugested name is **CustomLogs**
