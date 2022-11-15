using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BackendCommon.Code.Leagues;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using ServerLib;

namespace LeaguesTestServer
{
    class Program
    {
        private static List<Guid> players = new List<Guid>();
        private static Random rnd = new Random();
        private static AppSettings settings;
        private static LeagueLeaderboardConfig config;
        private static LeagueStateCache state;
        private static LeagueServer leagueServer;
        private static string[] benckmark;
        private static Dictionary<string, ILeagueStorage> storageImpls;
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(Program));

        static Program()
        {
            benckmark = new[]
            {
                "create 10000",
                "score 1000",
                "score 1000",
                "score 1000",
                "score 1000",
                "score 1000",
                "score 1000",
                "score 1000",
                "score 1000",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "login 100",
                "login 100",
                "login 100",
                "login 100",
                "login 100",
                "login 100",
                "login 100",
                "login 100",
                "login 100",
                "login 100",
                "login 100",
                "login 100",
                "globaltop 100",
                "login 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "rank 100",
                "rank 100",
                "rank 100",
                "rank 100",
                "rank 100",
                "rank 100",

                "endseason",
                "loginall",
                "score 1000",
                "score 1000",
                "score 1000",
                "score 1000",
                "score 1000",
                "score 1000",
                "score 1000",
                "score 1000",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "rank 100",
                "rank 100",
                "rank 100",
                "rank 100",
                "rank 100",
                "rank 100",

                "endseason",
                "loginall",
                "score 1000",
                "score 1000",
                "score 1000",
                "score 1000",
                "score 1000",
                "score 1000",
                "score 1000",
                "score 1000",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "rank 100",
                "rank 100",
                "rank 100",
                "rank 100",
                "rank 100",
                "rank 100",

                "endseason",
                "loginall",
                "score 1000",
                "score 1000",
                "score 1000",
                "score 1000",
                "score 1000",
                "score 1000",
                "score 1000",
                "score 1000",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "rank 100",
                "rank 100",
                "rank 100",
                "rank 100",
                "rank 100",
                "rank 100",

                "endseason",
                "loginall",
                "score 1000",
                "score 1000",
                "score 1000",
                "score 1000",
                "score 1000",
                "score 1000",
                "score 1000",
                "score 1000",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "globaltop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "leaguetop 100",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "divtop",
                "rank 100",
                "rank 100",
                "rank 100",
                "rank 100",
                "rank 100",
                "rank 100",
            };
        }

        static Dictionary<string, List<double>> _perf = new Dictionary<string, List<double>>();

