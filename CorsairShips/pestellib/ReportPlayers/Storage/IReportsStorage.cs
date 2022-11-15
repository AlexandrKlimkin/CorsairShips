using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReportPlayersProtocol;

namespace ReportPlayers
{
    public interface IReportsStorage
    {
        void RemoveReportsByReported(Guid reported, bool wipeAllReports);
        void RegisterReport(PlayerReportData reportData);
        void IncrementSessionCounter(Guid playerId, string playerName);

        PlayerCounterData GetCounterData(Guid playerId);
        Task<IEnumerable<PlayerReportData>> GetReportsByReportedAsync(Guid reported, int limit);
        Task<IEnumerable<PlayerCounterData>> GetCheatersTopAsync(int limit);

        void AddUserToWhitelist(Guid playerId);
        void RemoveUserFromWhitelist(Guid playerId);
    }
}