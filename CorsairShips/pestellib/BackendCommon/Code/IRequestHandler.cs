using System.Threading.Tasks;

namespace BackendCommon.Code
{
    public class RequestContext
    {
        public string RemoteAddr;
    }

    public interface IRequestHandler
    {
        Task<byte[]> Process(byte[] data, RequestContext ctx);
    }
}
