using ServerShared.GlobalConflict;
using UnityEngine;

namespace GlobalConflictModule.Scripts
{
    [ExecuteInEditMode]
    public class PointOfInterestsDesc : MonoBehaviour
    {
        public string Name;
        public bool ForAllTeams;
        public int BonusTimeInMinutes;
        public int DeployCooldownInMinutes;
        public bool AutoDeploy;
        public string Type;
        public PointOfInterestBonusDesc[] Bonuses;
        [HideInInspector]
        public int GeneralLevel;
        [HideInInspector]
        public string Team;

        void Update()
        {
            if (Bonuses != null)
            {
                foreach (var b in Bonuses)
                {
                    if (b.ServerType == PointOfInterestServerLogic.None)
                        continue;

                    b.ClientType = b.ServerType.ToString();
                }
            }
        }
    }
}