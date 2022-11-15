using System.Collections.Generic;
using ClansClientLib;
using MessageClient;
using MessageServer.Sources;
using PestelLib.UniversalSerializer;

namespace ClansServerLib
{
    partial class ClansServer
    {
        class Dispatcher : MtMessageDispatcher
        {
            public Dispatcher(ClansServer server)
            {
                _server = server;
                _serializer = new BinaryMessagePackSerializer();

                RegisterHandler(0, new AsyncMessageHandler<byte, byte>(_serializer, _server.Ping));
                RegisterHandler((int)ClanMessageType.GetClanByPlayer, new AsyncMessageHandler<GetClanByPlayerRequest, ClanRecord>(_serializer, _server.GetClanByPlayer));
                RegisterHandler((int)ClanMessageType.GetClan, new AsyncMessageHandler<GetClanRequest, ClanRecord>(_serializer, _server.GetClan));
                RegisterHandler((int)ClanMessageType.GetTopClans, new AsyncMessageHandler<GetTopClansRequest, List<ClanRecord>>(_serializer, _server.GetTopClans));
                RegisterHandler((int)ClanMessageType.FindByNameOrTag, new AsyncMessageHandler<FindByNameOrTagRequest, List<ClanRecord>>(_serializer, _server.FindByNameOrTag));
                RegisterHandler((int)ClanMessageType.FindByParamsExact, new AsyncMessageHandler<FindByParamsExactRequest, List<ClanRecord>>(_serializer, _server.FindByParamsExact));
                RegisterHandler((int)ClanMessageType.FindByMaxParams, new AsyncMessageHandler<FindByMaxParamsRequest, List<ClanRecord>>(_serializer, _server.FindByMaxParams));
                RegisterHandler((int)ClanMessageType.CreateClan, new AsyncMessageHandler<CreateClanRequest, CreateClanResult>(_serializer, _server.CreateClan));
                RegisterHandler((int)ClanMessageType.JoinClanRequest, new AsyncMessageHandler<JoinClanRequestRequest, ClanJoinRequestResult>(_serializer, _server.JoinClanRequest));
                RegisterHandler((int)ClanMessageType.GetMyRequests, new AsyncMessageHandler<GetMyRequestsRequest, ClanRequestRecord[]>(_serializer, _server.GetMyRequests));
                RegisterHandler((int)ClanMessageType.GetClanRequests, new AsyncMessageHandler<GetClanRequestsRequest, ClanRequestRecord[]>(_serializer, _server.GetClanRequests));
                RegisterHandler((int)ClanMessageType.AcceptClanRequest, new AsyncMessageHandler<AcceptClanRequestRequest, AcceptRejectClanRequestResult>(_serializer, _server.AcceptClanRequest));
                RegisterHandler((int)ClanMessageType.RejectClanRequest, new AsyncMessageHandler<RejectClanRequestRequest, AcceptRejectClanRequestResult>(_serializer, _server.RejectClanRequest));
                RegisterHandler((int)ClanMessageType.CancelClanRequest, new AsyncMessageHandler<CancelClanRequestRequest, CancelClanRequestResult>(_serializer, _server.CancelClanRequest));
                RegisterHandler((int)ClanMessageType.CancelClanRequests, new AsyncMessageHandler<CancelClanRequestsRequest, CancelClanRequestResult[]>(_serializer, _server.CancelClanRequests));
                RegisterHandler((int)ClanMessageType.TransferOwnership, new AsyncMessageHandler<TransferOwnershipRequest, TransferOwnershipResult>(_serializer, _server.TransferOwnership));
                RegisterHandler((int)ClanMessageType.LeaveClan, new AsyncMessageHandler<LeaveClanRequest, LeaveClanResult>(_serializer, _server.LeaveClan));
                RegisterHandler((int)ClanMessageType.KickClan, new AsyncMessageHandler<KickClanRequest, LeaveClanResult>(_serializer, _server.KickClan));
                RegisterHandler((int)ClanMessageType.SetLevel, new AsyncMessageHandler<SetLevelRequest, bool>(_serializer, _server.SetLevel));
                RegisterHandler((int)ClanMessageType.SetRating, new AsyncMessageHandler<SetRatingRequest, SetClanRatingResult>(_serializer, _server.SetRating));
                RegisterHandler((int)ClanMessageType.SetPlayerRating, new AsyncMessageHandler<SetPlayerRatingRequest, bool>(_serializer, _server.SetPlayerRating));
                RegisterHandler((int)ClanMessageType.ActivateBooster, new AsyncMessageHandler<ActivateBoosterRequest, ActivateBoosterResult>(_serializer, _server.ActivateBooster));
                RegisterHandler((int)ClanMessageType.AddBooster, new AsyncMessageHandler<AddBoosterRequest, bool>(_serializer, _server.AddBooster));
                RegisterHandler((int)ClanMessageType.AddBoosters, new AsyncMessageHandler<AddBoostersRequest, bool>(_serializer, _server.AddBoosters));
                RegisterHandler((int)ClanMessageType.Donate, new AsyncMessageHandler<DonateRequest, bool>(_serializer, _server.Donate));
                RegisterHandler((int)ClanMessageType.SetClanDescription, new AsyncMessageHandler<SetClanDescriptionRequest, SetClanDescriptionResult>(_serializer, _server.SetClanDescription));
                RegisterHandler((int)ClanMessageType.GetDonationsForPeriod, new AsyncMessageHandler<GetDonationsForPeriodRequest, PeriodicClanDonationByPlayer>(_serializer, _server.GetDonationsForPeriod));
                RegisterHandler((int)ClanMessageType.GetClanConsumables, new AsyncMessageHandler<GetClanConsumablesRequest, GetClanConsumablesResult>(_serializer, server.GetClanConsumables));
                RegisterHandler((int)ClanMessageType.GiveConsumable, new AsyncMessageHandler<GiveConsumableRequest, GiveConsumableResult>(_serializer, _server.GiveConsumable));
                RegisterHandler((int)ClanMessageType.TakeConsumable, new AsyncMessageHandler<TakeConsumableRequest, TakeConsumableResult>(_serializer, _server.TakeConsumable));
                RegisterHandler((int)ClanMessageType.ActivateBoosterWithConsumbles, new AsyncMessageHandler<ActivateBoosterRequest, ActivateBoosterResult>(_serializer, _server.ActivateBoosterWithConsumables));
            }

            private ClansServer _server;
            private BinaryMessagePackSerializer _serializer;
        }
    }
}
