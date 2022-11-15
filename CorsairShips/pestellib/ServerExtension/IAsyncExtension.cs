using System.Threading.Tasks;
using PestelLib.ServerShared;

namespace ServerExtension
{
    public interface IAsyncExtension
    {
        Task<ServerResponse> ProcessRequestAsync(byte[] requestData);
    }
}