using System;
using System.Globalization;
using log4net;
using MessagePack;
using ServerShared.GlobalConflict;
using UnityDI;
using PestelLib.ServerClientUtils;
using S;
using ServerShared;

namespace GlobalConflictModule.Sources.Scripts
{
    public class GlobalConflictClient : GlobalConflictApi
    {
        [Dependency]
        RequestQueue _requestQueue;

        bool Validate()
        {
            if(_requestQueue != null)
                return true;
            ContainerHolder.Container.BuildUp(this);

            UniversalAssert.IsNotNull(_requestQueue, "RequestQueue not available");
            return _requestQueue != null;
        }

        public GlobalConflictClient()   
        {
            ContainerHolder.Container.BuildUp(this);
            PlayersApi = new PlayersClient(this);
            ConflictsScheduleApi = new ConflictsScheduleClient(this);
            DonationApi = new DonationStageClient(this);
            BattleApi = new BattleClient(this);
            LeaderboardsApi = new LeaderboardsClient(this);
            ConflictResultsApi = new ConflictResultClient(this);
            PointOfInterestApi = new PointOfInterestClient(this);
            DebugApi = new DebugClient(this);
        }

        public void SendMessage(string method, TypedApiCall apiCall, Action<Response,DataCollection> callback)
        {
            if(!Validate())
                return;
            _requestQueue.SendRequest(
                method,
                new Request()
                {
                    GlobalConflictApiCall = apiCall
                }, 
                callback
                );
        }
    }

