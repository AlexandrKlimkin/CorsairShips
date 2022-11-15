using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MessagePack;
using Newtonsoft.Json;
using S;
using UnityDI;
using UnityDI.Providers;
using log4net;
using PestelLib.Serialization;
using PestelLib.UniversalSerializer;

namespace PestelLib.SharedLogicBase
{
    public class SharedLogicDefault <TDef> : ISharedLogic, IScheduledActionCaller
    {
        protected static readonly ILog _log = LogManager.GetLogger(typeof(ISharedLogic));
        public Container container = new Container();

        public Action<string> OnLogMessage = x => { };
        protected UserProfile _state;

        public TDef Defs { get; private set; } //TODO: extract all defs to game dependent subclass

        private static List<Type> _serializableModules;

        public Container Container
        {
            get { return container; }
        }

        public Guid PlayerId
        {
            get { return new Guid(_state.UserId); }
            set { _state.UserId = value.ToByteArray(); }
        }

        public virtual string PlayerName
        {
            get { return string.Empty; }
        }

        public virtual List<Type> SerializableModules
        {
            get
            {
                if (_serializableModules == null)
                {
                    var allModules = typeof(TDef).Assembly
                        .GetTypes()
                        .Where(type =>
                            typeof(ISharedLogicModule).IsAssignableFrom(type)
                            && !type.IsInterface
                            && !type.IsGenericType
                            && !type.IsAbstract)
                        .OrderBy(x => x.Name)
                        .ToList();


                    //оставляем только те модули, у которых нет классов-наследников:
                    _serializableModules = allModules.Where(x => allModules.All(y => y.BaseType != x)).ToList();
                }
                return _serializableModules;
            }
        }

        public ISerializer Serializer { get; set; }

        private OrderedScheduledActionCaller _OrderedScheduledActionCaller;

        public SharedLogicDefault(UserProfile state, TDef defs, IFeature features = null)
        {
            _state = state;
            Defs = defs;

            if (features != null)
            {
                features.Visit(container);
            }

            Serializer = UniversalSerializer.Serializer.Implementation;

            _OrderedScheduledActionCaller = new OrderedScheduledActionCaller();
            InitDependencyInjection(state.ModulesDict, defs);
        }

        /*
         * Можно вызвать этот метод на этапе инициализации клиента, чтобы получать потом
         * любые части из Definitions через DI
         */
        public static void RegisterDefsInContainer(TDef definitions, Container container)
        {
            var defType = typeof(TDef);
            var defFields = defType.GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (var fieldInfo in defFields)
            {
                container.RegisterTypeProviderWithInheritance(
                    fieldInfo.FieldType,
                    new InstanceProviderNonGeneric(fieldInfo.GetValue(definitions))
                );

                if (!Attribute.IsDefined(fieldInfo, typeof(GooglePageRefAttribute)))
                    continue;
                
                var attr = (GooglePageRefAttribute) Attribute.GetCustomAttribute(
                    fieldInfo,
                    typeof(GooglePageRefAttribute)
                );

                container.RegisterTypeProviderWithInheritance(
                    fieldInfo.FieldType,
                    new InstanceProviderNonGeneric(fieldInfo.GetValue(definitions)), 
                    attr.PageName
                );
            }
        }

        /*
         * Регистрация всех модулей в контейнере клиента
         */
        public void RegisterModulesInContainer(Container clientContainer)
        {
            foreach (var serializableModuleType in SerializableModules)
            {
                clientContainer.RegisterCustom(serializableModuleType, () => container.Resolve(serializableModuleType));
            }
        }
        
