namespace ClansServerLib
{
    public interface INameFilter
    {
        bool CheckName(string name);
        bool CheckTag(string tag);
    }
}
