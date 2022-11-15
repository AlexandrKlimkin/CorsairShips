using System.Linq;
using ServerShared.GlobalConflict;

namespace BackendCommon.Code.GlobalConflict
{
    public class Team
    {
        private readonly TeamState _state;

        public Team(TeamState state)
        {
            _state = state;
            if (_state.DonationBonuses == null)
                _state.DonationBonuses = new DonationBonus[] { };
        }

        public float GetTeamPointsBonus(bool win)
        {
            if (_state.DonationBonuses.Length < 1)
                return 0f;

            var teamPointBonuses = _state.DonationBonuses.Where(_ => _.ServerType == DonationBonusType.TeamPointsBuff).Sum(_ => _.Value);

            return (float) teamPointBonuses;
        }
    }
}