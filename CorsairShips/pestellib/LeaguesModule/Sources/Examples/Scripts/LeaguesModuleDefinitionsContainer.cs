using PestelLib.SharedLogic;
using UnityEngine;
using PestelLib.Serialization;

public class LeaguesModuleDefinitionsContainer : MonoBehaviour, ISerializationCallbackReceiver
{
#if UNITY_EDITOR
    [GoogleSpreadsheetUri(
        "https://script.google.com/macros/s/AKfycbyJWowhzlMEggsadmLRgAPS3SnfTcfzqP0IacI66dL0b5APxuE/exec?id=1kZMQgXrMU4lWQV5s6eQruRxYbIQm9HjnAQSHBimoUI4", 
        "/../ProjectLib/ConcreteServer/App_Data/")]
#endif
    [SerializeField]
    public LeaguesModuleTestDefinitions SharedLogicDefs;

    public void OnBeforeSerialize()
    {

    }

    public void OnAfterDeserialize()
    {
        SharedLogicDefs.OnAfterDeserialize();
    }
}
