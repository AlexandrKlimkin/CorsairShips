using System.Collections;
using GoogleSpreadsheet;

namespace PestelLib.Serialization
{
    public interface IValidatableDef
    {
//#if UNITY_EDITOR
    	bool Validate(IList oldPage, IList newPage, IGameDefinitions newDefs, out string error);
//#endif
    }
}
