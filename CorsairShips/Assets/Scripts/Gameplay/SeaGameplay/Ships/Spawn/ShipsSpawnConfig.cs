using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.SeaGameplay.Spawn {
    
    [CreateAssetMenu(fileName = "ShipsSpawnConfig", menuName = "Configs/ShipsSpawnConfig")]
    public class ShipsSpawnConfig : ScriptableObject {

        [Serializable]
        public struct ShipSpawnData {
            public string ShipId;
            public float Weight;
        }

        [SerializeField]
        private string _DefaultShipId = "Sloop";
        [SerializeField]
        private string _DefaultEnemyShipId = "Sloop";
        [SerializeField]
        private List<ShipSpawnData> _SpawnShips;

        public string DefaultShipId => _DefaultShipId;
        public string DefaultEnemyShipId => _DefaultEnemyShipId;
        public List<ShipSpawnData> SpawnShips => _SpawnShips;
    }
}