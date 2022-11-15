using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using PestelLib.MatchmakerServer.Stub;
using ServerLib;

namespace PestelLib.MatchmakerServer
{

    class Program
    {
        static readonly StubServer _matchmakerServer;

        static Program()
        {
            string configPath = AppDomain.CurrentDomain.BaseDirectory + "appsettings.json";
            AppSettings.LoadConfig(configPath);

            // instantiate server
            _matchmakerServer = new StubServer();
        }

        static void Main(string[] args)
        {
            ServerCommon.Log.Init();
            _matchmakerServer.Start();

            //var client = new StubTestClient("localhost", 8500, 0.01f);

            while(true)
                Console.ReadLine();
        }
    }
}
