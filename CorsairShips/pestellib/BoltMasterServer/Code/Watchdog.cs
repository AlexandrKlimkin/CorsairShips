using log4net;
using System;
using System.Threading.Tasks;

namespace BoltMasterServer
{
    /// <summary>
    /// Watchdog останавливает игровые сервера, которые зависли и перестали посылать своё состояние
    /// мастеру. По настройке - можно смотреть комментарии в Settings.
    /// Так же останавливает игровые сервера, которые остались без игроков
    /// </summary>
    class Watchdog
    {
        private static ILog _log = LogManager.GetLogger(typeof(Watchdog));

        private Settings settings;
        private IGameServerLauncher gameServerLauncher;
        private GameServersCollection gameServersCollection;

        public Watchdog(Settings settings, IGameServerLauncher gameServerLauncher, GameServersCollection gameServersCollection)
        {
            this.gameServersCollection = gameServersCollection;
            this.gameServerLauncher = gameServerLauncher;
            this.settings = settings;
            Task.Run(() => CheckInstances());
        }

        private async Task CheckInstances()
        {
            while (true)
            {
                try
                {
                    foreach (var server in gameServersCollection)
                    {
                        if (server.Value.TimeSinceUpdate > settings.MaximumAllowedTimeWithoutReports
                            || server.Value.IsItTimeToShutdownServer())
                        {
                            _ = ShutdownInstance(server.Value);
                        }

                    }
                } 
                catch (Exception e)
                {
                    _log.Error(e.Message + " " + e.StackTrace + " " + e.InnerException);
                }

                await Task.Delay(1000);
            }
        }

        private async Task ShutdownInstance(IGameServer s)
        {
            _log.Warn("Trying to kill server: " + s.GameServerId + " it is unresponsive for " + s.TimeSinceUpdate);
            gameServersCollection.TryRemove(s.GameServerId, out var gameServerStateReport);
            s.Shutdown();            
            await Task.Delay(5000);
            gameServerLauncher.KillProcess(s.GameServerId);
        }
    }
}
