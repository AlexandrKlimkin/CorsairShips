using System.Collections.Generic;
using System.Linq;
using PestelLib.Serialization;
using PestelLib.SharedLogicBase;
using UnityDI;

namespace PestelLib.SharedLogic.Modules
{
    public class MyTestModuleLogic : SharedLogicModule<MyTestModuleState>
    {
#pragma warning disable 649
        [GooglePageRef("MyTestModuleDef")]
        [Dependency] private List<MyTestModuleDef> Definitions;
#pragma warning restore 649

        public int Ingame
        {
            get { return State.Ingame; }
        }

        [SharedCommand]
        internal void IncrementMoney() 
        {
            State.Ingame += 100;  
        }

        [SharedCommand]
        internal void AnotherTestCommand()
        {
            State.Ingame += 55;
        }

        [SharedCommand]
        internal void AddByDefinition(string id)
        {
            var row = Definitions.First(x => x.Id == id);
            State.Ingame += row.Amount;
        }
    }
}