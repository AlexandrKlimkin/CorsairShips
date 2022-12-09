using Stats;
using UnityEngine;

namespace Game.SeaGameplay {
    [CreateAssetMenu(fileName = "ShipConfig", menuName = "Configs/ShipConfig")]
    public class ShipStatsConfig : ScriptableObject {
        [Header("Health")]
        [Stat(Id = StatId.MaxHealth, StatType = typeof(FloatStat))]
        public float MaxHealth;
        
        [Header("Movement")]
        [Stat(Id = StatId.MaxSpeed, StatType = typeof(FloatStat))]
        public float MaxSpeed;
        [Stat(Id = StatId.Acceleration, StatType = typeof(FloatStat))]
        public float Acceleration;
        [Stat(Id = StatId.Deceleration, StatType = typeof(FloatStat))]
        public float Deceleration;
        [Stat(Id = StatId.RotateToForwardFactor, StatType = typeof(FloatStat))]
        public float RotateToForwardFactor;

        [Stat(Id = StatId.MaxRotateYSpeed, StatType = typeof(FloatStat))]
        public float MaxRotateYSpeed;
        [Stat(Id = StatId.RotateYAcceleration, StatType = typeof(FloatStat))]
        public float RotateYAcceleration;
        [Stat(Id = StatId.RotateYDeceleration, StatType = typeof(FloatStat))]
        public float RotateYDeceleration;
        [Stat(Id = StatId.RotateYSlowAngle, StatType = typeof(FloatStat))]
        public float RotateYSlowAngle;
    }
}