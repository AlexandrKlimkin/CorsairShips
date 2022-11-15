using System;

namespace ReportPlayers
{
    public class PlayerCounterData
    {
        public Guid PlayerGuid;
        public string PlayerName;
        public int SessionCounter;
        public int ReportsCounter;
        public float ReportsPerSession;
        public bool Whitelisted;
    }
}