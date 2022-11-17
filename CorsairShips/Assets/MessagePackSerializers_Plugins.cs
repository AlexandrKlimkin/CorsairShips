#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace MessagePack.Resolvers
{
    using System;
    using MessagePack;

    public class GeneratedResolverPlugins : global::MessagePack.IFormatterResolver
    {
        public static readonly global::MessagePack.IFormatterResolver Instance = new GeneratedResolverPlugins();

        GeneratedResolverPlugins()
        {

        }

        public global::MessagePack.Formatters.IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.formatter;
        }

        static class FormatterCache<T>
        {
            public static readonly global::MessagePack.Formatters.IMessagePackFormatter<T> formatter;

            static FormatterCache()
            {
                var f = GeneratedResolverPluginsGetFormatterHelper.GetFormatter(typeof(T));
                if (f != null)
                {
                    formatter = (global::MessagePack.Formatters.IMessagePackFormatter<T>)f;
                }
            }
        }
    }

    internal static class GeneratedResolverPluginsGetFormatterHelper
    {
        static readonly global::System.Collections.Generic.Dictionary<Type, int> lookup;

        static GeneratedResolverPluginsGetFormatterHelper()
        {
            lookup = new global::System.Collections.Generic.Dictionary<Type, int>(117)
            {
                {typeof(global::System.Collections.Generic.Dictionary<string, byte[]>), 0 },
                {typeof(global::System.Collections.Generic.List<string>), 1 },
                {typeof(global::System.Collections.Generic.List<byte[]>), 2 },
                {typeof(global::System.Collections.Generic.List<global::S.ServerMessage>), 3 },
                {typeof(global::System.Collections.Generic.List<int>), 4 },
                {typeof(global::System.Collections.Generic.List<global::S.LeaderboardRecord>), 5 },
                {typeof(global::System.Collections.Generic.Dictionary<string, string>), 6 },
                {typeof(global::System.Collections.Generic.List<global::S.SeasonEndInfo>), 7 },
                {typeof(global::S.LeaguePlayerInfo[]), 8 },
                {typeof(global::System.Collections.Generic.List<global::ServerShared.PlayerProfile.Achievement>), 9 },
                {typeof(global::System.Collections.Generic.Dictionary<string, long>), 10 },
                {typeof(global::ServerShared.PlayerProfile.ProfileDTO[]), 11 },
                {typeof(global::System.Collections.Generic.List<global::S.ConflictResultPoints>), 12 },
                {typeof(global::System.Collections.Generic.Dictionary<string, int>), 13 },
                {typeof(global::ServerShared.GlobalConflict.NodeState[]), 14 },
                {typeof(global::ServerShared.GlobalConflict.PointOfInterestBonus[]), 15 },
                {typeof(global::ServerShared.GlobalConflict.DonationBonus[]), 16 },
                {typeof(global::ServerShared.GlobalConflict.DonationBonusLevels[]), 17 },
                {typeof(global::ServerShared.GlobalConflict.StageDef[]), 18 },
                {typeof(global::ServerShared.GlobalConflict.PointOfInterest[]), 19 },
                {typeof(global::ServerShared.GlobalConflict.NodeQuest[]), 20 },
                {typeof(global::ServerShared.GlobalConflict.TeamState[]), 21 },
                {typeof(global::System.Collections.Generic.List<global::PestelLib.SharedLogicBase.Command>), 22 },
                {typeof(global::S.NetworkType), 23 },
                {typeof(global::S.PromoCodeResponseCode), 24 },
                {typeof(global::S.ResponseCode), 25 },
                {typeof(global::S.GetProfileByFacebookIdCode), 26 },
                {typeof(global::S.MatchResult), 27 },
                {typeof(global::ServerShared.GlobalConflict.DonationBonusType), 28 },
                {typeof(global::ServerShared.GlobalConflict.NodeStatus), 29 },
                {typeof(global::ServerShared.GlobalConflict.StageType), 30 },
                {typeof(global::ServerShared.GlobalConflict.PointOfInterestServerLogic), 31 },
                {typeof(global::ServerShared.GlobalConflict.TeamAssignType), 32 },
                {typeof(global::ServerShared.PlayerProfile.Achievement), 33 },
                {typeof(global::S.UnityPurchaseReceipt), 34 },
                {typeof(global::PestelLib.SharedLogic.Modules.CompositeModuleState), 35 },
                {typeof(global::S.InitRequest), 36 },
                {typeof(global::S.ProcessCommandsBatchRequest), 37 },
                {typeof(global::S.ResetRequest), 38 },
                {typeof(global::S.SetFacebookIdRequest), 39 },
                {typeof(global::S.GetProfileByFacebookIdRequest), 40 },
                {typeof(global::S.GetRandomUserIds), 41 },
                {typeof(global::S.SyncTime), 42 },
                {typeof(global::S.ReplaceStateRequest), 43 },
                {typeof(global::S.RegisterPayment), 44 },
                {typeof(global::S.SendFeedback), 45 },
                {typeof(global::S.LeaderboardRegisterRecord), 46 },
                {typeof(global::S.LeaderboardGetRank), 47 },
                {typeof(global::S.LeaderboardGetRankTop), 48 },
                {typeof(global::S.LeaderboardGetRankTopChunk), 49 },
                {typeof(global::S.LeaderboardGetFacebookFriendsTop), 50 },
                {typeof(global::S.UsePromo), 51 },
                {typeof(global::S.ServerMessage), 52 },
                {typeof(global::S.SendServerMessage), 53 },
                {typeof(global::S.ClearServerMessagesInbox), 54 },
                {typeof(global::S.GetServerMessagesInbox), 55 },
                {typeof(global::S.LeagueRegisterRequest), 56 },
                {typeof(global::S.LeaguePlayerGlobalRankRequest), 57 },
                {typeof(global::S.LeaguePlayerLeagueRankRequest), 58 },
                {typeof(global::S.LeagueDivisionRanksRequest), 59 },
                {typeof(global::S.LeagueTopRequest), 60 },
                {typeof(global::S.LeagueGlobalTopRequest), 61 },
                {typeof(global::S.DefsRequest), 62 },
                {typeof(global::S.DeleteUserRequest), 63 },
                {typeof(global::S.TypedApiCall), 64 },
                {typeof(global::S.ValidateSessionRequest), 65 },
                {typeof(global::S.LeaderboardGetSeasonInfoRequest), 66 },
                {typeof(global::S.ExtensionModuleRequest), 67 },
                {typeof(global::S.Request), 68 },
                {typeof(global::PestelLib.ServerShared.ServerRequest), 69 },
                {typeof(global::S.BenchmarkRequest), 70 },
                {typeof(global::S.DataCollection), 71 },
                {typeof(global::S.UsePromoResponse), 72 },
                {typeof(global::S.SendMessageResponse), 73 },
                {typeof(global::S.SendServerMessageResponse), 74 },
                {typeof(global::S.ServerMessagesInbox), 75 },
                {typeof(global::S.DefsData), 76 },
                {typeof(global::S.Response), 77 },
                {typeof(global::S.SetFacebookIdResponse), 78 },
                {typeof(global::S.GetRandomUserIdsResponse), 79 },
                {typeof(global::S.GetProfileByFacebookIdResponse), 80 },
                {typeof(global::S.LeaderboardGetRankResponse), 81 },
                {typeof(global::S.LeaderboardGetSeasonInfoResponse), 82 },
                {typeof(global::S.LeaderboardRecord), 83 },
                {typeof(global::S.LeaderboardGetRankTopResponse), 84 },
                {typeof(global::S.LeaderboardGetRankTopChunkResponse), 85 },
                {typeof(global::S.MatchEnd), 86 },
                {typeof(global::S.SeasonEndInfo), 87 },
                {typeof(global::S.LeaguePlayerInfo), 88 },
                {typeof(global::S.DivisionInfo), 89 },
                {typeof(global::S.LeagueRegisterResponse), 90 },
                {typeof(global::S.LeagueTopResponse), 91 },
                {typeof(global::S.MadId), 92 },
                {typeof(global::ServerShared.PlayerProfile.PlayerTitle), 93 },
                {typeof(global::S.CommandContainer), 94 },
                {typeof(global::ServerShared.PlayerProfile.ProfileDTO), 95 },
                {typeof(global::ServerShared.PlayerProfile.ProfileDTOArray), 96 },
                {typeof(global::PestelLib.ServerShared.ServerResponse), 97 },
                {typeof(global::PestelLib.SharedLogic.Modules.EmptyState), 98 },
                {typeof(global::S.ConflictResultPoints), 99 },
                {typeof(global::S.ConflictResult), 100 },
                {typeof(global::ServerShared.GlobalConflict.TeamPlayersStat), 101 },
                {typeof(global::ServerShared.GlobalConflict.DonationBonusLevels), 102 },
                {typeof(global::ServerShared.GlobalConflict.DonationBonus), 103 },
                {typeof(global::ServerShared.GlobalConflict.NodeState), 104 },
                {typeof(global::ServerShared.GlobalConflict.MapState), 105 },
                {typeof(global::ServerShared.GlobalConflict.StageDef), 106 },
                {typeof(global::ServerShared.GlobalConflict.PointOfInterestBonus), 107 },
                {typeof(global::ServerShared.GlobalConflict.PointOfInterest), 108 },
                {typeof(global::ServerShared.GlobalConflict.NodeQuest), 109 },
                {typeof(global::ServerShared.GlobalConflict.PlayerState), 110 },
                {typeof(global::ServerShared.GlobalConflict.TeamState), 111 },
                {typeof(global::ServerShared.GlobalConflict.GlobalConflictState), 112 },
                {typeof(global::S.ModuleData), 113 },
                {typeof(global::PestelLib.SharedLogicBase.Command), 114 },
                {typeof(global::PestelLib.SharedLogicBase.CommandBatch), 115 },
                {typeof(global::S.UserProfile), 116 },
            };
        }

        internal static object GetFormatter(Type t)
        {
            int key;
            if (!lookup.TryGetValue(t, out key)) return null;

            switch (key)
            {
                case 0: return new global::MessagePack.Formatters.DictionaryFormatter<string, byte[]>();
                case 1: return new global::MessagePack.Formatters.ListFormatter<string>();
                case 2: return new global::MessagePack.Formatters.ListFormatter<byte[]>();
                case 3: return new global::MessagePack.Formatters.ListFormatter<global::S.ServerMessage>();
                case 4: return new global::MessagePack.Formatters.ListFormatter<int>();
                case 5: return new global::MessagePack.Formatters.ListFormatter<global::S.LeaderboardRecord>();
                case 6: return new global::MessagePack.Formatters.DictionaryFormatter<string, string>();
                case 7: return new global::MessagePack.Formatters.ListFormatter<global::S.SeasonEndInfo>();
                case 8: return new global::MessagePack.Formatters.ArrayFormatter<global::S.LeaguePlayerInfo>();
                case 9: return new global::MessagePack.Formatters.ListFormatter<global::ServerShared.PlayerProfile.Achievement>();
                case 10: return new global::MessagePack.Formatters.DictionaryFormatter<string, long>();
                case 11: return new global::MessagePack.Formatters.ArrayFormatter<global::ServerShared.PlayerProfile.ProfileDTO>();
                case 12: return new global::MessagePack.Formatters.ListFormatter<global::S.ConflictResultPoints>();
                case 13: return new global::MessagePack.Formatters.DictionaryFormatter<string, int>();
                case 14: return new global::MessagePack.Formatters.ArrayFormatter<global::ServerShared.GlobalConflict.NodeState>();
                case 15: return new global::MessagePack.Formatters.ArrayFormatter<global::ServerShared.GlobalConflict.PointOfInterestBonus>();
                case 16: return new global::MessagePack.Formatters.ArrayFormatter<global::ServerShared.GlobalConflict.DonationBonus>();
                case 17: return new global::MessagePack.Formatters.ArrayFormatter<global::ServerShared.GlobalConflict.DonationBonusLevels>();
                case 18: return new global::MessagePack.Formatters.ArrayFormatter<global::ServerShared.GlobalConflict.StageDef>();
                case 19: return new global::MessagePack.Formatters.ArrayFormatter<global::ServerShared.GlobalConflict.PointOfInterest>();
                case 20: return new global::MessagePack.Formatters.ArrayFormatter<global::ServerShared.GlobalConflict.NodeQuest>();
                case 21: return new global::MessagePack.Formatters.ArrayFormatter<global::ServerShared.GlobalConflict.TeamState>();
                case 22: return new global::MessagePack.Formatters.ListFormatter<global::PestelLib.SharedLogicBase.Command>();
                case 23: return new MessagePack.Formatters.S.NetworkTypeFormatter();
                case 24: return new MessagePack.Formatters.S.PromoCodeResponseCodeFormatter();
                case 25: return new MessagePack.Formatters.S.ResponseCodeFormatter();
                case 26: return new MessagePack.Formatters.S.GetProfileByFacebookIdCodeFormatter();
                case 27: return new MessagePack.Formatters.S.MatchResultFormatter();
                case 28: return new MessagePack.Formatters.ServerShared.GlobalConflict.DonationBonusTypeFormatter();
                case 29: return new MessagePack.Formatters.ServerShared.GlobalConflict.NodeStatusFormatter();
                case 30: return new MessagePack.Formatters.ServerShared.GlobalConflict.StageTypeFormatter();
                case 31: return new MessagePack.Formatters.ServerShared.GlobalConflict.PointOfInterestServerLogicFormatter();
                case 32: return new MessagePack.Formatters.ServerShared.GlobalConflict.TeamAssignTypeFormatter();
                case 33: return new MessagePack.Formatters.ServerShared.PlayerProfile.AchievementFormatter();
                case 34: return new MessagePack.Formatters.S.UnityPurchaseReceiptFormatter();
                case 35: return new MessagePack.Formatters.PestelLib.SharedLogic.Modules.CompositeModuleStateFormatter();
                case 36: return new MessagePack.Formatters.S.InitRequestFormatter();
                case 37: return new MessagePack.Formatters.S.ProcessCommandsBatchRequestFormatter();
                case 38: return new MessagePack.Formatters.S.ResetRequestFormatter();
                case 39: return new MessagePack.Formatters.S.SetFacebookIdRequestFormatter();
                case 40: return new MessagePack.Formatters.S.GetProfileByFacebookIdRequestFormatter();
                case 41: return new MessagePack.Formatters.S.GetRandomUserIdsFormatter();
                case 42: return new MessagePack.Formatters.S.SyncTimeFormatter();
                case 43: return new MessagePack.Formatters.S.ReplaceStateRequestFormatter();
                case 44: return new MessagePack.Formatters.S.RegisterPaymentFormatter();
                case 45: return new MessagePack.Formatters.S.SendFeedbackFormatter();
                case 46: return new MessagePack.Formatters.S.LeaderboardRegisterRecordFormatter();
                case 47: return new MessagePack.Formatters.S.LeaderboardGetRankFormatter();
                case 48: return new MessagePack.Formatters.S.LeaderboardGetRankTopFormatter();
                case 49: return new MessagePack.Formatters.S.LeaderboardGetRankTopChunkFormatter();
                case 50: return new MessagePack.Formatters.S.LeaderboardGetFacebookFriendsTopFormatter();
                case 51: return new MessagePack.Formatters.S.UsePromoFormatter();
                case 52: return new MessagePack.Formatters.S.ServerMessageFormatter();
                case 53: return new MessagePack.Formatters.S.SendServerMessageFormatter();
                case 54: return new MessagePack.Formatters.S.ClearServerMessagesInboxFormatter();
                case 55: return new MessagePack.Formatters.S.GetServerMessagesInboxFormatter();
                case 56: return new MessagePack.Formatters.S.LeagueRegisterRequestFormatter();
                case 57: return new MessagePack.Formatters.S.LeaguePlayerGlobalRankRequestFormatter();
                case 58: return new MessagePack.Formatters.S.LeaguePlayerLeagueRankRequestFormatter();
                case 59: return new MessagePack.Formatters.S.LeagueDivisionRanksRequestFormatter();
                case 60: return new MessagePack.Formatters.S.LeagueTopRequestFormatter();
                case 61: return new MessagePack.Formatters.S.LeagueGlobalTopRequestFormatter();
                case 62: return new MessagePack.Formatters.S.DefsRequestFormatter();
                case 63: return new MessagePack.Formatters.S.DeleteUserRequestFormatter();
                case 64: return new MessagePack.Formatters.S.TypedApiCallFormatter();
                case 65: return new MessagePack.Formatters.S.ValidateSessionRequestFormatter();
                case 66: return new MessagePack.Formatters.S.LeaderboardGetSeasonInfoRequestFormatter();
                case 67: return new MessagePack.Formatters.S.ExtensionModuleRequestFormatter();
                case 68: return new MessagePack.Formatters.S.RequestFormatter();
                case 69: return new MessagePack.Formatters.PestelLib.ServerShared.ServerRequestFormatter();
                case 70: return new MessagePack.Formatters.S.BenchmarkRequestFormatter();
                case 71: return new MessagePack.Formatters.S.DataCollectionFormatter();
                case 72: return new MessagePack.Formatters.S.UsePromoResponseFormatter();
                case 73: return new MessagePack.Formatters.S.SendMessageResponseFormatter();
                case 74: return new MessagePack.Formatters.S.SendServerMessageResponseFormatter();
                case 75: return new MessagePack.Formatters.S.ServerMessagesInboxFormatter();
                case 76: return new MessagePack.Formatters.S.DefsDataFormatter();
                case 77: return new MessagePack.Formatters.S.ResponseFormatter();
                case 78: return new MessagePack.Formatters.S.SetFacebookIdResponseFormatter();
                case 79: return new MessagePack.Formatters.S.GetRandomUserIdsResponseFormatter();
                case 80: return new MessagePack.Formatters.S.GetProfileByFacebookIdResponseFormatter();
                case 81: return new MessagePack.Formatters.S.LeaderboardGetRankResponseFormatter();
                case 82: return new MessagePack.Formatters.S.LeaderboardGetSeasonInfoResponseFormatter();
                case 83: return new MessagePack.Formatters.S.LeaderboardRecordFormatter();
                case 84: return new MessagePack.Formatters.S.LeaderboardGetRankTopResponseFormatter();
                case 85: return new MessagePack.Formatters.S.LeaderboardGetRankTopChunkResponseFormatter();
                case 86: return new MessagePack.Formatters.S.MatchEndFormatter();
                case 87: return new MessagePack.Formatters.S.SeasonEndInfoFormatter();
                case 88: return new MessagePack.Formatters.S.LeaguePlayerInfoFormatter();
                case 89: return new MessagePack.Formatters.S.DivisionInfoFormatter();
                case 90: return new MessagePack.Formatters.S.LeagueRegisterResponseFormatter();
                case 91: return new MessagePack.Formatters.S.LeagueTopResponseFormatter();
                case 92: return new MessagePack.Formatters.S.MadIdFormatter();
                case 93: return new MessagePack.Formatters.ServerShared.PlayerProfile.PlayerTitleFormatter();
                case 94: return new MessagePack.Formatters.S.CommandContainerFormatter();
                case 95: return new MessagePack.Formatters.ServerShared.PlayerProfile.ProfileDTOFormatter();
                case 96: return new MessagePack.Formatters.ServerShared.PlayerProfile.ProfileDTOArrayFormatter();
                case 97: return new MessagePack.Formatters.PestelLib.ServerShared.ServerResponseFormatter();
                case 98: return new MessagePack.Formatters.PestelLib.SharedLogic.Modules.EmptyStateFormatter();
                case 99: return new MessagePack.Formatters.S.ConflictResultPointsFormatter();
                case 100: return new MessagePack.Formatters.S.ConflictResultFormatter();
                case 101: return new MessagePack.Formatters.ServerShared.GlobalConflict.TeamPlayersStatFormatter();
                case 102: return new MessagePack.Formatters.ServerShared.GlobalConflict.DonationBonusLevelsFormatter();
                case 103: return new MessagePack.Formatters.ServerShared.GlobalConflict.DonationBonusFormatter();
                case 104: return new MessagePack.Formatters.ServerShared.GlobalConflict.NodeStateFormatter();
                case 105: return new MessagePack.Formatters.ServerShared.GlobalConflict.MapStateFormatter();
                case 106: return new MessagePack.Formatters.ServerShared.GlobalConflict.StageDefFormatter();
                case 107: return new MessagePack.Formatters.ServerShared.GlobalConflict.PointOfInterestBonusFormatter();
                case 108: return new MessagePack.Formatters.ServerShared.GlobalConflict.PointOfInterestFormatter();
                case 109: return new MessagePack.Formatters.ServerShared.GlobalConflict.NodeQuestFormatter();
                case 110: return new MessagePack.Formatters.ServerShared.GlobalConflict.PlayerStateFormatter();
                case 111: return new MessagePack.Formatters.ServerShared.GlobalConflict.TeamStateFormatter();
                case 112: return new MessagePack.Formatters.ServerShared.GlobalConflict.GlobalConflictStateFormatter();
                case 113: return new MessagePack.Formatters.S.ModuleDataFormatter();
                case 114: return new MessagePack.Formatters.PestelLib.SharedLogicBase.CommandFormatter();
                case 115: return new MessagePack.Formatters.PestelLib.SharedLogicBase.CommandBatchFormatter();
                case 116: return new MessagePack.Formatters.S.UserProfileFormatter();
                default: return null;
            }
        }
    }
}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace MessagePack.Formatters.S
{
    using System;
    using MessagePack;

    public sealed class NetworkTypeFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.NetworkType>
    {
        public int Serialize(ref byte[] bytes, int offset, global::S.NetworkType value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt32(ref bytes, offset, (Int32)value);
        }
        
        public global::S.NetworkType Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            return (global::S.NetworkType)MessagePackBinary.ReadInt32(bytes, offset, out readSize);
        }
    }

    public sealed class PromoCodeResponseCodeFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.PromoCodeResponseCode>
    {
        public int Serialize(ref byte[] bytes, int offset, global::S.PromoCodeResponseCode value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt32(ref bytes, offset, (Int32)value);
        }
        
        public global::S.PromoCodeResponseCode Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            return (global::S.PromoCodeResponseCode)MessagePackBinary.ReadInt32(bytes, offset, out readSize);
        }
    }

    public sealed class ResponseCodeFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.ResponseCode>
    {
        public int Serialize(ref byte[] bytes, int offset, global::S.ResponseCode value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt32(ref bytes, offset, (Int32)value);
        }
        
        public global::S.ResponseCode Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            return (global::S.ResponseCode)MessagePackBinary.ReadInt32(bytes, offset, out readSize);
        }
    }

    public sealed class GetProfileByFacebookIdCodeFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.GetProfileByFacebookIdCode>
    {
        public int Serialize(ref byte[] bytes, int offset, global::S.GetProfileByFacebookIdCode value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt32(ref bytes, offset, (Int32)value);
        }
        
        public global::S.GetProfileByFacebookIdCode Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            return (global::S.GetProfileByFacebookIdCode)MessagePackBinary.ReadInt32(bytes, offset, out readSize);
        }
    }

    public sealed class MatchResultFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.MatchResult>
    {
        public int Serialize(ref byte[] bytes, int offset, global::S.MatchResult value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt32(ref bytes, offset, (Int32)value);
        }
        
        public global::S.MatchResult Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            return (global::S.MatchResult)MessagePackBinary.ReadInt32(bytes, offset, out readSize);
        }
    }


}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace MessagePack.Formatters.ServerShared.GlobalConflict
{
    using System;
    using MessagePack;

    public sealed class DonationBonusTypeFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::ServerShared.GlobalConflict.DonationBonusType>
    {
        public int Serialize(ref byte[] bytes, int offset, global::ServerShared.GlobalConflict.DonationBonusType value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt32(ref bytes, offset, (Int32)value);
        }
        
        public global::ServerShared.GlobalConflict.DonationBonusType Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            return (global::ServerShared.GlobalConflict.DonationBonusType)MessagePackBinary.ReadInt32(bytes, offset, out readSize);
        }
    }

    public sealed class NodeStatusFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::ServerShared.GlobalConflict.NodeStatus>
    {
        public int Serialize(ref byte[] bytes, int offset, global::ServerShared.GlobalConflict.NodeStatus value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt32(ref bytes, offset, (Int32)value);
        }
        
        public global::ServerShared.GlobalConflict.NodeStatus Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            return (global::ServerShared.GlobalConflict.NodeStatus)MessagePackBinary.ReadInt32(bytes, offset, out readSize);
        }
    }

    public sealed class StageTypeFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::ServerShared.GlobalConflict.StageType>
    {
        public int Serialize(ref byte[] bytes, int offset, global::ServerShared.GlobalConflict.StageType value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt32(ref bytes, offset, (Int32)value);
        }
        
        public global::ServerShared.GlobalConflict.StageType Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            return (global::ServerShared.GlobalConflict.StageType)MessagePackBinary.ReadInt32(bytes, offset, out readSize);
        }
    }

    public sealed class PointOfInterestServerLogicFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::ServerShared.GlobalConflict.PointOfInterestServerLogic>
    {
        public int Serialize(ref byte[] bytes, int offset, global::ServerShared.GlobalConflict.PointOfInterestServerLogic value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt32(ref bytes, offset, (Int32)value);
        }
        
        public global::ServerShared.GlobalConflict.PointOfInterestServerLogic Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            return (global::ServerShared.GlobalConflict.PointOfInterestServerLogic)MessagePackBinary.ReadInt32(bytes, offset, out readSize);
        }
    }

    public sealed class TeamAssignTypeFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::ServerShared.GlobalConflict.TeamAssignType>
    {
        public int Serialize(ref byte[] bytes, int offset, global::ServerShared.GlobalConflict.TeamAssignType value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt32(ref bytes, offset, (Int32)value);
        }
        
        public global::ServerShared.GlobalConflict.TeamAssignType Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            return (global::ServerShared.GlobalConflict.TeamAssignType)MessagePackBinary.ReadInt32(bytes, offset, out readSize);
        }
    }


}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612


