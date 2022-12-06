using PestelLib.SharedLogic.Modules; 
namespace S{
    [MessagePack.MessagePackObject]
    public class MyTestModuleLogic_IncrementMoney {}

    [MessagePack.MessagePackObject]
    public class MyTestModuleLogic_AnotherTestCommand {}

    [MessagePack.MessagePackObject]
    public class MyTestModuleLogic_AddByDefinition {
        [MessagePack.Key(1)]
        public string id { get; set;}
	}


}