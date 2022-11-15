using System;
using System.Threading.Tasks;
using ClansClientLib;

namespace ClansServerLib.Mongo
{
    interface IClanPlayerRequirementsChecker
    {
        Task<bool> EligibleToJoin(ClanRecord clanRecord, Guid playerId);
    }
}