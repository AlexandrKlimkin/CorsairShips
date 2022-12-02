using Game.Dmg;
using UnityEngine;

namespace Game.Shooting
{
    public class ProjectileDataBase
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public float LifeTime;
        public float BirthTime;
        public Damage Damage;
    }
}