#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace MessagePack.Formatters.ServerShared.PlayerProfile
{
    using System;
    using MessagePack;


    public sealed class AchievementFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::ServerShared.PlayerProfile.Achievement>
    {

        public int Serialize(ref byte[] bytes, int offset, global::ServerShared.PlayerProfile.Achievement value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 3);
            offset += formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Serialize(ref bytes, offset, value.Time, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.DefId, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.SlotId);
            return offset - startOffset;
        }

        public global::ServerShared.PlayerProfile.Achievement Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Time__ = default(global::System.DateTime);
            var __DefId__ = default(string);
            var __SlotId__ = default(int);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __Time__ = formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __DefId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __SlotId__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::ServerShared.PlayerProfile.Achievement();
            ____result.Time = __Time__;
            ____result.DefId = __DefId__;
            ____result.SlotId = __SlotId__;
            return ____result;
        }
    }


    public sealed class PlayerTitleFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::ServerShared.PlayerProfile.PlayerTitle>
    {

        public int Serialize(ref byte[] bytes, int offset, global::ServerShared.PlayerProfile.PlayerTitle value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Serialize(ref bytes, offset, value.Time, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.DefId, formatterResolver);
            return offset - startOffset;
        }

        public global::ServerShared.PlayerProfile.PlayerTitle Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Time__ = default(global::System.DateTime);
            var __DefId__ = default(string);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __Time__ = formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __DefId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::ServerShared.PlayerProfile.PlayerTitle();
            ____result.Time = __Time__;
            ____result.DefId = __DefId__;
            return ____result;
        }
    }


    public sealed class ProfileDTOFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::ServerShared.PlayerProfile.ProfileDTO>
    {

        public int Serialize(ref byte[] bytes, int offset, global::ServerShared.PlayerProfile.ProfileDTO value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 15);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Guid>().Serialize(ref bytes, offset, value.PlayerId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Serialize(ref bytes, offset, value.UpdateTime, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::ServerShared.PlayerProfile.Achievement>>().Serialize(ref bytes, offset, value.Achievements, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::ServerShared.PlayerProfile.PlayerTitle>().Serialize(ref bytes, offset, value.Title, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<string, string>>().Serialize(ref bytes, offset, value.StrStats, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<string, long>>().Serialize(ref bytes, offset, value.IntStats, formatterResolver);
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value.IsBot);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Nick, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Avatar, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Country, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Level);
            offset += formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Serialize(ref bytes, offset, value.Expiry, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Guid>().Serialize(ref bytes, offset, value.CreatedBy, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Version);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<string, byte[]>>().Serialize(ref bytes, offset, value.BinStats, formatterResolver);
            return offset - startOffset;
        }

        public global::ServerShared.PlayerProfile.ProfileDTO Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __PlayerId__ = default(global::System.Guid);
            var __UpdateTime__ = default(global::System.DateTime);
            var __Achievements__ = default(global::System.Collections.Generic.List<global::ServerShared.PlayerProfile.Achievement>);
            var __Title__ = default(global::ServerShared.PlayerProfile.PlayerTitle);
            var __StrStats__ = default(global::System.Collections.Generic.Dictionary<string, string>);
            var __IntStats__ = default(global::System.Collections.Generic.Dictionary<string, long>);
            var __IsBot__ = default(bool);
            var __Nick__ = default(string);
            var __Avatar__ = default(string);
            var __Country__ = default(string);
            var __Level__ = default(int);
            var __Expiry__ = default(global::System.DateTime);
            var __CreatedBy__ = default(global::System.Guid);
            var __Version__ = default(int);
            var __BinStats__ = default(global::System.Collections.Generic.Dictionary<string, byte[]>);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __PlayerId__ = formatterResolver.GetFormatterWithVerify<global::System.Guid>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __UpdateTime__ = formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Achievements__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::ServerShared.PlayerProfile.Achievement>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __Title__ = formatterResolver.GetFormatterWithVerify<global::ServerShared.PlayerProfile.PlayerTitle>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __StrStats__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<string, string>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 5:
                        __IntStats__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<string, long>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 6:
                        __IsBot__ = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        break;
                    case 7:
                        __Nick__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 8:
                        __Avatar__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 9:
                        __Country__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 10:
                        __Level__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 11:
                        __Expiry__ = formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 12:
                        __CreatedBy__ = formatterResolver.GetFormatterWithVerify<global::System.Guid>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 13:
                        __Version__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 14:
                        __BinStats__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<string, byte[]>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::ServerShared.PlayerProfile.ProfileDTO();
            ____result.PlayerId = __PlayerId__;
            ____result.UpdateTime = __UpdateTime__;
            ____result.Achievements = __Achievements__;
            ____result.Title = __Title__;
            ____result.StrStats = __StrStats__;
            ____result.IntStats = __IntStats__;
            ____result.IsBot = __IsBot__;
            ____result.Nick = __Nick__;
            ____result.Avatar = __Avatar__;
            ____result.Country = __Country__;
            ____result.Level = __Level__;
            ____result.Expiry = __Expiry__;
            ____result.CreatedBy = __CreatedBy__;
            ____result.Version = __Version__;
            ____result.BinStats = __BinStats__;
            return ____result;
        }
    }


    public sealed class ProfileDTOArrayFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::ServerShared.PlayerProfile.ProfileDTOArray>
    {

        public int Serialize(ref byte[] bytes, int offset, global::ServerShared.PlayerProfile.ProfileDTOArray value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 1);
            offset += formatterResolver.GetFormatterWithVerify<global::ServerShared.PlayerProfile.ProfileDTO[]>().Serialize(ref bytes, offset, value.Array, formatterResolver);
            return offset - startOffset;
        }

        public global::ServerShared.PlayerProfile.ProfileDTOArray Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Array__ = default(global::ServerShared.PlayerProfile.ProfileDTO[]);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __Array__ = formatterResolver.GetFormatterWithVerify<global::ServerShared.PlayerProfile.ProfileDTO[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::ServerShared.PlayerProfile.ProfileDTOArray();
            ____result.Array = __Array__;
            return ____result;
        }
    }

}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace MessagePack.Formatters.S
{
    using System;
    using MessagePack;


    public sealed class UnityPurchaseReceiptFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.UnityPurchaseReceipt>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.UnityPurchaseReceipt value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 4);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Store, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.TransactionID, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Payload, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.DeveloperPayload, formatterResolver);
            return offset - startOffset;
        }

        public global::S.UnityPurchaseReceipt Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Store__ = default(string);
            var __TransactionID__ = default(string);
            var __Payload__ = default(string);
            var __DeveloperPayload__ = default(string);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __Store__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __TransactionID__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Payload__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __DeveloperPayload__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.UnityPurchaseReceipt();
            ____result.Store = __Store__;
            ____result.TransactionID = __TransactionID__;
            ____result.Payload = __Payload__;
            ____result.DeveloperPayload = __DeveloperPayload__;
            return ____result;
        }
    }


    public sealed class InitRequestFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.InitRequest>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.InitRequest value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.State, formatterResolver);
            return offset - startOffset;
        }

        public global::S.InitRequest Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __State__ = default(byte[]);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __State__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.InitRequest();
            ____result.State = __State__;
            return ____result;
        }
    }


    public sealed class ProcessCommandsBatchRequestFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.ProcessCommandsBatchRequest>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.ProcessCommandsBatchRequest value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 6);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.hashCode);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.CommandCount);
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value.IsEditor);
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value.Integrity);
            return offset - startOffset;
        }

        public global::S.ProcessCommandsBatchRequest Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __hashCode__ = default(int);
            var __CommandCount__ = default(int);
            var __IsEditor__ = default(bool);
            var __Integrity__ = default(bool);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 2:
                        __hashCode__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 3:
                        __CommandCount__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 4:
                        __IsEditor__ = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        break;
                    case 5:
                        __Integrity__ = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.ProcessCommandsBatchRequest();
            ____result.hashCode = __hashCode__;
            ____result.CommandCount = __CommandCount__;
            ____result.IsEditor = __IsEditor__;
            ____result.Integrity = __Integrity__;
            return ____result;
        }
    }


    public sealed class ResetRequestFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.ResetRequest>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.ResetRequest value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 0);
            return offset - startOffset;
        }

        public global::S.ResetRequest Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;


            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.ResetRequest();
            return ____result;
        }
    }


    public sealed class SetFacebookIdRequestFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.SetFacebookIdRequest>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.SetFacebookIdRequest value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 3);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.FacebookId, formatterResolver);
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value.Forced);
            return offset - startOffset;
        }

        public global::S.SetFacebookIdRequest Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __FacebookId__ = default(string);
            var __Forced__ = default(bool);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __FacebookId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Forced__ = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.SetFacebookIdRequest();
            ____result.FacebookId = __FacebookId__;
            ____result.Forced = __Forced__;
            return ____result;
        }
    }


    public sealed class GetProfileByFacebookIdRequestFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.GetProfileByFacebookIdRequest>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.GetProfileByFacebookIdRequest value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 7);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.FacebookId, formatterResolver);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.PlayerId, formatterResolver);
            return offset - startOffset;
        }

        public global::S.GetProfileByFacebookIdRequest Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __FacebookId__ = default(string);
            var __PlayerId__ = default(byte[]);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 2:
                        __FacebookId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 6:
                        __PlayerId__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.GetProfileByFacebookIdRequest();
            ____result.FacebookId = __FacebookId__;
            ____result.PlayerId = __PlayerId__;
            return ____result;
        }
    }


    public sealed class GetRandomUserIdsFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.GetRandomUserIds>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.GetRandomUserIds value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 3);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<string>>().Serialize(ref bytes, offset, value.IgnoreFacebookIds, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<byte[]>>().Serialize(ref bytes, offset, value.IgnorePlayerIds, formatterResolver);
            return offset - startOffset;
        }

        public global::S.GetRandomUserIds Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __IgnoreFacebookIds__ = default(global::System.Collections.Generic.List<string>);
            var __IgnorePlayerIds__ = default(global::System.Collections.Generic.List<byte[]>);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __IgnoreFacebookIds__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<string>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __IgnorePlayerIds__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<byte[]>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.GetRandomUserIds();
            ____result.IgnoreFacebookIds = __IgnoreFacebookIds__;
            ____result.IgnorePlayerIds = __IgnorePlayerIds__;
            return ____result;
        }
    }


    public sealed class SyncTimeFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.SyncTime>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.SyncTime value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 0);
            return offset - startOffset;
        }

        public global::S.SyncTime Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;


            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.SyncTime();
            return ____result;
        }
    }


    public sealed class ReplaceStateRequestFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.ReplaceStateRequest>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.ReplaceStateRequest value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.State, formatterResolver);
            return offset - startOffset;
        }

        public global::S.ReplaceStateRequest Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __State__ = default(byte[]);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __State__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.ReplaceStateRequest();
            ____result.State = __State__;
            return ____result;
        }
    }


    public sealed class RegisterPaymentFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.RegisterPayment>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.RegisterPayment value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 6);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.userName, formatterResolver);
            offset += MessagePackBinary.WriteSingle(ref bytes, offset, value.paymentAmountOut);
            offset += MessagePackBinary.WriteSingle(ref bytes, offset, value.paymentAmountLocal);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.paymentCurrency, formatterResolver);
            return offset - startOffset;
        }

        public global::S.RegisterPayment Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __userName__ = default(string);
            var __paymentAmountOut__ = default(float);
            var __paymentAmountLocal__ = default(float);
            var __paymentCurrency__ = default(string);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 2:
                        __userName__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __paymentAmountOut__ = MessagePackBinary.ReadSingle(bytes, offset, out readSize);
                        break;
                    case 4:
                        __paymentAmountLocal__ = MessagePackBinary.ReadSingle(bytes, offset, out readSize);
                        break;
                    case 5:
                        __paymentCurrency__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.RegisterPayment();
            ____result.userName = __userName__;
            ____result.paymentAmountOut = __paymentAmountOut__;
            ____result.paymentAmountLocal = __paymentAmountLocal__;
            ____result.paymentCurrency = __paymentCurrency__;
            return ____result;
        }
    }


    public sealed class SendFeedbackFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.SendFeedback>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.SendFeedback value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 4);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.email, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.caption, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.description, formatterResolver);
            return offset - startOffset;
        }

        public global::S.SendFeedback Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __email__ = default(string);
            var __caption__ = default(string);
            var __description__ = default(string);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __email__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __caption__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __description__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.SendFeedback();
            ____result.email = __email__;
            ____result.caption = __caption__;
            ____result.description = __description__;
            return ____result;
        }
    }


    public sealed class LeaderboardRegisterRecordFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.LeaderboardRegisterRecord>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.LeaderboardRegisterRecord value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 6);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += MessagePackBinary.WriteDouble(ref bytes, offset, value.Score);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Type, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Name, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.FacebookId, formatterResolver);
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value.Add);
            return offset - startOffset;
        }

        public global::S.LeaderboardRegisterRecord Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Score__ = default(double);
            var __Type__ = default(string);
            var __Name__ = default(string);
            var __FacebookId__ = default(string);
            var __Add__ = default(bool);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __Score__ = MessagePackBinary.ReadDouble(bytes, offset, out readSize);
                        break;
                    case 2:
                        __Type__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __Name__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __FacebookId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 5:
                        __Add__ = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.LeaderboardRegisterRecord();
            ____result.Score = __Score__;
            ____result.Type = __Type__;
            ____result.Name = __Name__;
            ____result.FacebookId = __FacebookId__;
            ____result.Add = __Add__;
            return ____result;
        }
    }


    public sealed class LeaderboardGetRankFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.LeaderboardGetRank>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.LeaderboardGetRank value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Type, formatterResolver);
            return offset - startOffset;
        }

        public global::S.LeaderboardGetRank Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Type__ = default(string);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __Type__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.LeaderboardGetRank();
            ____result.Type = __Type__;
            return ____result;
        }
    }


    public sealed class LeaderboardGetRankTopFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.LeaderboardGetRankTop>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.LeaderboardGetRankTop value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 5);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Type, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Amount);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.From);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.To);
            return offset - startOffset;
        }

        public global::S.LeaderboardGetRankTop Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Type__ = default(string);
            var __Amount__ = default(int);
            var __From__ = default(int);
            var __To__ = default(int);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __Type__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Amount__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 3:
                        __From__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 4:
                        __To__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.LeaderboardGetRankTop();
            ____result.Type = __Type__;
            ____result.Amount = __Amount__;
            ____result.From = __From__;
            ____result.To = __To__;
            return ____result;
        }
    }


    public sealed class LeaderboardGetRankTopChunkFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.LeaderboardGetRankTopChunk>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.LeaderboardGetRankTopChunk value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 4);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Type, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.LeagueIndex);
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value.UseLeagueIndex);
            return offset - startOffset;
        }

        public global::S.LeaderboardGetRankTopChunk Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Type__ = default(string);
            var __LeagueIndex__ = default(int);
            var __UseLeagueIndex__ = default(bool);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __Type__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __LeagueIndex__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 3:
                        __UseLeagueIndex__ = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.LeaderboardGetRankTopChunk();
            ____result.Type = __Type__;
            ____result.LeagueIndex = __LeagueIndex__;
            ____result.UseLeagueIndex = __UseLeagueIndex__;
            return ____result;
        }
    }


    public sealed class LeaderboardGetFacebookFriendsTopFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.LeaderboardGetFacebookFriendsTop>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.LeaderboardGetFacebookFriendsTop value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 3);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<string>>().Serialize(ref bytes, offset, value.Friends, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Type, formatterResolver);
            return offset - startOffset;
        }

        public global::S.LeaderboardGetFacebookFriendsTop Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Friends__ = default(global::System.Collections.Generic.List<string>);
            var __Type__ = default(string);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __Friends__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<string>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Type__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.LeaderboardGetFacebookFriendsTop();
            ____result.Friends = __Friends__;
            ____result.Type = __Type__;
            return ____result;
        }
    }


    public sealed class UsePromoFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.UsePromo>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.UsePromo value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Promo, formatterResolver);
            return offset - startOffset;
        }

        public global::S.UsePromo Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Promo__ = default(string);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __Promo__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.UsePromo();
            ____result.Promo = __Promo__;
            return ____result;
        }
    }


    public sealed class ServerMessageFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.ServerMessage>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.ServerMessage value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 3);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.Data, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.MessageType, formatterResolver);
            return offset - startOffset;
        }

        public global::S.ServerMessage Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Data__ = default(byte[]);
            var __MessageType__ = default(string);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __Data__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __MessageType__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.ServerMessage();
            ____result.Data = __Data__;
            ____result.MessageType = __MessageType__;
            return ____result;
        }
    }


    public sealed class SendServerMessageFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.SendServerMessage>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.SendServerMessage value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<global::S.ServerMessage>().Serialize(ref bytes, offset, value.Message, formatterResolver);
            return offset - startOffset;
        }

        public global::S.SendServerMessage Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Message__ = default(global::S.ServerMessage);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __Message__ = formatterResolver.GetFormatterWithVerify<global::S.ServerMessage>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.SendServerMessage();
            ____result.Message = __Message__;
            return ____result;
        }
    }


    public sealed class ClearServerMessagesInboxFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.ClearServerMessagesInbox>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.ClearServerMessagesInbox value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 0);
            return offset - startOffset;
        }

        public global::S.ClearServerMessagesInbox Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;


            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.ClearServerMessagesInbox();
            return ____result;
        }
    }


    public sealed class GetServerMessagesInboxFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.GetServerMessagesInbox>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.GetServerMessagesInbox value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 6);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value.GetBroadcasts);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.LastSeenBroadcastSerialId);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.SystemLanguage, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Sequence);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.StateBirthday);
            return offset - startOffset;
        }

        public global::S.GetServerMessagesInbox Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __GetBroadcasts__ = default(bool);
            var __LastSeenBroadcastSerialId__ = default(long);
            var __SystemLanguage__ = default(string);
            var __Sequence__ = default(int);
            var __StateBirthday__ = default(long);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __GetBroadcasts__ = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        break;
                    case 2:
                        __LastSeenBroadcastSerialId__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 3:
                        __SystemLanguage__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __Sequence__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 5:
                        __StateBirthday__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.GetServerMessagesInbox();
            ____result.GetBroadcasts = __GetBroadcasts__;
            ____result.LastSeenBroadcastSerialId = __LastSeenBroadcastSerialId__;
            ____result.SystemLanguage = __SystemLanguage__;
            ____result.Sequence = __Sequence__;
            ____result.StateBirthday = __StateBirthday__;
            return ____result;
        }
    }


    public sealed class LeagueRegisterRequestFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.LeagueRegisterRequest>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.LeagueRegisterRequest value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 3);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Guid>().Serialize(ref bytes, offset, value.PlayerId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Name, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.FacebookId, formatterResolver);
            return offset - startOffset;
        }

        public global::S.LeagueRegisterRequest Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __PlayerId__ = default(global::System.Guid);
            var __Name__ = default(string);
            var __FacebookId__ = default(string);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __PlayerId__ = formatterResolver.GetFormatterWithVerify<global::System.Guid>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Name__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __FacebookId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.LeagueRegisterRequest();
            ____result.PlayerId = __PlayerId__;
            ____result.Name = __Name__;
            ____result.FacebookId = __FacebookId__;
            return ____result;
        }
    }


    public sealed class LeaguePlayerGlobalRankRequestFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.LeaguePlayerGlobalRankRequest>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.LeaguePlayerGlobalRankRequest value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 1);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Guid>().Serialize(ref bytes, offset, value.PlayerId, formatterResolver);
            return offset - startOffset;
        }

        public global::S.LeaguePlayerGlobalRankRequest Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __PlayerId__ = default(global::System.Guid);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __PlayerId__ = formatterResolver.GetFormatterWithVerify<global::System.Guid>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.LeaguePlayerGlobalRankRequest();
            ____result.PlayerId = __PlayerId__;
            return ____result;
        }
    }


    public sealed class LeaguePlayerLeagueRankRequestFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.LeaguePlayerLeagueRankRequest>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.LeaguePlayerLeagueRankRequest value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 1);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Guid>().Serialize(ref bytes, offset, value.PlayerId, formatterResolver);
            return offset - startOffset;
        }

        public global::S.LeaguePlayerLeagueRankRequest Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __PlayerId__ = default(global::System.Guid);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __PlayerId__ = formatterResolver.GetFormatterWithVerify<global::System.Guid>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.LeaguePlayerLeagueRankRequest();
            ____result.PlayerId = __PlayerId__;
            return ____result;
        }
    }


    public sealed class LeagueDivisionRanksRequestFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.LeagueDivisionRanksRequest>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.LeagueDivisionRanksRequest value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 1);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Guid>().Serialize(ref bytes, offset, value.PlayerId, formatterResolver);
            return offset - startOffset;
        }

        public global::S.LeagueDivisionRanksRequest Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __PlayerId__ = default(global::System.Guid);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __PlayerId__ = formatterResolver.GetFormatterWithVerify<global::System.Guid>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.LeagueDivisionRanksRequest();
            ____result.PlayerId = __PlayerId__;
            return ____result;
        }
    }


    public sealed class LeagueTopRequestFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.LeagueTopRequest>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.LeagueTopRequest value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Guid>().Serialize(ref bytes, offset, value.PlayerId, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Amount);
            return offset - startOffset;
        }

        public global::S.LeagueTopRequest Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __PlayerId__ = default(global::System.Guid);
            var __Amount__ = default(int);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __PlayerId__ = formatterResolver.GetFormatterWithVerify<global::System.Guid>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Amount__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.LeagueTopRequest();
            ____result.PlayerId = __PlayerId__;
            ____result.Amount = __Amount__;
            return ____result;
        }
    }


    public sealed class LeagueGlobalTopRequestFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.LeagueGlobalTopRequest>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.LeagueGlobalTopRequest value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Guid>().Serialize(ref bytes, offset, value.PlayerId, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Amount);
            return offset - startOffset;
        }

        public global::S.LeagueGlobalTopRequest Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __PlayerId__ = default(global::System.Guid);
            var __Amount__ = default(int);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __PlayerId__ = formatterResolver.GetFormatterWithVerify<global::System.Guid>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Amount__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.LeagueGlobalTopRequest();
            ____result.PlayerId = __PlayerId__;
            ____result.Amount = __Amount__;
            return ____result;
        }
    }


    public sealed class DefsRequestFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.DefsRequest>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.DefsRequest value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 0);
            return offset - startOffset;
        }

        public global::S.DefsRequest Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;


            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.DefsRequest();
            return ____result;
        }
    }


    public sealed class DeleteUserRequestFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.DeleteUserRequest>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.DeleteUserRequest value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 0);
            return offset - startOffset;
        }

        public global::S.DeleteUserRequest Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;


            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.DeleteUserRequest();
            return ____result;
        }
    }


    public sealed class TypedApiCallFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.TypedApiCall>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.TypedApiCall value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Type);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.Data, formatterResolver);
            return offset - startOffset;
        }

        public global::S.TypedApiCall Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Type__ = default(int);
            var __Data__ = default(byte[]);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __Type__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 1:
                        __Data__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.TypedApiCall();
            ____result.Type = __Type__;
            ____result.Data = __Data__;
            return ____result;
        }
    }


    public sealed class ValidateSessionRequestFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.ValidateSessionRequest>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.ValidateSessionRequest value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 0);
            return offset - startOffset;
        }

        public global::S.ValidateSessionRequest Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;


            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.ValidateSessionRequest();
            return ____result;
        }
    }


    public sealed class LeaderboardGetSeasonInfoRequestFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.LeaderboardGetSeasonInfoRequest>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.LeaderboardGetSeasonInfoRequest value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 0);
            return offset - startOffset;
        }

        public global::S.LeaderboardGetSeasonInfoRequest Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;


            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.LeaderboardGetSeasonInfoRequest();
            return ____result;
        }
    }


    public sealed class ExtensionModuleRequestFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.ExtensionModuleRequest>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.ExtensionModuleRequest value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.ModuleType, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.Request, formatterResolver);
            return offset - startOffset;
        }

        public global::S.ExtensionModuleRequest Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __ModuleType__ = default(string);
            var __Request__ = default(byte[]);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __ModuleType__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Request__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.ExtensionModuleRequest();
            ____result.ModuleType = __ModuleType__;
            ____result.Request = __Request__;
            return ____result;
        }
    }


    public sealed class RequestFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.Request>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.Request value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteArrayHeader(ref bytes, offset, 48);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.UserId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.NetworkType>().Serialize(ref bytes, offset, value.NetworkId, formatterResolver);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.SessionId);
            offset += formatterResolver.GetFormatterWithVerify<global::S.InitRequest>().Serialize(ref bytes, offset, value.InitRequest, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.ProcessCommandsBatchRequest>().Serialize(ref bytes, offset, value.ProcessCommandsBatchRequest, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.ResetRequest>().Serialize(ref bytes, offset, value.ResetRequest, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.DeviceUniqueId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.SetFacebookIdRequest>().Serialize(ref bytes, offset, value.SetFacebookIdRequest, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.GetProfileByFacebookIdRequest>().Serialize(ref bytes, offset, value.GetProfileByFacebookIdRequest, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.GetRandomUserIds>().Serialize(ref bytes, offset, value.GetRandomUserIds, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.SharedLogicVersion);
            offset += formatterResolver.GetFormatterWithVerify<global::S.SyncTime>().Serialize(ref bytes, offset, value.SyncTime, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.ReplaceStateRequest>().Serialize(ref bytes, offset, value.ReplaceStateRequest, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.RegisterPayment>().Serialize(ref bytes, offset, value.RegisterPayment, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.SerialNumber);
            offset += formatterResolver.GetFormatterWithVerify<global::S.SendFeedback>().Serialize(ref bytes, offset, value.SendFeedback, formatterResolver);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.DefsVersion);
            offset += formatterResolver.GetFormatterWithVerify<global::S.LeaderboardRegisterRecord>().Serialize(ref bytes, offset, value.LeaderboardRegisterRecord, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.LeaderboardGetRank>().Serialize(ref bytes, offset, value.LeaderboardGetRank, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.LeaderboardGetRankTop>().Serialize(ref bytes, offset, value.LeaderboardGetRankTop, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.LeaderboardGetRankTopChunk>().Serialize(ref bytes, offset, value.LeaderboardGetRankTopChunk, formatterResolver);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<global::S.LeaderboardGetFacebookFriendsTop>().Serialize(ref bytes, offset, value.LeaderboardGetFacebookFriendsTop, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.UsePromo>().Serialize(ref bytes, offset, value.UsePromo, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.SendServerMessage>().Serialize(ref bytes, offset, value.SendServerMessage, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.ClearServerMessagesInbox>().Serialize(ref bytes, offset, value.ClearServerMessagesInbox, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.GetServerMessagesInbox>().Serialize(ref bytes, offset, value.GetServerMessagesInbox, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.GameSpecificRequest, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.LeagueRegisterRequest>().Serialize(ref bytes, offset, value.LeagueRegister, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.LeaguePlayerGlobalRankRequest>().Serialize(ref bytes, offset, value.LeaguePlayerGlobalRank, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.LeaguePlayerLeagueRankRequest>().Serialize(ref bytes, offset, value.LeaguePlayerLeagueRank, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.LeagueDivisionRanksRequest>().Serialize(ref bytes, offset, value.LeagueDivisionRanks, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.LeagueTopRequest>().Serialize(ref bytes, offset, value.LeagueTop, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.LeagueGlobalTopRequest>().Serialize(ref bytes, offset, value.LeagueGlobalTop, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.DefsRequest>().Serialize(ref bytes, offset, value.DefsRequest, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.DeleteUserRequest>().Serialize(ref bytes, offset, value.DeleteUserRequest, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.TypedApiCall>().Serialize(ref bytes, offset, value.GlobalConflictApiCall, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.TypedApiCall>().Serialize(ref bytes, offset, value.PlayerProfile, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.ValidateSessionRequest>().Serialize(ref bytes, offset, value.ValidateSessionRequest, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.LeaderboardGetSeasonInfoRequest>().Serialize(ref bytes, offset, value.LeaderboardGetSeasonInfoRequest, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.ExtensionModuleRequest>().Serialize(ref bytes, offset, value.ExtensionModuleRequest, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.ExtensionModuleRequest>().Serialize(ref bytes, offset, value.ExtensionModuleAsyncRequest, formatterResolver);
            return offset - startOffset;
        }

        public global::S.Request Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __UserId__ = default(byte[]);
            var __DeviceUniqueId__ = default(string);
            var __NetworkId__ = default(global::S.NetworkType);
            var __SessionId__ = default(long);
            var __InitRequest__ = default(global::S.InitRequest);
            var __ProcessCommandsBatchRequest__ = default(global::S.ProcessCommandsBatchRequest);
            var __ResetRequest__ = default(global::S.ResetRequest);
            var __SetFacebookIdRequest__ = default(global::S.SetFacebookIdRequest);
            var __GetProfileByFacebookIdRequest__ = default(global::S.GetProfileByFacebookIdRequest);
            var __GetRandomUserIds__ = default(global::S.GetRandomUserIds);
            var __SyncTime__ = default(global::S.SyncTime);
            var __ReplaceStateRequest__ = default(global::S.ReplaceStateRequest);
            var __RegisterPayment__ = default(global::S.RegisterPayment);
            var __SendFeedback__ = default(global::S.SendFeedback);
            var __LeaderboardRegisterRecord__ = default(global::S.LeaderboardRegisterRecord);
            var __LeaderboardGetRank__ = default(global::S.LeaderboardGetRank);
            var __LeaderboardGetRankTop__ = default(global::S.LeaderboardGetRankTop);
            var __LeaderboardGetRankTopChunk__ = default(global::S.LeaderboardGetRankTopChunk);
            var __LeaderboardGetFacebookFriendsTop__ = default(global::S.LeaderboardGetFacebookFriendsTop);
            var __UsePromo__ = default(global::S.UsePromo);
            var __SendServerMessage__ = default(global::S.SendServerMessage);
            var __ClearServerMessagesInbox__ = default(global::S.ClearServerMessagesInbox);
            var __GetServerMessagesInbox__ = default(global::S.GetServerMessagesInbox);
            var __GameSpecificRequest__ = default(byte[]);
            var __SharedLogicVersion__ = default(int);
            var __DefsVersion__ = default(int);
            var __SerialNumber__ = default(int);
            var __LeagueRegister__ = default(global::S.LeagueRegisterRequest);
            var __LeaguePlayerGlobalRank__ = default(global::S.LeaguePlayerGlobalRankRequest);
            var __LeaguePlayerLeagueRank__ = default(global::S.LeaguePlayerLeagueRankRequest);
            var __LeagueDivisionRanks__ = default(global::S.LeagueDivisionRanksRequest);
            var __LeagueTop__ = default(global::S.LeagueTopRequest);
            var __LeagueGlobalTop__ = default(global::S.LeagueGlobalTopRequest);
            var __DefsRequest__ = default(global::S.DefsRequest);
            var __DeleteUserRequest__ = default(global::S.DeleteUserRequest);
            var __GlobalConflictApiCall__ = default(global::S.TypedApiCall);
            var __PlayerProfile__ = default(global::S.TypedApiCall);
            var __ValidateSessionRequest__ = default(global::S.ValidateSessionRequest);
            var __LeaderboardGetSeasonInfoRequest__ = default(global::S.LeaderboardGetSeasonInfoRequest);
            var __ExtensionModuleRequest__ = default(global::S.ExtensionModuleRequest);
            var __ExtensionModuleAsyncRequest__ = default(global::S.ExtensionModuleRequest);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __UserId__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 7:
                        __DeviceUniqueId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __NetworkId__ = formatterResolver.GetFormatterWithVerify<global::S.NetworkType>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __SessionId__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 4:
                        __InitRequest__ = formatterResolver.GetFormatterWithVerify<global::S.InitRequest>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 5:
                        __ProcessCommandsBatchRequest__ = formatterResolver.GetFormatterWithVerify<global::S.ProcessCommandsBatchRequest>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 6:
                        __ResetRequest__ = formatterResolver.GetFormatterWithVerify<global::S.ResetRequest>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 8:
                        __SetFacebookIdRequest__ = formatterResolver.GetFormatterWithVerify<global::S.SetFacebookIdRequest>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 9:
                        __GetProfileByFacebookIdRequest__ = formatterResolver.GetFormatterWithVerify<global::S.GetProfileByFacebookIdRequest>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 10:
                        __GetRandomUserIds__ = formatterResolver.GetFormatterWithVerify<global::S.GetRandomUserIds>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 12:
                        __SyncTime__ = formatterResolver.GetFormatterWithVerify<global::S.SyncTime>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 13:
                        __ReplaceStateRequest__ = formatterResolver.GetFormatterWithVerify<global::S.ReplaceStateRequest>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 14:
                        __RegisterPayment__ = formatterResolver.GetFormatterWithVerify<global::S.RegisterPayment>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 16:
                        __SendFeedback__ = formatterResolver.GetFormatterWithVerify<global::S.SendFeedback>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 19:
                        __LeaderboardRegisterRecord__ = formatterResolver.GetFormatterWithVerify<global::S.LeaderboardRegisterRecord>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 20:
                        __LeaderboardGetRank__ = formatterResolver.GetFormatterWithVerify<global::S.LeaderboardGetRank>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 21:
                        __LeaderboardGetRankTop__ = formatterResolver.GetFormatterWithVerify<global::S.LeaderboardGetRankTop>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 22:
                        __LeaderboardGetRankTopChunk__ = formatterResolver.GetFormatterWithVerify<global::S.LeaderboardGetRankTopChunk>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 28:
                        __LeaderboardGetFacebookFriendsTop__ = formatterResolver.GetFormatterWithVerify<global::S.LeaderboardGetFacebookFriendsTop>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 29:
                        __UsePromo__ = formatterResolver.GetFormatterWithVerify<global::S.UsePromo>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 30:
                        __SendServerMessage__ = formatterResolver.GetFormatterWithVerify<global::S.SendServerMessage>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 31:
                        __ClearServerMessagesInbox__ = formatterResolver.GetFormatterWithVerify<global::S.ClearServerMessagesInbox>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 32:
                        __GetServerMessagesInbox__ = formatterResolver.GetFormatterWithVerify<global::S.GetServerMessagesInbox>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 33:
                        __GameSpecificRequest__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 11:
                        __SharedLogicVersion__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 18:
                        __DefsVersion__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 15:
                        __SerialNumber__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 34:
                        __LeagueRegister__ = formatterResolver.GetFormatterWithVerify<global::S.LeagueRegisterRequest>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 35:
                        __LeaguePlayerGlobalRank__ = formatterResolver.GetFormatterWithVerify<global::S.LeaguePlayerGlobalRankRequest>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 36:
                        __LeaguePlayerLeagueRank__ = formatterResolver.GetFormatterWithVerify<global::S.LeaguePlayerLeagueRankRequest>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 37:
                        __LeagueDivisionRanks__ = formatterResolver.GetFormatterWithVerify<global::S.LeagueDivisionRanksRequest>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 38:
                        __LeagueTop__ = formatterResolver.GetFormatterWithVerify<global::S.LeagueTopRequest>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 39:
                        __LeagueGlobalTop__ = formatterResolver.GetFormatterWithVerify<global::S.LeagueGlobalTopRequest>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 40:
                        __DefsRequest__ = formatterResolver.GetFormatterWithVerify<global::S.DefsRequest>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 41:
                        __DeleteUserRequest__ = formatterResolver.GetFormatterWithVerify<global::S.DeleteUserRequest>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 42:
                        __GlobalConflictApiCall__ = formatterResolver.GetFormatterWithVerify<global::S.TypedApiCall>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 43:
                        __PlayerProfile__ = formatterResolver.GetFormatterWithVerify<global::S.TypedApiCall>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 44:
                        __ValidateSessionRequest__ = formatterResolver.GetFormatterWithVerify<global::S.ValidateSessionRequest>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 45:
                        __LeaderboardGetSeasonInfoRequest__ = formatterResolver.GetFormatterWithVerify<global::S.LeaderboardGetSeasonInfoRequest>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 46:
                        __ExtensionModuleRequest__ = formatterResolver.GetFormatterWithVerify<global::S.ExtensionModuleRequest>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 47:
                        __ExtensionModuleAsyncRequest__ = formatterResolver.GetFormatterWithVerify<global::S.ExtensionModuleRequest>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.Request();
            ____result.UserId = __UserId__;
            ____result.DeviceUniqueId = __DeviceUniqueId__;
            ____result.NetworkId = __NetworkId__;
            ____result.SessionId = __SessionId__;
            ____result.InitRequest = __InitRequest__;
            ____result.ProcessCommandsBatchRequest = __ProcessCommandsBatchRequest__;
            ____result.ResetRequest = __ResetRequest__;
            ____result.SetFacebookIdRequest = __SetFacebookIdRequest__;
            ____result.GetProfileByFacebookIdRequest = __GetProfileByFacebookIdRequest__;
            ____result.GetRandomUserIds = __GetRandomUserIds__;
            ____result.SyncTime = __SyncTime__;
            ____result.ReplaceStateRequest = __ReplaceStateRequest__;
            ____result.RegisterPayment = __RegisterPayment__;
            ____result.SendFeedback = __SendFeedback__;
            ____result.LeaderboardRegisterRecord = __LeaderboardRegisterRecord__;
            ____result.LeaderboardGetRank = __LeaderboardGetRank__;
            ____result.LeaderboardGetRankTop = __LeaderboardGetRankTop__;
            ____result.LeaderboardGetRankTopChunk = __LeaderboardGetRankTopChunk__;
            ____result.LeaderboardGetFacebookFriendsTop = __LeaderboardGetFacebookFriendsTop__;
            ____result.UsePromo = __UsePromo__;
            ____result.SendServerMessage = __SendServerMessage__;
            ____result.ClearServerMessagesInbox = __ClearServerMessagesInbox__;
            ____result.GetServerMessagesInbox = __GetServerMessagesInbox__;
            ____result.GameSpecificRequest = __GameSpecificRequest__;
            ____result.SharedLogicVersion = __SharedLogicVersion__;
            ____result.DefsVersion = __DefsVersion__;
            ____result.SerialNumber = __SerialNumber__;
            ____result.LeagueRegister = __LeagueRegister__;
            ____result.LeaguePlayerGlobalRank = __LeaguePlayerGlobalRank__;
            ____result.LeaguePlayerLeagueRank = __LeaguePlayerLeagueRank__;
            ____result.LeagueDivisionRanks = __LeagueDivisionRanks__;
            ____result.LeagueTop = __LeagueTop__;
            ____result.LeagueGlobalTop = __LeagueGlobalTop__;
            ____result.DefsRequest = __DefsRequest__;
            ____result.DeleteUserRequest = __DeleteUserRequest__;
            ____result.GlobalConflictApiCall = __GlobalConflictApiCall__;
            ____result.PlayerProfile = __PlayerProfile__;
            ____result.ValidateSessionRequest = __ValidateSessionRequest__;
            ____result.LeaderboardGetSeasonInfoRequest = __LeaderboardGetSeasonInfoRequest__;
            ____result.ExtensionModuleRequest = __ExtensionModuleRequest__;
            ____result.ExtensionModuleAsyncRequest = __ExtensionModuleAsyncRequest__;
            return ____result;
        }
    }


    public sealed class BenchmarkRequestFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.BenchmarkRequest>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.BenchmarkRequest value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<byte[]>>().Serialize(ref bytes, offset, value.SerializedRequest, formatterResolver);
            return offset - startOffset;
        }

        public global::S.BenchmarkRequest Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __SerializedRequest__ = default(global::System.Collections.Generic.List<byte[]>);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __SerializedRequest__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<byte[]>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.BenchmarkRequest();
            ____result.SerializedRequest = __SerializedRequest__;
            return ____result;
        }
    }


    public sealed class DataCollectionFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.DataCollection>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.DataCollection value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 5);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.Request, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.Response, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.Data, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.State, formatterResolver);
            return offset - startOffset;
        }

        public global::S.DataCollection Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Request__ = default(byte[]);
            var __Response__ = default(byte[]);
            var __Data__ = default(byte[]);
            var __State__ = default(byte[]);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __Request__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Response__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __Data__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __State__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.DataCollection();
            ____result.Request = __Request__;
            ____result.Response = __Response__;
            ____result.Data = __Data__;
            ____result.State = __State__;
            return ____result;
        }
    }


    public sealed class UsePromoResponseFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.UsePromoResponse>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.UsePromoResponse value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 5);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<global::S.PromoCodeResponseCode>().Serialize(ref bytes, offset, value.PromoResponseCode, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Function, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Parameter, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.PromoCode, formatterResolver);
            return offset - startOffset;
        }

        public global::S.UsePromoResponse Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __PromoResponseCode__ = default(global::S.PromoCodeResponseCode);
            var __Function__ = default(string);
            var __Parameter__ = default(string);
            var __PromoCode__ = default(string);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __PromoResponseCode__ = formatterResolver.GetFormatterWithVerify<global::S.PromoCodeResponseCode>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Function__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __Parameter__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __PromoCode__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.UsePromoResponse();
            ____result.PromoResponseCode = __PromoResponseCode__;
            ____result.Function = __Function__;
            ____result.Parameter = __Parameter__;
            ____result.PromoCode = __PromoCode__;
            return ____result;
        }
    }


    public sealed class SendMessageResponseFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.SendMessageResponse>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.SendMessageResponse value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value.MessageSent);
            return offset - startOffset;
        }

        public global::S.SendMessageResponse Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __MessageSent__ = default(bool);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __MessageSent__ = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.SendMessageResponse();
            ____result.MessageSent = __MessageSent__;
            return ____result;
        }
    }


    public sealed class SendServerMessageResponseFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.SendServerMessageResponse>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.SendServerMessageResponse value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value.MessageSent);
            return offset - startOffset;
        }

        public global::S.SendServerMessageResponse Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __MessageSent__ = default(bool);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __MessageSent__ = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.SendServerMessageResponse();
            ____result.MessageSent = __MessageSent__;
            return ____result;
        }
    }


    public sealed class ServerMessagesInboxFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.ServerMessagesInbox>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.ServerMessagesInbox value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::S.ServerMessage>>().Serialize(ref bytes, offset, value.Messages, formatterResolver);
            return offset - startOffset;
        }

        public global::S.ServerMessagesInbox Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Messages__ = default(global::System.Collections.Generic.List<global::S.ServerMessage>);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __Messages__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::S.ServerMessage>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.ServerMessagesInbox();
            ____result.Messages = __Messages__;
            return ____result;
        }
    }


    public sealed class DefsDataFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.DefsData>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.DefsData value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 4);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<string>>().Serialize(ref bytes, offset, value.PageName, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<string>>().Serialize(ref bytes, offset, value.Json, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Version);
            return offset - startOffset;
        }

        public global::S.DefsData Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __PageName__ = default(global::System.Collections.Generic.List<string>);
            var __Json__ = default(global::System.Collections.Generic.List<string>);
            var __Version__ = default(int);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __PageName__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<string>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Json__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<string>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __Version__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.DefsData();
            ____result.PageName = __PageName__;
            ____result.Json = __Json__;
            ____result.Version = __Version__;
            return ____result;
        }
    }


    public sealed class ResponseFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.Response>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.Response value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 15);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<global::S.ResponseCode>().Serialize(ref bytes, offset, value.ResponseCode, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.ServerStackTrace, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.PlayerId, formatterResolver);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.Timestamp);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<global::S.DefsData>().Serialize(ref bytes, offset, value.DefsData, formatterResolver);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.MaintenanceTimestamp);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.Inbox, formatterResolver);
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value.Banned);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.DebugInfo, formatterResolver);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.SessionId);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.ActualUserProfile, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.Token, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.ShortId);
            return offset - startOffset;
        }

        public global::S.Response Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __ResponseCode__ = default(global::S.ResponseCode);
            var __ServerStackTrace__ = default(string);
            var __PlayerId__ = default(byte[]);
            var __Timestamp__ = default(long);
            var __DefsData__ = default(global::S.DefsData);
            var __MaintenanceTimestamp__ = default(long);
            var __Inbox__ = default(byte[]);
            var __Banned__ = default(bool);
            var __DebugInfo__ = default(string);
            var __SessionId__ = default(long);
            var __ActualUserProfile__ = default(byte[]);
            var __Token__ = default(byte[]);
            var __ShortId__ = default(int);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __ResponseCode__ = formatterResolver.GetFormatterWithVerify<global::S.ResponseCode>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __ServerStackTrace__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __PlayerId__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __Timestamp__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 6:
                        __DefsData__ = formatterResolver.GetFormatterWithVerify<global::S.DefsData>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 7:
                        __MaintenanceTimestamp__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 8:
                        __Inbox__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 9:
                        __Banned__ = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        break;
                    case 10:
                        __DebugInfo__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 11:
                        __SessionId__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 12:
                        __ActualUserProfile__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 13:
                        __Token__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 14:
                        __ShortId__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.Response();
            ____result.ResponseCode = __ResponseCode__;
            ____result.ServerStackTrace = __ServerStackTrace__;
            ____result.PlayerId = __PlayerId__;
            ____result.Timestamp = __Timestamp__;
            ____result.DefsData = __DefsData__;
            ____result.MaintenanceTimestamp = __MaintenanceTimestamp__;
            ____result.Inbox = __Inbox__;
            ____result.Banned = __Banned__;
            ____result.DebugInfo = __DebugInfo__;
            ____result.SessionId = __SessionId__;
            ____result.ActualUserProfile = __ActualUserProfile__;
            ____result.Token = __Token__;
            ____result.ShortId = __ShortId__;
            return ____result;
        }
    }


    public sealed class SetFacebookIdResponseFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.SetFacebookIdResponse>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.SetFacebookIdResponse value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 3);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value.Success);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.NewPlayerId, formatterResolver);
            return offset - startOffset;
        }

        public global::S.SetFacebookIdResponse Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Success__ = default(bool);
            var __NewPlayerId__ = default(byte[]);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __Success__ = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        break;
                    case 2:
                        __NewPlayerId__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.SetFacebookIdResponse();
            ____result.Success = __Success__;
            ____result.NewPlayerId = __NewPlayerId__;
            return ____result;
        }
    }


    public sealed class GetRandomUserIdsResponseFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.GetRandomUserIdsResponse>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.GetRandomUserIdsResponse value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<int>>().Serialize(ref bytes, offset, value.UserIds, formatterResolver);
            return offset - startOffset;
        }

        public global::S.GetRandomUserIdsResponse Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __UserIds__ = default(global::System.Collections.Generic.List<int>);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __UserIds__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<int>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.GetRandomUserIdsResponse();
            ____result.UserIds = __UserIds__;
            return ____result;
        }
    }


    public sealed class GetProfileByFacebookIdResponseFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.GetProfileByFacebookIdResponse>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.GetProfileByFacebookIdResponse value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 4);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.FacebookId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.Profile, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::S.GetProfileByFacebookIdCode>().Serialize(ref bytes, offset, value.ResultCode, formatterResolver);
            return offset - startOffset;
        }

        public global::S.GetProfileByFacebookIdResponse Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __FacebookId__ = default(string);
            var __Profile__ = default(byte[]);
            var __ResultCode__ = default(global::S.GetProfileByFacebookIdCode);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __FacebookId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Profile__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __ResultCode__ = formatterResolver.GetFormatterWithVerify<global::S.GetProfileByFacebookIdCode>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.GetProfileByFacebookIdResponse();
            ____result.FacebookId = __FacebookId__;
            ____result.Profile = __Profile__;
            ____result.ResultCode = __ResultCode__;
            return ____result;
        }
    }


    public sealed class LeaderboardGetRankResponseFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.LeaderboardGetRankResponse>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.LeaderboardGetRankResponse value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 3);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Rank);
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value.UserFound);
            return offset - startOffset;
        }

        public global::S.LeaderboardGetRankResponse Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Rank__ = default(int);
            var __UserFound__ = default(bool);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __Rank__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 2:
                        __UserFound__ = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.LeaderboardGetRankResponse();
            ____result.Rank = __Rank__;
            ____result.UserFound = __UserFound__;
            return ____result;
        }
    }


    public sealed class LeaderboardGetSeasonInfoResponseFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.LeaderboardGetSeasonInfoResponse>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.LeaderboardGetSeasonInfoResponse value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 3);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.SeasonId, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.SeasonIndex);
            return offset - startOffset;
        }

        public global::S.LeaderboardGetSeasonInfoResponse Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __SeasonId__ = default(string);
            var __SeasonIndex__ = default(int);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __SeasonId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __SeasonIndex__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.LeaderboardGetSeasonInfoResponse();
            ____result.SeasonId = __SeasonId__;
            ____result.SeasonIndex = __SeasonIndex__;
            return ____result;
        }
    }


    public sealed class LeaderboardRecordFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.LeaderboardRecord>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.LeaderboardRecord value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 6);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Rank);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Name, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Score);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.UserId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.FacebookId, formatterResolver);
            return offset - startOffset;
        }

        public global::S.LeaderboardRecord Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Rank__ = default(int);
            var __Name__ = default(string);
            var __Score__ = default(int);
            var __UserId__ = default(byte[]);
            var __FacebookId__ = default(string);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __Rank__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 2:
                        __Name__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __Score__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 4:
                        __UserId__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 5:
                        __FacebookId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.LeaderboardRecord();
            ____result.Rank = __Rank__;
            ____result.Name = __Name__;
            ____result.Score = __Score__;
            ____result.UserId = __UserId__;
            ____result.FacebookId = __FacebookId__;
            return ____result;
        }
    }


    public sealed class LeaderboardGetRankTopResponseFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.LeaderboardGetRankTopResponse>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.LeaderboardGetRankTopResponse value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::S.LeaderboardRecord>>().Serialize(ref bytes, offset, value.Records, formatterResolver);
            return offset - startOffset;
        }

        public global::S.LeaderboardGetRankTopResponse Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Records__ = default(global::System.Collections.Generic.List<global::S.LeaderboardRecord>);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __Records__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::S.LeaderboardRecord>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.LeaderboardGetRankTopResponse();
            ____result.Records = __Records__;
            return ____result;
        }
    }


    public sealed class LeaderboardGetRankTopChunkResponseFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.LeaderboardGetRankTopChunkResponse>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.LeaderboardGetRankTopChunkResponse value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::S.LeaderboardRecord>>().Serialize(ref bytes, offset, value.Records, formatterResolver);
            return offset - startOffset;
        }

        public global::S.LeaderboardGetRankTopChunkResponse Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Records__ = default(global::System.Collections.Generic.List<global::S.LeaderboardRecord>);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __Records__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::S.LeaderboardRecord>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.LeaderboardGetRankTopChunkResponse();
            ____result.Records = __Records__;
            return ____result;
        }
    }


    public sealed class MatchEndFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.MatchEnd>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.MatchEnd value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 4);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.MatchId, formatterResolver);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<global::S.MatchResult>().Serialize(ref bytes, offset, value.Result, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<string, string>>().Serialize(ref bytes, offset, value.Extra, formatterResolver);
            return offset - startOffset;
        }

        public global::S.MatchEnd Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __MatchId__ = default(string);
            var __Result__ = default(global::S.MatchResult);
            var __Extra__ = default(global::System.Collections.Generic.Dictionary<string, string>);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __MatchId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Result__ = formatterResolver.GetFormatterWithVerify<global::S.MatchResult>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __Extra__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<string, string>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.MatchEnd();
            ____result.MatchId = __MatchId__;
            ____result.Result = __Result__;
            ____result.Extra = __Extra__;
            return ____result;
        }
    }


    public sealed class SeasonEndInfoFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.SeasonEndInfo>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.SeasonEndInfo value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 7);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Season);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.GlobalPlace);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.LeaguePlace);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.DivisionPlace);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.LeagueLevel);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.LeagueChange);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.Score);
            return offset - startOffset;
        }

        public global::S.SeasonEndInfo Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Season__ = default(int);
            var __GlobalPlace__ = default(int);
            var __LeaguePlace__ = default(int);
            var __DivisionPlace__ = default(int);
            var __LeagueLevel__ = default(int);
            var __LeagueChange__ = default(int);
            var __Score__ = default(long);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __Season__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 1:
                        __GlobalPlace__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 2:
                        __LeaguePlace__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 3:
                        __DivisionPlace__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 4:
                        __LeagueLevel__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 5:
                        __LeagueChange__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 6:
                        __Score__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.SeasonEndInfo();
            ____result.Season = __Season__;
            ____result.GlobalPlace = __GlobalPlace__;
            ____result.LeaguePlace = __LeaguePlace__;
            ____result.DivisionPlace = __DivisionPlace__;
            ____result.LeagueLevel = __LeagueLevel__;
            ____result.LeagueChange = __LeagueChange__;
            ____result.Score = __Score__;
            return ____result;
        }
    }


    public sealed class LeaguePlayerInfoFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.LeaguePlayerInfo>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.LeaguePlayerInfo value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 11);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Guid>().Serialize(ref bytes, offset, value.PlayerId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.FacebookId, formatterResolver);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.Score);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Season);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.LeagueLevel);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Guid>().Serialize(ref bytes, offset, value.DivisionId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::S.SeasonEndInfo>>().Serialize(ref bytes, offset, value.UnclaimedRewards, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Name, formatterResolver);
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value.IsBot);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.BestScore);
            return offset - startOffset;
        }

        public global::S.LeaguePlayerInfo Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __PlayerId__ = default(global::System.Guid);
            var __FacebookId__ = default(string);
            var __Score__ = default(long);
            var __Season__ = default(int);
            var __LeagueLevel__ = default(int);
            var __DivisionId__ = default(global::System.Guid);
            var __UnclaimedRewards__ = default(global::System.Collections.Generic.List<global::S.SeasonEndInfo>);
            var __Name__ = default(string);
            var __IsBot__ = default(bool);
            var __BestScore__ = default(long);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __PlayerId__ = formatterResolver.GetFormatterWithVerify<global::System.Guid>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __FacebookId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __Score__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 4:
                        __Season__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 5:
                        __LeagueLevel__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 6:
                        __DivisionId__ = formatterResolver.GetFormatterWithVerify<global::System.Guid>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 7:
                        __UnclaimedRewards__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::S.SeasonEndInfo>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 8:
                        __Name__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 9:
                        __IsBot__ = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        break;
                    case 10:
                        __BestScore__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.LeaguePlayerInfo();
            ____result.PlayerId = __PlayerId__;
            ____result.FacebookId = __FacebookId__;
            ____result.Score = __Score__;
            ____result.Season = __Season__;
            ____result.LeagueLevel = __LeagueLevel__;
            ____result.DivisionId = __DivisionId__;
            ____result.UnclaimedRewards = __UnclaimedRewards__;
            ____result.Name = __Name__;
            ____result.IsBot = __IsBot__;
            ____result.BestScore = __BestScore__;
            return ____result;
        }
    }


    public sealed class DivisionInfoFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.DivisionInfo>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.DivisionInfo value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 5);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Guid>().Serialize(ref bytes, offset, value.Id, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.LeagueLevel);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Population);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Season);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.BotsCount);
            return offset - startOffset;
        }

        public global::S.DivisionInfo Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Id__ = default(global::System.Guid);
            var __LeagueLevel__ = default(int);
            var __Population__ = default(int);
            var __Season__ = default(int);
            var __BotsCount__ = default(int);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __Id__ = formatterResolver.GetFormatterWithVerify<global::System.Guid>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __LeagueLevel__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 2:
                        __Population__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 3:
                        __Season__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 4:
                        __BotsCount__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.DivisionInfo();
            ____result.Id = __Id__;
            ____result.LeagueLevel = __LeagueLevel__;
            ____result.Population = __Population__;
            ____result.Season = __Season__;
            ____result.BotsCount = __BotsCount__;
            return ____result;
        }
    }


    public sealed class LeagueRegisterResponseFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.LeagueRegisterResponse>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.LeagueRegisterResponse value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 4);
            offset += formatterResolver.GetFormatterWithVerify<global::S.LeaguePlayerInfo>().Serialize(ref bytes, offset, value.PlayerInfo, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.CurrentSeason);
            offset += formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Serialize(ref bytes, offset, value.SeasonStart, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Serialize(ref bytes, offset, value.SeasonEnd, formatterResolver);
            return offset - startOffset;
        }

        public global::S.LeagueRegisterResponse Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __PlayerInfo__ = default(global::S.LeaguePlayerInfo);
            var __CurrentSeason__ = default(int);
            var __SeasonStart__ = default(global::System.DateTime);
            var __SeasonEnd__ = default(global::System.DateTime);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __PlayerInfo__ = formatterResolver.GetFormatterWithVerify<global::S.LeaguePlayerInfo>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __CurrentSeason__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 2:
                        __SeasonStart__ = formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __SeasonEnd__ = formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.LeagueRegisterResponse();
            ____result.PlayerInfo = __PlayerInfo__;
            ____result.CurrentSeason = __CurrentSeason__;
            ____result.SeasonStart = __SeasonStart__;
            ____result.SeasonEnd = __SeasonEnd__;
            return ____result;
        }
    }


    public sealed class LeagueTopResponseFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.LeagueTopResponse>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.LeagueTopResponse value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += formatterResolver.GetFormatterWithVerify<global::S.LeaguePlayerInfo[]>().Serialize(ref bytes, offset, value.Ranks, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.PlayerRank);
            return offset - startOffset;
        }

        public global::S.LeagueTopResponse Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Ranks__ = default(global::S.LeaguePlayerInfo[]);
            var __PlayerRank__ = default(int);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __Ranks__ = formatterResolver.GetFormatterWithVerify<global::S.LeaguePlayerInfo[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __PlayerRank__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.LeagueTopResponse();
            ____result.Ranks = __Ranks__;
            ____result.PlayerRank = __PlayerRank__;
            return ____result;
        }
    }


    public sealed class MadIdFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.MadId>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.MadId value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 0);
            return offset - startOffset;
        }

        public global::S.MadId Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                throw new InvalidOperationException("typecode is null, struct not supported");
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;


            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.MadId();
            return ____result;
        }
    }


    public sealed class CommandContainerFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.CommandContainer>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.CommandContainer value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 4);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.CommandId);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.DebugInfo, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.CommandData, formatterResolver);
            return offset - startOffset;
        }

        public global::S.CommandContainer Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __CommandId__ = default(int);
            var __DebugInfo__ = default(string);
            var __CommandData__ = default(byte[]);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __CommandId__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 2:
                        __DebugInfo__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __CommandData__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.CommandContainer();
            ____result.CommandId = __CommandId__;
            ____result.DebugInfo = __DebugInfo__;
            ____result.CommandData = __CommandData__;
            return ____result;
        }
    }


    public sealed class ConflictResultPointsFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.ConflictResultPoints>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.ConflictResultPoints value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Team, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Points);
            return offset - startOffset;
        }

        public global::S.ConflictResultPoints Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Team__ = default(string);
            var __Points__ = default(int);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __Team__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Points__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.ConflictResultPoints();
            ____result.Team = __Team__;
            ____result.Points = __Points__;
            return ____result;
        }
    }


    public sealed class ConflictResultFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.ConflictResult>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.ConflictResult value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 8);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.ConflictId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.WinnerTeam, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.MyTeam, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.MyPlace);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.MySector);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::S.ConflictResultPoints>>().Serialize(ref bytes, offset, value.ResultPoints, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.TeamRewardId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.LeaderboardRewardId, formatterResolver);
            return offset - startOffset;
        }

        public global::S.ConflictResult Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __ConflictId__ = default(string);
            var __WinnerTeam__ = default(string);
            var __MyTeam__ = default(string);
            var __MyPlace__ = default(int);
            var __MySector__ = default(int);
            var __ResultPoints__ = default(global::System.Collections.Generic.List<global::S.ConflictResultPoints>);
            var __TeamRewardId__ = default(string);
            var __LeaderboardRewardId__ = default(string);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __ConflictId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __WinnerTeam__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __MyTeam__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __MyPlace__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 4:
                        __MySector__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 5:
                        __ResultPoints__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::S.ConflictResultPoints>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 6:
                        __TeamRewardId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 7:
                        __LeaderboardRewardId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.ConflictResult();
            ____result.ConflictId = __ConflictId__;
            ____result.WinnerTeam = __WinnerTeam__;
            ____result.MyTeam = __MyTeam__;
            ____result.MyPlace = __MyPlace__;
            ____result.MySector = __MySector__;
            ____result.ResultPoints = __ResultPoints__;
            ____result.TeamRewardId = __TeamRewardId__;
            ____result.LeaderboardRewardId = __LeaderboardRewardId__;
            return ____result;
        }
    }


    public sealed class ModuleDataFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.ModuleData>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.ModuleData value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.ModuleName, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.Data, formatterResolver);
            return offset - startOffset;
        }

        public global::S.ModuleData Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __ModuleName__ = default(string);
            var __Data__ = default(byte[]);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __ModuleName__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Data__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.ModuleData();
            ____result.ModuleName = __ModuleName__;
            ____result.Data = __Data__;
            return ____result;
        }
    }


    public sealed class UserProfileFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::S.UserProfile>
    {

        public int Serialize(ref byte[] bytes, int offset, global::S.UserProfile value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 7);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.UserId, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.CommandSerialNumber);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<string, byte[]>>().Serialize(ref bytes, offset, value.ModulesDict, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Version);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.SerializationVersion);
            offset += MessagePackBinary.WriteUInt32(ref bytes, offset, value.SharedLogicVersion);
            return offset - startOffset;
        }

        public global::S.UserProfile Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __UserId__ = default(byte[]);
            var __CommandSerialNumber__ = default(int);
            var __ModulesDict__ = default(global::System.Collections.Generic.Dictionary<string, byte[]>);
            var __Version__ = default(int);
            var __SerializationVersion__ = default(int);
            var __SharedLogicVersion__ = default(uint);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __UserId__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __CommandSerialNumber__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 3:
                        __ModulesDict__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<string, byte[]>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __Version__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 5:
                        __SerializationVersion__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 6:
                        __SharedLogicVersion__ = MessagePackBinary.ReadUInt32(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::S.UserProfile();
            ____result.UserId = __UserId__;
            ____result.CommandSerialNumber = __CommandSerialNumber__;
            ____result.ModulesDict = __ModulesDict__;
            ____result.Version = __Version__;
            ____result.SerializationVersion = __SerializationVersion__;
            ____result.SharedLogicVersion = __SharedLogicVersion__;
            return ____result;
        }
    }

}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace MessagePack.Formatters.PestelLib.SharedLogic.Modules
{
    using System;
    using MessagePack;


    public sealed class CompositeModuleStateFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::PestelLib.SharedLogic.Modules.CompositeModuleState>
    {

        public int Serialize(ref byte[] bytes, int offset, global::PestelLib.SharedLogic.Modules.CompositeModuleState value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 1);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<string, byte[]>>().Serialize(ref bytes, offset, value.ModulesDict, formatterResolver);
            return offset - startOffset;
        }

        public global::PestelLib.SharedLogic.Modules.CompositeModuleState Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __ModulesDict__ = default(global::System.Collections.Generic.Dictionary<string, byte[]>);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __ModulesDict__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<string, byte[]>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::PestelLib.SharedLogic.Modules.CompositeModuleState();
            ____result.ModulesDict = __ModulesDict__;
            return ____result;
        }
    }


    public sealed class EmptyStateFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::PestelLib.SharedLogic.Modules.EmptyState>
    {

        public int Serialize(ref byte[] bytes, int offset, global::PestelLib.SharedLogic.Modules.EmptyState value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 0);
            return offset - startOffset;
        }

        public global::PestelLib.SharedLogic.Modules.EmptyState Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;


            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::PestelLib.SharedLogic.Modules.EmptyState();
            return ____result;
        }
    }

}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace MessagePack.Formatters.PestelLib.ServerShared
{
    using System;
    using MessagePack;


    public sealed class ServerRequestFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::PestelLib.ServerShared.ServerRequest>
    {

        public int Serialize(ref byte[] bytes, int offset, global::PestelLib.ServerShared.ServerRequest value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 5);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<global::S.Request>().Serialize(ref bytes, offset, value.Request, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.Data, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.State, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.HostAddr, formatterResolver);
            return offset - startOffset;
        }

        public global::PestelLib.ServerShared.ServerRequest Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Request__ = default(global::S.Request);
            var __Data__ = default(byte[]);
            var __State__ = default(byte[]);
            var __HostAddr__ = default(string);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __Request__ = formatterResolver.GetFormatterWithVerify<global::S.Request>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Data__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __State__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __HostAddr__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::PestelLib.ServerShared.ServerRequest();
            ____result.Request = __Request__;
            ____result.Data = __Data__;
            ____result.State = __State__;
            ____result.HostAddr = __HostAddr__;
            return ____result;
        }
    }


    public sealed class ServerResponseFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::PestelLib.ServerShared.ServerResponse>
    {

        public int Serialize(ref byte[] bytes, int offset, global::PestelLib.ServerShared.ServerResponse value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 7);
            offset += formatterResolver.GetFormatterWithVerify<global::S.ResponseCode>().Serialize(ref bytes, offset, value.ResponseCode, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Guid>().Serialize(ref bytes, offset, value.PlayerId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.Data, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.ActualUserProfile, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.DebugInfo, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.Token, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.ShortId);
            return offset - startOffset;
        }

        public global::PestelLib.ServerShared.ServerResponse Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __ResponseCode__ = default(global::S.ResponseCode);
            var __PlayerId__ = default(global::System.Guid);
            var __Data__ = default(byte[]);
            var __ActualUserProfile__ = default(byte[]);
            var __DebugInfo__ = default(string);
            var __Token__ = default(byte[]);
            var __ShortId__ = default(int);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __ResponseCode__ = formatterResolver.GetFormatterWithVerify<global::S.ResponseCode>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __PlayerId__ = formatterResolver.GetFormatterWithVerify<global::System.Guid>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Data__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __ActualUserProfile__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __DebugInfo__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 5:
                        __Token__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 6:
                        __ShortId__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::PestelLib.ServerShared.ServerResponse();
            ____result.ResponseCode = __ResponseCode__;
            ____result.PlayerId = __PlayerId__;
            ____result.Data = __Data__;
            ____result.ActualUserProfile = __ActualUserProfile__;
            ____result.DebugInfo = __DebugInfo__;
            ____result.Token = __Token__;
            ____result.ShortId = __ShortId__;
            return ____result;
        }
    }

}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace MessagePack.Formatters.ServerShared.GlobalConflict
{
    using System;
    using MessagePack;


    public sealed class TeamPlayersStatFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::ServerShared.GlobalConflict.TeamPlayersStat>
    {

        public int Serialize(ref byte[] bytes, int offset, global::ServerShared.GlobalConflict.TeamPlayersStat value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 4);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.ConflictId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string[]>().Serialize(ref bytes, offset, value.Teams, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<int[]>().Serialize(ref bytes, offset, value.PlayersCount, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<int[]>().Serialize(ref bytes, offset, value.GeneralsCount, formatterResolver);
            return offset - startOffset;
        }

        public global::ServerShared.GlobalConflict.TeamPlayersStat Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __ConflictId__ = default(string);
            var __Teams__ = default(string[]);
            var __PlayersCount__ = default(int[]);
            var __GeneralsCount__ = default(int[]);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __ConflictId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Teams__ = formatterResolver.GetFormatterWithVerify<string[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __PlayersCount__ = formatterResolver.GetFormatterWithVerify<int[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __GeneralsCount__ = formatterResolver.GetFormatterWithVerify<int[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::ServerShared.GlobalConflict.TeamPlayersStat();
            ____result.ConflictId = __ConflictId__;
            ____result.Teams = __Teams__;
            ____result.PlayersCount = __PlayersCount__;
            ____result.GeneralsCount = __GeneralsCount__;
            return ____result;
        }
    }


    public sealed class DonationBonusLevelsFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::ServerShared.GlobalConflict.DonationBonusLevels>
    {

        public int Serialize(ref byte[] bytes, int offset, global::ServerShared.GlobalConflict.DonationBonusLevels value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 3);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Level);
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value.Team);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Threshold);
            return offset - startOffset;
        }

        public global::ServerShared.GlobalConflict.DonationBonusLevels Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Level__ = default(int);
            var __Team__ = default(bool);
            var __Threshold__ = default(int);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __Level__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 1:
                        __Team__ = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        break;
                    case 2:
                        __Threshold__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::ServerShared.GlobalConflict.DonationBonusLevels();
            ____result.Level = __Level__;
            ____result.Team = __Team__;
            ____result.Threshold = __Threshold__;
            return ____result;
        }
    }


    public sealed class DonationBonusFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::ServerShared.GlobalConflict.DonationBonus>
    {

        public int Serialize(ref byte[] bytes, int offset, global::ServerShared.GlobalConflict.DonationBonus value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 5);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.ClientType, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.DonationBonusType>().Serialize(ref bytes, offset, value.ServerType, formatterResolver);
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value.Team);
            offset += formatterResolver.GetFormatterWithVerify<decimal>().Serialize(ref bytes, offset, value.Value, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Level);
            return offset - startOffset;
        }

        public global::ServerShared.GlobalConflict.DonationBonus Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __ClientType__ = default(string);
            var __ServerType__ = default(global::ServerShared.GlobalConflict.DonationBonusType);
            var __Team__ = default(bool);
            var __Value__ = default(decimal);
            var __Level__ = default(int);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __ClientType__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __ServerType__ = formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.DonationBonusType>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Team__ = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        break;
                    case 3:
                        __Value__ = formatterResolver.GetFormatterWithVerify<decimal>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __Level__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::ServerShared.GlobalConflict.DonationBonus();
            ____result.ClientType = __ClientType__;
            ____result.ServerType = __ServerType__;
            ____result.Team = __Team__;
            ____result.Value = __Value__;
            ____result.Level = __Level__;
            return ____result;
        }
    }


    public sealed class NodeStateFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::ServerShared.GlobalConflict.NodeState>
    {

        public int Serialize(ref byte[] bytes, int offset, global::ServerShared.GlobalConflict.NodeState value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteArrayHeader(ref bytes, offset, 22);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Id);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.GameMode, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.GameMap, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.BaseForTeam, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.WinPoints);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.LosePoints);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.NeutralThreshold);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.CaptureThreshold);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.CaptureBonus);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.CaptureLimit);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.ResultPoints);
            offset += MessagePackBinary.WriteSingle(ref bytes, offset, value.SupportBonus);
            offset += MessagePackBinary.WriteSingle(ref bytes, offset, value.BattleBonus);
            offset += MessagePackBinary.WriteSingle(ref bytes, offset, value.RewardBonus);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.ContentBonus, formatterResolver);
            offset += MessagePackBinary.WriteSingle(ref bytes, offset, value.PositionX);
            offset += MessagePackBinary.WriteSingle(ref bytes, offset, value.PositionY);
            offset += formatterResolver.GetFormatterWithVerify<int[]>().Serialize(ref bytes, offset, value.LinkedNodes, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.NodeStatus>().Serialize(ref bytes, offset, value.NodeStatus, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<string, int>>().Serialize(ref bytes, offset, value.TeamPoints, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Owner, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.PointOfInterestId, formatterResolver);
            return offset - startOffset;
        }

        public global::ServerShared.GlobalConflict.NodeState Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Id__ = default(int);
            var __GameMode__ = default(string);
            var __GameMap__ = default(string);
            var __BaseForTeam__ = default(string);
            var __WinPoints__ = default(int);
            var __LosePoints__ = default(int);
            var __NeutralThreshold__ = default(int);
            var __CaptureThreshold__ = default(int);
            var __CaptureBonus__ = default(int);
            var __CaptureLimit__ = default(int);
            var __ResultPoints__ = default(int);
            var __SupportBonus__ = default(float);
            var __BattleBonus__ = default(float);
            var __RewardBonus__ = default(float);
            var __ContentBonus__ = default(string);
            var __PositionX__ = default(float);
            var __PositionY__ = default(float);
            var __LinkedNodes__ = default(int[]);
            var __NodeStatus__ = default(global::ServerShared.GlobalConflict.NodeStatus);
            var __TeamPoints__ = default(global::System.Collections.Generic.Dictionary<string, int>);
            var __Owner__ = default(string);
            var __PointOfInterestId__ = default(string);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __Id__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 1:
                        __GameMode__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __GameMap__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __BaseForTeam__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __WinPoints__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 5:
                        __LosePoints__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 6:
                        __NeutralThreshold__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 7:
                        __CaptureThreshold__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 8:
                        __CaptureBonus__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 9:
                        __CaptureLimit__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 10:
                        __ResultPoints__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 11:
                        __SupportBonus__ = MessagePackBinary.ReadSingle(bytes, offset, out readSize);
                        break;
                    case 12:
                        __BattleBonus__ = MessagePackBinary.ReadSingle(bytes, offset, out readSize);
                        break;
                    case 13:
                        __RewardBonus__ = MessagePackBinary.ReadSingle(bytes, offset, out readSize);
                        break;
                    case 14:
                        __ContentBonus__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 15:
                        __PositionX__ = MessagePackBinary.ReadSingle(bytes, offset, out readSize);
                        break;
                    case 16:
                        __PositionY__ = MessagePackBinary.ReadSingle(bytes, offset, out readSize);
                        break;
                    case 17:
                        __LinkedNodes__ = formatterResolver.GetFormatterWithVerify<int[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 18:
                        __NodeStatus__ = formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.NodeStatus>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 19:
                        __TeamPoints__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<string, int>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 20:
                        __Owner__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 21:
                        __PointOfInterestId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::ServerShared.GlobalConflict.NodeState();
            ____result.Id = __Id__;
            ____result.GameMode = __GameMode__;
            ____result.GameMap = __GameMap__;
            ____result.BaseForTeam = __BaseForTeam__;
            ____result.WinPoints = __WinPoints__;
            ____result.LosePoints = __LosePoints__;
            ____result.NeutralThreshold = __NeutralThreshold__;
            ____result.CaptureThreshold = __CaptureThreshold__;
            ____result.CaptureBonus = __CaptureBonus__;
            ____result.CaptureLimit = __CaptureLimit__;
            ____result.ResultPoints = __ResultPoints__;
            ____result.SupportBonus = __SupportBonus__;
            ____result.BattleBonus = __BattleBonus__;
            ____result.RewardBonus = __RewardBonus__;
            ____result.ContentBonus = __ContentBonus__;
            ____result.PositionX = __PositionX__;
            ____result.PositionY = __PositionY__;
            ____result.LinkedNodes = __LinkedNodes__;
            ____result.NodeStatus = __NodeStatus__;
            ____result.TeamPoints = __TeamPoints__;
            ____result.Owner = __Owner__;
            ____result.PointOfInterestId = __PointOfInterestId__;
            return ____result;
        }
    }


    public sealed class MapStateFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::ServerShared.GlobalConflict.MapState>
    {

        public int Serialize(ref byte[] bytes, int offset, global::ServerShared.GlobalConflict.MapState value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.TextureId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.NodeState[]>().Serialize(ref bytes, offset, value.Nodes, formatterResolver);
            return offset - startOffset;
        }

        public global::ServerShared.GlobalConflict.MapState Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __TextureId__ = default(string);
            var __Nodes__ = default(global::ServerShared.GlobalConflict.NodeState[]);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __TextureId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Nodes__ = formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.NodeState[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::ServerShared.GlobalConflict.MapState();
            ____result.TextureId = __TextureId__;
            ____result.Nodes = __Nodes__;
            return ____result;
        }
    }


    public sealed class StageDefFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::ServerShared.GlobalConflict.StageDef>
    {

        public int Serialize(ref byte[] bytes, int offset, global::ServerShared.GlobalConflict.StageDef value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.StageType>().Serialize(ref bytes, offset, value.Id, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.TimeSpan>().Serialize(ref bytes, offset, value.Period, formatterResolver);
            return offset - startOffset;
        }

        public global::ServerShared.GlobalConflict.StageDef Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Id__ = default(global::ServerShared.GlobalConflict.StageType);
            var __Period__ = default(global::System.TimeSpan);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __Id__ = formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.StageType>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Period__ = formatterResolver.GetFormatterWithVerify<global::System.TimeSpan>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::ServerShared.GlobalConflict.StageDef();
            ____result.Id = __Id__;
            ____result.Period = __Period__;
            return ____result;
        }
    }


    public sealed class PointOfInterestBonusFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::ServerShared.GlobalConflict.PointOfInterestBonus>
    {

        public int Serialize(ref byte[] bytes, int offset, global::ServerShared.GlobalConflict.PointOfInterestBonus value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 3);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.ClientType, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.PointOfInterestServerLogic>().Serialize(ref bytes, offset, value.ServerType, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<decimal>().Serialize(ref bytes, offset, value.Value, formatterResolver);
            return offset - startOffset;
        }

        public global::ServerShared.GlobalConflict.PointOfInterestBonus Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __ClientType__ = default(string);
            var __ServerType__ = default(global::ServerShared.GlobalConflict.PointOfInterestServerLogic);
            var __Value__ = default(decimal);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __ClientType__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __ServerType__ = formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.PointOfInterestServerLogic>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Value__ = formatterResolver.GetFormatterWithVerify<decimal>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::ServerShared.GlobalConflict.PointOfInterestBonus();
            ____result.ClientType = __ClientType__;
            ____result.ServerType = __ServerType__;
            ____result.Value = __Value__;
            return ____result;
        }
    }


    public sealed class PointOfInterestFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::ServerShared.GlobalConflict.PointOfInterest>
    {

        public int Serialize(ref byte[] bytes, int offset, global::ServerShared.GlobalConflict.PointOfInterest value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 11);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Id, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.NodeId);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.OwnerTeam, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.TimeSpan>().Serialize(ref bytes, offset, value.BonusTime, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.TimeSpan>().Serialize(ref bytes, offset, value.DeployCooldown, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Serialize(ref bytes, offset, value.BonusExpiryDate, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Serialize(ref bytes, offset, value.NextDeploy, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.GeneralLevel);
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value.Auto);
            offset += formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.PointOfInterestBonus[]>().Serialize(ref bytes, offset, value.Bonuses, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Type, formatterResolver);
            return offset - startOffset;
        }

        public global::ServerShared.GlobalConflict.PointOfInterest Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Id__ = default(string);
            var __NodeId__ = default(int);
            var __OwnerTeam__ = default(string);
            var __BonusTime__ = default(global::System.TimeSpan);
            var __DeployCooldown__ = default(global::System.TimeSpan);
            var __BonusExpiryDate__ = default(global::System.DateTime);
            var __NextDeploy__ = default(global::System.DateTime);
            var __GeneralLevel__ = default(int);
            var __Auto__ = default(bool);
            var __Bonuses__ = default(global::ServerShared.GlobalConflict.PointOfInterestBonus[]);
            var __Type__ = default(string);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __Id__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __NodeId__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 2:
                        __OwnerTeam__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __BonusTime__ = formatterResolver.GetFormatterWithVerify<global::System.TimeSpan>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __DeployCooldown__ = formatterResolver.GetFormatterWithVerify<global::System.TimeSpan>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 5:
                        __BonusExpiryDate__ = formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 6:
                        __NextDeploy__ = formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 7:
                        __GeneralLevel__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 8:
                        __Auto__ = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        break;
                    case 9:
                        __Bonuses__ = formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.PointOfInterestBonus[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 10:
                        __Type__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::ServerShared.GlobalConflict.PointOfInterest();
            ____result.Id = __Id__;
            ____result.NodeId = __NodeId__;
            ____result.OwnerTeam = __OwnerTeam__;
            ____result.BonusTime = __BonusTime__;
            ____result.DeployCooldown = __DeployCooldown__;
            ____result.BonusExpiryDate = __BonusExpiryDate__;
            ____result.NextDeploy = __NextDeploy__;
            ____result.GeneralLevel = __GeneralLevel__;
            ____result.Auto = __Auto__;
            ____result.Bonuses = __Bonuses__;
            ____result.Type = __Type__;
            return ____result;
        }
    }


    public sealed class NodeQuestFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::ServerShared.GlobalConflict.NodeQuest>
    {

        public int Serialize(ref byte[] bytes, int offset, global::ServerShared.GlobalConflict.NodeQuest value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 8);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Id, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.QuestLevel);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.ClientType, formatterResolver);
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value.Auto);
            offset += formatterResolver.GetFormatterWithVerify<global::System.TimeSpan>().Serialize(ref bytes, offset, value.ActiveTime, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.TimeSpan>().Serialize(ref bytes, offset, value.DeployCooldown, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.RewardId, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Weight);
            return offset - startOffset;
        }

        public global::ServerShared.GlobalConflict.NodeQuest Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Id__ = default(string);
            var __QuestLevel__ = default(int);
            var __ClientType__ = default(string);
            var __Auto__ = default(bool);
            var __ActiveTime__ = default(global::System.TimeSpan);
            var __DeployCooldown__ = default(global::System.TimeSpan);
            var __RewardId__ = default(string);
            var __Weight__ = default(int);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __Id__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __QuestLevel__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 2:
                        __ClientType__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __Auto__ = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        break;
                    case 4:
                        __ActiveTime__ = formatterResolver.GetFormatterWithVerify<global::System.TimeSpan>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 5:
                        __DeployCooldown__ = formatterResolver.GetFormatterWithVerify<global::System.TimeSpan>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 6:
                        __RewardId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 7:
                        __Weight__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::ServerShared.GlobalConflict.NodeQuest();
            ____result.Id = __Id__;
            ____result.QuestLevel = __QuestLevel__;
            ____result.ClientType = __ClientType__;
            ____result.Auto = __Auto__;
            ____result.ActiveTime = __ActiveTime__;
            ____result.DeployCooldown = __DeployCooldown__;
            ____result.RewardId = __RewardId__;
            ____result.Weight = __Weight__;
            return ____result;
        }
    }


    public sealed class PlayerStateFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::ServerShared.GlobalConflict.PlayerState>
    {

        public int Serialize(ref byte[] bytes, int offset, global::ServerShared.GlobalConflict.PlayerState value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 10);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Id, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.ConflictId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.TeamId, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.WinPoints);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.DonationPoints);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.GeneralLevel);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.DonationBonus[]>().Serialize(ref bytes, offset, value.DonationBonuses, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Serialize(ref bytes, offset, value.RegisterTime, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Name, formatterResolver);
            return offset - startOffset;
        }

        public global::ServerShared.GlobalConflict.PlayerState Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Id__ = default(string);
            var __ConflictId__ = default(string);
            var __TeamId__ = default(string);
            var __WinPoints__ = default(int);
            var __DonationPoints__ = default(int);
            var __GeneralLevel__ = default(int);
            var __DonationBonuses__ = default(global::ServerShared.GlobalConflict.DonationBonus[]);
            var __RegisterTime__ = default(global::System.DateTime);
            var __Name__ = default(string);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __Id__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __ConflictId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __TeamId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __WinPoints__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 4:
                        __DonationPoints__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 5:
                        __GeneralLevel__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 7:
                        __DonationBonuses__ = formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.DonationBonus[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 8:
                        __RegisterTime__ = formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 9:
                        __Name__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::ServerShared.GlobalConflict.PlayerState();
            ____result.Id = __Id__;
            ____result.ConflictId = __ConflictId__;
            ____result.TeamId = __TeamId__;
            ____result.WinPoints = __WinPoints__;
            ____result.DonationPoints = __DonationPoints__;
            ____result.GeneralLevel = __GeneralLevel__;
            ____result.DonationBonuses = __DonationBonuses__;
            ____result.RegisterTime = __RegisterTime__;
            ____result.Name = __Name__;
            return ____result;
        }
    }


    public sealed class TeamStateFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::ServerShared.GlobalConflict.TeamState>
    {

        public int Serialize(ref byte[] bytes, int offset, global::ServerShared.GlobalConflict.TeamState value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 5);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Id, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.WinPoints);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.ResultPoints);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.DonationPoints);
            offset += formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.DonationBonus[]>().Serialize(ref bytes, offset, value.DonationBonuses, formatterResolver);
            return offset - startOffset;
        }

        public global::ServerShared.GlobalConflict.TeamState Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Id__ = default(string);
            var __WinPoints__ = default(int);
            var __ResultPoints__ = default(int);
            var __DonationPoints__ = default(int);
            var __DonationBonuses__ = default(global::ServerShared.GlobalConflict.DonationBonus[]);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __Id__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __WinPoints__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 2:
                        __ResultPoints__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 3:
                        __DonationPoints__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 4:
                        __DonationBonuses__ = formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.DonationBonus[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::ServerShared.GlobalConflict.TeamState();
            ____result.Id = __Id__;
            ____result.WinPoints = __WinPoints__;
            ____result.ResultPoints = __ResultPoints__;
            ____result.DonationPoints = __DonationPoints__;
            ____result.DonationBonuses = __DonationBonuses__;
            return ____result;
        }
    }


    public sealed class GlobalConflictStateFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::ServerShared.GlobalConflict.GlobalConflictState>
    {

        public int Serialize(ref byte[] bytes, int offset, global::ServerShared.GlobalConflict.GlobalConflictState value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteArrayHeader(ref bytes, offset, 20);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.PrizePlaces);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.PrizesCount);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Id, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string[]>().Serialize(ref bytes, offset, value.Teams, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.TeamAssignType>().Serialize(ref bytes, offset, value.AssignType, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.CaptureTime);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.LastRound);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.BattleCost);
            offset += formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.MapState>().Serialize(ref bytes, offset, value.Map, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.DonationBonusLevels[]>().Serialize(ref bytes, offset, value.DonationBonusLevels, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.DonationBonus[]>().Serialize(ref bytes, offset, value.DonationBonuses, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.StageDef[]>().Serialize(ref bytes, offset, value.Stages, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.GeneralsCount);
            offset += formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.PointOfInterest[]>().Serialize(ref bytes, offset, value.PointsOfInterest, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.MaxPointsOfInterestAtNode);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.MaxSameTypePointsOfInterestAtNode);
            offset += formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.NodeQuest[]>().Serialize(ref bytes, offset, value.Quests, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.TeamState[]>().Serialize(ref bytes, offset, value.TeamsStates, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Serialize(ref bytes, offset, value.StartTime, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Serialize(ref bytes, offset, value.EndTime, formatterResolver);
            return offset - startOffset;
        }

        public global::ServerShared.GlobalConflict.GlobalConflictState Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __PrizePlaces__ = default(int);
            var __PrizesCount__ = default(int);
            var __Id__ = default(string);
            var __Teams__ = default(string[]);
            var __AssignType__ = default(global::ServerShared.GlobalConflict.TeamAssignType);
            var __CaptureTime__ = default(int);
            var __LastRound__ = default(int);
            var __BattleCost__ = default(int);
            var __Map__ = default(global::ServerShared.GlobalConflict.MapState);
            var __DonationBonusLevels__ = default(global::ServerShared.GlobalConflict.DonationBonusLevels[]);
            var __DonationBonuses__ = default(global::ServerShared.GlobalConflict.DonationBonus[]);
            var __Stages__ = default(global::ServerShared.GlobalConflict.StageDef[]);
            var __GeneralsCount__ = default(int);
            var __PointsOfInterest__ = default(global::ServerShared.GlobalConflict.PointOfInterest[]);
            var __MaxPointsOfInterestAtNode__ = default(int);
            var __MaxSameTypePointsOfInterestAtNode__ = default(int);
            var __Quests__ = default(global::ServerShared.GlobalConflict.NodeQuest[]);
            var __TeamsStates__ = default(global::ServerShared.GlobalConflict.TeamState[]);
            var __StartTime__ = default(global::System.DateTime);
            var __EndTime__ = default(global::System.DateTime);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __PrizePlaces__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 1:
                        __PrizesCount__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 2:
                        __Id__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __Teams__ = formatterResolver.GetFormatterWithVerify<string[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __AssignType__ = formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.TeamAssignType>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 5:
                        __CaptureTime__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 6:
                        __LastRound__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 7:
                        __BattleCost__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 8:
                        __Map__ = formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.MapState>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 9:
                        __DonationBonusLevels__ = formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.DonationBonusLevels[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 10:
                        __DonationBonuses__ = formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.DonationBonus[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 11:
                        __Stages__ = formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.StageDef[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 12:
                        __GeneralsCount__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 13:
                        __PointsOfInterest__ = formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.PointOfInterest[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 14:
                        __MaxPointsOfInterestAtNode__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 15:
                        __MaxSameTypePointsOfInterestAtNode__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 16:
                        __Quests__ = formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.NodeQuest[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 17:
                        __TeamsStates__ = formatterResolver.GetFormatterWithVerify<global::ServerShared.GlobalConflict.TeamState[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 18:
                        __StartTime__ = formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 19:
                        __EndTime__ = formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::ServerShared.GlobalConflict.GlobalConflictState();
            ____result.PrizePlaces = __PrizePlaces__;
            ____result.PrizesCount = __PrizesCount__;
            ____result.Id = __Id__;
            ____result.Teams = __Teams__;
            ____result.AssignType = __AssignType__;
            ____result.CaptureTime = __CaptureTime__;
            ____result.LastRound = __LastRound__;
            ____result.BattleCost = __BattleCost__;
            ____result.Map = __Map__;
            ____result.DonationBonusLevels = __DonationBonusLevels__;
            ____result.DonationBonuses = __DonationBonuses__;
            ____result.Stages = __Stages__;
            ____result.GeneralsCount = __GeneralsCount__;
            ____result.PointsOfInterest = __PointsOfInterest__;
            ____result.MaxPointsOfInterestAtNode = __MaxPointsOfInterestAtNode__;
            ____result.MaxSameTypePointsOfInterestAtNode = __MaxSameTypePointsOfInterestAtNode__;
            ____result.Quests = __Quests__;
            ____result.TeamsStates = __TeamsStates__;
            ____result.StartTime = __StartTime__;
            ____result.EndTime = __EndTime__;
            return ____result;
        }
    }

}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace MessagePack.Formatters.PestelLib.SharedLogicBase
{
    using System;
    using MessagePack;


    public sealed class CommandFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::PestelLib.SharedLogicBase.Command>
    {

        public int Serialize(ref byte[] bytes, int offset, global::PestelLib.SharedLogicBase.Command value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 3);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.Timestamp);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.SerialNumber);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.SerializedCommandData, formatterResolver);
            return offset - startOffset;
        }

        public global::PestelLib.SharedLogicBase.Command Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Timestamp__ = default(long);
            var __SerialNumber__ = default(int);
            var __SerializedCommandData__ = default(byte[]);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __Timestamp__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 1:
                        __SerialNumber__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 2:
                        __SerializedCommandData__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::PestelLib.SharedLogicBase.Command();
            ____result.Timestamp = __Timestamp__;
            ____result.SerialNumber = __SerialNumber__;
            ____result.SerializedCommandData = __SerializedCommandData__;
            return ____result;
        }
    }


    public sealed class CommandBatchFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::PestelLib.SharedLogicBase.CommandBatch>
    {

        public int Serialize(ref byte[] bytes, int offset, global::PestelLib.SharedLogicBase.CommandBatch value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 5);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::PestelLib.SharedLogicBase.Command>>().Serialize(ref bytes, offset, value.commandsList, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.controlHash);
            offset += MessagePackBinary.WriteUInt32(ref bytes, offset, value.SharedLogicCrc);
            offset += MessagePackBinary.WriteUInt32(ref bytes, offset, value.DefinitionsVersion);
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value.IsEditor);
            return offset - startOffset;
        }

        public global::PestelLib.SharedLogicBase.CommandBatch Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __commandsList__ = default(global::System.Collections.Generic.List<global::PestelLib.SharedLogicBase.Command>);
            var __controlHash__ = default(int);
            var __SharedLogicCrc__ = default(uint);
            var __DefinitionsVersion__ = default(uint);
            var __IsEditor__ = default(bool);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __commandsList__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::PestelLib.SharedLogicBase.Command>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __controlHash__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 2:
                        __SharedLogicCrc__ = MessagePackBinary.ReadUInt32(bytes, offset, out readSize);
                        break;
                    case 3:
                        __DefinitionsVersion__ = MessagePackBinary.ReadUInt32(bytes, offset, out readSize);
                        break;
                    case 4:
                        __IsEditor__ = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::PestelLib.SharedLogicBase.CommandBatch();
            ____result.commandsList = __commandsList__;
            ____result.controlHash = __controlHash__;
            ____result.SharedLogicCrc = __SharedLogicCrc__;
            ____result.DefinitionsVersion = __DefinitionsVersion__;
            ____result.IsEditor = __IsEditor__;
            return ____result;
        }
    }

}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
