using System.Collections.Generic;
using MessagePack;

namespace S
{
    [MessagePackObject]
    public class UserProfile
    {
        [Key(1)]
        public byte[] UserId;

        [Key(2)]
        public int CommandSerialNumber;

        //[JsonConverter(typeof(UserProfileModulesConverter))]
        [Key(3)]
        public Dictionary<string, byte[]> ModulesDict = new Dictionary<string, byte[]>();

        /*
         * Версия сохранённых данных (высокоуровневая), используется при изменении стейта в новой версии игры
         * Рекомендуемое использование:
         * В SharedLogicCore переопределить метод OnSharedLogicInitialized
         * if (_state.Version == 0)
           {
              //конвертация в сохранение следущей версии
              var myModule = GetModule<MyModule>();
              myModule.DoSomethingWithLegacyData();

              _state.Version = 1;
           }
           
           if (_state.Version == 1)
           {
              //конвертация
              //...
              _state.Version = 2;
           }
         * При создании инстанса ШЛ может произойти от 0 до нескольких конвертаций за раз,
         * в зависимоти от того, как давно игрок играл в прошлый раз.
         * При апгрейде таких данных инстанс ШЛ уже инициализирован, можно получать модули и
         * вызывать у них методы.
         * Важно! не забывайте соответствующим образом менять версию сохранения в вашем наследнике
         * DefaultStateFactory. Иначе может получится, что сохранение созданное уже в новой версии будет
         * пытаться обновиться, это может привести к совсем неожиданным последствиям.
         *
         * Альтернативный вариант - версионирование на уровне модуля, с обновлением через
         * переопределение методов SharedLogicModule.SerializedState и SharedLogicModule.MakeDefaultState
         * Но в этом случае невозможно надежное взаимодействие между модулями т.к. при апгрейде модуля А может
         * понадобится модуль Б, который ещё не получил своё состояние.
         *
         * Рекомендуемый вариант - именно через метод OnSharedLogicInitialized, он наиболее прозрачный
         */
        [Key(4)]
        public int Version;

        /*
         * Версия сохранённых данных (низкоуровневая); Можно использовать для изменения формата сохранения
         * Была добавлена для того, чтобы избавиться от больших сохранений в тех стейтах, где
         * использовались аттрибуты MessagePack.Key с индексами разнесёнными: например Key(0), затем сразу Key(1000)
         * Теоретически можно использовать, если перейдем с MessagePack на какой-либо другой формат
         * сохранения данных состояния модуля.
         * Предполагаемое использование:
         * В SharedLogicCore переопределить метод BeforeSharedLogicInitialization, добавить туда конвертацию
         * if (_state.SerializationVersion == 0)
           {
              var shipModuleName = typeof(ShipModule).Name;
              if (datas.ContainsKey(shipModuleName))
              {
                 var state = (ShipModuleState)MessagePackSerializer.Deserialize<LegacyShipModuleState>(datas[shipModuleName]);
                 datas[shipModuleName] = MessagePackSerializer.Serialize(state);
              }          
              _state.SerializationVersion = 1;
           }
         * В BeforeSharedLogicInitialization доступны уже и стейты в виде byte[] и дефы, но он вызывается ещё до того, как
         * создастся какой-либо модуль
         */
        [Key(5)]
        public int SerializationVersion;

        [Key(6)]
        public uint SharedLogicVersion;
    }
}