using Quartz;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BackendCommon.Code.Jobs
{
    public abstract class JobWithServiceProvider : IJob
    {
        public JobWithServiceProvider()
            :this(false)
        { }

        public JobWithServiceProvider(bool throwOnNull)
        {
            var t = GetType();
            foreach (var m in t.GetMethods())
            {
                if (m.Name != "Execute")
                    continue;
                if (m.DeclaringType != t)
                    continue;
                if (m.ReturnType != typeof(Task))
                    continue;
                _method = m;
                _params = m.GetParameters().ToArray();
            }
            if (_params == null)
                throw new Exception($"{t.Name} must implement Task Execute().");
            _throwOnNull = throwOnNull;
        }

        public Task Execute(IJobExecutionContext context)
        {
            if (_serviceProvider == null)
            {
                _serviceProvider = context.MergedJobDataMap[nameof(IServiceProvider)] as IServiceProvider;
                if (_serviceProvider == null)
                {
                    Log.Debug("ServiceProvider not found");
                    return Task.CompletedTask;
                }
            }

            _method.Invoke(this, GetArguments());

            return Task.CompletedTask;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private object[] GetArguments()
        {
            if (_injected != null)
                return _injected;
            _injected = new object[_params.Length];
            for (var i = 0; i < _params.Length; ++i)
            {
                _injected[i] = _serviceProvider.GetService(_params[i].ParameterType);
                if (_injected[i] == null)
                    if (_throwOnNull)
                        throw new Exception($"Can't resolve arg parameter of type {_params[i].ParameterType.Name}.");
                    else
                        Log.Warn($"Can't resolve arg parameter of type {_params[i].ParameterType.Name}.");
            }
            return _injected;
        }

        private ParameterInfo[] _params;
        private MethodInfo _method;
        private IServiceProvider _serviceProvider;
        private object[] _injected;
        private static readonly ILog Log = LogManager.GetLogger(typeof(JobWithServiceProvider));
        private readonly bool _throwOnNull;
    }
}
