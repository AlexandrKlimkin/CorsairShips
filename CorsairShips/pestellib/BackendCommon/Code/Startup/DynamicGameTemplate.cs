using System;
using System.Collections.Generic;
using System.Reflection;
using BackendCommon.Code;
using GoogleSpreadsheet;
using Newtonsoft.Json;
using PestelLib.SharedLogicBase;
using PestelLib.UniversalSerializer;
using S;
using ServerLib;
using UnityDI;

namespace Server
{
    public class DynamicGameTemplate : IGameDependent
    {
        public IGameDefinitions Definitions { get; }
        private readonly ConstructorInfo _sharedLogicConstructorInfo;
        private readonly DefaultStateFactory _defaultStateFactory;

        public DynamicGameTemplate(ConstructorInfo sharedLogicConstructorInfo, Type definitionsType, Action<IGameDefinitions> definitionsLoaded, DefaultStateFactory defaultStateFactory)
        {
            _defaultStateFactory = defaultStateFactory;
            _sharedLogicConstructorInfo = sharedLogicConstructorInfo;
            Definitions = DefsLoader.Load(definitionsType);
            Definitions.OnAfterDeserialize();
            if (definitionsLoaded != null)
                definitionsLoaded.Invoke(Definitions);
        }

        public byte[] DefaultState(Guid userId)
        {
            return Serializer.Serialize(_defaultStateFactory.MakeDefaultState(userId));
        }
        
        public ISharedLogic SharedLogic(byte[] stateBytes, IFeature feature)
        {
            var state = Serializer.Deserialize<UserProfile>(stateBytes);
            return (ISharedLogic) _sharedLogicConstructorInfo.Invoke(new Object[] {state, Definitions, feature});
        }

        public List<object> DeserializeCommands(byte[] commandsData)
        {
            var batch = Serializer.Deserialize<CommandBatch>(commandsData);
            return batch.commandsList.ConvertAll(cmd => (object)cmd);
        }

        public string StateToJson(byte[] stateBytes)
        {
            var state = Serializer.Deserialize<UserProfile>(stateBytes);
            return JsonConvert.SerializeObject(state, Formatting.Indented);
        }
    }
}
