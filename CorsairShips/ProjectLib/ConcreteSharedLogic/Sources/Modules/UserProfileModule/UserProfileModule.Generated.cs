using PestelLib.SharedLogic.Modules; 
namespace S{
    [MessagePack.MessagePackObject]
    public class UserProfileModule_ChangeNickname {
        [MessagePack.Key(1)]
        public string nickName { get; set;}
	}


}