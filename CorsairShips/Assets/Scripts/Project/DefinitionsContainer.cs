using PestelLib.Serialization;
using PestelLib.SharedLogic;
using PestelLib.SharedLogic.Defs;
using UnityEngine;

public class DefinitionsContainer : MonoBehaviour
{
#if UNITY_EDITOR
    [GoogleSpreadsheetUri(
        "https://script.google.com/macros/s/AKfycbyJWowhzlMEggsadmLRgAPS3SnfTcfzqP0IacI66dL0b5APxuE/exec?id=1zgAM-z7GxjDqxHIpUlQbv28z8e8y_OGLHmUC5nTnEP4",
        "/../ProjectLib/ConcreteServer/App_Data/")]
#endif
    public Definitions Definitions;

#if UNITY_EDITOR
    [GoogleSpreadsheetUri("https://script.google.com/macros/s/AKfycbyJWowhzlMEggsadmLRgAPS3SnfTcfzqP0IacI66dL0b5APxuE/exec?id=1zgAM-z7GxjDqxHIpUlQbv28z8e8y_OGLHmUC5nTnEP4",
        "/../ProjectLib/ConcreteServer/App_Data/")]
#endif
    public LocalizationDefContainer LocalizationDefContainer;
}