        protected virtual void InitDependencyInjection(Dictionary<string, byte[]> datas, TDef definitions)
        {
            BeforeSharedLogicInitialization(datas, definitions);

            container.RegisterInstance<ISharedLogic>(this);
            container.RegisterInstance<IScheduledActionCaller>(this);
            container.RegisterInstance<IOrderedScheduledActionCaller>(_OrderedScheduledActionCaller);

            RegisterDefsInContainer(definitions, container);

            foreach (var serializableModule in SerializableModules)
            {
                container.RegisterTypeProviderWithInheritance(serializableModule, new ModuleProvider(serializableModule, datas));
            }

            List<ISharedLogicModule> modules = new List<ISharedLogicModule>(SerializableModules.Count);

            foreach (var moduleType in SerializableModules)
            {
                var module = (ISharedLogicModule) container.Resolve(moduleType);
                if (module != null)
                {
                    modules.Add(module);
                }
                else
                {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
                    UnityEngine.Debug.LogError("Can't resolve module: " + moduleType);
#endif
                }
                
                module.OnDependenciesResolved();
            }

            modules = modules.OrderByDescending(x => x.MakeDefaultStatePriority).ToList();

            foreach (var module in modules)
            {
                var key = module.GetType().Name;
                if (!datas.ContainsKey(key))
                {
                    module.MakeDefaultState();
                }
                else
                {
                    module.SerializedState = datas[key];
                }
            }

            OnSharedLogicInitialized();
        }

        protected virtual void OnSharedLogicInitialized() {

        }

        protected virtual void BeforeSharedLogicInitialization(Dictionary<string, byte[]> datas, TDef definitions) { }

        public virtual T Process<T>(object commandObject)
        {
            var result = default(T);

            var command = (Command)commandObject;
            var serializedCommand = ((Command)commandObject).SerializedCommandData;
            var autoCommand = Serializer.Deserialize<CommandContainer>(serializedCommand);

            if (CommandSerialNumber >= command.SerialNumber) return result; //skip command duplicates

            if (CommandSerialNumber != (command.SerialNumber - 1))
            {
                throw new SharedLogicException(SharedLogicException.SharedLogicExceptionType.LOST_COMMANDS,
                    "Some commands were lost. State command serial number: " + CommandSerialNumber + " current command serial number: " + command.SerialNumber + " command type: " + GetCommandType(autoCommand)
                );
            }

            CommandSerialNumber = command.SerialNumber;

            CommandTimestamp = new DateTime(command.Timestamp);

            if (autoCommand != null)
            {
#if UNITY_EDITOR
                var cmdType = SharedLogicUtils<TDef>.CommandCodeToType[autoCommand.CommandId];
                var deserializedCommand = Serializer.Deserialize(cmdType, autoCommand.CommandData);
                var cmdParts = cmdType.Name.Split('_');
                var moduleName = cmdParts[0];
                var methodName = cmdParts[1];
                UnityEngine.Debug.LogFormat("Shared logic command: {0}.{1} args: {2}", moduleName, methodName, JsonConvert.SerializeObject(deserializedCommand));
#endif
                return (T)ProcessAutoCommand(autoCommand);
            }
            else
            {
                return result;
            }
        }

        private Type GetCommandType(CommandContainer command)
        {
            return SharedLogicUtils<TDef>.CommandCodeToType[command.CommandId];
        }

        protected virtual object ProcessAutoCommand(CommandContainer autoCommandContainer)
        {

            if (!SharedLogicUtils<TDef>.CommandCodeToType.TryGetValue(autoCommandContainer.CommandId, out var cmdType))
            {
                throw new InvalidOperationException($"Wrong command id. dump=" + JsonConvert.SerializeObject(autoCommandContainer));
            }

            var deserializedCommand = Serializer.Deserialize(cmdType, autoCommandContainer.CommandData);

            var cmdParts = cmdType.Name.Split('_');
            var moduleName = cmdParts[0];
            var methodName = cmdParts[1];

            if (!SharedLogicUtils<TDef>.AllModules.TryGetValue(moduleName, out var moduleType))
            {
                throw new InvalidOperationException("Module " + moduleName + " not found. dump=" + JsonConvert.SerializeObject(autoCommandContainer));
            }
            
            var moduleInstance = container.Resolve(moduleType);
            var method = moduleType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            var parameters = method.GetParameters();

            List<object> parameterValues = new List<object>();
            foreach (var parameterInfo in parameters)
            {
                var cmdField = cmdType.GetProperty(parameterInfo.Name);
                parameterValues.Add(cmdField.GetValue(deserializedCommand, null));
            }

            try
            {
                return method.Invoke(moduleInstance, parameterValues.ToArray());
            }
            catch (TargetInvocationException e)
            {
                // unwrap possible ResponseException expetion needed in MainHandlerBase
                if (e.InnerException == null)
                    throw;

                if (e.InnerException != null && (
                            e.InnerException.GetType().IsSubclassOf(typeof(ResponseException)) ||
                            e.InnerException.GetType() == typeof(ResponseException)
                         )
                    )
                {
                    _log.Error("", e);
                    throw e.InnerException;
                }

                throw;
            }
        }

