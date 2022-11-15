using System;
using ClansClientLib;
using ClansServerLib.Mongo;
using MessageServer.Server.Tcp;
using MessageServer.Sources;
using MongoDB.Driver;
using PestelLib.ServerCommon;
using PestelLib.ServerCommon.Db.Auth;
using UnityDI;

namespace ClansServerLib
{
    static class Program
    {
        static void Main(string[] args)
        {
            Log.Init();
            ContainerHolder.Container.RegisterSingleton<ClanRecordCache>();
            ContainerHolder.Container.RegisterSingleton<ClanServerUserBinder>();

            var config = ClansConfigCache.Get();
            TcpMessageProvider messageProvider = new TcpMessageProvider(config.Port, new EnumMessageTypeStringGetter<ClanMessageType>());
            IMessageSender messageSender = messageProvider;
            ContainerHolder.Container.RegisterInstance(messageSender);
            ContainerHolder.Container.RegisterInstance<INotifyPlayers>(new NotifyPlayers());
            var clanDb = new MongoClanDb();
            if (!string.IsNullOrEmpty(config.TokenStorageConnectionString))
            {
                ITokenStore tokenStore = new MongoTokenStorage(new MongoUrl(config.TokenStorageConnectionString));
                ContainerHolder.Container.RegisterInstance(tokenStore);
            }

            ContainerHolder.Container.RegisterInstance<IClanDB>(clanDb);
            ContainerHolder.Container.RegisterInstance<IClansDbPrivate>(clanDb);

            var internalServer = new ClansInternalServer();
            using (new ClansServer(messageProvider))
            {
                messageProvider.Start();
                while (true)
                {
                    var command = Console.ReadLine();
                    if (command == "exit") break;
                }
            }
        }
    }
}
