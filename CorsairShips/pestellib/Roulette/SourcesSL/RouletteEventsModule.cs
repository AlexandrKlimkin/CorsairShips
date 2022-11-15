using System;
using PestelLib.SharedLogicBase;
using S;
using UnityDI;

namespace PestelLib.SharedLogic.Modules
{
    [System.Reflection.Obfuscation(ApplyToMembers=false)]
    public class RouletteEventsModule : SharedLogicModule<RouletteEventsState>
    {
        [Dependency] private RouletteModule _rouletteModule;
        [Dependency] private SettingsModuleBase _settings;

        [SharedCommand]
        internal void SetAdsTimeStamp()
        {
            State.AdsBoxTimestamp = CommandTimestamp.Ticks;
        }

        [SharedCommand]
        internal void StartSeason()
        {
            var period = TimeSpan.FromDays(_settings.GetValue("PirateBoxSeasonPeriodDays", 7));
            DateTime seasonEndTime = CommandTimestamp + period;
            State.UltraBoxEndTimestamp = seasonEndTime.Ticks;

            _rouletteModule.ResetMegaBoxProgress();
        }

        // local state only
        public void ResetAds()
        {
            State.AdsBoxTimestamp = 0;
        }

        public TimeSpan GetSeasonRemainTime(DateTime now)
        {
            return GetSeasonEndTime() - now;
        }

        public bool AdsCooldownPassed
        {
            get { return GetAdsCooldown() <= TimeSpan.Zero; }
        }

        public TimeSpan GetAdsCooldown()
        {
            return GetAdsCooldown(CommandTimestamp);
        }

        public TimeSpan GetAdsCooldown(DateTime now)
        {
            TimeSpan cooldown = TimeSpan.FromHours(_settings.GetValue("BoxAdsCooldownHours", 4));
            DateTime lastAdsTime = GetLastAdsDate();
            return cooldown - new TimeSpan(now.Ticks - lastAdsTime.Ticks);
        }

        private DateTime GetLastAdsDate()
        {
            return new DateTime(State.AdsBoxTimestamp);
        }

        private DateTime GetSeasonEndTime()
        {
            return new DateTime(State.UltraBoxEndTimestamp);
        }
    }
}
