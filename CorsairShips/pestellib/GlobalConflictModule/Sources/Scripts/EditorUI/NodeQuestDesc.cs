using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServerShared.GlobalConflict;
using UnityEngine;

namespace GlobalConflictModule.Scripts
{
    [ExecuteInEditMode]
    public class NodeQuestDesc : MonoBehaviour
    {
        public string Name;
        public int Level;
        public string Type;
        public bool Auto;
        public int ActiveTimeInMinutes;
        public int DeployCooldownInMinutes;
        public string RewardId;
        public int Weight;

        void Update()
        {
            Weight = Math.Max(1, Weight);
        }
    }
}
