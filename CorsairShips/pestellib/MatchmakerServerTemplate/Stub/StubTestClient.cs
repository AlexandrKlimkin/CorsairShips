using System;
using System.Diagnostics;
using log4net;
using System.Threading.Tasks;
using PestelLib.MatchmakerShared;

namespace PestelLib.MatchmakerServer.Stub
{
    class StubTestClient
    {
        private static ILog Log = LogManager.GetLogger(typeof(Program));
        static Random _rnd = new Random();
        MatchmakerClientProtocol<MatchmakerRequest, StubMatch, StubMatchingStats> _clientProtocol;

        public StubTestClient(string host, int port, float spawnPeriod)
        {
            _clientProtocol = MatchmakerClientFactory.CreateTcp<MatchmakerRequest, StubMatch, StubMatchingStats>(host, port);

            _clientProtocol.OnHostMatch += (m) => { PrintMatch(m.Match); return true; };
            _clientProtocol.OnJoinMatch += (m) => true;

            Task.Run(() => RequestsSpawnerLoop(spawnPeriod));
        }

        private MatchmakerRequest GenerateUser()
        {
            var d = (float)_rnd.NextDouble();
            return
                new MatchmakerRequest()
                {
                    PlayerId = Guid.NewGuid(),
                    RegTime = DateTime.UtcNow
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
                    _clientProtocol.Register(new ClientMatchmakerRegister<MatchmakerRequest>() { MatchParams = mm });
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
            var searchTime = (DateTime.UtcNow - m.Master.RegTime).TotalMilliseconds;
            Log.Debug($"Match {m.Id} search time {searchTime} bots {m.CountBots} fitment {m.Fitment}");
            Console.WriteLine($"Match {m.Id} with {m.CountBots} bots found after {searchTime} ms");
        }
    }
}
