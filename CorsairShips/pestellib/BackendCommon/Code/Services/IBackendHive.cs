using System.Collections.Generic;

namespace BackendCommon.Services
{
    public interface IBackendHive
    {
        IBackendService SelfService { get; }
        IEnumerable<IBackendService> GetByVersion(uint slCrc, uint defVersion);
    }
}
