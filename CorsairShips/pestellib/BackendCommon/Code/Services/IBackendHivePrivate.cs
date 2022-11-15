using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PestelLib.ClientConfig;

namespace BackendCommon.Services
{
    public interface IBackendHivePrivate
    {
        Task SetOnlineStatus(IBackendService service, bool online);
        IEnumerable<IBackendService> AllServices();
        void RegisterSharedConfig(Uri url, SharedConfig config);
        void SetMaintenance(IBackendService service, DateTime val);
        void RemoveMaintenance(IBackendService service);
        void SetPublicAccess(IBackendService service, bool val);
        void SetInternalAccess(IBackendService service, bool val);
    }
}
