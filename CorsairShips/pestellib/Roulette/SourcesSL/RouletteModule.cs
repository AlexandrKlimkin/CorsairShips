using System;
using System.Collections.Generic;
using System.Linq;
using PestelLib.Serialization;
using PestelLib.SharedLogicBase;
using ServerShared;
using UnityDI;

namespace PestelLib.SharedLogic.Modules
{
    [System.Reflection.Obfuscation(ApplyToMembers=false)]
    public class PirateBoxResult
    {
        public List<int> RewardIndices = new List<int>();
        public List<ChestsRewardDef> PossibleRewards = new List<ChestsRewardDef>();
        public List<ChestsRewardDef> BaitRewards = new List<ChestsRewardDef>();
        public int BonusSoft;
    }
    
    [System.Reflection.Obfuscation(ApplyToMembers=false)]
    public class RouletteModule : SharedLogicModule<RouletteModuleState>
    {
        [GooglePageRef("ChestRewards")] [Dependency] protected List<ChestsRewardDef> RewardDefs;
        [GooglePageRef("ChestRewardsPool")] [Dependency] protected List<ChestsRewardPoolDef> RewardPoolDefs;
        [GooglePageRef("Chests")] [Dependency] protected List<ChestDef> ChestDefs;
        [GooglePageRef("ChestRewardsPoolList")] [Dependency] protected List<ChestsRewardPoolListDef> RewardPoolListDefs;
        [GooglePageRef("PirateBoxes")] [Dependency] protected List<PirateBoxDef> PirateBoxDefs;
        [GooglePageRef("PirateBoxChests")] [Dependency] protected List<PirateBoxChestDef> PirateBoxChestDefs;

        [Dependency] protected RandomModule _randomModule;
        [Dependency] protected ChestModule _chestModule;
        [Dependency] protected RouletteEventsModule _rouletteEventsModule;
        [Dependency] protected SettingsModuleBase _settings;

        public ScheduledAction<int, string, string> OnKeysEarned;
        public ScheduledAction<int, string> OnKeysSpent;
        public ScheduledAction OnKeysChanged;
        public ScheduledAction OnProgressChanged;

        public const string OpenWithAds = "Ads";
        public const string OpenWithKeys = "Shop";

        public RouletteModule()
        {
            OnKeysEarned = new ScheduledAction<int, string, string>(ScheduledActionCaller);
            OnKeysSpent = new ScheduledAction<int, string>(ScheduledActionCaller);
            OnKeysChanged = new ScheduledAction(ScheduledActionCaller);
            OnProgressChanged = new ScheduledAction(ScheduledActionCaller);
        }

        public int Keys { get { return State.Keys; } }
        public float MegaChestProgress { get { return State.MegaChestProgress; } }

        //TODO cheat
        [SharedCommand]
        internal int AddKeys(int amount, string source, string itemId)
        {
            State.Keys += amount;
            OnKeysEarned.Schedule(amount, source, itemId);
            OnKeysChanged.Schedule();

            return State.Keys;
        }

        internal int WithdrawKeys(int keys, string target)
        {
            State.Keys -= keys;
            OnKeysSpent.Schedule(keys, target);
            OnKeysChanged.Schedule();

            return State.Keys;
        }

        internal float AddMegaBoxProgress(float progress)
        {
            State.MegaChestProgress += progress;
            OnProgressChanged.Schedule();

            return State.MegaChestProgress;
        }

        internal void ResetMegaBoxProgress()
        {
            State.MegaChestProgress = 0;
            OnProgressChanged.Schedule();
        }

        [SharedCommand]
        internal PirateBoxResult OpenBox(string boxId, bool free)
        {
            return OpenRouletteBox(boxId, free);
        }

        [SharedCommand]
        internal PirateBoxResult OpenBoxTyped(string boxId, string type)
        {
            return OpenRouletteBox(boxId, type);
        }

        protected virtual bool ValidateChest(PirateBoxChestDef chest, bool free)
        {
            if (free && !chest.ForFree)
            {
                Log("Chest cant be free");
                return false;
            }

            if (free && !_rouletteEventsModule.AdsCooldownPassed)
            {
                Log("Ads cooldown not passed");
                return false;
            }

            return true;
        }

        internal virtual PirateBoxResult OpenRouletteBox(string boxId, bool free)
        {
            return OpenRouletteBox(boxId, free ? OpenWithAds : OpenWithKeys);
        }

