using MessagePack;

namespace PestelLib.SharedLogic.Modules {
    
    [MessagePackObject]
    public class UserProfileModuleState {
        [Key(0)]
        public string Nickname;
    }
}