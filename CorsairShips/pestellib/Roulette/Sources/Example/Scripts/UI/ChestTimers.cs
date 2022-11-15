using PestelLib.ServerClientUtils;
using PestelLib.ServerShared;
using System;
using System.Globalization;
using UnityDI;
using UnityEngine;

namespace PlanetCommander
{
    public class ChestTimers
    {
        [Dependency] private SharedTime _sharedTime;

        private const long SEASON_PERIOD_DAYS = 7;
        private const string SEASON_END_TIME = "UltraBoxSeasonEndTime";
        private const int BOX_ADS_COOLDOWN_HOURS = 4;
        private const string BOX_ADS_LAST_TIME = "BoxAdsLastTime";

        private DateTime _seasonEndTime;
        private static ChestTimers _instance;

        public TimeSpan SeasonRemainTime
        {
            get
            {
                return _seasonEndTime - _sharedTime.Now;
            }
        }

        public static ChestTimers Instance
        {
            get
            {
                if (_instance == null)
                    Initialize();
                return _instance;
            }
        }

        public static void Initialize()
        {
            if (_instance != null)
                return;

            _instance = new ChestTimers();

            _instance._sharedTime = ContainerHolder.Container.Resolve<SharedTime>();

            if (PlayerPrefs.HasKey(SEASON_END_TIME))
            {
                var seasonEnd = PlayerPrefs.GetString(SEASON_END_TIME);
                if (string.IsNullOrEmpty(seasonEnd))
                {
                    seasonEnd = "0";
                }

                _instance._seasonEndTime = TimeUtils.ConvertFromUnixTimestamp(long.Parse(seasonEnd));
            }
            else
                _instance.StartSeason();
        }


        public void SetLastAdsTime()
        {
            PlayerPrefs.SetString(BOX_ADS_LAST_TIME, TimeUtils.ConvertToUnixTimestamp(_sharedTime.Now).ToString(CultureInfo.InvariantCulture));
            //SavedData.Instance.SaveNow();
        }

        public TimeSpan GetCooldown()
        {
            var cooldown = TimeSpan.FromHours(BOX_ADS_COOLDOWN_HOURS);
            var lastAdsTime = GetLastAdsDate();
            return cooldown - new TimeSpan(_sharedTime.Now.Ticks - lastAdsTime.Ticks);
        }

        public void StartSeason()
        {
            var period = TimeSpan.FromDays(SEASON_PERIOD_DAYS);
            _seasonEndTime = _sharedTime.Now + period;
            SaveSeasonEnd();
        }



        private void SaveSeasonEnd()
        {
            PlayerPrefs.SetString(SEASON_END_TIME, TimeUtils.ConvertToUnixTimestamp(_seasonEndTime).ToString(CultureInfo.InvariantCulture));
            //SavedData.Instance.SaveNow();
        }

        private DateTime GetLastAdsDate()
        {
            string lastAds = "0";

            if (PlayerPrefs.HasKey(BOX_ADS_LAST_TIME))
                lastAds = PlayerPrefs.GetString(BOX_ADS_LAST_TIME);

            return TimeUtils.ConvertFromUnixTimestamp(long.Parse(lastAds));
        }
    }
}
