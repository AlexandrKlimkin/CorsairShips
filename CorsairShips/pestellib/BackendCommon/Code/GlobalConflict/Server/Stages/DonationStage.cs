using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerShared.GlobalConflict;

namespace BackendCommon.Code.GlobalConflict.Server.Stages
{
    public class DonationStage : Stage
    {
        public override async Task<bool> HasWork()
        {
            var result = await _api.DonationStagePrivateApi.UnprocessedDonationsAsync(1).ConfigureAwait(false);
            if (result.Length > 0)
                return true;
            var conflictState = await _api.ConflictsSchedulePrivateApi.GetCurrentConflictAsync().ConfigureAwait(false);
            var conflict = new Conflict(conflictState);
            if (await conflict.IsCurrentStage<DonationStage>().ConfigureAwait(false))
            {
                return false;
            }
            var teamStats = await _api.PlayersPrivateApi.GetTeamPlayersStatAsync(conflictState.Id).ConfigureAwait(false);
            var generalsAssigned = true;
            for(var i = 0; i < teamStats.Teams.Length; ++i)
            {
                generalsAssigned &= teamStats.GeneralsCount[i] == conflictState.GeneralsCount ||
                                    teamStats.GeneralsCount[i] == teamStats.PlayersCount[i];
            }
            return !generalsAssigned;
        }

        // выдать генералов топу донатеров. происходит с конце этапа после того как обработаются все донаты
        private async Task<bool> UpdateGenerals(GlobalConflictState conflictState)
        {
            if (conflictState.GeneralsCount < 1)
                return false;
            var hasNewGenerals = false;
            for (var t = 0; t < conflictState.Teams.Length; ++t)
            {
                var team = conflictState.Teams[t];
                var generals = await _api.LeaderboardsPrivateApi.GetDonationTopAsync(conflictState.Id, team, 0, conflictState.GeneralsCount).ConfigureAwait(false);
                var newGenerals = generals.Where(_ => _.GeneralLevel < 1).ToArray();
                hasNewGenerals |= newGenerals.Length > 0;
                var parallelTasks = new Task[newGenerals.Length];
                for (var i = 0; i < newGenerals.Length; ++i)
                {
                    var g = newGenerals[i];
                    g.GeneralLevel = conflictState.GeneralsCount - i;
                    parallelTasks[i] = _api.PlayersPrivateApi.SaveAsync(g);
                }

                await Task.WhenAll(parallelTasks).ConfigureAwait(false);
            }

            return hasNewGenerals;
        }

        public override async Task<bool> Update()
        {
            // мы не хотим чтобы донат засчитывался несколько раз
            using (await _lockManager.LockAsync(StageLock, 0, false).ConfigureAwait(false))
            {
                var donationsT = _api.DonationStagePrivateApi.UnprocessedDonationsAsync(1000);
                var conflictStateT = _api.ConflictsSchedulePrivateApi.GetCurrentConflictAsync();
                await Task.WhenAll(donationsT, conflictStateT).ConfigureAwait(false);

                var donations = donationsT.Result;
                var conflictState = conflictStateT.Result;
                var conflict = new Conflict(conflictState);
                if (!await conflict.IsCurrentStage<DonationStage>().ConfigureAwait(false) && donations.Length == 0)
                    return await UpdateGenerals(conflictState).ConfigureAwait(false);
                var processedDonations = new List<Donation>();
                var playersCache = new Dictionary<string, PlayerState>();
                if (donations.Length == 0)
                    return false;
                try
                {
                    foreach (var donation in donations)
                    {
                        if (!playersCache.TryGetValue(donation.UserId, out var playerState))
                            playersCache[donation.UserId] = playerState = await _api.PlayersPrivateApi.GetPlayerAsync(donation.UserId, conflictState.Id).ConfigureAwait(false);
                        var playerDonationPoints = playerState.DonationPoints;
                        var isLvlUp = conflict.IsDonationLevelUp(playerState.DonationPoints, donation.Amount);
                        var isTeamLvlUp = conflict.IsDonationTeamLevelUp(playerState.TeamId, donation.Amount);
                        await _api.PlayersPrivateApi.AddDonationAsync(conflictState.Id, donation).ConfigureAwait(false);
                        processedDonations.Add(donation);
                        var teamState = conflict.GetTeamState(playerState.TeamId);
                        var teamDonationPoints = teamState.DonationPoints;
                        teamState.DonationPoints += donation.Amount;
                        if (!isLvlUp && !isTeamLvlUp)
                            continue;

                        if (isLvlUp)
                        {
                            var curDonationLevel = conflict.GetDonationLevel(playerDonationPoints, false) + 1;
                            var donationLvl = conflict.GetDonationLevel(playerDonationPoints + donation.Amount, false);
                            var b = Enumerable.Range(curDonationLevel, donationLvl - curDonationLevel + 1)
                                .SelectMany(i => conflict.GetDonationBonuses(i, false)).ToArray();
                            await _api.PlayersPrivateApi.GiveBonusesToPlayerAsync(conflictState.Id, playerState.Id, b).ConfigureAwait(false);
                        }
                        if(isTeamLvlUp)
                        {
                            var team = conflict.GetTeamState(playerState.TeamId);
                            var curDonationLevel = conflict.GetDonationLevel(teamDonationPoints, true) + 1;
                            var donationLvl = conflict.GetDonationLevel(teamDonationPoints + donation.Amount, true);
                            var b = Enumerable.Range(curDonationLevel, donationLvl - curDonationLevel + 1)
                                .SelectMany(i => conflict.GetDonationBonuses(i, true)).ToArray();
                            team.DonationBonuses = team.DonationBonuses.Union(b).ToArray();
                        }
                    }
                }
                finally
                {
                    await _api.DonationStagePrivateApi.ProcessedDonationsAsync(processedDonations.ToArray()).ConfigureAwait(false);
                }
                await _api.ConflictsSchedulePrivateApi.SaveAsync(conflictState).ConfigureAwait(false);
            }
            return true;
        }
    }
}