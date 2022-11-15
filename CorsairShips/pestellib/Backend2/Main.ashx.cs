using System.Threading.Tasks;
using System.Web;
using Backend.Code.Statistics;
using BackendCommon.Code;

namespace Backend2
{
    /// <summary>
    /// Summary description for Main
    /// </summary>
    public class Main : HttpTaskAsyncHandler
    {
        private readonly MainHandlerBase _mainHandler = new MainHandlerBase();

        public override async Task ProcessRequestAsync(HttpContext context)
        {
            MainHandlerBaseStats.Instance.NewRequest();
            var sz = context.Request.ContentLength > 0 ? context.Request.ContentLength : context.Request.TotalBytes;
            byte[] signedData = context.Request.BinaryRead(sz);

            var hostAddr = context.Request.UserHostAddress;
            byte[] containerData = await _mainHandler.Process(signedData, new RequestContext { RemoteAddr = hostAddr });

            var response = context.Response;
            await response.OutputStream.WriteAsync(containerData, 0, containerData.Length);
        }
    }
}