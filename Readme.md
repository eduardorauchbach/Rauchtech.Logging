# **CustomLogs**
Log library to facilitate the logging of complex workflows in Azure

## **Startup**
 1 - To work, it must be registered using the "RegisterCustomLog()" command.

```
services.RegisterCustomLog();
```

2 - Using dependency injection, it works similar to the ILogger: 
```
private readonly ICustomLog<T> _logger;

public SampleClass(ICustomLog<T> logger)
{
     _logger = logger;
}
```

## **Default Use Events**
<br/>

**AddKey**: Used to add key information in the logs. The Behavior can vary following the sequence or the hole scope.
- **string key**: Name of the key, it will be normalized using "Snake Case". Ex: **AsTest => as_test**
- **object value**: Value of the key, can be any object.
</br>

**LogCustom**: Used to add a new Log register.

- **LogLevel logLevel** - Severity of the log (Microsoft.Extensions.Logging)(required)
- **LogType? logType = LogType.Log** - used to define between "Log" and "Dashboard" information
- **EventId? eventId = null** - EventId in case it's being used (Microsoft.Extensions.Logging.EventId)
- **Exception exception = null** - Exception
- **string message = null** - Simple message
- **string? sourceContext = null** - Do not send
- **[CallerMemberName] string memberName = null** - Do not send
- **[CallerLineNumber] int sourceLineNumber = 0**  - Do not send
- **params ValueTuple<string, object>[] args** - Tupple array (Name, Objet), the objects will be **serialized using Json**
```
new (string, object)[]
{
    ("IsNewFlow", message.IsNewFlow),
    ("HasNewFlow", message.HasNewFlow),
    ("BusinessType", message.BusinessType)
});
```

## **Configuration**:

- **APPINSIGHTS_INSTRUMENTATIONKEY** <span style="color:orange">(native)</span>: Application Insights instrumentation key
- **APPLICATIONINSIGHTS_CONNECTION_STRING** <span style="color:orange">(native)</span>: Application Insights connection string
- **ApplicationName**: string Sets the ApplicationName property at the Log custom column
- **EnableScopeKeys**: bool Sets if the Keys will be used from the usage onwards or if they will be set for the entire scope.

    - **Warning** : If this property is set to true, the logs will only be fired after calling "Finish()", and it's advised to put this at the finish clause of trye/cath in the Function call.

```
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
### **Note**:

Use the AddKey method before any other Log in case the "EnableScopeKeys" is not true.
<br/>
<br/>

## Application Insights Customization

At the resource's Insights, go to **Monitoring/Logs**, and in the query put this Kusto script bellow:

```
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