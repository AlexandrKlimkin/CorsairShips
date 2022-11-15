using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PestelLib.MatchmakerShared;
using PestelLib.MatchmakerServer;

namespace MatchmakerTest.Dummies
{
    class SimpleMatch : Match<MatchmakerRequest>
    {
        public List<MatchmakerRequest> SimpleParty = new List<MatchmakerRequest>();
        
        public override List<MatchmakerRequest> Party => SimpleParty;
        public override MatchmakerRequest Master => SimpleParty.FirstOrDefault(_ => !_.IsBot);
        public override bool IsFull => Party.Count == 3;
        public override float Fitment => 1f;

        public override void Leave(Guid id)
        {
            var u = SimpleParty.Find(_ => _.PlayerId == id);
            u.IsBot = true;
        }
    }
}
