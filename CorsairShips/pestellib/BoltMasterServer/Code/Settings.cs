using System;

namespace BoltMasterServer
{
    internal class Settings
    {        
        /// <summary>   Максимальное кол-во игровых серверов на 1 мастер сервер. </summary>
        public int MaxServers = 50;
        
        /// <summary>   Путь к юнити билду сервера </summary>
        public string BuildFilePath = "PhotonBoltTest.exe";

        /// <summary>   Адрес игрового сервера, он же и адрес мастер сервера </summary>
        public string MasterServerIp = "127.0.0.1";

        /// <summary>
        /// Каждый мастер создаёт игровые сервера в своём диапазоне портов от FirstGameServerPort до
        /// FirstGameServerPort + MaxServers.
        /// </summary>
        public int FirstGameServerPort;

        /// <summary>
        /// Как долго игровое сервер может работать с 0 игроков. После этого периода он блокируется.
        /// Фактический shutdown выполняется позже т.к. какие-то игроки могут быть в процессе подключения
        /// к этому серверу, если кто-то зайдет между InstanceBlockTimeout и InstanceShutdownTimeout
        /// сервер будет разлокирован для подключения других игроков.
        /// </summary>
        public readonly TimeSpan InstanceBlockTimeout = TimeSpan.FromSeconds(20);

        /// <summary>
        /// Время, после которого игровой сервер, оставшийся без игроков, будет отключен. Должно быть
        /// больше, чем InstanceBlockTimeout.
        /// </summary>
        public readonly TimeSpan InstanceShutdownTimeout = TimeSpan.FromSeconds(40);

        /// <summary>
        /// Максимальное допустимое время, в течении которого можно не получать сообщения от игрового
        /// сервера. Если оно выйдет - значит сервер завис и он немедленно останавливается.
        /// </summary>
        public readonly TimeSpan MaximumAllowedTimeWithoutReports = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Лоадбалансинг 1, мастер серверов много. И зачастую лоадбалансинг запущен на другой машине,
        /// поэтому нужно знать его ip адрес.
        /// </summary>
        public string LoadBalancingIp = "127.0.0.1";
        public int LoadBalancingPort = 9999;

        /// <summary>   Порт, на котором мастер сервер случает подключения игровых серверов. </summary>
        public int MasterListenerPort;

        /// <summary>   Имя игры - например Submarines, DesertWars etc. </summary>
        public string GameName = "Face of War";

        /// <summary>
        /// Версия игры - для одной игры может быть запущенно несколько мастер серверов разных версий.
        /// </summary>
        public string GameVersion = "0.6";

        public string AgonesFleetName = "fleet-v6";

        public bool EnabledUnityGameServerLogsInConsole = false;

        public string GraphiteHost = "pestelcrew.com";

        public int GraphitePort = 2003;

        public bool Test;
    }
}
