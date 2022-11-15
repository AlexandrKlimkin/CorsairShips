using System;
using System.Collections.Generic;
using System.Linq;
using PestelLib.MatchmakerShared;

namespace PestelLib.MatchmakerServer.DeepWaters
{
    class DeepWatersMatch : Match<DeepWatersMatchmakerRequest>
    {
        private int _matchCap;
        private int _maxPowerDiff;
        private DeepWatersMatchmakerRequest _master;
        public DeepWatersMatch(int matchCap, int maxPowerDiff)
        {
            _matchCap = matchCap;
            _maxPowerDiff = maxPowerDiff;
        }

        public float Difficulty => 0.5f - (Team1.Sum(_ => _.Power) - Team2.Sum(_ => _.Power)) * 0.5f / _maxPowerDiff;

        public float LastPrediction;
        public List<DeepWatersMatchmakerRequest> Team1;
        public List<DeepWatersMatchmakerRequest> Team2;
        public override DeepWatersMatchmakerRequest Master => _master ?? Team1.First();
        public override List<DeepWatersMatchmakerRequest> Party => Team1.Concat(Team2).ToList();
        public override bool IsFull => Party.Count() == _matchCap;
        public override float Fitment => (1 - Math.Abs(Difficulty - Master.Difficulty));

        public override void Leave(Guid id)
        {
            var p = Team1.FirstOrDefault(_ => _.PlayerId == id) ?? Team2.FirstOrDefault(_ => _.PlayerId == id);
            if (p == null)
                return;
            if (p.IsBot)
                return;
            p.IsBot = true;
            // change master
            if (Team1.First() == p)
            {
                _master = Party.Where(_ => !_.IsBot).FirstOrDefault();
            }
        }
    }
}
