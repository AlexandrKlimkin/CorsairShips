using SQLite4Unity3d;

namespace PestelLib.SaveSystem.SQLiteStorage
{
    /*
     * Класс SaveSystem.Command - это часть интерфейса IStorage
     * Мне нужен такой же класс, но с дополнительными аттрибутами для SQLite
     * Поэтому я завёл его и добавил в него операторы приведения в SaveSystem.Command и обратно
     */
    public class Command
    {
        [PrimaryKey]
        public int SerialNumber { get; set; }

        public byte[] SerializedCommandData { get; set; }

        public uint SharedLogicCrc { get; set; }

        public uint DefinitionsVersion { get; set; }

        public int? Hash { get; set; }

        public long Timestamp { get; set; }

        public static implicit operator Command(SaveSystem.Command command)
        {
            return new Command
            {
                Hash = command.Hash,
                DefinitionsVersion = command.DefinitionsVersion,
                SerializedCommandData = command.SerializedCommandData,
                SharedLogicCrc = command.SharedLogicCrc,
                SerialNumber = command.SerialNumber,
                Timestamp = command.Timestamp
            };
        }

        public static implicit operator SaveSystem.Command(Command command)
        {
            return new SaveSystem.Command
            {
                Hash = command.Hash,
                DefinitionsVersion = command.DefinitionsVersion,
                SerializedCommandData = command.SerializedCommandData,
                SharedLogicCrc = command.SharedLogicCrc,
                SerialNumber = command.SerialNumber,
                Timestamp = command.Timestamp
            };
        }
    }
}