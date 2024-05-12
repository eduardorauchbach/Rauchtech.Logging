using RauchTech.Logging.Services;
using MethodBoundaryAspect.Fody.Attributes;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace RauchTech.Logging.Aspects
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Method)]
    public class LogAspect : OnMethodBoundaryAspect
    {
        private CustomLog _logger;

        public override void OnEntry(MethodExecutionArgs arg)
        {
            GetLogger(arg)?.Log(LogLevel.Debug, CustomLogType.Log, sourceContext: arg.Instance.GetType().FullName, memberName: arg.Method.Name, sourceLineNumber: 0, message: CustomLogDefaultMessages.Begin);
        }


        public override void OnExit(MethodExecutionArgs arg)
        {
            if (arg.ReturnValue is Task t)
            {
                t.ContinueWith(_ =>
                {
                    GetLogger(arg)?.Log(LogLevel.Debug, CustomLogType.Log, sourceContext: arg.Instance.GetType().FullName, memberName: arg.Method.Name, sourceLineNumber: 0, message: CustomLogDefaultMessages.Finish);
                }, TaskContinuationOptions.ExecuteSynchronously);
            }
            else
            {
                GetLogger(arg)?.Log(LogLevel.Debug, CustomLogType.Log, sourceContext: arg.Instance.GetType().FullName, memberName: arg.Method.Name, sourceLineNumber: 0, message: CustomLogDefaultMessages.Finish);
            }
        }

        public override void OnException(MethodExecutionArgs arg)
        {
            GetLogger(arg)?.Log(LogLevel.Error, CustomLogType.Log, sourceContext: arg.Instance.GetType().FullName, memberName: arg.Method.Name, sourceLineNumber: 0, exception: arg.Exception);
        }

        private CustomLog? GetLogger(MethodExecutionArgs arg)
        {
            var type = arg.Instance.GetType();
            if (_logger is null)
            {
                var members = type?.FindMembers(MemberTypes.Field, BindingFlags.Instance | BindingFlags.NonPublic, (x, y) => ((FieldInfo)x).FieldType.Name.StartsWith("ICustomLog"), null);
                if (members?.Length > 0)
                {
                    FieldInfo f = (FieldInfo)members[0];
                    _logger = f.GetValue(arg?.Instance) as CustomLog;
                }
                else
                {
                    return null;
                }
            }
            return _logger;
        }
    }
}