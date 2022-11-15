using System;
using S;

namespace ClassicLeaderboards
{
    public interface ILeaderboards
    {
        void RegisterRecord(LeaderboardRegisterRecord cmd, Guid userId);
        Guid GetUserIdBySocialUserId(string lb, string socialId);
        int SeasonIndex { get; }
        string SeasonId { get; }
        long GetRank(string lb, Guid player);
        double GetScore(string lb, Guid player);
        LeaderboardRecord[] GetTop(string lb, int start, int amount);
        LeaderboardRecord[] GetChunk(string lb, Guid[] players);
        LeaderboardRecord GetPlayer(string lb, Guid player);
        LeaderboardRecord GetPlayer(string lb, string social);
    }
}