using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendCommon.Code;
using Microsoft.AspNetCore.Mvc;
using ServerShared;
using System.Buffers;
using log4net;

namespace CoreBackend.Controllers
{
    public abstract class BackendControllerBase : ControllerBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BackendControllerBase));
        [HttpPost]
        public async Task Post()
        {
            var sz = (int)(Request.ContentLength > 0 ? Request.ContentLength : 32 * 1024);
            var read = 0;
            var buff = new byte[sz];
            var hostAddr = Request.Host.HasValue ? Request.Host.Host : "";
            while (read < sz)
            {
                var prevRead = read;
                read += await Request.Body.ReadAsync(buff, read, sz - read);
                if (prevRead - read == 0 && Request.ContentLength.HasValue)
                {
                    Log.Error($"Unexpected end of stream. Expected size: {sz}, read: {read}, host: {hostAddr}.");
                    return;
                }
            }
            var r = await ProcessRequest(buff, new RequestContext() {RemoteAddr = hostAddr});
            if(r != null && r.Length > 0)
                await Response.Body.WriteAsync(r, 0, r.Length);
        }

        protected abstract Task<byte[]> ProcessRequest(byte[] buff, RequestContext ctx);
    }
}
