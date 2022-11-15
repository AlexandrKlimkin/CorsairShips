using System;
using PestelLib.ServerShared;

namespace ServerExtension
{
    public interface IExtension
    {
        ServerResponse ProcessRequest(byte[] requestData);
    }
}
