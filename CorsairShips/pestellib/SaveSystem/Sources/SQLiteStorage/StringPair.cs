using SQLite4Unity3d;

namespace PestelLib.SaveSystem.SQLiteStorage
{
    public class StringPair
    {
        [PrimaryKey] public string Key { get; set; }
        
        public string Value { get; set; }
    }
}