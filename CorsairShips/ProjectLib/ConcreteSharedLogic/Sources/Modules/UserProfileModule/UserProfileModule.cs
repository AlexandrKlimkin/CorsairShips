using PestelLib.SharedLogicBase;

namespace PestelLib.SharedLogic.Modules {
    public class UserProfileModule : SharedLogicModule<UserProfileModuleState> {

        public string Nickname => State.Nickname;
        
        public ScheduledAction<string> OnNicknameChanged;

        public UserProfileModule() {
            OnNicknameChanged = new ScheduledAction<string>(ScheduledActionCaller);
        }

        public override void MakeDefaultState() {
            base.MakeDefaultState();
            State.Nickname = $"Player";
        }

        [SharedCommand]
        internal void ChangeNickname(string nickName) {
            if(string.Equals(nickName, State.Nickname))
                return;
            State.Nickname = nickName;
            OnNicknameChanged?.Schedule(nickName);
        }
    }
}