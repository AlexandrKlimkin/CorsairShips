using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace PestelLib.MatchmakerShared
{
    public interface IMatch
    {
        Guid Id { get; set; }
        List<MatchmakerRequest> Party { get; }
        int CountBots { get; }
        MatchingStats Stats { get; }

        // server side
        MatchmakerRequest Master { get; }
        float Fitment { get; }
        bool IsFull { get; }

        // server side method
        void Leave(Guid id);
    }

    public abstract class Match<Request> : IMatch where Request : MatchmakerRequest
    {
        public Guid Id { get; set; }
        [JsonIgnore]
        public DateTime CreateTime = DateTime.UtcNow;
        [JsonIgnore]
        public MatchingStats Stats { get; set; }
        [JsonIgnore]
        public abstract List<Request> Party { get; }
        [JsonIgnore]
        public abstract Request Master { get; }
        [JsonIgnore]
        public int CountBots { get { return Party.Count(r => r.IsBot); } }
        [JsonIgnore]
        public abstract bool IsFull { get; }
        [JsonIgnore]
        public abstract float Fitment { get; }

        public abstract void Leave(Guid id);

        public Match(IMatch m)
        {
            Id = m.Id;
        }

        public Match()
        {
            Id = Guid.NewGuid();
        }

        #region IMatch implementation
        List<MatchmakerRequest> IMatch.Party { get { return Party.OfType<MatchmakerRequest>().ToList(); } }
        MatchmakerRequest IMatch.Master { get { return Master; } }
        #endregion
    }
}
