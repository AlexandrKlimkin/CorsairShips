namespace PestelLib.SaveSystem.FileSystemStorage
{
    public interface IPlayerPrefs
    {
        void SetString(string key, string val);
        string GetString(string key, string defaultValue = "");

        void SetInt(string key, int val);
        int GetInt(string key, int defaultValue = 0);
    }
}