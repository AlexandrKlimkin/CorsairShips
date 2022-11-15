using PestelLib.SaveSystem.FileSystemStorage;
using PestelLib.ServerLogClient;
using UnityDI;
using LegacyStorage = PestelLib.SaveSystem.FileSystemStorage.Storage;

namespace PestelLib.SaveSystem
{
    public static class StorageInitializer
    {
        public static void TryInitStorage()
        {
            /*
             * Теперь везде используется SQLite для хранения профиля игрока, команд ШЛ и PlayerId
             * SQLite решает проблемы с ненадежностью файловой системы (профиль игрока или его команды
             * могли биться или не записываться до конца) и так же решает вопросы с теми вещами, которые
             * должны меняться согласованно: например команды ШЛ и стейт игрока
             *
             * В вызове ниже SQLiteStorage регистрируется как хранилище по-умолчанию,
             * если только никто не зарегистрировал другую реализацию хранилища
             */
            SQLiteStorage.Storage.TryRegisterDefaultSqLiteStorage();

            /*
             * я выпилил всю логику миграции со старого стейта из соображений безопасности
             * юзеры скорее всего не используют данный мигратор а вот хакеры вполне могут
             * смотри git log
             */
        }
    }
}