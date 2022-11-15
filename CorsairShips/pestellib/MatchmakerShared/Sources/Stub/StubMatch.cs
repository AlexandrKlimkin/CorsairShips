using System;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace PestelLib.MatchmakerShared
{
    public class StubMatch : Match<MatchmakerRequest>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(StubMatch));
        public List<MatchmakerRequest> SimpleParty = new List<MatchmakerRequest>();
        
        public override List<MatchmakerRequest> Party { get { return SimpleParty; } }
        public override MatchmakerRequest Master { get { return SimpleParty.FirstOrDefault(_ => !_.IsBot); } }
        public override bool IsFull { get { return Party.Count == 3; } }
        public override float Fitment { get { return 1f; } }

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
