using System;
using System.Collections.Generic;
using MessagePack;

namespace S
{
    [MessagePackObject]
    public class BenchmarkRequest
    {
        [Key(1)]
        public List<byte[]> SerializedRequest = new List<byte[]>();
    }

    [MessagePackObject]
    public class DataCollection
    {
        [Key(1)]
        public byte[] Request;
        [Key(2)]
        public byte[] Response;
        [Key(3)]
        public byte[] Data;
        [Key(4)]
        public byte[] State;
    }

    [MessagePackObject]
    public class Request
    {
        [Key(1)]
        public byte[] UserId;
        [Key(7)]
        public string DeviceUniqueId;
        [Key(2)]
        public S.NetworkType NetworkId;
        [Key(3)]
        public long SessionId;
        [Key(4)]
        public S.InitRequest InitRequest;
        [Key(5)]
        public S.ProcessCommandsBatchRequest ProcessCommandsBatchRequest;
        [Key(6)]
        public S.ResetRequest ResetRequest;
        [Key(8)]
        public S.SetFacebookIdRequest SetFacebookIdRequest;
        [Key(9)]
        public S.GetProfileByFacebookIdRequest GetProfileByFacebookIdRequest;
        [Key(10)]
        public S.GetRandomUserIds GetRandomUserIds;
        [Key(12)]
        public S.SyncTime SyncTime;
        [Key(13)]
        public S.ReplaceStateRequest ReplaceStateRequest;
        [Key(14)]
        public S.RegisterPayment RegisterPayment;
        [Key(16)]
        public S.SendFeedback SendFeedback;
        [Key(19)]
        public S.LeaderboardRegisterRecord LeaderboardRegisterRecord;
        [Key(20)]
        public S.LeaderboardGetRank LeaderboardGetRank;
        [Key(21)]
        public S.LeaderboardGetRankTop LeaderboardGetRankTop;
        [Key(22)]
        public S.LeaderboardGetRankTopChunk LeaderboardGetRankTopChunk;
        //[Key(23)]
        //public S.RegisterReplay RegisterReplay;
        //[Key(24)]
        //public S.GetReplay GetReplay;
        //[Key(25)]
        //public S.GetSeason GetSeason; //don't use key 25!
        //[Key(26)]
        //public S.GetInbox GetInbox; //don't use key 26!
        //[Key(27)]
        //public S.SendMessage SendMessage; //don't use key 27!
        [Key(28)]
        public S.LeaderboardGetFacebookFriendsTop LeaderboardGetFacebookFriendsTop;

        [Key(29)]
        public S.UsePromo UsePromo;
        [Key(30)]
        public S.SendServerMessage SendServerMessage;
        [Key(31)]
        public S.ClearServerMessagesInbox ClearServerMessagesInbox;
        [Key(32)]
        public S.GetServerMessagesInbox GetServerMessagesInbox;

        [Key(33)]
        public byte[] GameSpecificRequest;

        [Key(11)]
        public int SharedLogicVersion;
        [Key(18)]
        public int DefsVersion;
        [Key(15)]
        public int SerialNumber;

        [Key(34)]
        public LeagueRegisterRequest LeagueRegister;
        [Key(35)]
        public LeaguePlayerGlobalRankRequest LeaguePlayerGlobalRank;
        [Key(36)]
        public LeaguePlayerLeagueRankRequest LeaguePlayerLeagueRank;
        [Key(37)]
        public LeagueDivisionRanksRequest LeagueDivisionRanks;
        [Key(38)]
        public LeagueTopRequest LeagueTop;
        [Key(39)]
        public LeagueGlobalTopRequest LeagueGlobalTop;
        [Key(40)]
        public DefsRequest DefsRequest;
        [Key(41)]
        public DeleteUserRequest DeleteUserRequest;
        [Key(42)]
        public TypedApiCall GlobalConflictApiCall;
        [Key(43)]
        public TypedApiCall PlayerProfile;
        [Key(44)]
        public ValidateSessionRequest ValidateSessionRequest;

        [Key(45)]
        public LeaderboardGetSeasonInfoRequest LeaderboardGetSeasonInfoRequest;
        [Key(46)]
        public ExtensionModuleRequest ExtensionModuleRequest;
        [Key(47)]
        public ExtensionModuleRequest ExtensionModuleAsyncRequest;
    }

