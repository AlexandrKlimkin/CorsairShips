using UnityEngine;

namespace Game.SeaGameplay.Spawn {
    
    [CreateAssetMenu(fileName = "ShipsSpawnConfig", menuName = "Configs/ShipsSpawnConfig")]
    public class ShipsSpawnConfig : ScriptableObject {
        [SerializeField]
        private string _DefaultShipId = "Sloop";
        [SerializeField]
        private string _DefaultEnemyShipId = "Sloop";
        [SerializeField]
        private string _DefaultLocalPlayerShipId = "Sloop";

        public string DefaultShipId => _DefaultShipId;
        public string DefaultEnemyShipId => _DefaultEnemyShipId;
        public string DefaultLocalPlayerShipId => _DefaultLocalPlayerShipId;
    }
}