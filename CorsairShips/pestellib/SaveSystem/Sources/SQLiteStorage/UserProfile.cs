using SQLite4Unity3d;

namespace PestelLib.SaveSystem.SQLiteStorage
{
    internal class UserProfile
    {
        [PrimaryKey]
        public int Id { get; set; }
        public byte[] Data { get; set; }
    }
}