        protected bool ResetClaimTypeCooldown(string type, long cooldown)
        {
            if(State.LastClaimsByType == null)
                State.LastClaimsByType = new List<KeyValuePair<string, long>>();
            type = type.ToLower();
            var idx = State.LastClaimsByType.FindIndex(_ => _.Key == type);
            if (idx < 0)
            {
                var t = new KeyValuePair<string, long>(type, CommandTimestamp.Ticks);
                State.LastClaimsByType.Add(t);
                return true;
            }
            else if (CommandTimestamp.Ticks - State.LastClaimsByType[idx].Value < cooldown)
                return false;

            State.LastClaimsByType[idx] = new KeyValuePair<string, long>(type, CommandTimestamp.Ticks);
            return true;

        }

        internal virtual PirateBoxResult OpenRouletteBox(string boxId, string type)
        {
            PirateBoxChestDef chestDef = PirateBoxChestDefs.FirstOrDefault(x => x.Id == boxId);

            UniversalAssert.IsNotNull(chestDef, "Can't find PirateBoxChestDef with ID: " + boxId);
            if (chestDef == null)
                return null;

            var free = type == OpenWithAds;
            if (!ValidateChest(chestDef, free))
                return null;

            if (free)
                _rouletteEventsModule.SetAdsTimeStamp();

            var cd = _settings.GetValue("RouletteCd" + type, TimeSpan.Zero);
            if (!ResetClaimTypeCooldown(type, cd.Ticks))
            {
                SharedLogic.Log(string.Format("Cooldown not passed for {0}", type));
                return null;
            }

            if (type == OpenWithKeys)
            {
                if (State.Keys < chestDef.CostKeys)
                {
                    SharedLogic.Log(string.Format("Not enough keys to open {0}. Need {1}, got {2}", boxId, chestDef.CostKeys, State.Keys));
                    return null;
                }
                WithdrawKeys(chestDef.CostKeys, boxId);
            }

            if (chestDef.Id.Contains("ultra"))
            {
                ResetMegaBoxProgress();
            }

            AddMegaBoxProgress(100 * (chestDef.ProgressNormalized >= 0 ? chestDef.ProgressNormalized : 0));

            var boxDef = GetBoxByRarity(chestDef.Rarity);
            if (boxDef == null)
                return null;

            PirateBoxResult result = GetResultBoxFromBox(boxDef);
          
            result.BonusSoft = GetBonusSoft(chestDef);

            return result;
        }
        


        protected virtual PirateBoxResult GetResultBoxFromBox(PirateBoxDef boxDef)
        {
            ChestsRewardPoolListDef[] poolList = RewardPoolListDefs.Where(x => x.Tag == boxDef.PoolListId).ToArray();
            UniversalAssert.IsTrue(poolList.Length > 0, "RewardPoolList is empty" + boxDef.PoolListId);

            PirateBoxResult result = GetBoxResultInPoolList(poolList);
          

            var roll = _randomModule.RandomDecimal((decimal)0, (decimal)1f);
            if ((float)roll <= boxDef.BaitChance)
            {
                roll = _randomModule.RandomDecimal((decimal)0, (decimal)1f);
                var realRewardIds = result.PossibleRewards.Select(x => x.ItemId).ToList();
                var baitRewards = GetBaitRewardDefs((float)roll < boxDef.BaitShipChance ? "ship" : "item", boxDef.BaitRarity, realRewardIds);

                result.BaitRewards.AddRange(baitRewards);
            }

            return result;
        }


        public virtual PirateBoxResult OpenRouletteBoxLeague(ChestsRewardPoolListDef[] poolList)
        {
            SharedCommandCallstack.CheckCallstack();
            PirateBoxResult result = GetBoxResultInPoolList(poolList);

            var baitRewards = new List<ChestsRewardDef>();

           // for (int i = 0; i < 12; i++)
           // {
            //    baitRewards.Add(RewardDefs[_randomModule.RandomInt(RewardDefs.Count - 1)]);
           // }

            result.BaitRewards.AddRange(baitRewards);

            return result;
        }