    [MessagePackObject()]
    public class ExtensionModuleRequest
    {
        [Key(0)]
        public string ModuleType;

        [Key(1)]
        public byte[] Request;
    }

    [MessagePackObject()]
    public class ValidateSessionRequest
    {
    }

    [MessagePackObject()]
    public class TypedApiCall
    {
        [Key(0)]
        public int Type;
        [Key(1)]
        public byte[] Data;
    }

    [MessagePackObject]
    public class DeleteUserRequest {}

    public enum PromoCodeResponseCode
    {
        LIMIT_MAX = 1,
        ALREADY = 2,
        ACTIVATED = 3,
        NO_PROMO =  4
    }

    [MessagePackObject]
    public class DefsRequest { }

    [MessagePackObject]
    public class UsePromo
    {
        [Key(1)]
        public string Promo;
    }

    [MessagePackObject]
    public class UsePromoResponse
    {
        [Key(1)]
        public PromoCodeResponseCode PromoResponseCode;

        [Key(2)]
        public string Function;

        [Key(3)]
        public string Parameter;

        [Key(4)]
        public string PromoCode;
    }

    [MessagePackObject]
    public class SendMessageResponse
    {
        [Key(1)]
        public bool MessageSent;
    }
    
    [MessagePackObject]
    public class SendServerMessage
    {
        [Key(1)]
        public ServerMessage Message;
    }

    [MessagePackObject]
    public class ServerMessage
    {
        [Key(1)]
        public byte[] Data;

        [Key(2)]
        public string MessageType;
    }

    [MessagePackObject]
    public class SendServerMessageResponse
    {
        [Key(1)]
        public bool MessageSent;
    }

    [MessagePackObject]
    public class GetServerMessagesInbox
    {
        [Key(1)]
        public bool GetBroadcasts;
        [Key(2)]
        public long LastSeenBroadcastSerialId;
        [Key(3)]
        public string SystemLanguage;
        [Key(4)]
        public int Sequence;
        [Key(5)]
        public long StateBirthday;
    }
    
    [MessagePackObject]
    public class ServerMessagesInbox
    {
        [Key(1)]
        public List<ServerMessage> Messages = new List<ServerMessage>();
    }

    [MessagePackObject]
    public class ClearServerMessagesInbox {}

    [MessagePackObject]
    public class InitRequest
    {
        [Obsolete("не используется больше на сервере, нет нужды заполнять")] [Key(1)]
        public byte[] State;
    }

    [MessagePackObject]
    public class ReplaceStateRequest
    {
        [Key(1)]
        public byte[] State;
    }

    [MessagePackObject]
    public class ProcessCommandsBatchRequest
    {
        [Key(2)]
        public int hashCode;
        [Key(3)]
        public int CommandCount;
        [Key(4)]
        public bool IsEditor;
        [Key(5)]
        public bool Integrity;
    }

    [MessagePackObject]
    public class SyncTime {}

    [MessagePackObject]
    public class ResetRequest {}

    [MessagePackObject]
    public class SetFacebookIdRequest {
        [Key(1)]
        public string FacebookId;
        [Key(2)]
        public bool Forced;
    }

    [MessagePackObject]
    public class GetProfileByFacebookIdRequest {
        [Key(2)]
        public string FacebookId;
        [Key(6)]
        public byte[] PlayerId;
    }

    [MessagePackObject]
    public class GetRandomUserIds
    {
        [Key(1)]
        public List<string> IgnoreFacebookIds;
        [Key(2)]
        public List<byte[]> IgnorePlayerIds;
    }

    [MessagePackObject]
    public class RegisterPayment
    {
        [Key(2)]
        public string userName;
        [Key(3)]
        public float paymentAmountOut;
        [Key(4)]
        public float paymentAmountLocal;
        [Key(5)]
        public string paymentCurrency;
    }

    [MessagePackObject]
    public class SendFeedback
    {
        [Key(1)]
        public string email;
        [Key(2)]
        public string caption;
        [Key(3)]
        public string description;
    }

    [MessagePackObject]
    public class LeaderboardRegisterRecord
    {
        [Key(1)]
        public double Score;
        [Key(2)]
        public string Type;
        [Key(3)]
        public string Name;
        [Key(4)]
        public string FacebookId;
		[Key(5)]
		public bool Add;
    }

