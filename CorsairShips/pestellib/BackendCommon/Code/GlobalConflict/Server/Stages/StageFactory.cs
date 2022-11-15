using System.Collections.Generic;
using System.Linq;
using log4net;
using ServerShared.GlobalConflict;

namespace BackendCommon.Code.GlobalConflict.Server.Stages
{
    class StageFactory
    {
        private static ILog Log = LogManager.GetLogger(typeof(StageFactory));
        private readonly Dictionary<StageType, Stage> _stages;

        public StageType[] Stages => _stages.Keys.ToArray();

        public StageFactory()
        {
            _stages = new Dictionary<StageType, Stage>()
            {
                {StageType.Donation, new DonationStage()},
                {StageType.Cooldown, new CooldownStage()},
                {StageType.Battle, new BattleStage()},
                {StageType.Final, new FinalStage()}
            };
        }

        public Stage GetStage(StageType type)
        {
            if (!_stages.TryGetValue(type, out var stage))
            {
                Log.Warn($"Unknown stage '{type}'");
                return null;
            }
            return _stages[type];
        }
    }
}