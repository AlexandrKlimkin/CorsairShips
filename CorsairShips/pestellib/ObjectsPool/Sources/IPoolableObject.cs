namespace PestelLib.ObjectsPool
{
    public interface IPoolableObject
    {
        void OnPlacedInPool();
        void OnSpawn();
        void OnDespawn();
    }
}