using System.Threading.Tasks;
using BackendCommon.Code;
using Microsoft.AspNetCore.Mvc;

namespace CoreBackend.Controllers
{
    [Route("notifyMatchEnd")]
    public class MatchEndController : BackendControllerBase
    {
        private MatchEndHandler _handler = new MatchEndHandler();
        protected override Task<byte[]> ProcessRequest(byte[] buff, RequestContext ctx)
        {
            return _handler.Process(buff, ctx);
        }
    }
}
