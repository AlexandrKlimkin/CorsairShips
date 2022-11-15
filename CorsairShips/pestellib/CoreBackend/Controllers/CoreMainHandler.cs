using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Code.Statistics;
using BackendCommon.Code;
using Microsoft.AspNetCore.Mvc;
using S;
using ServerShared;

namespace CoreBackend.Controllers
{
    [Route("api/{method?}")]
    [ApiController]
    public class CoreMainHandler : BackendControllerBase
    {
        private MainHandlerBase _mainHandler = new MainHandlerBase();

        [HttpGet]
        public ActionResult<string> Get(string method)
        {
            return "OK";
        }

        protected override Task<byte[]> ProcessRequest(byte[] buff, RequestContext ctx)
        {
            MainHandlerBaseStats.Instance.NewRequest();
            return _mainHandler.Process(buff, ctx);
        }
    }
}
