namespace ServerShared.Leagues
{
    public interface ILeagueDefProvider
    {
        bool IsValid { get; }
        /// <summary>
        /// Вычисляется по количеству LeagueDef
        /// </summary>
        int LeaguesAmount { get; }

        // далее блокполей которые загружаются из SettingDef
        int LeagueDivisionSize { get; }
        float LeaguePreClosureTimeFactor { get; }
        float LeagueDivisionReserveSlots { get; }
        int LeagueCycleTime { get; }

        // берется из LeagueDef
        float GetDivisionUpCoeff(int leagueLvl);
        float GetDivisionDownCoeff(int leagueLvl);
        long GetBotMinPoints(int leagueLvl);
        long GetBotMaxPoints(int leagueLvl);
    }
}