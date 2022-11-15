using System;
using PestelLib.ServerShared;
using S;

namespace BackendCommon.Services
{
    public interface IBackendService
    {
        /// <summary>
        /// Backend available from internet.
        /// </summary>
        bool Public { get; }
        /// <summary>
        /// Backend available for internal requests (backend -> backend).
        /// </summary>
        bool Internal { get; }
        bool Maintenance { get; }
        DateTime MaintenanceStart { get; }
        /// <summary>
        /// Cached value of last online check.
        /// </summary>
        bool Online { get; }

        bool IsOnline();
        ServerResponse ProcessRequest(ServerRequest request);
    }
}
