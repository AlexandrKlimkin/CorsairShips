using MessagePack;
using PestelLib.SharedLogicBase;
using S;
using PestelLib.SharedLogic.Modules;

namespace PestelLib.SharedLogic
{
    public class SharedLogicCore : SharedLogicDefault<Definitions>
    {
        public SharedLogicCore(UserProfile state, Definitions defs) : base(state, defs)
        {
        }

        protected override object ProcessAutoCommand(CommandContainer autoCommand)
        {
            object result = null;

#region AUTO_GENERATED_COMMAND_WRAPPER
                if (autoCommand.CommandId == 1439172566) {
                    var cmd = Serializer.Deserialize<MoneyModule_CheckBalance>(autoCommand.CommandData);
                       GetModule<MoneyModule>().CheckBalance();
                }

                else if (autoCommand.CommandId == -1320644514) {
                    var cmd = Serializer.Deserialize<MoneyModule_BuyMoneyPacket>(autoCommand.CommandData);
                       GetModule<MoneyModule>().BuyMoneyPacket(cmd.packetId);
                }

                else if (autoCommand.CommandId == 1555876551) {
                    var cmd = Serializer.Deserialize<MyTestModuleLogic_IncrementMoney>(autoCommand.CommandData);
                       GetModule<MyTestModuleLogic>().IncrementMoney();
                }

                else if (autoCommand.CommandId == -1644153916) {
                    var cmd = Serializer.Deserialize<MyTestModuleLogic_AnotherTestCommand>(autoCommand.CommandData);
                       GetModule<MyTestModuleLogic>().AnotherTestCommand();
                }

                else if (autoCommand.CommandId == -1424647763) {
                    var cmd = Serializer.Deserialize<MyTestModuleLogic_AddByDefinition>(autoCommand.CommandData);
                       GetModule<MyTestModuleLogic>().AddByDefinition(cmd.id);
                }

                else if (autoCommand.CommandId == -1031356253) {
                    var cmd = Serializer.Deserialize<PropertyModule_BuyProperty>(autoCommand.CommandData);
                       GetModule<PropertyModule>().BuyProperty(cmd.propertyId);
                }

                else if (autoCommand.CommandId == -1022893558) {
                    var cmd = Serializer.Deserialize<PropertyModule_SetPropertyAsOwned>(autoCommand.CommandData);
                       GetModule<PropertyModule>().SetPropertyAsOwned(cmd.propertyId);
                }

                else if (autoCommand.CommandId == 2100917234) {
                    var cmd = Serializer.Deserialize<PropertyModule_SetSpotted>(autoCommand.CommandData);
                       GetModule<PropertyModule>().SetSpotted(cmd.id);
                }

#endregion

            return result;
        }
    }
}

