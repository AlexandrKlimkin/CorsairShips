using System;
using System.Linq;
using System.Collections.Generic;

namespace PestelLib.MatchmakerServer.DeepWaters
{
    class DeepWatersMatchmakerStrategy : MatchmakerStrategyBase<DeepWatersMatchmakerRequest, DeepWatersMatch>
    {
        DeepWatersMatchmakerConfig _config;

        public DeepWatersMatchmakerStrategy(DeepWatersMatchmakerConfig config)
            :base(config)
        {
            _config = config;
        }

        private int GetBucket(float difficulty)
        {
            var r = (int)Math.Round(_config.BucketsCount * difficulty);
            return Math.Max(1, r);
        }

        public override void NewRequest(DeepWatersMatchmakerRequest request)
        {
            request.Bucket = GetBucket(request.Difficulty);
            base.NewRequest(request);
        }

        protected override DeepWatersMatch GenerateMatch(DeepWatersMatch match, HashSet<DeepWatersMatchmakerRequest> pool)
        {
            var MaxSearchTime = _config.MaxSearchTime;
            DeepWatersMatchmakerRequest master, player;
            master = player = match?.Master;
            if (master == null)
            {
                if (pool.Count == 0)
                    return null;
                master = player = pool.OrderBy(p => p.RegTime).FirstOrDefault();
                if (player == null || (DateTime.UtcNow - master.RegTime) < MaxSearchTime)
                    return null;
                pool.RemoveWhere(_ => _.Equals(player));
            }
            // _config can change while generation cycle so cache needed values on start
            var tierInfo = _config.TiersInfo[master.Tier];
            var maxPowerDiff = tierInfo.MaxPowerDiff;
            var team1Difficulty = master.Difficulty;
            var MaxErrorCoeff = _config.MaxErrorCoeff;
            var TeamCapacity = _config.TeamCapacity;
            var PartyCapacity = TeamCapacity * 2;
            var MaxWaitPlayersTime = MaxSearchTime + _config.MaxWaitPlayersTime;

            var matchResult = match ?? new DeepWatersMatch(PartyCapacity, maxPowerDiff);
            float maxPowerError = maxPowerDiff * MaxErrorCoeff;
            float currentPowerDiff = maxPowerDiff * ((0.5f - team1Difficulty) / 0.5f);
            float medianPower = 0;
            matchResult.Team1 = new List<DeepWatersMatchmakerRequest>();
            matchResult.Team2 = new List<DeepWatersMatchmakerRequest>();
            //Log.Debug("Target diff: " + currentPowerDiff);

            var i = matchResult.Party.Count;
            for (; i < PartyCapacity; i++)
            {
                //Log.Debug($"Pick {i + 1}. Team index: " + (i % 2 == 0 ? 1 : 2));
                //Log.Debug($"Pick {i + 1}. Player index: " + i);

                var diff = i % 2 == 0 ? master.Difficulty : 1f - master.Difficulty;
                var bucket = i % 2 == 0 ? master.Bucket : GetBucket(diff);
                if (i > 0)
                {
                    var specificPool = pool.Where(p => p.Tier == master.Tier && p.Bucket == bucket);
                    player = ExtractBest(matchResult, maxPowerError, specificPool);
                    if (player != null)
                    {
                        pool.Remove(player);
                    }
                }

                if (i == 0)
                { // if first player
                    medianPower = ((float)(player.Power)) - (currentPowerDiff / (TeamCapacity * 2));
                    //Log.Debug("Median power: " + medianPower);
                }

                if (player == null)
                {
                    if (DateTime.UtcNow - master.RegTime >= MaxWaitPlayersTime)
                    {
                        player = new DeepWatersMatchmakerRequest()
                        {
                            PlayerId = Guid.NewGuid(),
                            RegTime = DateTime.UtcNow,
                            BotOnly = true,
                            IsBot = true,
                            Bucket = bucket,
                            Difficulty = diff,
                            Power = (int)matchResult.LastPrediction,
                            ShipClass = master.ShipClass,
                            Tier = master.Tier
                        };
                    }
                    else
                        //Log.Debug($"Match not found. Selected only {team1.Concat(team2).Count()} of {TeamCapacity * 2}");
                        return matchResult;
                }

                player.TeamId = i % 2;

                if (i % 2 == 0)
                {
                    matchResult.Team1.Add(player);
                }
                else
                {
                    matchResult.Team2.Add(player);
                }

                //Log.Debug($"Pick {i + 1}. Player power: " + player.Power);
                //Log.Debug($"Pick {i + 1}. Player bucket: " + player.Bucket);
                //Log.Debug($"Pick {i + 1}. Player difficulty: " + player.Difficulty);

                // Calculate next player power
                var nextPlayerIndex = i + 1;
                var nextStageIndex = nextPlayerIndex / 2;
                if (nextPlayerIndex < TeamCapacity * 2)
                {
                    var team1Power = matchResult.Team1.Sum(_ => _.Power);
                    var team1Count = matchResult.Team1.Count();
                    var team2Power = matchResult.Team2.Sum(_ => _.Power);
                    var team2Count = matchResult.Team2.Count();

                    var targetPowerDiff = currentPowerDiff - ((team1Power - medianPower * team1Count) - (team2Power - medianPower * team2Count));
                    //var freePlayerSlots = (TeamCapacity * 2 - team1Count - team2Count);
                    var freePlayerSlots = (TeamCapacity - nextStageIndex);
                    var targetPower = 0;

                    if (nextPlayerIndex % 2 == 0)
                    {
                        targetPower = (int)Math.Round(medianPower + targetPowerDiff / (freePlayerSlots));
                    }
                    else
                    {
                        targetPower = (int)Math.Round(medianPower - targetPowerDiff / (freePlayerSlots));
                    }
                    matchResult.LastPrediction = targetPower;
                    //Log.Debug("NextStageIndex: " + nextStageIndex);
                    //Log.Debug("Target diff: " + targetPowerDiff);
                    //Log.Debug("Target power: " + targetPower);
                }
            }


            var resultingDiff = (matchResult.Team1.Sum(_ => _.Power) - matchResult.Team2.Sum(_ => _.Power));
            var resultingDifficulty = 0.5 - resultingDiff * 0.5 / maxPowerDiff;
            var totalFitment = (1 - Math.Abs(resultingDifficulty - team1Difficulty));
            var botFraction = (matchResult.Team1.Count(_ => _.IsBot) + matchResult.Team2.Count(_ => _.IsBot)) / ((float)(TeamCapacity * 2));
            //Log.Debug("===========Result===========");
            //Log.Debug("Team1 : Team2");
            /*for (int i = 0; i < TeamCapacity; i++)
            {
                //Log.Debug(team1[i].Power + (team1[i].IsBot ? "b" : " ") + " : " + team2[i].Power + (team2[i].IsBot ? "b" : " "));
            }*/
            //Log.Debug("Team 1 average: " + (int)team1.Average(_ => _.Power));
            //Log.Debug("Team 2 average: " + (int)team2.Average(_ => _.Power));
            //Log.Debug("Team 1 count: " + team1.Count);
            //Log.Debug("Team 2 count: " + team2.Count);
            //Log.Debug("Median power: " + medianPower);
            //Log.Debug("Target difficulty: " + team1Difficulty);
            //Log.Debug("Resulting difficulty: " + resultingDifficulty);
            //Log.Debug("Target diff: " + currentPowerDiff);
            //Log.Debug("Resulting diff: " + resultingDiff);
            //Log.Debug("Bot fraction: " + botFraction);
            //Log.Debug("Total fitment: " + totalFitment);

            return matchResult;
        }
        private DeepWatersMatchmakerRequest ExtractBest(DeepWatersMatch match, float maxError, IEnumerable<DeepWatersMatchmakerRequest> pool)
        {
            if (!pool.Any())
                return null;
            var target = match.LastPrediction;
            var sort = pool.OrderBy(_ => Math.Abs(_.Power - target));
            var result = sort.First();
            if (Math.Abs(result.Power - target) > maxError)
            {
                return null;
            }
            else
            {
                return result;
            }
        }
    }
}
