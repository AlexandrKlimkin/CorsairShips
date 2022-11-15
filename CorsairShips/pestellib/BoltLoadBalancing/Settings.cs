namespace BoltLoadBalancing
{
    static class Settings
    {
        /// <summary>
        /// Первый используемый порт игрового сервера. Каждый мастер сервер имеет диапазон портов,
        /// который он использует для запускаемых игровых серверов. Этот диапазон он запрашивает у
        /// лоадбалансинга при запуске.
        /// </summary>
        public static int FirstGameServerPort = 25000;

        /// <summary>
        /// Верхняя граница диапазона портов игровых серверов - это FirstGameServerPort
        /// +MaxGameServersPerMaster.
        /// </summary>
        public static int MaxGameServersPerMaster = 100;

        /// <summary>
        /// Лоадбалансинг назначет порты каждому мастер серверу, на которых они слушают игровые сервера.
        /// </summary>
        public static int FirstMasterServerPort = 7777;
        public static int LastMasterServerPort = 7877;
    }
}
