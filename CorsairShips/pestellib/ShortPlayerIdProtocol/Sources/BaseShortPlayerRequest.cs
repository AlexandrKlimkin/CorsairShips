using System;
using MessagePack;

namespace ShortPlayerIdProtocol
{
    [Union(0, typeof(GetPlayerIdRequest))]
    [Union(1, typeof(GetShortPlayerIdRequest))]
    [MessagePackObject]
    public abstract class BaseShortPlayerRequest
    {
    }
}
