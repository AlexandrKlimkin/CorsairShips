using PestelLib.MatchmakerShared;

namespace PestelLib.MatchmakerServer.DeepWaters
{
    enum ShipClass
    {
        Destroyer,
        Cruiser,
        Battleship,
        Carrier
    }

    class DeepWatersMatchmakerRequest : MatchmakerRequest
    {
        public int Tier;
        public float Difficulty;
        public int Power;
        public int ShipClass;
        public bool BotOnly;

        public int Bucket;
    }
}
