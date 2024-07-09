using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using RauchTech.Logging;
using RauchTech.Logging.Services;

namespace RauchTech.Logging.Api
{
    public class CustomLogFilter : IAsyncActionFilter
    {
        private readonly CustomLog<CustomLogFilter> _log;

        private string? _sourceContext;
        private string? _actionName;

        public CustomLogFilter(ICustomLog<CustomLogFilter> log)
        {
            _log = (CustomLog<CustomLogFilter>)log;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            _sourceContext = context.Controller.GetType().FullName!;
            _actionName = ((ControllerActionDescriptor)context.ActionDescriptor).ActionName;

            // Before the action executes
            LogEntry(context);

            var executedContext = await next();

            // After the action executes
            if (executedContext.Exception is null)
            {
                LogExit(context);
            }
            else
            {
                LogException(executedContext);
            }
        }

        private void LogEntry(ActionExecutingContext context)
        {
            _log.RegisterContextParameters(context, _sourceContext, _actionName);
            _log.Log(LogLevel.Information, sourceContext: _sourceContext, memberName: _actionName, message: CustomLogDefaultMessages.Begin);
        }

        private void LogExit(ActionExecutingContext context)
        {
            _log.Log(LogLevel.Information, sourceContext: _sourceContext, memberName: _actionName, message: CustomLogDefaultMessages.Finish);
        }

        private void LogException(ActionExecutedContext context)
        {
            _log.Log(LogLevel.Error, sourceContext: _sourceContext, memberName: _actionName, exception: context.Exception);
        }
    }

}
