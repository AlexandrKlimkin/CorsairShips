using PestelLib.SharedLogic.Modules; 
namespace S{
    [MessagePack.MessagePackObject]
    public class PromoModule_UsePromo {
        [MessagePack.Key(1)]
        public string promocode { get; set;}

        [MessagePack.Key(2)]
        public string function { get; set;}

        [MessagePack.Key(3)]
        public string parameter { get; set;}
	}


}