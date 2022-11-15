using System;
using System.Collections.Generic;
using PestelLib.UniversalSerializer;
using S;
using UnityDI;

namespace PestelLib.SharedLogicBase
{
    public interface ISharedLogic
    {
        int GetStateControlHash();
        byte[] SerializedState { get; }

        void ApplyCommands(List<Command> commands);
        T Process<T>(object command);
        string StateInJson();
        int CommandSerialNumber { get; set; }
        uint SharedLogicVersion { get; set; }
        void Log(string message);
        DateTime CommandTimestamp { get; set; }
        Action OnExecuteScheduled { get; set; }
        void ExecuteOrderedScheduledActions();
        T GetModule<T>();
        Container Container { get; }
        Guid PlayerId { get; set; }
        string PlayerName { get; }
        string CommandsInJson(List<Command> commands);
        List<Type> SerializableModules { get; }
        ISerializer Serializer { get; set; }
        string GetHumanReadableInfo(string format);
    }
}