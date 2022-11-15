using System;

namespace BoltMasterServer
{
    public interface ITimeProvider
    {
        DateTime UtcNow { get; }
    }
}