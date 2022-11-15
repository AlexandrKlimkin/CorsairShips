using System;
using System.Collections.Generic;
using MessagePack;

namespace ServerShared.PlayerProfile
{
    [MessagePackObject()]
    public class ProfileDTO
    {
        [Key(0)]
        public Guid PlayerId;
        [Key(1)]
        public DateTime UpdateTime = DateTime.UtcNow;
        [Key(2)]
        public List<Achievement> Achievements = new List<Achievement>();
        [Key(3)]
        public PlayerTitle Title;
        [Key(4)]
        public Dictionary<string, string> StrStats = new Dictionary<string, string>();
        [Key(5)]
        public Dictionary<string, long> IntStats = new Dictionary<string, long>();
        [Key(6)]
        public bool IsBot;
        [Key(7)]
        public string Nick;
        [Key(8)]
        public string Avatar;
        [Key(9)]
        public string Country; // ISO 3166-1 alpha-2
        [Key(10)]
        public int Level;
        [Key(11)]
        public DateTime Expiry = DateTime.MaxValue;
        [Key(12)]
        public Guid CreatedBy;
        [Key(13)]
        public int Version;
        [Key(14)]
        public Dictionary<string, byte[]> BinStats = new Dictionary<string, byte[]>();
    }

    // (morgan): dont remove, mpc fix
    [MessagePackObject()]
    public class ProfileDTOArray
    {
        [Key(0)]
        public ProfileDTO[] Array;
    }
}