using System;
using System.Collections.Generic;

namespace PestelLib.SaveSystem
{
    /*
     * Интерфейс для хранения локальных данных типичной игры с ШЛ и _requestQueue
     * Интерфейс решил вынести т.к. не уверен в том что остановимся на реализации основанной на
     * SQLite, соответственно хочется иметь возможность легко подменить реализацию
     */
    public interface IStorage : IDisposable
    {
        /*
         * Транзакции необходимы для согласованного изменения данных.
         * Например, если в лог записывается команда, то нужно записать и стейт,
         * который получился после этой команды.
         * Этот метод нужно вызвать перед началом изменения данных
         */
        void BeginTransaction();

        /*
         * Применение пакета изменений, выполненных после вызова BeginTransaction
         */
        void Commit();
        
        /*
         * Сохранение команды ШЛ
         * Не после каждой команды есть посчитанный хэш, из соображений производительности,
         * поэтому hash - nullable тип
         */
        void AddCommand(Command cmd);

        List<Command> GetCommands(int serialFrom, int serialTo);

        bool IsHaveAnyCommands { get; }

        int FirstCommandSerial { get; }

        int LastCommandSerial { get; }

        /*
         * Удаление тех команд, которые успешно отправлены на сервер и поэтому не
         * нуждаются в дальнейшем хранении
         */
        int RemoveCommandsRange(int serialFrom, int serialTo);
        
        byte[] UserProfile { get; set; }

        bool IsStorageEmpty { get; }

        /*
         * Аналогично соответствующим методам UnityEngine.PlayerPrefs.
         * Добавлено, чтобы можно было менять их согласованно с остальными данными;
         * без этого были ситуации, когда PlayerPrefs существуют, а все данные сохранённые в файлах стёрты
         */
        void SetString(string key, string val);

        string GetString(string key, string defaultValue = "");

        /*
         * Проверяет целостность сохраненного профиля.
         * Если проверка не поддерживается должен вернуть false.
         * Если профиля нет должен вернуть false.
         */
        bool IsUserProfileCorrupted();
        bool IsStringCorrupted(string key);
    }
}