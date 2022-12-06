using System.Collections.Generic;
using System.Linq;
using PestelLib.Serialization;
using PestelLib.SharedLogicBase;
using UnityDI;

namespace PestelLib.SharedLogic.Modules
{
    public class MoneyModule : SharedLogicModule<MoneyModuleState>
    {
        [GooglePageRef("BankCardsDefs")]
        [Dependency] protected List<BankCardsDef> _bankCards;

        public ScheduledAction<MoneyType, int> OnSpentInapp;
        public ScheduledAction<MoneyType, int> OnBuyMoneyPackage;
        public ScheduledAction OnIngameChanged;
        public ScheduledAction OnRealChanged;
        public ScheduledAction OnOutOfMoney;

        public MoneyModule()
        {
            OnSpentInapp = new ScheduledAction<MoneyType, int>(ScheduledActionCaller);
            OnBuyMoneyPackage = new ScheduledAction<MoneyType, int>(ScheduledActionCaller);
            OnIngameChanged = new ScheduledAction(ScheduledActionCaller);
            OnRealChanged = new ScheduledAction(ScheduledActionCaller);
            OnOutOfMoney = new ScheduledAction(ScheduledActionCaller);
        }

        public int Ingame
        {
            get
            {
                return State.Ingame + State.IngameInapp;
            }
        }


        [SharedCommand]
        internal void CheckBalance()
        {
            if (State.Ingame <= 0)
            {
                OnOutOfMoney.Schedule();
                Log("Out of money!");
                //MessageBoxScreen.Show("You're out of money", "Take some for free", "Credit", "Thanks", "", () => AddIngame(_lowestBid * 2), () => { }, () => AddIngame(_lowestBid * 2), "RewardMessageBoxScreen");
            }
        }

        internal void AddIngame(int ingame)
        {
            State.Ingame += ingame;
            OnIngameChanged.Schedule();
        }

        internal void SpendIngame(int ingame)
        {
            State.IngameInapp -= ingame;
            if (State.IngameInapp < 0)
            {
                State.Ingame += State.IngameInapp;
                State.IngameInapp = 0;
            }
            OnIngameChanged.Schedule();
        }

        public int Real
        {
            get
            {
                return State.Real + State.RealInapp;
            }
        }

        internal void AddReal(int real)
        {
            State.Real += real;
            OnRealChanged.Schedule();
        }

        internal void SpendReal(int real)
        {
            State.RealInapp -= real;
            if (State.RealInapp < 0)
            {
                State.Real += State.RealInapp;
                State.RealInapp = 0;
            }
            OnRealChanged.Schedule();
        }

        [SharedCommand]
        internal void BuyMoneyPacket(string packetId)
        {
            Log("buy packet " + packetId);
            
            var packetDef = _bankCards.First(x => x.Id == packetId);
            switch (packetDef.MoneyType)
            {
                case MoneyType.MoneyTypeIngame:
                    State.IngameInapp += packetDef.Amount;
                    OnIngameChanged.Schedule();
                    break;

                case MoneyType.MoneyTypeReal:
                    State.RealInapp += packetDef.Amount;
                    OnRealChanged.Schedule();
                    break;
            }

            OnBuyMoneyPackage.Schedule(packetDef.MoneyType, packetDef.Amount);
        }
    }
}