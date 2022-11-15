using System.Threading.Tasks;
using BackendCommon;
using BackendCommon.Code;
using BackendCommon.Code.IapValidator;
using Microsoft.AspNetCore.Mvc;
using PestelLib.ServerShared;

namespace CoreBackend.Controllers
{
    [Route("validateReceipt")]
    [ApiController]
    public class IapValidationController : BackendControllerBase
    {
        private IRequestHandler _handler;
        private bool IsAvailable()
        {
            if (_handler == null)
            {
                var validator = BackendInitializer.ServiceProvider.GetService(typeof(IIapValidator)) as IIapValidator;
                if (validator == null)
                    return false;
                _handler = new IapValidatorRouteHandler(validator, false);
            }

            return true;
        }

        protected override async Task<byte[]> ProcessRequest(byte[] buff, RequestContext ctx)
        {
            if (!IsAvailable())
            {
                Response.StatusCode = 503;
                return null;
            }
            return await _handler.Process(buff, ctx);
        }
    }
}