    [MessagePackObject]
    public class LeaderboardGetRank
    {
        [Key(1)]
        public string Type;
    }

    [MessagePackObject]
    public class LeaderboardGetRankTop
    {
        [Key(1)]
        public string Type;

        [Key(2)]
        public int Amount;

        [Key(3)]
        public int From;

        [Key(4)]
        public int To;
    }

    [MessagePackObject]
    public class LeaderboardGetRankTopChunk
    {
        [Key(1)]
        public string Type;

        [Key(2)]
        public int LeagueIndex;

        [Key(3)]
        public bool UseLeagueIndex;
    }

    [MessagePackObject]
    public class LeaderboardGetFacebookFriendsTop
    {
        [Key(1)]
        public List<string> Friends;

        [Key(2)]
        public string Type;
    }

    [MessagePackObject]
    public class Response
    {
        [Key(1)]
        public S.ResponseCode ResponseCode;
        [Key(2)]
        public string ServerStackTrace;
        [Key(3)]
        public byte[] PlayerId;
        [Key(4)]
        public long Timestamp;
        //[Key(5)]
        //public int RequestSerialNumber;
        [Key(6)]
        public S.DefsData DefsData;
        [Key(7)]
        public long MaintenanceTimestamp;
        [Key(8)]
        public byte[] Inbox;
        [Key(9)]
        public bool Banned;
        [Key(10)]
        public string DebugInfo;
        [Key(11)]
        public long SessionId;
        [Key(12)]
        public byte[] ActualUserProfile;
        [Key(13)]
        public byte[] Token;
        [Key(14)]
        public int ShortId;
    }

    [MessagePackObject]
    public class SetFacebookIdResponse
    {
        [Key(1)]
        public bool Success;
        [Key(2)]
        public byte[] NewPlayerId;
    }

    [MessagePackObject]
    public class GetRandomUserIdsResponse
    {
        [Key(1)]
        public List<int> UserIds;
    }

    [MessagePackObject]
    public class GetProfileByFacebookIdResponse
    {
        [Key(1)]
        public string FacebookId;
        [Key(2)]
        public byte[] Profile;
        [Key(3)]
        public GetProfileByFacebookIdCode ResultCode;
    }

    public enum GetProfileByFacebookIdCode
    {
        OK = 0,
        FACEBOOK_ID_NOT_FOUND = 1,
        USER_PROGRESS_NOT_FOUND = 2
    }

    [MessagePackObject]
    public class DefsData
    {
        [Key(1)]
        public  List<string> PageName = new List<string>();
        [Key(2)]
        public  List<string> Json = new List<string>();
        [Key(3)]
        public int Version;
    }

    [MessagePackObject]
    public class LeaderboardGetRankResponse
    {
        [Key(1)]
        public int Rank;
        [Key(2)]
        public bool UserFound;
    }

    [MessagePackObject]
    public class LeaderboardGetSeasonInfoRequest { }

    [MessagePackObject]
    public class LeaderboardGetSeasonInfoResponse
    {
        [Key(1)]
        public string SeasonId;
        [Key(2)]
        public int SeasonIndex;
    }

    [MessagePackObject]
    public class LeaderboardGetRankTopResponse
    {
        [Key(1)]
        public  List<S.LeaderboardRecord> Records = new List<LeaderboardRecord>();
    }

    [MessagePackObject]
    public class LeaderboardRecord
    {
        [Key(1)]
        public int Rank;
        [Key(2)]
        public string Name;
        [Key(3)]
        public int Score;
        [Key(4)]
        public byte[] UserId;
        [Key(5)]
        public string FacebookId;
    }

    [MessagePackObject]
    public class LeaderboardGetRankTopChunkResponse
    {
        [Key(1)]
        public  List<S.LeaderboardRecord> Records = new List<LeaderboardRecord>();
    }

    public enum MatchResult
    {
        None,
        Win,
        Lose,
        Draw
    }

    [MessagePackObject]
    public class MatchEnd
    {
        [Key(0)]
        public string MatchId;
        [Key(2)]
        public MatchResult Result;
        [Key(3)]
        public Dictionary<string,string> Extra;
    }

