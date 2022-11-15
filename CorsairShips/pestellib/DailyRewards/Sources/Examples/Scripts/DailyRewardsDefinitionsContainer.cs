using PestelLib.SharedLogic;
using UnityEngine;
using PestelLib.Serialization;

public class DailyRewardsDefinitionsContainer : MonoBehaviour, ISerializationCallbackReceiver {
#if UNITY_EDITOR
    [GoogleSpreadsheetUri(
        "https://script.google.com/macros/s/AKfycbzwd-Ik4Zp0sE1Mpr2aFcV5V75PsLogbMO8oTVutfupT5cNTe1s/exec?id=1kZMQgXrMU4lWQV5s6eQruRxYbIQm9HjnAQSHBimoUI4",
        "/../ProjectLib/ConcreteServer/App_Data/")]
#endif
    [SerializeField]
    public DailyRewardsTestDefinitions SharedLogicDefs;

    public void OnBeforeSerialize()
    {

    }

    public void OnAfterDeserialize()
    {
        SharedLogicDefs.OnAfterDeserialize();
    }
}

