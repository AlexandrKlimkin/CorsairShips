
using System.Collections.Generic;
using PestelLib.SharedLogic.Modules;
using PestelLib.SharedLogicBase;
using PestelLib.ServerShared;
using PestelLib.UniversalSerializer;
using UnityDI;
using S;

namespace PestelLib.SharedLogicClient {
    public class SharedLogicCommand
    {
        private static CommandProcessor _commandProcessor;

        private static CommandProcessor CommandProcessor
        {
            get
            {
                if (_commandProcessor == null)
                {
                    _commandProcessor = ContainerHolder.Container.Resolve<CommandProcessor>();
                }
                return _commandProcessor;
            }
        }

        
public partial class CurrencyModule
{
    [SharedCommand("")]
    public static void AddCurrency(CurrencyType currencyType,int count)
    {
        var cmd = new S.CommandContainer
        {
            CommandId = 240269443,
            CommandData = Serializer.Serialize(new CurrencyModule_AddCurrency
            {
                currencyType = currencyType
,count = count

            })
        };

        var cmdBytes = Serializer.Serialize(cmd);
        CommandProcessor.ExecuteCommand<object>(cmdBytes);
    }
}


public partial class CurrencyModule
{
    [SharedCommand("")]
    public static void SpendCurrency(CurrencyType currencyType,int count)
    {
        var cmd = new S.CommandContainer
        {
            CommandId = 949613451,
            CommandData = Serializer.Serialize(new CurrencyModule_SpendCurrency
            {
                currencyType = currencyType
,count = count

            })
        };

        var cmdBytes = Serializer.Serialize(cmd);
        CommandProcessor.ExecuteCommand<object>(cmdBytes);
    }
}


public partial class ItemsModule
{
    [SharedCommand("")]
    public static void AddItem(string itemId,int count)
    {
        var cmd = new S.CommandContainer
        {
            CommandId = 1020634962,
            CommandData = Serializer.Serialize(new ItemsModule_AddItem
            {
                itemId = itemId
,count = count

            })
        };

        var cmdBytes = Serializer.Serialize(cmd);
        CommandProcessor.ExecuteCommand<object>(cmdBytes);
    }
}


public partial class ItemsModule
{
    [SharedCommand("")]
    public static void SpendItem(string itemId,int count)
    {
        var cmd = new S.CommandContainer
        {
            CommandId = -694861833,
            CommandData = Serializer.Serialize(new ItemsModule_SpendItem
            {
                itemId = itemId
,count = count

            })
        };

        var cmdBytes = Serializer.Serialize(cmd);
        CommandProcessor.ExecuteCommand<object>(cmdBytes);
    }
}


public partial class MoneyModule
{
    [SharedCommand("")]
    public static void CheckBalance()
    {
        var cmd = new S.CommandContainer
        {
            CommandId = 1439172566,
            CommandData = Serializer.Serialize(new MoneyModule_CheckBalance
            {
                
            })
        };

        var cmdBytes = Serializer.Serialize(cmd);
        CommandProcessor.ExecuteCommand<object>(cmdBytes);
    }
}


public partial class MoneyModule
{
    [SharedCommand("")]
    public static void BuyMoneyPacket(string packetId)
    {
        var cmd = new S.CommandContainer
        {
            CommandId = -1320644514,
            CommandData = Serializer.Serialize(new MoneyModule_BuyMoneyPacket
            {
                packetId = packetId

            })
        };

        var cmdBytes = Serializer.Serialize(cmd);
        CommandProcessor.ExecuteCommand<object>(cmdBytes);
    }
}


public partial class MyTestModuleLogic
{
    [SharedCommand("")]
    public static void IncrementMoney()
    {
        var cmd = new S.CommandContainer
        {
            CommandId = 1555876551,
            CommandData = Serializer.Serialize(new MyTestModuleLogic_IncrementMoney
            {
                
            })
        };

        var cmdBytes = Serializer.Serialize(cmd);
        CommandProcessor.ExecuteCommand<object>(cmdBytes);
    }
}


public partial class MyTestModuleLogic
{
    [SharedCommand("")]
    public static void AnotherTestCommand()
    {
        var cmd = new S.CommandContainer
        {
            CommandId = -1644153916,
            CommandData = Serializer.Serialize(new MyTestModuleLogic_AnotherTestCommand
            {
                
            })
        };

        var cmdBytes = Serializer.Serialize(cmd);
        CommandProcessor.ExecuteCommand<object>(cmdBytes);
    }
}


public partial class MyTestModuleLogic
{
    [SharedCommand("")]
    public static void AddByDefinition(string id)
    {
        var cmd = new S.CommandContainer
        {
            CommandId = -1424647763,
            CommandData = Serializer.Serialize(new MyTestModuleLogic_AddByDefinition
            {
                id = id

            })
        };

        var cmdBytes = Serializer.Serialize(cmd);
        CommandProcessor.ExecuteCommand<object>(cmdBytes);
    }
}


public partial class PropertyModule
{
    [SharedCommand("")]
    public static void BuyProperty(string propertyId)
    {
        var cmd = new S.CommandContainer
        {
            CommandId = -1031356253,
            CommandData = Serializer.Serialize(new PropertyModule_BuyProperty
            {
                propertyId = propertyId

            })
        };

        var cmdBytes = Serializer.Serialize(cmd);
        CommandProcessor.ExecuteCommand<object>(cmdBytes);
    }
}


public partial class PropertyModule
{
    [SharedCommand("")]
    public static void SetPropertyAsOwned(string propertyId)
    {
        var cmd = new S.CommandContainer
        {
            CommandId = -1022893558,
            CommandData = Serializer.Serialize(new PropertyModule_SetPropertyAsOwned
            {
                propertyId = propertyId

            })
        };

        var cmdBytes = Serializer.Serialize(cmd);
        CommandProcessor.ExecuteCommand<object>(cmdBytes);
    }
}


public partial class PropertyModule
{
    [SharedCommand("")]
    public static void SetSpotted(string id)
    {
        var cmd = new S.CommandContainer
        {
            CommandId = 2100917234,
            CommandData = Serializer.Serialize(new PropertyModule_SetSpotted
            {
                id = id

            })
        };

        var cmdBytes = Serializer.Serialize(cmd);
        CommandProcessor.ExecuteCommand<object>(cmdBytes);
    }
}


public partial class RewardsModule
{
    [SharedCommand("")]
    public static void ClaimRewards(Match_Result matchResult,int points)
    {
        var cmd = new S.CommandContainer
        {
            CommandId = -505993302,
            CommandData = Serializer.Serialize(new RewardsModule_ClaimRewards
            {
                matchResult = matchResult
,points = points

            })
        };

        var cmdBytes = Serializer.Serialize(cmd);
        CommandProcessor.ExecuteCommand<object>(cmdBytes);
    }
}


public partial class UserProfileModule
{
    [SharedCommand("")]
    public static void ChangeNickname(string nickName)
    {
        var cmd = new S.CommandContainer
        {
            CommandId = -1438990908,
            CommandData = Serializer.Serialize(new UserProfileModule_ChangeNickname
            {
                nickName = nickName

            })
        };

        var cmdBytes = Serializer.Serialize(cmd);
        CommandProcessor.ExecuteCommand<object>(cmdBytes);
    }
}


    }
}
