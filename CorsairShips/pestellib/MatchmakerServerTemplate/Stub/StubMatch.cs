using System;
using System.Collections.Generic;
using System.Linq;
using PestelLib.MatchmakerShared;
using log4net;

namespace PestelLib.MatchmakerServer.Stub
{
    class StubMatch : Match<MatchmakerRequest>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(StubMatch));
        private DateTime _creationTime = DateTime.UtcNow;
        public List<MatchmakerRequest> SimpleParty = new List<MatchmakerRequest>();

        public TimeSpan GetWaitTime() { return DateTime.UtcNow - _creationTime; }

        public override List<MatchmakerRequest> Party => SimpleParty;
        public override MatchmakerRequest Master => SimpleParty.FirstOrDefault(_ => !_.IsBot);
        public override bool IsFull => Party.Count == 3;
        public override float Fitment => 1f;

        public override void Leave(Guid id)
        {
            Log.DebugFormat("Leaving match '{0}' user '{1}'", Id, id);
            var u = SimpleParty.Find(_ => _.PlayerId == id);
            if (u == null)
                return;
            u.IsBot = true;
        }
    }
}
