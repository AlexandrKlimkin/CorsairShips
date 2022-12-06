using PestelLib.SharedLogic.Modules; 
namespace S{
    [MessagePack.MessagePackObject]
    public class MoneyModule_CheckBalance {}

    [MessagePack.MessagePackObject]
    public class MoneyModule_BuyMoneyPacket {
        [MessagePack.Key(1)]
        public string packetId { get; set;}
	}


}