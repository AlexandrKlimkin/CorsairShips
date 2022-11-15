using System;

namespace PestelLib.Utils
{
    public interface IPlayerIdProvider
    {
        Guid PlayerId { get; set; }
        int ShortId { get; }
        string DeviceId { get; }
    }
}