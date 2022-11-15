using System;
using System.Threading.Tasks;
using UnityDI;
using ShortPlayerId.Storage;
using log4net;

namespace BackendCommon.Code.Utils
{
    public static class PlayerIdHelper
    {
        /// <summary>
        /// </summary>
        /// <param name="playerId">GUID или в формате короткого id. <see cref="ShortPlayerId.Storage.ShortPlayerIdStorage"></param>
        /// <returns></returns>
        public static Guid FromString(string playerId)
        {
            Guid guid;

            if (int.TryParse(playerId, out var shortPlayerId) && _shortPlayerIdStorage != null)
            {
                try
                {
                    var task = Task.Run(async () => await _shortPlayerIdStorage.GetFullPlayerId(shortPlayerId));
                    return task.ConfigureAwait(false).GetAwaiter().GetResult();
                }
                catch
                { }
            }

            if (!Guid.TryParse(playerId, out guid))
            {
                return Guid.Empty;
            }

            return guid;
        }

        private static ShortPlayerIdStorage _shortPlayerIdStorage = ContainerHolder.Container.Resolve<ShortPlayerIdStorage>();
        private static ILog Log = LogManager.GetLogger(typeof(PlayerIdHelper));
    }
}
