using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using PestelLib.ChatCommon;

namespace PestelLib.ChatServer
{
    public partial class ChatServer
    {
        private static readonly TimeSpan BigDelay = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan SmallDelay = TimeSpan.FromSeconds(1);
        private TimeSpan _banProcessorNextRun = TimeSpan.Zero;
        private Stopwatch _banProcessorSw = Stopwatch.StartNew();
        
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void ProcessBans()
        {
            if (_banProcessorSw.Elapsed < _banProcessorNextRun)
                return;
            try
            {
                _banProcessorNextRun = BigDelay;

                var banRequestStorage = _banStorageFactory.GetBanRequestStorage();
                var banStorage = _banStorageFactory.Get();

                if (banStorage == null)
                    return;

                var request = banRequestStorage?.GetBanRequest(ChatId);
                if (request == null)
                    return;

                var dt = DateTime.UtcNow;
                _banProcessorNextRun = SmallDelay;
                var expiry = request.CreateTime + request.Period;
                var token = FromPlayerId(request.PlayerId);
                // разбан
                if (request.Period == TimeSpan.Zero && (DateTime.UtcNow - request.CreateTime) < TimeSpan.FromHours(1))
                {
                    Log.Debug($"Processing unban request for player {request.PlayerId}. token={token}.");
                    banStorage.GrantBan(token, BanReason.AdminBan, request.Period);
                }
                else if (expiry > dt)
                {
                    var newPeriod = expiry - dt;
                    Log.Debug($"Processing ban request for player {request.PlayerId}. period={newPeriod}, token={token}.");
                    banStorage.GrantBan(token, BanReason.AdminBan, newPeriod);
                }

            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            finally
            {
                _banProcessorSw.Restart();
            }
        }
    }
}
