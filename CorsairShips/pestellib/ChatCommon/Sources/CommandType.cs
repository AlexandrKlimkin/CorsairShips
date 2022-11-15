//using ProtoBuf;
using System;

namespace PestelLib.ChatCommon
{
    // нельзя обфусцировать используется как JSON
    [System.Reflection.Obfuscation(Exclude = true)]
    public enum CommandType
    {
        // client => server commands
        Message = 0,
        ClientLoginInform = 1,
        ClientLogOffInform = 2,
        SendClientList = 3,
        SendHistory = 4,
        JoinChannel = 5,                // join user to specific channel
        LeaveChannel = 6,
        SendPrivateHistory = 7,
        MessageFilterReport = 8,
        ListChannels = 9,
        GetBanInfo = 10,

        // server => client commands
        LoginResult = 100,              // 
        StopFlood = 101,                // client messages are discarded and not be delivered for timeout 
        ClientsChanged = 102,           // eg somebody has left the channel
        JoinedChannel = 103,
        LeftChannel = 104,
        BanGranted = 105,
        ServiceMessage = 106,
        BanInfo = 107,

        // privileged client => server
        BanUser = 200,

        // server => server
        AddSuperByAuthData = 301,
        RemoveSuperByAuthData = 302,
    }
}
