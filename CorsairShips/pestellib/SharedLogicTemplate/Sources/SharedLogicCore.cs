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


#endregion

            return result;
        }
    }
}

