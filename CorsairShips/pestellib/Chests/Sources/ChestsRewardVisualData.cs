using UnityEngine;

namespace PestelLib.SharedLogic.Modules
{
    public class ChestsRewardVisualData
    {
        public string Name;
        public string Description;
        public string RewardLevelString; //Support Romans digits for PlanetCommander
        public Sprite Icon;
        public Color IconColor = Color.white;
        public Color BackgroundColor = Color.white;
        public int Amount;
        public Vector3 IconScale = Vector3.one; //Support fucked up Planet Commander icons
    }
}