    class DebugClient : IDebug
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ConflictResultClient));
        private readonly GlobalConflictClient _client;

        public DebugClient(GlobalConflictClient client)
        {
            _client = client;
        }

        public void AddTime(int secondsToAdd, Action callback)
        {
            var method = "IDebug_AddTime";
            _client.SendMessage(method,
                new TypedApiCall()
                {
                    Type = GlobalConflictApiTypes.Debug.AddTime,
                    Data = MessagePackSerializer.Serialize(new object[] {secondsToAdd})
                }, (response, collection) =>
                {
                    if (response.ResponseCode != ResponseCode.OK)
                    {
                        Log.ErrorFormat("{0}: Server error {1}", method, response.ResponseCode);
                        return;
                    }

                    callback();
                });
        }

        public void StartConflict(string id, Action<string> callback)
        {
            var method = "IDebug_StartConflictById";
            _client.SendMessage(method,
                new TypedApiCall()
                {
                    Type = GlobalConflictApiTypes.Debug.StartConflictById,
                    Data = MessagePackSerializer.Serialize(new object[] { id })
                }, (response, collection) =>
                {
                    if (response.ResponseCode != ResponseCode.OK)
                    {
                        Log.ErrorFormat("{0}: Server error {1}", method, response.ResponseCode);
                        return;
                    }

                    var result = MessagePackSerializer.Deserialize<string>(collection.Data);
                    callback(result);
                });
        }

        public void StartConflict(GlobalConflictState prototype, Action<string> callback)
        {
            var method = "IDebug_StartConflictByProto";
            _client.SendMessage(method,
                new TypedApiCall()
                {
                    Type = GlobalConflictApiTypes.Debug.StartConflictByProto,
                    Data = MessagePackSerializer.Serialize(prototype)
                }, (response, collection) =>
                {
                    if (response.ResponseCode != ResponseCode.OK)
                    {
                        Log.ErrorFormat("{0}: Server error {1}", method, response.ResponseCode);
                        return;
                    }

                    var result = MessagePackSerializer.Deserialize<string>(collection.Data);
                    callback(result);
                });
        }

        public void ListConflictPrototypes(Action<string[]> callback)
        {
            var method = "IDebug_ListConflictPrototypes";
            _client.SendMessage(method,
                new TypedApiCall()
                {
                    Type = GlobalConflictApiTypes.Debug.ListConflictPrototypes,
                    Data = MessagePackSerializer.Serialize(new object[] { })
                }, (response, collection) =>
                {
                    if (response.ResponseCode != ResponseCode.OK)
                    {
                        Log.ErrorFormat("{0}: Server error {1}", method, response.ResponseCode);
                        return;
                    }

                    var result = MessagePackSerializer.Deserialize<string[]>(collection.Data);
                    callback(result);
                });
        }
    }

    class PointOfInterestClient : IPointOfInterest
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ConflictResultClient));
        private readonly GlobalConflictClient _client;

        public PointOfInterestClient(GlobalConflictClient client)
        {
            _client = client;
        }

        public void GetTeamPointsOfInterest(string conflictId, string teamId, Action<PointOfInterest[]> callback)
        {
            var method = "IPointOfInterest_GetTeamPointsOfInterest";
            _client.SendMessage(method,
                new TypedApiCall()
                {
                    Type = GlobalConflictApiTypes.PointsOfInterest.GetTeamPointsOfInterest,
                    Data = MessagePackSerializer.Serialize(new object[] { conflictId, teamId })
                }, (response, collection) =>
                {
                    if (response.ResponseCode != ResponseCode.OK)
                    {
                        Log.ErrorFormat("{0}: Server error {1}", method, response.ResponseCode);
                        return;
                    }

                    var result = MessagePackSerializer.Deserialize<PointOfInterest[]>(collection.Data);
                    callback(result);
                });
        }

        public void DeployPointOfInterest(string conflictId, string playerId, string team, int nodeId, string poiId, Action<bool> callback)
        {
            var method = "IPointOfInterest_DeployPointOfInterest";
            _client.SendMessage(method,
                new TypedApiCall()
                {
                    Type = GlobalConflictApiTypes.PointsOfInterest.DeployPointOfInterestAsync,
                    Data = MessagePackSerializer.Serialize(new object[] { conflictId, playerId, team, nodeId, poiId })
                }, (response, collection) =>
                {
                    if (response.ResponseCode != ResponseCode.OK)
                    {
                        Log.ErrorFormat("{0}: Server error {1}", method, response.ResponseCode);
                        callback(false);
                        return;
                    }
                    callback(true);
                });
        }
    }

    class ConflictResultClient : IConflictResults
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ConflictResultClient));
        private readonly GlobalConflictClient _client;

        public ConflictResultClient(GlobalConflictClient client)
        {
            _client = client;
        }

        public void GetResult(string conflictId, Action<ConflictResult> callback)
        {
            var method = "IConflictResults_GetResult";
            _client.SendMessage(method,
                new TypedApiCall()
                {
                    Type = GlobalConflictApiTypes.ConflictResults.GetResult,
                    Data = MessagePackSerializer.Serialize(new object [] { conflictId})
                }, (response, collection) =>
                {
                    if (response.ResponseCode != ResponseCode.OK)
                    {
                        Log.ErrorFormat("{0}: Server error {1}", method, response.ResponseCode);
                        return;
                    }

                    var result = MessagePackSerializer.Deserialize<ConflictResult>(collection.Data);
                    callback(result);
                });
        }
    }

    class LeaderboardsClient : ILeaderboards
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LeaderboardsClient));
        private readonly GlobalConflictClient _client;

        public LeaderboardsClient(GlobalConflictClient client)
        {
            _client = client;
        }

        public void GetDonationTopMyPosition(string userId, bool myTeamOnly, string conflictId, Action<long> callback)
        {
            var method = "ILeaderboards_GetDonationTopMyPosition";
            _client.SendMessage(method,
                new TypedApiCall()
                {
                    Type = GlobalConflictApiTypes.Leaderboards.GetDonationTopMyPosition,
                    Data = MessagePackSerializer.Serialize(new object [] { userId, myTeamOnly, conflictId})
                }, (response, collection) =>
                {
                    if (response.ResponseCode != ResponseCode.OK)
                    {
                        Log.ErrorFormat("{0}: Server error {1}", method, response.ResponseCode);
                        return;
                    }

                    var result = MessagePackSerializer.Deserialize<long>(collection.Data);
                    callback(result);
                });
        }

        public void GetDonationTop(string conflictId, string team, int page, int pageSize, Action<PlayerState[]> callback)
        {
            var method = "ILeaderboards_GetDonationTop";
            _client.SendMessage(method,
                new TypedApiCall()
                {
                    Type = GlobalConflictApiTypes.Leaderboards.GetDonationTop,
                    Data = MessagePackSerializer.Serialize(new object [] { conflictId, team, page, pageSize})
                }, (response, collection) =>
                {
                    if (response.ResponseCode != ResponseCode.OK)
                    {
                        Log.ErrorFormat("{0}: Server error {1}", method, response.ResponseCode);
                        return;
                    }

                    var result = MessagePackSerializer.Deserialize<PlayerState[]>(collection.Data);
                    callback(result);
                });
        }

        public void GetWinPointsTopMyPosition(string userId, bool myTeamOnly, string conflictId, Action<long> callback)
        {
            var method = "ILeaderboards_GetWinPointsTopMyPosition";
            _client.SendMessage(method,
                new TypedApiCall()
                {
                    Type = GlobalConflictApiTypes.Leaderboards.GetWinPointsTopMyPosition,
                    Data = MessagePackSerializer.Serialize(new object [] { userId, myTeamOnly, conflictId})
                }, (response, collection) =>
                {
                    if (response.ResponseCode != ResponseCode.OK)
                    {
                        Log.ErrorFormat("{0}: Server error {1}", method, response.ResponseCode);
                        return;
                    }

                    var result = MessagePackSerializer.Deserialize<long>(collection.Data);
                    callback(result);
                });
        }

        public void GetWinPointsTop(string conflictId, string teamId, int page, int pageSize, Action<PlayerState[]> callback)
        {
            var method = "ILeaderboards_GetWinPointsTop";
            _client.SendMessage(method,
                new TypedApiCall()
                {
                    Type = GlobalConflictApiTypes.Leaderboards.GetWinPointsTop,
                    Data = MessagePackSerializer.Serialize(new object [] {conflictId, teamId, page, pageSize})
                }, (response, collection) =>
                {
                    if (response.ResponseCode != ResponseCode.OK)
                    {
                        Log.ErrorFormat("{0}: Server error {1}", method, response.ResponseCode);
                        return;
                    }

                    var result = MessagePackSerializer.Deserialize<PlayerState[]>(collection.Data);
                    callback(result);
                });
        }
    }

    class BattleClient : IBattle
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BattleClient));
        private readonly GlobalConflictClient _client;

        public BattleClient(GlobalConflictClient client)
        {
            _client = client;
        }

        public void RegisterBattleResult(string playerId, int nodeId, bool win, decimal winMod, decimal loseMod, Action callback)
        {
            var method = "IBattle_RegisterBattleResult";
            _client.SendMessage(method,
                new TypedApiCall()
                {
                    Type = GlobalConflictApiTypes.Battle.RegisterBattleResult,
                    Data = MessagePackSerializer.Serialize(
                        new object [] { playerId, nodeId, win, winMod.ToString(CultureInfo.InvariantCulture), loseMod.ToString(CultureInfo.InvariantCulture) })
                }, (response, collection) =>
                {
                    if (response.ResponseCode != ResponseCode.OK)
                    {
                        Log.ErrorFormat("{0}: Server error {1}", method, response.ResponseCode);
                        return;
                    }

                    callback();
                });
        }
    }

    class DonationStageClient : IDonationStage
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DonationStageClient));
        private readonly GlobalConflictClient _client;

        public DonationStageClient(GlobalConflictClient client)
        {
            _client = client;
        }

        public void Donate(string userId, int amount, Action callback)
        {
            var method = "IDonationStage_Donate";
            _client.SendMessage(method,
                new TypedApiCall()
                {
                    Type = GlobalConflictApiTypes.DonationStage.Donate,
                    Data = MessagePackSerializer.Serialize(new object[] { userId, amount})
                }, (response, collection) =>
                {
                    if (response.ResponseCode != ResponseCode.OK)
                    {
                        Log.ErrorFormat("{0}: Server error {1}", method, response.ResponseCode);
                        return;
                    }

                    callback();
                });
        }
    }

    class ConflictsScheduleClient : IConflictsSchedule
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ConflictsScheduleClient));
        private readonly GlobalConflictClient _client;

        public ConflictsScheduleClient(GlobalConflictClient client)
        {
            _client = client;
        }

        public void GetCurrentConflict(Action<GlobalConflictState> callback)
        {
            var method = "IConflictsSchedule_GetCurrentConflict";
            _client.SendMessage(method,
                new TypedApiCall()
                {
                    Type = GlobalConflictApiTypes.ConflictsSchedule.GetCurrentConflict
                }, (response, collection) =>
                {
                    if (response.ResponseCode != ResponseCode.OK)
                    {
                        Log.ErrorFormat("{0}: Server error {1}", method, response.ResponseCode);
                        return;
                    }

                    var result = MessagePackSerializer.Deserialize<GlobalConflictState>(collection.Data);
                    callback(result);
                });
        }

        public GlobalConflictState GetCurrentConflict()
        {
            throw new NotImplementedException();
        }
    }

    class PlayersClient : IPlayers
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PlayersClient));
        private readonly GlobalConflictClient _client;

        public PlayersClient(GlobalConflictClient client)
        {
            _client = client;
        }

        public void SetName(string userId, string name, Action callback)
        {
            var method = "IPlayers_SetName";
            _client.SendMessage(method,
                new TypedApiCall()
                {
                    Type = GlobalConflictApiTypes.Players.SetName,
                    Data = MessagePackSerializer.Serialize(new[] { name })
                }, (response, collection) =>
                {
                    if (response.ResponseCode != ResponseCode.OK)
                    {
                        Log.ErrorFormat("{0}: Server error {1}", method, response.ResponseCode);
                        return;
                    }
                    callback();
                });
        }

        public void Register(string conflictId, string userId, string teamId, Action<PlayerState> callback)
        {
            var method = "IPlayers_Register";
            _client.SendMessage(method,
                new TypedApiCall()
                {
                    Type = GlobalConflictApiTypes.Players.Register,
                    Data = MessagePackSerializer.Serialize(new [] { conflictId, userId, teamId})
                }, (response, collection) =>
                {
                    if (response.ResponseCode != ResponseCode.OK)
                    {
                        Log.ErrorFormat("{0}: Server error {1}", method, response.ResponseCode);
                        return;
                    }

                    var playerState = MessagePackSerializer.Deserialize<PlayerState>(collection.Data);
                    callback(playerState);
                });
        }

        public void GetPlayer(string userId, string conflictId, Action<PlayerState> callback)
        {
            var method = "IPlayers_GetPlayer";
            _client.SendMessage(method,
                new TypedApiCall()
                {
                    Type = GlobalConflictApiTypes.Players.GetPlayer,
                    Data = MessagePackSerializer.Serialize(new [] { userId, conflictId})
                }, (response, collection) =>
                {
                    if (response.ResponseCode != ResponseCode.OK)
                    {
                        Log.ErrorFormat("{0}: Server error {1}", method, response.ResponseCode);
                        return;
                    }

                    var playerState = MessagePackSerializer.Deserialize<PlayerState>(collection.Data);
                    callback(playerState);
                });
        }

        public void GetTeamPlayersStat(string conflict, Action<TeamPlayersStat> callback)
        {
            var method = "IPlayers_GetTeamPlayersStat";
            _client.SendMessage(method,
                new TypedApiCall()
                {
                    Type = GlobalConflictApiTypes.Players.GetTeamPlayersStat,
                    Data = MessagePackSerializer.Serialize(new [] {conflict})
                }, (response, collection) =>
                {
                    if (response.ResponseCode != ResponseCode.OK)
                    {
                        Log.ErrorFormat("{0}: Server error {1}", method, response.ResponseCode);
                        return;
                    }

                    var stats = MessagePackSerializer.Deserialize<TeamPlayersStat>(collection.Data);
                    callback(stats);
                });
        }

        public PlayerState GetPlayer(string userId, string conflictId)
        {
            throw new NotImplementedException();
        }
    }
}