    public enum NetworkType
    {
        IOS_NETWORK = 0,
        VK_NETWORK = 1,
        OK_NETWORK = 2,
        MM_NETWORK = 3,
        UNDEFINED_NETWORK = 4,
        ANDROID_NETWORK = 5,
        VK_DEBUG = 6
    }

    public enum ResponseCode
    {
        UNDEFINED_RESPONSE = 1,
        OK = 2,
        HASH_CHECK_FAILED = 3,
        COMMAND_NOT_FOUND = 4,
        BAD_SIGNATURE = 5,
        SERVER_EXCEPTION = 6,
        EMPTY_REQUEST = 7,
        INCOMPLETE_REQUEST = 8,
        WRONG_CLIENT_VERSION = 9,
        INVALID_COMMAND_COUNTER = 10,
        WRONG_COMMAND_ORDER = 11,
        LOST_COMMANDS = 12,
        UNKNOWN_SHARED_LOGIC_EXCEPTION = 13,
        WRONG_REQUEST_SERIAL_NUMBER = 14,
        SERVER_MAINTENANCE = 15,
        USER_STATE_SIZE_OVERFLOW = 16,
        IVALID_RECEIPT = 17,
        WRONG_SESSION = 18,
        BANNED = 19,
        WRONG_REQUEST_DATA = 20,
        EMPTY_SOCIAL_ID = 21,
        UNSUPPORTED_COMMANDS = 22
    }

    #region League.proto

    [MessagePackObject()]
    public class SeasonEndInfo
    {
        [Key(0)]
        public int Season;
        [Key(1)]
        public int GlobalPlace;
        [Key(2)]
        public int LeaguePlace;
        [Key(3)]
        public int DivisionPlace;
        [Key(4)]
        public int LeagueLevel;
        [Key(5)]
        public int LeagueChange; // -1 - понижена лига, 1 - повышена лига, 0 - лига неизменилась
        [Key(6)]
        public long Score;
    }

    [MessagePackObject()]
    public class LeaguePlayerInfo
    {
        [Key(1)]
        public Guid PlayerId;
        [Key(2)]
        public string FacebookId;
        [Key(3)]
        public long Score;
        [Key(4)]
        public int Season;
        [Key(5)]
        public int LeagueLevel;
        [Key(6)]
        public Guid DivisionId;
        [Key(7)]
        public List<SeasonEndInfo> UnclaimedRewards;
        [Key(8)]
        public string Name;
        [Key(9)]
        public bool IsBot;
        [Key(10)]
        public long BestScore;
    }

    [MessagePackObject()]
    public class DivisionInfo
    {
        [Key(0)]
        public Guid Id;
        [Key(1)]
        public int LeagueLevel;
        [Key(2)]
        public int Population;
        [Key(3)]
        public int Season;
        [Key(4)]
        public int BotsCount;
    }

    [MessagePackObject()]
    public class LeagueRegisterRequest
    {
        [Key(0)] public Guid PlayerId;
        [Key(1)] public string Name;
        [Key(2)] public string FacebookId;
    }

    [MessagePackObject()]
    public class LeaguePlayerGlobalRankRequest
    {
        [Key(0)] public Guid PlayerId;
    }

    [MessagePackObject()]
    public class LeaguePlayerLeagueRankRequest
    {
        [Key(0)] public Guid PlayerId;
    }

    [MessagePackObject()]
    public class LeagueDivisionRanksRequest
    {
        [Key(0)] public Guid PlayerId;
    }

    [MessagePackObject()]
    public class LeagueTopRequest
    {
        [Key(0)] public Guid PlayerId;
        [Key(1)] public int Amount;
    }

    [MessagePackObject()]
    public class LeagueGlobalTopRequest
    {
        [Key(0)] public Guid PlayerId;
        [Key(1)] public int Amount;
    }

    [MessagePackObject]
    public class LeagueRegisterResponse
    {
        [Key(0)] public LeaguePlayerInfo PlayerInfo;
        [Key(1)] public int CurrentSeason;
        [Key(2)] public DateTime SeasonStart;
        [Key(3)] public DateTime SeasonEnd;
    }

    [MessagePackObject]
    public class LeagueTopResponse
    {
        [Key(0)]
        public LeaguePlayerInfo[] Ranks;
        [Key(1)]
        public int PlayerRank;
    }

    #endregion

}