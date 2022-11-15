using System.Threading.Tasks;

namespace ClansClientLib.DefsProxy
{
    public interface IClanDefsLoader
    {
        Task<ClanLevelDefProxy> GetClanLevels();
    }
}
