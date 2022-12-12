using Stats;
using UnityEngine;

namespace Game.SeaGameplay {
    [CreateAssetMenu(fileName = "ShipConfig", menuName = "Configs/ShipConfig")]
    public class ShipStatsConfig : ScriptableObject {
        [Header("Health")]
        [Stat(Id = StatId.MaxHealth, StatType = typeof(FloatStat))]
        public float MaxHealth;

        [Header("Weapon")]
        [Stat(Id = StatId.WeaponsAngle, StatType = typeof(FloatStat))]
        public float WeaponsAngle;
        [Stat(Id = StatId.WeaponsCooldown, StatType = typeof(FloatStat))]
        public float WeaponsCooldown;
        [Stat(Id = StatId.ProjectileDamage, StatType = typeof(FloatStat))]
        public float ProjectileDamage;
        [Stat(Id = StatId.ProjectileScaleFactor, StatType = typeof(FloatStat))]
        public float ProjectileScaleFactor;
        [Stat(Id = StatId.ProjectileSpeed, StatType = typeof(FloatStat))]
        public float ProjectileSpeed;
        [Stat(Id = StatId.ProjectileLifetime, StatType = typeof(FloatStat))]
        public float ProjectileLifetime;
        [Stat(Id = StatId.ProjectileGravity, StatType = typeof(FloatStat))]
        public float ProjectileGravity;
        [Stat(Id = StatId.ProjectileXDispersionAngle, StatType = typeof(FloatStat))]
        public float ProjectileXDispersionAngle;
        [Stat(Id = StatId.ProjectileYDispersionAngle, StatType = typeof(FloatStat))]
        public float ProjectileYDispersionAngle;
        [Stat(Id = StatId.RandomShotDelayMin, StatType = typeof(FloatStat))]
        public float RandomShotDelayMin;
        [Stat(Id = StatId.RandomShoDelayMax, StatType = typeof(FloatStat))]
        public float RandomShoDelayMax;
        
        
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