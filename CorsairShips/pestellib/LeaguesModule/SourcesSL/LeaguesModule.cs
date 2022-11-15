using System;
using System.Collections.Generic;
using System.Linq;
using PestelLib.Serialization;
using PestelLib.ServerProtocol;
using PestelLib.SharedLogicBase;
using S;
using ServerShared;
using UnityDI;
using log4net;
using Newtonsoft.Json;

namespace PestelLib.SharedLogic.Modules
{
    [System.Reflection.Obfuscation(ApplyToMembers=false)]
    public class LeaguesModule : SharedLogicModule<LeaguesState>
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(LeaguesModule));
        [GooglePageRef("Leagues")] [Dependency] protected List<LeagueDef> _leagueDefs;
        [GooglePageRef("LeagueRewards")] [Dependency] protected List<LeagueRewardDef> _leagueRewardDefs;
        [GooglePageRef("Chests")] [Dependency] protected List<ChestDef> ChestDefs;
        [GooglePageRef("ChestRewards")] [Dependency] protected List<ChestsRewardDef> RewardDefs;
        [GooglePageRef("ChestRewardsPool")] [Dependency] protected List<ChestsRewardPoolDef> RewardPoolDefs;
        [GooglePageRef("ChestRewardsPoolList")] [Dependency] protected List<ChestsRewardPoolListDef> RewardPoolListDefs;

        [Dependency] protected ChestModule _chestModule;
        [Dependency] protected RouletteModule _rouletteModule;
        [Dependency] private ILeagueServerModuleApi _leagueServer;

        internal event Action<int> OnLeagueLevelChangedInternal = i => { };

        public int CurrentLeagueLevel
        {
            get { return State.CurrentLeague; }
        }

        public int MaxLeagueLevel
        {
            get { return State.MaxLeague; }
        }

        void ValidateDi()
        {
            if (_leagueServer == null)
                _leagueServer = Container.Resolve<ILeagueServerModuleApi>();
        }

        public bool IsClaimed(int seasonId)
        {
            if (State.ClaimedSeasons == null || State.ClaimedSeasons.Count == 0)
                return false;

            return State.ClaimedSeasons.Contains(seasonId);
        }

        internal void SaveClaimState(int seasonId)
        {
            if(IsClaimed(seasonId))
                return;

            SharedCommandCallstack.CheckCallstack();
            if(State.ClaimedSeasons == null)
                State.ClaimedSeasons = new List<int>();
            State.ClaimedSeasons.Add(seasonId);
        }

        internal ChestDef ClaimChest(SeasonEndInfo seasonEndInfo)
        {
            SaveClaimState(seasonEndInfo.Season);
            ValidateDi();

            ChestDef chestDef = GetRewardChest(seasonEndInfo);

            if (chestDef == null)
            {
                Log("Chest def is null for: " + seasonEndInfo.ToString());
                return null;
            }

            if (_leagueServer != null)
                seasonEndInfo = _leagueServer.PullSeasonEndInfo(SharedLogic.PlayerId, seasonEndInfo.Season);

            if (seasonEndInfo == null)
                return null;

            return chestDef;
        }

        [SharedCommand]
        internal PirateBoxResult ClaimRewards(SeasonEndInfo seasonEndInfo)
        {
            SaveClaimState(seasonEndInfo.Season);
            ValidateDi();
            var chestDef = GetRewardChest(seasonEndInfo);

            var pid = SharedLogic.PlayerId;
            if (_leagueServer != null)
            {
                seasonEndInfo = _leagueServer.PullSeasonEndInfo(pid, seasonEndInfo.Season);
                if (seasonEndInfo == null)
                    return null;
            }

            if (chestDef == null)
            {
                _log.Warn("No reward for season info: " + JsonConvert.SerializeObject(seasonEndInfo));
                return null;
            }

            //получаем эмблемы
            List<ChestsRewardDef> emblemsList = GetLeagueEmblemRewards(seasonEndInfo);
            foreach (ChestsRewardDef emblem in emblemsList)
            {
                var def = _chestModule.SetupReward(emblem);
                _chestModule.GiveRewardToPlayer(def);
            }

            //получаем софт
            var mod = RewardDefs.Find(x => x.Id == chestDef.PoolListId);
            UniversalAssert.IsNotNull(mod, "Can't find ChestRewardDef with PoolId: " + chestDef.PoolListId);
            int bonusSoft = (int)(mod.Amount);
            var defSoft = _chestModule.SetupReward(mod);
            _chestModule.GiveRewardToPlayer(defSoft);

            List<ChestsRewardPoolListDef> poolList = GetPoolListForRoulette(chestDef, emblemsList);
            // var rewards = _chestModule.GiveChestRewardsById(chestDef.Id);

            //получаем бокс для рулетки
            PirateBoxResult reward = _rouletteModule.OpenRouletteBoxLeague(poolList.ToArray());           
            reward.BonusSoft = bonusSoft;

            State.CurrentLeague = seasonEndInfo.LeagueLevel + seasonEndInfo.LeagueChange;
            State.MaxLeague = Math.Max(State.CurrentLeague, State.MaxLeague);

            OnLeagueLevelChangedInternal?.Invoke(State.CurrentLeague);

            return reward;
        }
      
        public ChestDef GetRewardChest(SeasonEndInfo seasonEndInfo)
        {
            var lDef = GetLeagueDef(seasonEndInfo.LeagueLevel);
            if (lDef == null)
                return null;

            var rDef = GetRewardDef(lDef, seasonEndInfo.DivisionPlace);
            if (rDef == null)
                return null;

            var chestDef = ChestDefs.FirstOrDefault(x => x.Id == rDef.ChestId);
            return chestDef;
        }

        //получаем гарантированные эмблемы после прохождения лиги
        public List<ChestsRewardDef> GetLeagueEmblemRewards(SeasonEndInfo seasonEndInfo)
        {
            var result = new List<ChestsRewardDef>();
            ChestDef chestDef = GetRewardChest(seasonEndInfo);
            var poolList = RewardPoolListDefs.Where(x => x.Tag == chestDef.PoolListId).ToArray();
            UniversalAssert.IsTrue(poolList.Length > 0, "RewardPoolList is empty" + chestDef.PoolListId);

            foreach (var rewardPoolElement in poolList)
            {
                var possibleReward = RewardPoolDefs.Where(x => x.Tag == rewardPoolElement.RewardPoolId).ToArray();
                UniversalAssert.IsTrue(possibleReward.Length > 0, "Reward pool is empty: " + rewardPoolElement.RewardPoolId);

                foreach (var reward in possibleReward)
                {
                    var rewardDef = RewardDefs.Find(x => x.Id == reward.RewardId && x.Rarity == 3 && x.ItemType == "emblem");
                    if (rewardDef != null)
                        result.Add(rewardDef);
                }
            }

            return result;
        }

        //получаем пул наград для рулетки без пула где есть гарантированные эмблемы
        private List<ChestsRewardPoolListDef> GetPoolListForRoulette(ChestDef chestDef, List<ChestsRewardDef> emblems)
        {
            List<ChestsRewardPoolListDef> poolList = RewardPoolListDefs.Where(x => x.Tag == chestDef.PoolListId).ToList();
            UniversalAssert.IsTrue(poolList.Count() > 0, "RewardPoolList is empty" + chestDef.PoolListId);

            var emblemIds = emblems.Select(x => x.Id);
            var poolDefsTags = RewardPoolDefs.Where(x => emblemIds.Contains(x.RewardId)).Select(x => x.Tag);
            return poolList.Where(x => !poolDefsTags.Contains(x.RewardPoolId)).ToList();

        }

        public void Score(long score)
        {
            SharedCommandCallstack.CheckCallstack();
            ValidateDi();
            if (_leagueServer == null)
                return;
            var pid = SharedLogic.PlayerId;
            _leagueServer.Score(pid, score);
        }

        [SharedCommand]
        internal void ScoreSL(long score)
        {
            Score(score);
        }

        public LeagueRewardDef GetRewardDef(SeasonEndInfo seasonEnd)
        {
            return GetRewardDef(GetLeagueDef(seasonEnd.LeagueLevel), seasonEnd.DivisionPlace);
        }

        internal LeagueRewardDef GetRewardDef(LeagueDef leagueDef, int place)
        {
            var rewards = _leagueRewardDefs.Where(_ => _.LeagueId == leagueDef.Id && _.Top >= place).ToArray();
            if (rewards.Length == 0)
                return null;
           
            return rewards.OrderBy(_ => _.Top).First();
        }

        public LeagueRewardDef[] GetRewardDefs(int leagueLvl, int lenghtDivision)
        {
            var leagueDef = GetLeagueDef(leagueLvl);
            var rewards = _leagueRewardDefs.Where(_ => _.LeagueId == leagueDef.Id).ToArray();
            if (rewards.Length == 0)
                return null;

            var newRewards = new List<LeagueRewardDef>();
            for (int i = 0; i < rewards.Length; i++)
            {
                newRewards.Add(rewards[i]);
                if (rewards[i].Top > lenghtDivision)
                    break;
            }
        
            return newRewards.OrderBy(_ => _.Top).ToArray();
        }

        internal LeagueDef GetLeagueDef(int leagueLvl)
        {
            var def = _leagueDefs.FirstOrDefault(_ => _.Level == leagueLvl);
            UniversalAssert.IsTrue(def != null, "Cant find league def with level " + leagueLvl);
            return def;
        }

        public string GetLeagueName(int leagueLevel)
        {
            return GetLeagueDef(leagueLevel).Name;
        }

        public float GetUpCoeff (int leagueLvl)
        {
            return GetLeagueDef(leagueLvl).UpCoeff;
        }

        public float GetDownCoeff(int leagueLvl)
        {
            return GetLeagueDef(leagueLvl).DownCoeff;
        }
    }
}
