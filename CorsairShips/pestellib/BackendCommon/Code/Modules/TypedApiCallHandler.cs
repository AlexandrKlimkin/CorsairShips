using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using S;
using log4net;
using PestelLib.ServerShared;

namespace Backend.Code.Modules
{
    public abstract class TypedApiCallHandler
    {
        protected static readonly ILog _log = LogManager.GetLogger(typeof(TypedApiCallHandler));
        private ConcurrentDictionary<int, Func<byte[], ServerRequest, Task<byte[]>>> _mappings = new ConcurrentDictionary<int, Func<byte[], ServerRequest, Task<byte[]>>>();

        protected void RegisterHandler(int type, Func<byte[], ServerRequest, Task<byte[]>> handle)
        {
            if(_mappings.ContainsKey(type))
                throw new Exception($"Already has handle for '{type}'");
            _mappings[type] = handle;
        }

        public async Task<byte[]> Process(TypedApiCall apiCall, ServerRequest request)
        {
            if(!_mappings.TryGetValue(apiCall.Type, out var func))
                _log.Debug($"Handler for command {apiCall.Type} not found. {Environment.StackTrace}" );
            return await func(apiCall.Data, request).ConfigureAwait(false);
        }

    }
}