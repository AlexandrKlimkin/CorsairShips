using System;
using PestelLib.ServerClientUtils;
using PestelLib.UniversalSerializer;
using S;
using ShortPlayerIdProtocol;
using UnityEngine;

namespace ShortPlayerIdClient
{
    public static class ShortPlayerIdApi
    {
        public static void GetShortPlayerId(this RequestQueue rq, Guid fullPlayerId, Action<int> onResultCallback)
        {
            var myId = rq.PlayerId == fullPlayerId;
            if(myId && rq.ShortId > 0)
            {
                onResultCallback(rq.ShortId);
                return;
            }
            rq.SendRequest("ShortPlayerIdModule", new Request
            {
                ExtensionModuleAsyncRequest = new ExtensionModuleRequest
                {
                    ModuleType = "ShortPlayerIdModule",
                    Request = Serializer.Serialize<BaseShortPlayerRequest>(
                        new GetShortPlayerIdRequest
                        {
                            Guid = fullPlayerId.ToByteArray()
                        }
                    )
                }
            }, (response, dataCollection) =>
            {
                var resp = Serializer.Deserialize<GetShortPlayerIdResponse>(dataCollection.Data);
                Debug.Log("Short player id is " + resp.ShortPlayerId);
                if(myId)
                    rq.ShortId = resp.ShortPlayerId;
                onResultCallback(resp.ShortPlayerId);
            });
        }
    }
}