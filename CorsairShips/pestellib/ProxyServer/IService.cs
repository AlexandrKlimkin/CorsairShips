using System;
using System.Threading.Tasks;

namespace ProxyServerApp
{
    interface IService
    {
        event Action<int, byte[]> OnAnswer;
        Task<bool> Process(int sender, byte[] data);
    }
}