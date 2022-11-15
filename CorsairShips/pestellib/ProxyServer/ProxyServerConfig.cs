using System.Collections.Generic;
using PestelLib.ServerCommon.Config;

namespace ProxyServerApp
{
    class ProxyServerConfig
    {
        public int Port = 9090;
        public string MessageQueueConnectionString = "rabbitmq,localhost";
        public Dictionary<int, string> Services;
    }

    static class ProxyServerConfigCache
    {
        public static ProxyServerConfig Get()
        {
            if (_inst == null)
            {
                _inst = SimpleJsonConfigLoader.LoadConfigFromFile<ProxyServerConfig>("ProxyServerConfig.json", false);
                Validate(_inst);
            }

            return _inst;
        }

        private static void Validate(ProxyServerConfig config)
        {
        }

        private static ProxyServerConfig _inst;
    }
}
