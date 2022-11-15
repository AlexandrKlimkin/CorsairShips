using System;
using System.Threading.Tasks;

namespace BackendCommon.Code.GlobalConflict
{
    public class GlobalConflictHttpHandler : IRequestHandler
    {
        public Task<byte[]> Process(byte[] data, RequestContext ctx)
        {
            throw new NotImplementedException();
        }
    }
}