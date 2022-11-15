
namespace ServerShared
{
    public interface IStatsProvider<T>
    {
        T GetStats();
    }
}
