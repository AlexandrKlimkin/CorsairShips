using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PestelLib.SharedLogic.Modules;
using PestelLib.SharedLogicBase;
using ServerShared.Leagues;
using UnityDI;
using log4net;
using PestelLib.SharedLogic;

namespace PestelLib.Leagues.SourcesSL
{
    public class LeagueDefProvider : ILeagueDefProvider
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LeagueDefProvider));
        
        public int LeaguesAmount { get; private set; }
        public bool IsValid { get; private set; }
        // SettingDef
        public int LeagueDivisionSize { get; private set; }
        public float LeaguePreClosureTimeFactor { get; private set; }
        public float LeagueDivisionReserveSlots { get; private set; }
        public int LeagueCycleTime { get; private set; }

        private List<LeagueDef> _leagueDefs;
        private Dictionary<string, SettingDef> _settingsDict;

        public LeagueDefProvider(List<LeagueDef> leagueDefs, Dictionary<string, SettingDef> settingsDict)
        {
            _leagueDefs = leagueDefs;
            _settingsDict = settingsDict;

            if (_leagueDefs == null)
            {
                Log.Warn("LeagueDef not defined");
                IsValid = false;
                return;
            }

            if (_settingsDict == null)
            {
                Log.Warn("SettingDef dict not defined");
                IsValid = false;
                return;
            }

            IsValid = LoadSettings();

            if(!IsValid)
                return;

            LeaguesAmount = _leagueDefs.Count;
        }

        private bool LoadSettings()
        {
            SettingDef settingDef;
            if (!_settingsDict.TryGetValue("LeagueDivisionSize", out settingDef))
            {
                Log.WarnFormat("Setting '{0}' not defined", "LeagueDivisionSize");
                return false;
            }
            LeagueDivisionSize = SettingsModuleBase.Convert<int>(settingDef);

            if (!_settingsDict.TryGetValue("LeaguePreClosureTimeFactor", out settingDef))
            {
                Log.WarnFormat("Setting '{0}' not defined", "LeaguePreClosureTimeFactor");
                return false;
            }
            LeaguePreClosureTimeFactor = SettingsModuleBase.Convert<float>(settingDef);

            if (!_settingsDict.TryGetValue("LeagueDivisionReserveSlots", out settingDef))
            {
                Log.WarnFormat("Setting '{0}' not defined", "LeagueDivisionReserveSlots");
                return false;
            }
            LeagueDivisionReserveSlots = SettingsModuleBase.Convert<float>(settingDef);

            if (!_settingsDict.TryGetValue("LeagueCycleTime", out settingDef))
            {
                Log.WarnFormat("Setting '{0}' not defined", "LeagueCycleTime");
                return false;
            }
            LeagueCycleTime = SettingsModuleBase.Convert<int>(settingDef);
            return true;
        }

        public float GetDivisionUpCoeff(int leagueLvl)
        {
            if (!IsValid)
                return float.NaN;

            var def = _leagueDefs.First(_ => _.Level == leagueLvl);
            return def.UpCoeff;
        }

        public float GetDivisionDownCoeff(int leagueLvl)
        {
            if (!IsValid)
                return float.NaN;

            var def = _leagueDefs.First(_ => _.Level == leagueLvl);
            return def.DownCoeff;
        }

        public long GetBotMinPoints(int leagueLvl)
        {
            if (!IsValid)
                return 0;
            var def = _leagueDefs.First(_ => _.Level == leagueLvl);
            return def.BotPointsMin;
        }

        public long GetBotMaxPoints(int leagueLvl)
        {
            if (!IsValid)
                return 0;
            var def = _leagueDefs.First(_ => _.Level == leagueLvl);
            return def.BotPointsMax;
        }
    }
}
