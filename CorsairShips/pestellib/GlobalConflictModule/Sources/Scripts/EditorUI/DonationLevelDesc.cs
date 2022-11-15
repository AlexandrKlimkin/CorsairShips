using PestelLib.SharedLogicBase;
using ServerShared.GlobalConflict;
using UnityEngine;

namespace GlobalConflictModule.Scripts
{
    [ExecuteInEditMode]
    public class DonationLevelDesc : MonoBehaviour
    {
        public bool IsTeam;
        public int DonationsAmount;
        [HideInInspector]
        public int Level;
        public DonationLevelBonusDesc[] Bonuses;

        void Start()
        {
            if(!Application.isPlaying)
                return;
        }

        void Update()
        {
            if (Bonuses != null)
            {
                foreach (var b in Bonuses)
                {
                    if (b.ServerType == DonationBonusType.Unpecified)
                        continue;

                    b.ClientType = b.ServerType.ToString();
                }
            }
        }
    }
}