        private PirateBoxResult GetBoxResultInPoolList(ChestsRewardPoolListDef[] poolList)
        {
            PirateBoxResult result = new PirateBoxResult();

            List<ChestsRewardDef> shuffledRewards = new List<ChestsRewardDef>();
            List<ChestsRewardDef> actualRewards = new List<ChestsRewardDef>();
            foreach (var rewardPoolElement in poolList)
            {
                var possibleRewardPools = GetPossibleRewardsPolls(rewardPoolElement);
                UniversalAssert.IsTrue(possibleRewardPools.Count > 0, "Reward pool is empty: " + rewardPoolElement.RewardPoolId);

                List<ChestsRewardDef> possibleRewardDefs = RewardDefs.Where(x => possibleRewardPools.Any(p => p.RewardId == x.Id)).Distinct().ToList();

                ChestsRewardDef rewardDef = null;
                while (rewardDef == null)
                {
                    ChestsRewardPoolDef randomRewardPool = _randomModule.ChooseByRandom(possibleRewardPools, x => x.RewardWeight); //get random rewards pool by wheight
                    ChestsRewardDef reward = possibleRewardDefs.FirstOrDefault(x => x.Id == randomRewardPool.RewardId);
                    List<ChestsRewardDef> rewards = GetPossibleRewards(reward).ToList(); //get possible rewards by tier and rarity
                    while (rewardDef == null && rewards.Count > 0)
                    {
                        ChestsRewardDef rewardToCheck = rewards[_randomModule.RandomInt(0, rewards.Count - 1)];
                        //TODO do we need to check -> Has player this reward already?
                        rewardToCheck = _chestModule.SetupReward(rewardToCheck);
                        rewardDef = _chestModule.GiveRewardToPlayer(rewardToCheck);
                        if (rewardDef == null)
                            rewards.Remove(rewardToCheck);
                    }
                    if (rewards.Count == 0)
                    {
                        possibleRewardDefs.Remove(reward);
                        possibleRewardPools.Remove(randomRewardPool);
                    }
                }

                List<ChestsRewardDef> allRewards = new List<ChestsRewardDef>();
                foreach (var rawRewardDef in possibleRewardDefs)
                {
                    var rewardDefs = GetPossibleRewards(rawRewardDef).ToList();
                    allRewards.AddRange(rewardDefs);
                }

                actualRewards.Add(rewardDef);

                shuffledRewards.AddRange(allRewards.OrderBy(item => _randomModule.RandomInt(0, 1000)));
            }

            foreach (var item in actualRewards)
            {
                var reward = shuffledRewards.FirstOrDefault(x => x.Id == item.Id);
                var rewardIndex = shuffledRewards.IndexOf(reward);
                shuffledRewards[rewardIndex] = item;
                result.RewardIndices.Add(rewardIndex);
            }

            result.PossibleRewards.AddRange(shuffledRewards);
            result.BonusSoft = 0;
            return result;
        }

        protected virtual List<ChestsRewardPoolDef> GetPossibleRewardsPolls(ChestsRewardPoolListDef rewardPoolElement)
        {
            return RewardPoolDefs.Where(
                x => x.Tag == rewardPoolElement.RewardPoolId && RewardDefs.Any(r => r.Id == x.RewardId)).ToList();
        }

        internal PirateBoxDef GetBoxByRarity(int boxRarity)
        {
            var boxes = PirateBoxDefs.Where(x => x.Rarity == boxRarity && !IsBoxLocked(x.LockerId)).ToArray();
            if (boxes.Length == 0)
            {
                return null;
            }
            var isSpecialBox = IsSpecialBoxAllowed(boxRarity);
            var boxesBySpecial = boxes.Where(x => x.IsSpecialBox == isSpecialBox).ToArray();
            if (boxesBySpecial.Length > 0)
                boxes = boxesBySpecial;

            var box = boxes.LastOrDefault();
            UniversalAssert.IsNotNull(box, "Empty box list?");

            return box;
        }

        #region VIRTUAL_METHODS

        protected virtual bool IsBoxLocked(string lockerId)
        {
            //TODO Check lockers using lockers module

            /*
            LockerDef lockerDef = _pseudoDefinitions.GetLockerDef(lockerId);
            int requiredLevel = ShopUtilities.GetRequiredLevelByLocker(lockerDef);
            var playerLevel = Utilities.GetPlayerLevel();

            return requiredLevel > playerLevel;
            */

            return false;
        }

        protected virtual bool IsSpecialBoxAllowed(int rarity)
        {
            //TODO Check whether player opened first box of choosen rarity

            /*
            var boxOpened = (SavedData.Instance.UserProfile.BoxesOpened >> rarity) & 1;
            if (boxOpened != 1)
                return true;

            int count = PlayerPrefsExtended.GetInt(PlayerPrefsExtended.SPECIAL_BOXES_COUNT_KEY);
            return count > 0;
            */

            return false;
        }

        protected virtual int GetBonusSoft(PirateBoxChestDef chestDef)
        {
            //TODO get bonus soft if needed
            return 0;
        }

