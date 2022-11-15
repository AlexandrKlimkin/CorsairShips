using System;

namespace PestelLib.SharedLogic.Modules
{
    [Serializable]
    public class LeagueDef
    {
        public string Id;
        public int Level;
        public string Name;
		public float UpCoeff;
		public float DownCoeff;
        public long BotPointsMin;
        public long BotPointsMax;
    }
}
