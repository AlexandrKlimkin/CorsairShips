using System;
using UnityEngine;

namespace Menu.Hagar {
    public class HangarSceneDataContainer : MonoBehaviour {
        [SerializeField]
        private Transform _ShipSpawnPoint;

        public Transform ShipSpawnPoint => _ShipSpawnPoint;
        
    }
}