        protected virtual List<ChestsRewardDef> GetBaitRewardDefs(string itemType, int rarity, List<string> idsToExclude)
        {
            //TODO Find good elements from All game items and show it nearby choosen reward element

            /***
            var baits = new List<RewardDef>();

            var blackMarket = _pseudoSharedLogic.PseudoContext.BlackMarketModule;
            var currentTier = blackMarket.GetCurrentTier();

            switch (itemType)
            {
                case "item":
                    {
                        var itemsFiltredByTier = GetItemsByTier(currentTier);
                        var itemsFiltredByRarity = itemsFiltredByTier.Where(x => x.rarity.ID == rarity);
                        var itemsForBait = itemsFiltredByRarity.Where(x => !idsToExclude.Contains(x.name)).ToList();

                        if (itemsForBait.Count > 0)
                        {
                            var item = itemsForBait.RandomElement();
                            var rewardDef = new RewardDef();
                            rewardDef.ItemType = itemType;
                            rewardDef.ItemId = item.name;
                            baits.Add(rewardDef);
                        }
                        break;
                    }
                case "ship":
                    {
                        var shipsForBait = new List<ShipDef>();
                        foreach (var ship in UOW_ShipConstructor.Instance.ShipReferences)
                        {
                            if (idsToExclude.Contains(ship.Name))
                                continue;

                            var shipDef = _pseudoSharedLogic.Defs.GetShipDef(ship.Name);

                            if (!shipDef.IsInShop)
                                continue;

                            int shipTier = ShipShopReference.CompareShipDisplayClass(shipDef.Type);
                            if (rarity != CUSTOM_RARITY && shipTier != currentTier && shipTier != currentTier + 1)
                                continue;

                            var propertyDef = _pseudoSharedLogic.Defs.GetPropertyDef(shipDef.PropertyId);
                            var isRealMoney = ShopUtilities.GetCostTypeByProperty(propertyDef) == ItemCostType.RealMoney;
                            var currencyType = ShopUtilities.GetCurrencyTypeByProperty(propertyDef);
                            bool allowed = false;

                            switch (rarity)
                            {
                                case CUSTOM_RARITY:
                                case (int)BlackMarketModule.ULTIMATE_RARITY:
                                    allowed = isRealMoney;
                                    break;
                                case (int)BlackMarketModule.ADVANCED_RARITY:
                                    allowed = !isRealMoney && (currencyType == CurrencyType.SOFT);
                                    break;
                                case (int)BlackMarketModule.UNIQUE_RARITY:
                                    allowed = !isRealMoney && (currencyType == CurrencyType.HARD);
                                    break;
                                default:
                                    break;
                            }

                            if (!allowed) continue;

                            shipsForBait.Add(shipDef);
                        }

                        if (rarity == CUSTOM_RARITY)
                        {
                            foreach (var item in shipsForBait)
                            {
                                var rewardDef = new RewardDef();
                                rewardDef.ItemType = itemType;
                                rewardDef.ItemId = item.Id;
                                baits.Add(rewardDef);
                            }
                        }
                        else
                        {
                            var rewardDef = new RewardDef();
                            rewardDef.ItemType = itemType;
                            rewardDef.ItemId = shipsForBait.RandomElement().Id;
                            baits.Add(rewardDef);
                        }
                        break;
                    }
                default:
                    break;
            }

            return baits;
            */

            var result = new List<ChestsRewardDef>();

            for (int i = 0; i < 12; i++)
            {
                result.Add(RewardDefs[_randomModule.RandomInt(RewardDefs.Count - 1)]);
            }

            return result;
        }

        protected virtual IEnumerable<ChestsRewardDef> GetPossibleRewards(ChestsRewardDef rewardDef)
        {
            //TODO Get list of possible chest rewards
            var result = new List<ChestsRewardDef>();

            /*
            if (rewardDef.UseRarityAndTier)
            {
                var itemsFiltredByTier = GetItemsByTier(rewardDef.Tier);
                var itemsFiltredByRarity = itemsFiltredByTier.Where(x => x.rarity.ID == rewardDef.Rarity).ToList();

                foreach (var item in itemsFiltredByRarity)
                {
                    var newReward = new ChestsRewardDef();
                    newReward.ItemId = item.name;
                    newReward.ItemType = "item";
                    result.Add(newReward);
                }
            }
            else
            {
                if ((rewardDef.ItemType == "soft_currency") || (rewardDef.ItemType == "hard_currency"))
                    rewardDef.ItemId = string.Format("{0}_{1}", rewardDef.Id, rewardDef.ItemType);

                result.Add(rewardDef);
            }
            */
            //if ((rewardDef.ItemType == "soft_currency") || (rewardDef.ItemType == "hard_currency"))
            //rewardDef.ItemId = string.Format("{0}_{1}", rewardDef.Id, rewardDef.ItemType);

            result.Add(rewardDef);

            return result;
        }
        #endregion
    }
}