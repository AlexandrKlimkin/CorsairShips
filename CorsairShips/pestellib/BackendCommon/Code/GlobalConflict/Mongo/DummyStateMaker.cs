using System;
using System.IO;
using Newtonsoft.Json;
using ServerShared.GlobalConflict;

namespace BackendCommon.Code.GlobalConflict.Mongo
{
    static class DummyStateMaker
    {
        public static void MakeDefault()
        {
            var gc = new GlobalConflictState()
            {
                Id = "test_global_conflict",
                AssignType = TeamAssignType.BasicAuto,
                BattleCost = 100,
                CaptureTime = default(int),
                StartTime = default(DateTime),
                EndTime = default(DateTime),
                Teams = new[] { "team_one", "team_two" },
                Stages = new[]
                {
                    new StageDef()
                    {
                        Id = StageType.Donation,
                        Period = TimeSpan.FromMinutes(1)
                    },
                    new StageDef()
                    {
                        Id = StageType.Cooldown,
                        Period = TimeSpan.FromMinutes(1)
                    },
                    new StageDef()
                    {
                        Id = StageType.Battle,
                        Period = TimeSpan.FromMinutes(1)
                    },
                    new StageDef()
                    {
                        Id = StageType.Final,
                        Period = TimeSpan.FromMinutes(1)
                    },
                    new StageDef()
                    {
                        Id = StageType.Cooldown,
                        Period = TimeSpan.FromMinutes(1)
                    },
                },
                DonationBonusLevels = new[]
                {
                    new DonationBonusLevels()
                    {
                        Level = 1,
                        Threshold = 100
                    },
                    new DonationBonusLevels()
                    {
                        Level = 2,
                        Threshold = 300
                    },
                    new DonationBonusLevels()
                    {
                        Level = 3,
                        Threshold = 1000
                    },
                    new DonationBonusLevels()
                    {
                        Level = 1,
                        Team = true,
                        Threshold = 10000
                    }
                },
                DonationBonuses = new[]
                {
                    new DonationBonus()
                    {
                        Level = 1,
                        ClientType = "hp",
                        Team = false,
                        Value = .05m
                    },
                    new DonationBonus()
                    {
                        Level = 1,
                        ServerType = DonationBonusType.TeamPointsBuff,
                        Team = false,
                        Value = .2m
                    },
                    new DonationBonus()
                    {
                        Level = 2,
                        ServerType = DonationBonusType.TeamPointsBuff,
                        Team = false,
                        Value = .05m
                    },
                    new DonationBonus()
                    {
                        Level = 2,
                        ClientType = "start_nodes",
                        Team = false,
                        Value = 1m
                    },
                    new DonationBonus()
                    {
                        Level = 2,
                        ClientType = "damage",
                        Team = false,
                        Value = .1m
                    },
                    new DonationBonus()
                    {
                        Level = 3,
                        ClientType = "hp",
                        Team = false,
                        Value = .1m
                    },
                    new DonationBonus()
                    {
                        Level = 3,
                        ClientType = "hp",
                        Team = false,
                        Value = .1m
                    },
                    new DonationBonus()
                    {
                        Level = 1,
                        ClientType = "start_nodes",
                        Team = true,
                        Value = 1m
                    }
                },
                Map = new MapState()
                {
                    TextureId = "global_conflict_dummy",
                    Nodes = new[]
                    {
                        new NodeState()
                        {
                            Id = 1,
                            LinkedNodes = new [] {3},
                            BaseForTeam = "team_one",
                            Owner = "team_one",
                            NodeStatus = NodeStatus.Base,
                            PositionX = 0.1f,
                            PositionY = 0.1f
                        },
                        new NodeState()
                        {
                            Id = 2,
                            LinkedNodes = new [] {3},
                            BaseForTeam = "team_two",
                            Owner = "team_two",
                            NodeStatus = NodeStatus.Base,
                            PositionX = 0.9f,
                            PositionY = 0.9f
                        },
                        new NodeState()
                        {
                            Id = 3,
                            LinkedNodes = new [] {1,2},
                            BaseForTeam = default(string),
                            Owner = default(string),
                            WinPoints = 100,
                            CaptureBonus = 1000,
                            CaptureLimit = 5000,
                            BattleBonus = default(float),
                            ContentBonus = default(string),
                            CaptureThreshold = 500,
                            GameMap = default(string),
                            GameMode = default(string),
                            LosePoints = 50,
                            NeutralThreshold = 300,
                            NodeStatus = NodeStatus.Neutral,
                            ResultPoints = 5,
                            RewardBonus = default(float),
                            SupportBonus = 0.1f,
                            PositionX = 0.2f,
                            PositionY = 0.2f
                        }
                    }
                }
            };

            using (var f = new StreamWriter(File.OpenWrite("d:\\test_global_conflict.json")))
            {
                var gcJson = JsonConvert.SerializeObject(gc);
                f.Write(gcJson);
            }
        }
    }
}