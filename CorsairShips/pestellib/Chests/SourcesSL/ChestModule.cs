using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PestelLib.Serialization;
using PestelLib.ServerShared;
using PestelLib.SharedLogicBase;
using ServerShared;
using UnityDI;

namespace PestelLib.SharedLogic.Modules
{
    [System.Reflection.Obfuscation(ApplyToMembers=false)]
    public class ChestModule : SharedLogicModule<ChestModuleState>
    {
        [GooglePageRef("ChestRewards")] [Dependency] protected List<ChestsRewardDef> RewardDefs;
        [GooglePageRef("ChestRewardsPool")] [Dependency] protected List<ChestsRewardPoolDef> RewardPoolDefs;
        [GooglePageRef("Chests")] [Dependency] protected List<ChestDef> ChestDefs;
        [GooglePageRef("ChestRewardsPoolList")] [Dependency] protected List<ChestsRewardPoolListDef> RewardPoolListDefs;

        [Dependency] protected Dictionary<string, ChestsRewardDef> _chestsRewardDefDict;
        [Dependency] protected RandomModule _randomModule;

        public virtual ChestsRewardDef SetupReward(ChestsRewardDef rewardDef)
        {
            SharedCommandCallstack.CheckCallstack();
            return CloneMessagePackObject(rewardDef); //don't modify defs
        }

        public virtual List<ChestsRewardDef> GiveChestRewardsById(string chestId)
        {
            var result = new List<ChestsRewardDef>();

            var chestDef = ChestDefs.FirstOrDefault(x => x.Id == chestId);
            UniversalAssert.IsNotNull(chestDef, "Chest not found: " + chestId);

            var poolList = RewardPoolListDefs.Where(x => x.Tag == chestDef.PoolListId).ToArray();
            UniversalAssert.IsTrue(poolList.Length > 0, "RewardPoolList is empty" + chestDef.PoolListId);

            foreach (var rewardPoolElement in poolList)
            {
                var possibleReward = RewardPoolDefs.Where(x => x.Tag == rewardPoolElement.RewardPoolId).ToArray();
                UniversalAssert.IsTrue(possibleReward.Length > 0, "Reward pool is empty: " + rewardPoolElement.RewardPoolId);

                var randomRewardId = possibleReward.ChooseByPredefinedRandom((x) => x.RewardWeight, _randomModule.Random()).RewardId;

                var rewardDef = RewardDefs.FirstOrDefault(x => x.Id == randomRewardId);
                UniversalAssert.IsNotNull(rewardDef, "Can't find rewardDef by id: " + randomRewardId);

                var def = SetupReward(rewardDef);
                var givedReward = GiveRewardToPlayer(def);
                if (givedReward != null)
                {
                    result.Add(givedReward);
                }
            }
            return result;
        }

        [SharedCommand]
        internal List<ChestsRewardDef> GiveChestById(string chestId)
        {
            SharedCommandCallstack.CheckCallstack();
            return GiveChestRewardsById(chestId);
        }

        [SharedCommand]
        internal List<ChestsRewardDef> GiveChestByRarity(int chestRarity)
        {
            var chests = ChestDefs.Where(x => x.ChestRarity == chestRarity).ToArray();
            UniversalAssert.IsTrue(chests.Length > 0, "chest with specified rarity not found " + chestRarity);
            var chest = chests.ChooseByPredefinedRandom((chestDef) => chestDef.ChestRandomWeight, _randomModule.Random());
            UniversalAssert.IsNotNull(chest, "Empty chest list?");
            return GiveChestById(chest.Id);
        }

        [SharedCommand]
        internal ChestsRewardDef GiveByRewardId(string rewardId)
        {
            UniversalAssert.IsTrue(_chestsRewardDefDict.ContainsKey(rewardId), $"Reward {rewardId} not found.");
            if (!_chestsRewardDefDict.TryGetValue(rewardId, out var def))
            {
                return null;
            }
            def = SetupReward(_chestsRewardDefDict[rewardId]);
            GiveRewardToPlayer(def);
            return def;
        }

        public virtual ChestsRewardDef GiveRewardToPlayer(ChestsRewardDef rewardDef)
        {
            //копия объекта - не стоит модифицировать сам деф
            rewardDef = CloneMessagePackObject(rewardDef);

            SharedCommandCallstack.CheckCallstack();

            Log("ChestModule :: GiveRewardToPlayer");

            Log("Reward for player: " + JsonConvert.SerializeObject(rewardDef, Formatting.Indented));
            //этот код вызывается из команды ШЛ, считайте что вы внутри обычной [SharedCommand],
            //тут можно дёргать любые internal методы ШЛ и менять стейт игрока

            Log("Received reward: " + JsonConvert.SerializeObject(rewardDef));

            switch (rewardDef.ItemType)
            {
                case "ingame":
                    //_moneyModule.AddIngame(rewardDef.Amount);
                    break;
                case "real":
                    //_moneyModule.AddReal(rewardDef.Amount);
                    break;
                case "ship":
                    var allShips = new[] {"ArbitrCruiser", "ArchangelBattleship"};

                    rewardDef.ItemId = allShips[_randomModule.RandomInt(allShips.Length)];
                    break;
                case "item":
                    var allItems = new[] { "AdditionalMissiles", "ClusterMissiles" };
                    rewardDef.ItemId = allItems[_randomModule.RandomInt(allItems.Length)];
                    break;
                /*
                case "weapon":
                     //В сундуке может быть указано, что он даёт какое-то определенное оружие
                     //тогда у него будет в rewardDef UseRarityAndTier == false
                     //А может быть просто случайное оружие заданной редкости и тира
                    if (rewardDef.UseRarityAndTier)
                    {
                        var allPossibleWeapons = _weaponDefs.Where(x => x.Tier == rewardDef.Tier && x.Rarity == rewardDef.Rarity).ToList();
                        if (allPossibleWeapons.Count == 0) return false; //если мы не можем выдать ничего игроку подходящего по параметрам, нужно вернуть false

                        if (rewardDef.UseMaxPlayerTier)
                        {
                            //тут можно дополнительно отфильтровать слишком высокоуровневое для игрока оружие
                        }

                        //не забывайте что нельзя рандом не из ШЛ использовать внутри команды ШЛ
                        //var randomWeaponDef = allPossibleWeapons[_randomModule.RandomInt(allPossibleWeapons.Count)];

                        rewardDef.ItemId = randomWeaponDef.Id; //нужно поставить в ItemType, что бы сундук мог в итоге показать, что в нём выпало
                                                               //тут должен быть сам код, который добавляет в стейт игрока оружие по дефу randomWeaponDef
                    }
                    else
                    {
                        var weaponDef = _weaponDefsDict[rewardDef.ItemId];
                        //в этом случае геймдизайнером уже задано, какое должно выдаться оружие
                    }
                    break;
                    */
            }
            return rewardDef;
        }

    }
}