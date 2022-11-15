using System;

namespace PestelLib.MatchmakerShared
{
    public class MatchmakerRequest
    {
        public Guid PlayerId;
        // server overrides following
        public DateTime RegTime;
        public int TeamId;
        public bool IsBot;

        public override bool Equals(object obj)
        {
            var mmr = obj as MatchmakerRequest;
            if (mmr == null)
                return false;
            return PlayerId == mmr.PlayerId;
        }

        public override int GetHashCode()
        {
            return PlayerId.GetHashCode();
        }
    }
}