        public virtual void ApplyCommands(List<Command> commands)
        {
            if (commands.Count == 0) { return; }

            var firstCommand = commands[0];

            for (int i = firstCommand.SerialNumber; i < firstCommand.SerialNumber + commands.Count; i++)
            {
                var cmd = commands[i - firstCommand.SerialNumber];
                if (cmd.SerialNumber != i)
                {
                    throw new SharedLogicException(
                        SharedLogicException.SharedLogicExceptionType.WRONG_COMMANDS_ORDER,
                        "Wrong commands order. Commands in packet: " + commands.Count + " 'for loop' index: " + i + " actual serial number: " + cmd.SerialNumber
                    );
                }
                Process<object>(cmd);
            }
        }

        public virtual string CommandsInJson(List<Command> commands)
        {
            if (commands.Count == 0) { return ""; }

            var commandList = new List<string>();

            for (int i = 0; i < commands.Count; i++)
            {
                var cmd = commands[i];
                var serializedCommand = cmd.SerializedCommandData;
                var autoCommand = Serializer.Deserialize<CommandContainer>(serializedCommand);
                var cmdType = SharedLogicUtils<TDef>.CommandCodeToType[autoCommand.CommandId];
                var deserializedCommand = Serializer.Deserialize(cmdType, autoCommand.CommandData);

                commandList.Add("\"" + cmdType + "\" : "+ JsonConvert.SerializeObject(deserializedCommand));
            }

            return "{\r\n" + string.Join(",\r\n", commandList.ToArray()) + "\r\n}";
        }

        public virtual string StateInJson()
        {
            var header = _state;
            var modules = new Dictionary<string, object>();
            foreach (var m in SerializableModules)
            {
                var module = (ISharedLogicModule)container.Resolve(m);
                modules[m.Name] = module.RawState;
            }
            return JsonConvert.SerializeObject(new { header, modules }, Formatting.Indented);
        }


        public Action OnExecuteScheduled { get; set; }
        public DateTime CommandTimestamp { get; set; }

        public uint SharedLogicVersion
        {
            get { return _state.SharedLogicVersion; }
            set { _state.SharedLogicVersion = value; }
        }

        public void Log(string message)
        {
            OnLogMessage(message);
        }

        public virtual T GetModule<T>()
        {
            return container.Resolve<T>();
        }

        public int GetStateControlHash()
        {
            var userId = _state.UserId;
            _state.UserId = Guid.Empty.ToByteArray();

            var sharedLogicVersion = _state.SharedLogicVersion;
            _state.SharedLogicVersion = 0;

            var commandSerialNumber = _state.CommandSerialNumber;
            _state.CommandSerialNumber = 0;

            var hash = HashUtils.ComputeHash(SerializedState);

            _state.UserId = userId;
            _state.SharedLogicVersion = sharedLogicVersion;
            _state.CommandSerialNumber = commandSerialNumber;
            
            return hash;
        }

        public void ExecuteOrderedScheduledActions()
        {
            _OrderedScheduledActionCaller.Execute();
        }

        public int CommandSerialNumber
        {
            get { return _state.CommandSerialNumber; }
            set { _state.CommandSerialNumber = value; }
        }

        public byte[] SerializedState
        {
            get
            {
                for (var i = 0; i < SerializableModules.Count; i++)
                {
                    var moduleType = SerializableModules[i];
                    var module = (ISharedLogicModule)container.Resolve(moduleType);
                    _state.ModulesDict[moduleType.Name] = module.SerializedState;
                }

                return Serializer.Serialize(_state);
            }
        }
        
        public virtual string GetHumanReadableInfo(string format)
        {
            return "Not implemented";
        }
    }
}
