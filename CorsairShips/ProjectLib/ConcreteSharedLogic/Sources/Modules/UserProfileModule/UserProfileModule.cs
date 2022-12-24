using System;
using System.Collections.Generic;
using PestelLib.SharedLogicBase;
using UnityDI;

namespace PestelLib.SharedLogic.Modules {
    public class UserProfileModule : SharedLogicModule<UserProfileModuleState> {

        [Dependency]
        private readonly RandomModule _RandomModule;
        [Dependency]
        private readonly List<NicknameDef> _NicknameDefs;
        
        public string Nickname => State.Nickname;
        
        public ScheduledAction<string> OnNicknameChanged;

        public UserProfileModule() {
            OnNicknameChanged = new ScheduledAction<string>(ScheduledActionCaller);
        }

        public override void MakeDefaultState() {
            base.MakeDefaultState();
            State.Nickname = GenerateNickname_Player_Number((float)_RandomModule.RandomDecimal(0,1));
        }

        [SharedCommand]
        internal void ChangeNickname(string nickName) {
            if(string.Equals(nickName, State.Nickname))
                return;
            State.Nickname = nickName;
            OnNicknameChanged?.Schedule(nickName);
        }

        public string GenerateNickname_Player_Number(float randValue) {
            return $"Player_{Math.Round(randValue * 10000)}";
        }
        
        public string GenerateNickname_Database(float randValue) {
            var count = _NicknameDefs.Count;
            var index = (int)Math.Round(count * randValue);
            return _NicknameDefs[index].Nickname;
        }

        public string GenerateBotNickname(float typeRandValue, float indexRandValue) {
            if (typeRandValue > 0.3) {
                return GenerateNickname_Database(indexRandValue);
            }
            else {
                return GenerateNickname_Player_Number(indexRandValue);
            }
        }
    }
}