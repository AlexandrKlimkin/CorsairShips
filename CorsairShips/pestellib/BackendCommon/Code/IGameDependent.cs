using System;
using GoogleSpreadsheet;
using PestelLib.SharedLogicBase;
using UnityDI;

namespace BackendCommon.Code
{
    public interface IGameDependent
    {
        IGameDefinitions Definitions { get; }
        byte[] DefaultState(Guid userId);
        ISharedLogic SharedLogic(byte[] stateBytes, IFeature feature);
        string StateToJson(byte[] state);
    }
}