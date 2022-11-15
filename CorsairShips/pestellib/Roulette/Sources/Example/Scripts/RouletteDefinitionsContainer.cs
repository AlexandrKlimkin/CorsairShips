using PestelLib.Serialization;
using PestelLib.SharedLogic;
using UnityEngine;

public class RouletteDefinitionsContainer : MonoBehaviour
{
#if UNITY_EDITOR
    [GoogleSpreadsheetUri(
        "https://script.google.com/macros/s/AKfycbzwd-Ik4Zp0sE1Mpr2aFcV5V75PsLogbMO8oTVutfupT5cNTe1s/exec?id=1LfAhdfsxQr8poSRjHC0DgJt1AodnraeXuuhQ-BWZUEk",
        "/../ProjectLib/ConcreteServer/App_Data/")]
#endif
    [SerializeField]
    public RouletteTestDefinitions SharedLogicDefs;

    public void OnBeforeSerialize()
    {

    }

    public void OnAfterDeserialize()
    {
        SharedLogicDefs.OnAfterDeserialize();
    }
}

