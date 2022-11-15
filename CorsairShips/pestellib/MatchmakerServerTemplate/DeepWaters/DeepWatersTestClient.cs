using System;
using System.Collections.Generic;
using PestelLib.MatchmakerServer.Config;
using PestelLib.MatchmakerShared;
using log4net;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace PestelLib.MatchmakerServer.DeepWaters
{
    class DeepWatersTestClient
    {
        private static ILog Log = LogManager.GetLogger(typeof(Program));
        static Random _rnd = new Random();
        int ShipClassCount = Enum.GetValues(typeof(ShipClass)).Length;
        DeepWatersMatchmakerConfig _config = new DeepWatersMatchmakerConfig();
        readonly MatchmakerClientProtocol<DeepWatersMatchmakerRequest, DeepWatersMatch, DeepWatersMatchStats> _clientProtocol;

        public DeepWatersTestClient(string host, int port, float spawnPeriod, bool createConfig = false)
        {
            InitConfig(createConfig);

            _clientProtocol = MatchmakerClientFactory.CreateTcp<DeepWatersMatchmakerRequest, DeepWatersMatch, DeepWatersMatchStats>(host, port);

            _clientProtocol.OnHostMatch += (m) => { PrintMatch(m.Match); return true; };
            _clientProtocol.OnJoinMatch += (m) => true;

            Task.Run(() => RequestsSpawnerLoop(spawnPeriod));
        }

        public void InitConfig(bool forced)
        {
        }

        private DeepWatersMatchmakerRequest GenerateUser()
        {
            var t = _rnd.Next(1, _config.TiersInfo.Count);
            var ti = _config.TiersInfo[t];
            var d = (float)_rnd.NextDouble();
            return
                new DeepWatersMatchmakerRequest()
                {
                    PlayerId = Guid.NewGuid(),
                    RegTime = DateTime.UtcNow - TimeSpan.FromSeconds(_config.MaxSearchTime.TotalSeconds * _rnd.NextDouble()),
                    Tier = t,
                    Difficulty = d,
                    Power = _rnd.Next(ti.MinPower, ti.MaxPower),
                    ShipClass = _rnd.Next(0, ShipClassCount),
                    BotOnly = false,
                };
        }

        private void RequestsSpawnerLoop(float spawnSpeed)
        {
            var sw = Stopwatch.StartNew();
            var swGen = Stopwatch.StartNew();
            var start = 0L;
            var fromLastSpawn = 0f;
            while (true)
            {
                var dt = (float)(sw.ElapsedMilliseconds - start) / 1000;
                start = sw.ElapsedMilliseconds;
                fromLastSpawn += dt;
                if (fromLastSpawn > spawnSpeed)
                {
                    swGen.Restart();
                    var mm = GenerateUser();
                    _clientProtocol.Register(new ClientMatchmakerRegister<DeepWatersMatchmakerRequest>() { MatchParams = mm });
                    //_strategy.NewRequest(mm);
                    if (swGen.ElapsedMilliseconds > spawnSpeed * 1000f)
                    {
                        //Console.WriteLine($"User gen time: {swGen.ElapsedMilliseconds}ms");
                    }
                    fromLastSpawn -= spawnSpeed;
                }
            }
        }


        private void PrintMatch(IMatch m)
        {
            var dm = m as DeepWatersMatch;
            var mStr = JsonConvert.SerializeObject(m);

            var ti = _config.TiersInfo[dm.Master.Tier];
            var maxPowerError = ti.MaxPowerDiff * dm.Master.Tier * (_config.MaxErrorCoeff);
            var resultingDiff = (dm.Team1.Sum(_ => _.Power) - dm.Team2.Sum(_ => _.Power));
            var resultingDifficulty = 0.5 - resultingDiff * 0.5 / ti.MaxPowerDiff;
            var totalFitment = (1 - Math.Abs(resultingDifficulty - dm.Master.Difficulty));
            float currentPowerDiff = ti.MaxPowerDiff * ((0.5f - dm.Master.Difficulty) / 0.5f);
            var botFraction = (dm.Team1.Count(_ => _.IsBot) + dm.Team2.Count(_ => _.IsBot)) / ((float)(_config.TeamCapacity * 2));
            var medianPower = ((float)(dm.Master.Power)) - (currentPowerDiff / (_config.TeamCapacity * 2));
            var searchTime = (DateTime.UtcNow - dm.Master.RegTime).TotalMilliseconds;
            Log.Debug("===========Result===========");
            Log.Debug("Team1 : Team2");
            for (int i = 0; i < _config.TeamCapacity; i++)
            {
                Log.Debug(dm.Team1[i].Power + (dm.Team1[i].IsBot ? "b" : " ") + " : " + dm.Team2[i].Power + (dm.Team2[i].IsBot ? "b" : " "));
            }
            Log.Debug("Team 1 average: " + (int)dm.Team1.Average(_ => _.Power));
            Log.Debug("Team 2 average: " + (int)dm.Team2.Average(_ => _.Power));
            Log.Debug("Team 1 count: " + dm.Team1.Count);
            Log.Debug("Team 2 count: " + dm.Team2.Count);
            Log.Debug("Median power: " + medianPower);
            Log.Debug("Target difficulty: " + dm.Master.Difficulty);
            Log.Debug("Resulting difficulty: " + resultingDifficulty);
            Log.Debug("Target diff: " + currentPowerDiff);
            Log.Debug("Resulting diff: " + resultingDiff);
            Log.Debug("Bot fraction: " + botFraction);
            Log.Debug("Total fitment: " + totalFitment);
            Console.WriteLine($"Match {m.Id} with {m.CountBots} bots found after {searchTime} ms.");
        }

    }
}