        static void ProcessCommand(string command, string[] commandArgs, bool silent = false)
        {
            var sw = Stopwatch.StartNew();
            int playersAmount = 1;
            switch (command)
            {
                case "storage":
                    var leagueStorage = storageImpls[commandArgs[0]];
                    leagueServer = new LeagueServer(leagueStorage, config, null, null);
                    players = new List<Guid>();
                    break;
                case "endseason":
                    state.SeasonEnds = DateTime.UtcNow.AddSeconds(-1);
                    leagueServer.SeasonController.Update();
                    break;
                case "benchmark":
                    _perf.Clear();
                    for (var i = 0; i < benckmark.Length; ++i)
                    {
                        ParseCommand(benckmark[i], out var b_command, out var b_commandArgs);
                        ProcessCommand(b_command, b_commandArgs, true);
                    }

                    foreach (var kv in _perf)
                    {
                        Log.Info($"{kv.Key} speed {kv.Value.Average()}");
                    }
                    break;
                case "seasonupdate":
                    leagueServer.SeasonController.Update();
                    break;
                case "create":
                    playersAmount = int.Parse(commandArgs[0]);
                    for (var i = 0; i < playersAmount; ++i)
                    {
                        var p = Guid.NewGuid();
                        var name = "Player" + p.ToString("N").Substring(0, 4);
                        players.Add(p);
                        var pi = leagueServer.Register(p, name, null).Result;
                        //Console.WriteLine(JsonConvert.SerializeObject(pi));
                    }
                    break;
                case "loginall":
                    playersAmount = players.Count;
                    for (var i = 0; i < players.Count; ++i)
                    {
                        var p = players[i];
                        var pi = leagueServer.Register(p, null, null).Result;
                    }
                    break;
                case "login":
                    if (players.Count == 0)
                    {
                        Log.Info("No players created");
                        break;
                    }
                    playersAmount = int.Parse(commandArgs[0]);
                    for (var i = 0; i < playersAmount; ++i)
                    {
                        var p = players[rnd.Next(0, players.Count)];
                        var pi = leagueServer.Register(p, null, null).Result;
                        if(!silent)
                            Console.WriteLine(JsonConvert.SerializeObject(pi));
                    }
                    break;
                case "score":
                    if (players.Count == 0)
                    {
                        Log.Info("No players created");
                        break;
                    }
                    playersAmount = int.Parse(commandArgs[0]);
                    for (var i = 0; i < playersAmount; ++i)
                    {
                        var p = players[rnd.Next(0, players.Count)];
                        leagueServer.Score(p, rnd.Next(0, 100));
                    }
                    break;
                case "globaltop":
                {
                    if (players.Count == 0)
                    {
                        Log.Info("No players created");
                        break;
                    }
                    playersAmount = int.Parse(commandArgs[0]);
                    var p = players[rnd.Next(0, players.Count)];
                    var gt = leagueServer.GlobalTop(p, playersAmount).Result.Ranks;
                    for (int i = 0; i < gt.Length; i++)
                    {
                        if (!silent)
                            Log.Info($"{i}: {gt[i].Name} {gt[i].Score}");
                    }
                }
                    break;
                case "leaguetop":
                    if (players.Count == 0)
                    {
                        Log.Info("No players created");
                        break;
                    }
                    playersAmount = int.Parse(commandArgs[0]);
                    var lp = players[rnd.Next(0, players.Count)];
                    var lt =leagueServer.LeagueTop(lp, playersAmount).Result.Ranks;
                    for (int i = 0; i < lt.Length; i++)
                    {
                        if (!silent)
                            Log.Info($"{i}: {lt[i].Name} {lt[i].Score}");
                    }
                    break;
                case "divtop":
                    if (players.Count == 0)
                    {
                        Log.Info("No players created");
                        break;
                    }
                    var dp = players[rnd.Next(0, players.Count)];
                    var dt = leagueServer.DivisionPlayersRanks(dp).Result.Ranks;
                    for (int i = 0; i < dt.Length; i++)
                    {
                        if (!silent)
                            Log.Info($"{i}: {dt[i].Name} {dt[i].Score}");
                    }
                    break;
                case "rank":
                    if (players.Count == 0)
                    {
                        Log.Info("No players created");
                        break;
                    }
                    playersAmount = int.Parse(commandArgs[0]);
                    for (int i = 0; i < playersAmount; i++)
                    {
                        var rp = players[rnd.Next(0, players.Count)];
                        var r = leagueServer.PlayerGlobalRank(rp).Result;
                        if (!silent)
                            Log.Info($"Player {rp} global rank {r}");
                        r = leagueServer.PlayerLeagueRank(rp).Result;
                        if (!silent)
                            Log.Info($"Player {rp} league rank {r}");
                    }
                    break;
            }

            if (!_perf.TryGetValue(command, out var list))
            {
                list = new List<double>();
                _perf[command] = list;
            }
            list.Add(sw.Elapsed.TotalMilliseconds / playersAmount);
            Log.Info($"{command} time: {sw.ElapsedMilliseconds} ms");
        }

        private static void ParseCommand(string commandRaw, out string command, out string[] commandArgs)
        {
            var commandParts = commandRaw.ToLower().Split(' ');
            command = commandParts[0];
            commandArgs = commandParts.Skip(1).ToArray();
        }

        static void Main(string[] args)
        {
            PestelLib.ServerCommon.Log.Init();
            var cb = new ConfigurationBuilder();

            string configPath = AppDomain.CurrentDomain.BaseDirectory + "appsettings.json";
            settings = AppSettings.LoadConfig(configPath);

            config = new LeagueLeaderboardConfig();
            state = new LeagueStateCache();
            //var leagueStorage = new LeagueStorageRedis(config);
            var mongoUrl = new MongoUrl(settings.PersistentStorageSettings.StorageConnectionString);

            storageImpls = new Dictionary<string, ILeagueStorage>()
            {
                //{ "redis", new LeagueStorageRedis(config, new LeagueStateCache(), new LeagueDefHelper(null))},
                /*{ "mongo", new LeagueStorageMongo(mongoUrl, "Leagues", config)},
                { "memory",new LeagueStorageMemory(config)}*/
            };

            string command;
            ProcessCommand("storage", new[] { "redis" });
            do
            {
                ParseCommand(Console.ReadLine(), out command, out var commandArgs);
                ProcessCommand(command, commandArgs);
            } while (command != "exit");
        }
    }
}
