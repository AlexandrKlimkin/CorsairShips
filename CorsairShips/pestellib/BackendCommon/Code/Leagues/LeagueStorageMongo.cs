using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;
using S;
using ServerLib;
using UnityDI;
using ReturnDocument = MongoDB.Driver.ReturnDocument;
using PestelLib.ServerCommon.Db;

namespace BackendCommon.Code.Leagues
{
#pragma warning disable 0649
    public class LeagueBanMongo
    {
        public Guid PlayerId;
    }

    public class SeasonInfoMongo
    {
        public ObjectId Id;
        public Guid PlayerId;
        public DateTime ClaimDate;
        public bool IsClaimed;
        public SeasonEndInfo Info;
        public Guid Division;
    }

    class ChangedDivision
    {
        public Guid DivisionId;
        public int Queries;
    }

    class PendingAddScore
    {
        public Guid PlayerId;
        public long ScoreDelta;
    }
#pragma warning restore 0649

    public class LeaguePlayerInfoMongo : LeaguePlayerInfo
    {
        public DateTime LastLogin;
        public DateTime LastScore;

        public LeaguePlayerInfoMongo()
        {
        }

        public LeaguePlayerInfoMongo(LeaguePlayerInfo pi, DateTime lastLogin = default(DateTime))
        {
            
            PlayerId = pi.PlayerId;
            FacebookId = pi.FacebookId;
            Score = pi.Score;
            Season = pi.Season;
            LeagueLevel = pi.LeagueLevel;
            DivisionId = pi.DivisionId;
            UnclaimedRewards = pi.UnclaimedRewards;
            Name = pi.Name;
            IsBot = pi.IsBot;
            BestScore = pi.BestScore;
            LastLogin = lastLogin;
        }
    }

    public class LeagueStorageMongo : LeagueStorageBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LeagueStorageMongo));
        private readonly IMongoCollection<LeaguePlayerInfoMongo> _playersCollection;
        private readonly IMongoCollection<DivisionInfo> _divisionsCollection;
        private readonly IMongoCollection<LeagueBanMongo> _banCollection;
        private readonly IMongoCollection<SeasonInfoMongo> _seasonEndInfoCollection;
        private readonly IMongoCollection<PendingAddScore> _pendingAddScore;
        private readonly IMongoCollection<ChangedDivision> _changedDivisionsCollection;
        private readonly ThreadLocal<Random> _safeRandom = new ThreadLocal<Random>(() => new Random());
        private LeagueDefHelper _defs;
        private Task _migrateSeasonInfosTask;

        static LeagueStorageMongo()
        {
            BsonClassMap.RegisterClassMap<PendingAddScore>(cm =>
            {
                cm.AutoMap();
                cm.MapIdField(_ => _.PlayerId);
                cm.SetIgnoreExtraElements(true);
            });
            BsonClassMap.RegisterClassMap<SeasonInfoMongo>(cm =>
            {
                cm.AutoMap();
                cm.MapIdField(_ => _.Id);
                cm.SetIgnoreExtraElements(true);
            });
            BsonClassMap.RegisterClassMap<LeagueBanMongo>(cm =>
            {
                cm.AutoMap();
                cm.MapIdField(_ => _.PlayerId);
                cm.SetIgnoreExtraElements(true);
            });
            BsonClassMap.RegisterClassMap<LeaguePlayerInfo>(cm =>
            {
                cm.AutoMap();
                cm.MapIdField(_ => _.PlayerId);
                cm.SetIgnoreExtraElements(true);
            });
            BsonClassMap.RegisterClassMap<LeaguePlayerInfoMongo>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
            BsonClassMap.RegisterClassMap<DivisionInfo>(cm =>
            {
                cm.AutoMap();
                cm.MapIdField(_ => _.Id);
                cm.SetIgnoreExtraElements(true);
            });
            BsonClassMap.RegisterClassMap<ChangedDivision>(cm =>
            {
                cm.AutoMap();
                cm.MapIdField(_ => _.DivisionId);
                cm.SetIgnoreExtraElements(true);
            });
        }

        public LeagueStorageMongo(MongoUrl connectionString, string dataCollectionPrefix, LeagueLeaderboardConfig config, LeagueStateCache state, LeagueDefHelper leagueDefHelper)
            :base(config, state)
        {
            _defs = leagueDefHelper;
            var c = connectionString.GetServer();
            var db = c.GetDatabase(connectionString.DatabaseName);
            _banCollection = db.GetCollection<LeagueBanMongo>(dataCollectionPrefix + "Bans");
            _playersCollection = db.GetCollection<LeaguePlayerInfoMongo>(dataCollectionPrefix + "Players");
            _divisionsCollection = db.GetCollection<DivisionInfo>(dataCollectionPrefix + "Divisions");
            _seasonEndInfoCollection = db.GetCollection<SeasonInfoMongo>(dataCollectionPrefix + "SeasonEndInfos");
            _pendingAddScore = db.GetCollection<PendingAddScore>(dataCollectionPrefix + "PendingScores");
            _changedDivisionsCollection = db.GetCollection<ChangedDivision>(dataCollectionPrefix + "ChangedDivisions");
            _playersCollection.Indexes.CreateOne(Builders<LeaguePlayerInfoMongo>.IndexKeys.Descending(_ => _.LastLogin));
            _playersCollection.Indexes.CreateOne(Builders<LeaguePlayerInfoMongo>.IndexKeys.Combine(
                Builders<LeaguePlayerInfoMongo>.IndexKeys.Descending(_ => _.Season),
                Builders<LeaguePlayerInfoMongo>.IndexKeys.Descending(_ => _.Score)));
            _playersCollection.Indexes.CreateOne(Builders<LeaguePlayerInfoMongo>.IndexKeys.Combine(
                Builders<LeaguePlayerInfoMongo>.IndexKeys.Descending(_ => _.Season),
                Builders<LeaguePlayerInfoMongo>.IndexKeys.Descending(_ => _.LeagueLevel),
                Builders<LeaguePlayerInfoMongo>.IndexKeys.Descending(_ => _.Score)));
            _playersCollection.Indexes.CreateOne(Builders<LeaguePlayerInfoMongo>.IndexKeys.Combine(
                Builders<LeaguePlayerInfoMongo>.IndexKeys.Descending(_ => _.Season),
                Builders<LeaguePlayerInfoMongo>.IndexKeys.Descending(_ => _.LeagueLevel)));
            _playersCollection.Indexes.CreateOne(Builders<LeaguePlayerInfoMongo>.IndexKeys.Combine(
                Builders<LeaguePlayerInfoMongo>.IndexKeys.Descending(_ => _.LeagueLevel),
                Builders<LeaguePlayerInfoMongo>.IndexKeys.Descending(_ => _.LastScore)));
            _playersCollection.Indexes.CreateOne(Builders<LeaguePlayerInfoMongo>.IndexKeys.Combine(
                Builders<LeaguePlayerInfoMongo>.IndexKeys.Ascending(_ => _.DivisionId),
                Builders<LeaguePlayerInfoMongo>.IndexKeys.Descending(_ => _.Score)
            ));

            _playersCollection.Indexes.CreateOne(Builders<LeaguePlayerInfoMongo>.IndexKeys.Ascending(_ => _.DivisionId));
            _playersCollection.Indexes.CreateOne(Builders<LeaguePlayerInfoMongo>.IndexKeys.Ascending(_ => _.IsBot));

            _divisionsCollection.Indexes.CreateOne(Builders<DivisionInfo>.IndexKeys.Combine(
                Builders<DivisionInfo>.IndexKeys.Ascending(_ => _.LeagueLevel),
                Builders<DivisionInfo>.IndexKeys.Ascending(_ => _.Season)));

            _seasonEndInfoCollection.Indexes.CreateOne(Builders<SeasonInfoMongo>.IndexKeys.Combine(
                Builders<SeasonInfoMongo>.IndexKeys.Descending(_ => _.Info.Season),
                Builders<SeasonInfoMongo>.IndexKeys.Ascending(_ => _.Info.GlobalPlace)));

            _seasonEndInfoCollection.Indexes.CreateOne(Builders<SeasonInfoMongo>.IndexKeys.Combine(
                Builders<SeasonInfoMongo>.IndexKeys.Ascending(_ => _.PlayerId),
                Builders<SeasonInfoMongo>.IndexKeys.Ascending(_ => _.IsClaimed)
            ));

            _seasonEndInfoCollection.Indexes.CreateOne(Builders<SeasonInfoMongo>.IndexKeys.Combine(
                Builders<SeasonInfoMongo>.IndexKeys.Ascending(_ => _.PlayerId),
                Builders<SeasonInfoMongo>.IndexKeys.Ascending(_ => _.Info.Season)
            ));

            _migrateSeasonInfosTask = Task.Run(() => MigrateSeasonInfos());
        }

        public override long IncrementBotScores(int leagueLvl, long scoreMin, long scoreMax, int limit, bool force = false)
        {
            var dtOffset = force ? TimeSpan.Zero : _config.BotsUpdateDelay;
            var filter = Builders<LeaguePlayerInfoMongo>.Filter.And(
                Builders<LeaguePlayerInfoMongo>.Filter.Eq(_ => _.LeagueLevel, leagueLvl),
                Builders<LeaguePlayerInfoMongo>.Filter.Eq(_ => _.IsBot, true),
                Builders<LeaguePlayerInfoMongo>.Filter.Lt(_ => _.LastScore, DateTime.UtcNow - dtOffset)
            );
            var opts = new FindOptions<LeaguePlayerInfoMongo>()
            {
                Limit = limit
            };
            var cursor = _playersCollection.FindSync(filter, opts);
            long count = 0;
            while (cursor.MoveNext())
            {
                if(cursor.Current == null)
                    break;
                foreach (var bot in cursor.Current)
                {
                    var score = scoreMin + _safeRandom.Value.Next((int)(scoreMax - scoreMin));
                    ScheduleAddScore(bot.PlayerId, score);
                }

                count += Enumerable.Count<LeaguePlayerInfoMongo>(cursor.Current);
            }
            return count;
        }

        public override SeasonEndInfo PopSeasonInfo(Guid playerId, int season)
        {
            var filter = Builders<SeasonInfoMongo>.Filter.And(
                Builders<SeasonInfoMongo>.Filter.Eq(_ => _.PlayerId, playerId),
                Builders<SeasonInfoMongo>.Filter.Eq(_ => _.Info.Season, season),
                Builders<SeasonInfoMongo>.Filter.Or(
                    Builders<SeasonInfoMongo>.Filter.Eq(_ => _.IsClaimed, false),
                    Builders<SeasonInfoMongo>.Filter.Exists(_ => _.IsClaimed, false)
                    ));
            var update = Builders<SeasonInfoMongo>.Update.Combine(
                Builders<SeasonInfoMongo>.Update.Set(_ => _.IsClaimed, true),
                Builders<SeasonInfoMongo>.Update.Set(_ => _.ClaimDate, DateTime.UtcNow));
            var r = _seasonEndInfoCollection.FindOneAndUpdate(filter, update);
            return r?.Info;
        }

        private UpdateOneModel<LeaguePlayerInfoMongo> SeasonEndBulk(Guid playerId, SeasonEndInfo info)
        {
            Log.Debug($"SeasonEndBulk. playerId={playerId}.");
            var season = _state.Season;
            var filter = Builders<LeaguePlayerInfoMongo>.Filter.And(
                Builders<LeaguePlayerInfoMongo>.Filter.Eq(_ => _.Season, season),
                Builders<LeaguePlayerInfoMongo>.Filter.Eq(_ => _.PlayerId, playerId));
            var update = Builders<LeaguePlayerInfoMongo>.Update.Combine(
                Builders<LeaguePlayerInfoMongo>.Update.Set(_ => _.Season, season + 1),
                Builders<LeaguePlayerInfoMongo>.Update.Set(_ => _.DivisionId, Guid.Empty),
                Builders<LeaguePlayerInfoMongo>.Update.Set(_ => _.Score, 0),
                Builders<LeaguePlayerInfoMongo>.Update.Inc(_ => _.LeagueLevel, info.LeagueChange));
            return new UpdateOneModel<LeaguePlayerInfoMongo>(filter, update);
        }

        public override void SeasonEnd(Guid playerId, SeasonEndInfo info)
        {
            _playersCollection.BulkWrite(new[] {SeasonEndBulk(playerId, info)});
        }

        public override LeaguePlayerInfo GetPlayer(Guid playerId)
        {
            var filter = Builders<LeaguePlayerInfoMongo>.Filter.And(
                Builders<LeaguePlayerInfoMongo>.Filter.Eq(nameof(LeaguePlayerInfoMongo.PlayerId), playerId));

            var pi = _playersCollection.FindSync(filter).FirstOrDefault();
            return pi;
        }

        private SeasonInfoMongo GetMostRecentSeasonInfo(Guid playerId)
        {
            var filterSi = Builders<SeasonInfoMongo>.Filter.Eq(_ => _.PlayerId, playerId);
            var optsSi = new FindOptions<SeasonInfoMongo>();
            optsSi.Sort = Builders<SeasonInfoMongo>.Sort.Descending(_ => _.Info.Season);
            optsSi.Limit = 1;
            var recentSeason = _seasonEndInfoCollection.FindSync(filterSi, optsSi);
            if (!recentSeason.MoveNext())
                return null;
            return recentSeason.Current?.FirstOrDefault();
        }

        private LeaguePlayerInfoMongo SetPlayerSeason(LeaguePlayerInfoMongo pi, long setScore = 0, Guid division = default(Guid))
        {
            if (pi.Season >= _state.Season)
                return pi;
            Log.Debug($"SetPlayerSeason({JsonConvert.SerializeObject(pi)}, {setScore}, {division}). pi.Season={pi.Season}, _state.Season={_state.Season}.");
            return SetPlayerSeason((Guid) pi.PlayerId, setScore, division) ?? pi;
        }

        private LeaguePlayerInfoMongo SetPlayerSeason(Guid playerId, long setScore = 0, Guid division = default(Guid))
        {
            if (_state.SeasonEnds <= DateTime.UtcNow)
                return null;

            var season = _state.Season;
            var updates = new List<UpdateDefinition<LeaguePlayerInfoMongo>>();
            var seasonInfo = GetMostRecentSeasonInfo(playerId);
            if (seasonInfo != null)
            {
                var league = seasonInfo.Info.LeagueLevel + seasonInfo.Info.LeagueChange;
                updates.Add(
                    Builders<LeaguePlayerInfoMongo>.Update.Set(_ => _.LeagueLevel, league)
                    );
            }

            var filter = Builders<LeaguePlayerInfoMongo>.Filter.And(
                Builders<LeaguePlayerInfoMongo>.Filter.Eq(_ => _.PlayerId, playerId),
                Builders<LeaguePlayerInfoMongo>.Filter.Lt(_ => _.Season, season)
            );

            updates.AddRange(new []
            {
                Builders<LeaguePlayerInfoMongo>.Update.Set(_ => _.Season, season),
                Builders<LeaguePlayerInfoMongo>.Update.Set(_ => _.Score, setScore),
                Builders<LeaguePlayerInfoMongo>.Update.Set(_ => _.DivisionId, division)
            });

            var update = Builders<LeaguePlayerInfoMongo>.Update.Combine(updates);
            var opts = new FindOneAndUpdateOptions<LeaguePlayerInfoMongo>();
            opts.ReturnDocument = ReturnDocument.After;
            var r = _playersCollection.FindOneAndUpdate(filter, update, opts);
            return r;
        }

        public override void SetPlayerDivision(Guid playerId, Guid division)
        {
            if(division == Guid.Empty)
                Log.Warn($"PlayerId={playerId}, Division={division}. stack={Environment.StackTrace}");
            var pfilter = Builders<LeaguePlayerInfoMongo>.Filter.Eq(_ => _.PlayerId, playerId);
            var pupdate = Builders<LeaguePlayerInfoMongo>.Update.Set(_ => _.DivisionId, division);
            var opts = new FindOneAndUpdateOptions<LeaguePlayerInfoMongo>();
            opts.ReturnDocument = ReturnDocument.Before;
            var pi = _playersCollection.FindOneAndUpdate(pfilter, pupdate);
            if (pi != null)
            {
                if (pi.DivisionId != Guid.Empty)
                {
                    var dfilter = Builders<DivisionInfo>.Filter.Eq(_ => _.Id, division);
                    var dupdate = pi.IsBot ? Builders<DivisionInfo>.Update.Inc(_ => _.BotsCount, -1) : Builders<DivisionInfo>.Update.Inc(_ => _.Population, -1);
                    var div = _divisionsCollection.FindOneAndUpdate(dfilter, dupdate);
                }

                {
                    pi = SetPlayerSeason(playerId, division: division) ?? pi;
                    var dfilter = Builders<DivisionInfo>.Filter.Eq(_ => _.Id, division);
                    var dupdate = pi.IsBot
                        ? Builders<DivisionInfo>.Update.Inc(_ => _.BotsCount, 1)
                        : Builders<DivisionInfo>.Update.Inc(_ => _.Population, 1);
                    var div = _divisionsCollection.FindOneAndUpdate(dfilter, dupdate);
                    if (!pi.IsBot && div.BotsCount > 0)
                    {
                        pfilter = Builders<LeaguePlayerInfoMongo>.Filter.And(
                            Builders<LeaguePlayerInfoMongo>.Filter.Eq(_ => _.IsBot, true),
                            Builders<LeaguePlayerInfoMongo>.Filter.Eq(_ => _.DivisionId, division));
                        var r = _playersCollection.DeleteOne(pfilter);
                        if (r.DeletedCount > 0)
                        {
                            dupdate = Builders<DivisionInfo>.Update.Inc(_ => _.BotsCount, -1);
                            _divisionsCollection.FindOneAndUpdate(dfilter, dupdate);
                        }
                    }
                }
            }
        }

        public override LeaguePlayerInfo CreatePlayer(Guid playerId, string name, string facebookId, Guid division, bool isBot, int leagueLvl)
        {
            var pi = new LeaguePlayerInfoMongo()
            {
                PlayerId = playerId,
                Name = name,
                Season = _state.Season,
                DivisionId = division,
                LastLogin = DateTime.UtcNow,
                LastScore = isBot ? DateTime.UtcNow - _config.BotsUpdateDelay.Add(TimeSpan.FromHours(_safeRandom.Value.NextDouble())) : default(DateTime),
                UnclaimedRewards = new List<SeasonEndInfo>(),
                FacebookId = facebookId,
                IsBot = isBot,
                LeagueLevel = leagueLvl
            };
            _playersCollection.InsertOne(pi);
            if (division != Guid.Empty)
            {
                var filter = Builders<DivisionInfo>.Filter.Eq(_ => _.Id, division);
                var update = isBot
                    ? Builders<DivisionInfo>.Update.Inc(_ => _.BotsCount, 1)
                    : Builders<DivisionInfo>.Update.Inc(_ => _.Population, 1);
                _divisionsCollection.FindOneAndUpdate(filter, update);
                if (!isBot)
                {
                    var pfilter = Builders<LeaguePlayerInfoMongo>.Filter.And(
                        Builders<LeaguePlayerInfoMongo>.Filter.Eq(_ => _.IsBot, true),
                        Builders<LeaguePlayerInfoMongo>.Filter.Eq(_ => _.DivisionId, division));
                    var r = _playersCollection.DeleteOne(pfilter);
                    if (r.DeletedCount > 0)
                    {
                        var dupdate = Builders<DivisionInfo>.Update.Inc(_ => _.BotsCount, -1);
                        _divisionsCollection.FindOneAndUpdate(filter, dupdate);
                    }
                }
            }

            return pi;
        }

        private int GetGlobalRank(int season, long score)
        {
            var filter = Builders<LeaguePlayerInfoMongo>.Filter.And(
                Builders<LeaguePlayerInfoMongo>.Filter.Gt(_ => _.Score, score),
                Builders<LeaguePlayerInfoMongo>.Filter.Eq(_ => _.Season, season));

            var rank = _playersCollection.Count(filter) + 1;
            return (int)rank;
        }

        public override int GetGlobalRank(int season, Guid playerId)
        {
            var pi = GetPlayer(playerId);
            return GetGlobalRank(season, pi.Score);
        }

        private int GetLeagueRank(int season, int leagueLvl, long score)
        {
            var filter = Builders<LeaguePlayerInfoMongo>.Filter.And(
                Builders<LeaguePlayerInfoMongo>.Filter.Gt(_ => _.Score, score),
                Builders<LeaguePlayerInfoMongo>.Filter.Eq(_ => _.LeagueLevel, leagueLvl),
                Builders<LeaguePlayerInfoMongo>.Filter.Eq(_ => _.Season, season));

            var rank = _playersCollection.Count(filter) + 1;
            return (int)rank;
        }

        public override int GetLeagueRank(int season, Guid playerId, int leagueLvl)
        {
            var pi = GetPlayer(playerId);
            return GetLeagueRank(season, leagueLvl, pi.Score);
        }

        public override IEnumerable<LeaguePlayerInfo> GetGlobalTop(int season, int amount)
        {
            var filter = Builders<LeaguePlayerInfoMongo>.Filter.Eq(_ => _.Season, season);
            var sort = Builders<LeaguePlayerInfoMongo>.Sort.Descending(_ => _.Score);

            var opts = new FindOptions<LeaguePlayerInfoMongo>();
            opts.Limit = amount;
            opts.Sort = sort;
            var r = _playersCollection.FindSync(filter, opts);
            while (r.MoveNext())
            {
                if(r.Current == null)
                    yield break;
                foreach (var LeaguePlayerInfoMongo in r.Current)
                {
                    yield return LeaguePlayerInfoMongo;
                }
            }
        }

        public override IEnumerable<LeaguePlayerInfo> GetLeagueTop(int season, int leagueLvl, int amount)
        {
            var filter = Builders<LeaguePlayerInfoMongo>.Filter.And(
                Builders<LeaguePlayerInfoMongo>.Filter.Eq(_ => _.LeagueLevel, leagueLvl),
                Builders<LeaguePlayerInfoMongo>.Filter.Eq(_ => _.Season, season));
            var sort = Builders<LeaguePlayerInfoMongo>.Sort.Descending(_ => _.Score);

            var opts = new FindOptions<LeaguePlayerInfoMongo>()
            {
                Sort = sort,
                Limit = amount
            };
            var r = _playersCollection.FindSync(filter, opts);
            while (r.MoveNext())
            {
                if(r.Current == null)
                    yield break;
                foreach (var LeaguePlayerInfoMongo in r.Current)
                {
                    yield return LeaguePlayerInfoMongo;
                }
            }
        }

        public override IEnumerable<LeaguePlayerInfo> GetDivision(Guid divisionId)
        {
            var filter = Builders<LeaguePlayerInfoMongo>.Filter.And(
                Builders<LeaguePlayerInfoMongo>.Filter.Eq(_ => _.DivisionId, divisionId));
            var sort = Builders<LeaguePlayerInfoMongo>.Sort.Descending(_ => _.Score);

            var r = _playersCollection.FindSync(filter, new FindOptions<LeaguePlayerInfoMongo>() { Sort = sort });
            while (r.MoveNext())
            {
                if (r.Current == null)
                    yield break;
                foreach (var LeaguePlayerInfoMongo in r.Current)
                {
                    yield return LeaguePlayerInfoMongo;
                }
            }
        }

        public override DivisionInfo CreateDivision(int leagueLvl)
        {
            var id = Guid.NewGuid();
            var div = new DivisionInfo
            {
                Id = id,
                Population = 0,
                LeagueLevel = leagueLvl,
                Season = _state.Season
            };
            _divisionsCollection.InsertOne(div);
            return div;
        }

        public override IEnumerable<DivisionInfo> GetDivisions(int season, int leagueLevel)
        {
            var filter = Builders<DivisionInfo>.Filter.And(
                Builders<DivisionInfo>.Filter.Eq(_ => _.LeagueLevel, leagueLevel),
                Builders<DivisionInfo>.Filter.Eq(_ => _.Season, season));
            var r = _divisionsCollection.FindSync(filter);
            while (r.MoveNext())
            {
                if (r.Current == null)
                    yield break;
                foreach (var divisionInfo in r.Current)
                {
                    yield return divisionInfo;
                }
            }
        }

        public override IEnumerable<DivisionInfo> GetDivisionsWithMaxPopulation(int season, int leagueLevel, int maxPopulation)
        {
            var filter = Builders<DivisionInfo>.Filter.And(
                Builders<DivisionInfo>.Filter.Eq(_ => _.LeagueLevel, leagueLevel),
                Builders<DivisionInfo>.Filter.Eq(_ => _.Season, season),
                Builders<DivisionInfo>.Filter.Lt(_ => _.Population, maxPopulation));
            var r = _divisionsCollection.FindSync(filter);
            while (r.MoveNext())
            {
                if (r.Current == null)
                    yield break;
                foreach (var divisionInfo in r.Current)
                {
                    yield return divisionInfo;
                }
            }
        }

        class DummyDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }

        private static Lazy<TaskCompletionSource<IDisposable>> _dummyTcs = new Lazy<TaskCompletionSource<IDisposable>>(
            () =>
            {
                var r = new TaskCompletionSource<IDisposable>();
                r.SetResult(new DummyDisposable());
                return r;
            }, true);
        public override Task<IDisposable> Lock()
        {
            return _dummyTcs.Value.Task;
        }

        // adhoc
        private void CheckSetDivision(LeaguePlayerInfo pi)
        {
            if(pi == null || pi.DivisionId != Guid.Empty)
                return;

            var closureHours = (int)(_defs.CycleTime * _defs.PreClosureTimeFactor);
            var hoursToEnd = (_state.SeasonEnds - DateTime.UtcNow).TotalHours;
            var isClosure = hoursToEnd <= closureHours;
            var threshold = _defs.DivisionSize;
            if (!isClosure)
                threshold -= (int) (_defs.DivisionSize * _defs.DivisionReserveSlots);
            var filter = Builders<DivisionInfo>.Filter.And(
                Builders<DivisionInfo>.Filter.Eq(_ => _.LeagueLevel, pi.LeagueLevel),
                Builders<DivisionInfo>.Filter.Eq(_ => _.Season, _state.Season),
                Builders<DivisionInfo>.Filter.Lt(_ => _.Population, threshold));
            var opts = new FindOptions<DivisionInfo>();
            opts.Limit = 1;
            var r = _divisionsCollection.FindSync(filter, opts);
            var divisionInfo = r.SingleOrDefault();
            if (divisionInfo == null)
                divisionInfo = CreateDivision(pi.LeagueLevel);
            pi.DivisionId = divisionInfo.Id;
            SetPlayerDivision(pi.PlayerId, divisionInfo.Id);
        }

        private void AddScore(Guid playerId, long scoreDelta)
        {
            if(IsBanned(playerId)) return;
            var filter = Builders<LeaguePlayerInfoMongo>.Filter.Eq(_ => _.PlayerId, playerId);
            var update = Builders<LeaguePlayerInfoMongo>.Update.Combine(
                Builders<LeaguePlayerInfoMongo>.Update.Inc(_ => _.Score, scoreDelta),
                Builders<LeaguePlayerInfoMongo>.Update.Set(_ => _.LastScore, DateTime.UtcNow));
            var opts = new FindOneAndUpdateOptions<LeaguePlayerInfoMongo>()
            {
                ReturnDocument = ReturnDocument.After
            };
            var pi = _playersCollection.FindOneAndUpdate(filter, update, opts);
            // season changed between player login and first score call
            if (pi?.Season < _state.Season)
                pi = SetPlayerSeason(pi.PlayerId, scoreDelta);
            if (pi?.Score > pi?.BestScore)
            {
                update = Builders<LeaguePlayerInfoMongo>.Update.Set(_ => _.BestScore, pi.Score);
                pi = _playersCollection.FindOneAndUpdate(filter, update);
            }

            if (pi != null) // bot can be removed before add score opt will happend
            {
                CheckSetDivision(pi);
                ScheduleDivisionUpdate(pi.DivisionId);
            }
        }

        private List<SeasonEndInfo> GetUnclaimedRewards(Guid playerId)
        {
            var filter = Builders<SeasonInfoMongo>.Filter.And(
                Builders<SeasonInfoMongo>.Filter.Eq(_ => _.PlayerId, playerId),
                Builders<SeasonInfoMongo>.Filter.Lt(_ => _.Info.Season, _state.Season),
                Builders<SeasonInfoMongo>.Filter.Or(
                    Builders<SeasonInfoMongo>.Filter.Eq(_ => _.IsClaimed, false),
                    Builders<SeasonInfoMongo>.Filter.Exists(_ => _.IsClaimed, false))
            );
            var cursor = _seasonEndInfoCollection.FindSync(filter);
            var result = new List<SeasonEndInfo>();
            while (cursor.MoveNext())
            {
                if(cursor.Current == null)
                    continue;
                result.AddRange(cursor.Current.Select(_ => _.Info));
            }

            return result;
        }

        public override LeaguePlayerInfo PlayerLogin(Guid playerId, string name, string facebookId = null)
        {
            var filter = Builders<LeaguePlayerInfoMongo>.Filter.Eq(_ => _.PlayerId, playerId);
            var update = Builders<LeaguePlayerInfoMongo>.Update.Combine(
                Builders<LeaguePlayerInfoMongo>.Update.Set(_ => _.LastLogin, DateTime.UtcNow),
                Builders<LeaguePlayerInfoMongo>.Update.Set(_ => _.Name, name),
                Builders<LeaguePlayerInfoMongo>.Update.Set(_ => _.FacebookId, facebookId));

			var opts = new FindOneAndUpdateOptions<LeaguePlayerInfoMongo>();
            opts.ReturnDocument = ReturnDocument.After;
            var pi = _playersCollection.FindOneAndUpdate(filter, update, opts);
            if (pi == null)
                return null;

            pi = SetPlayerSeason(pi);

            pi.UnclaimedRewards = GetUnclaimedRewards(playerId);
            return pi;
        }

        public override IEnumerable<LeaguePlayerInfo> ActivePlayers(int amount)
        {
            var filter = Builders<LeaguePlayerInfoMongo>.Filter.Eq(_ => _.IsBot, false);
            var sort = Builders<LeaguePlayerInfoMongo>.Sort.Descending(_ => _.LastLogin);
            var it = _playersCollection.FindSync(filter, new FindOptions<LeaguePlayerInfoMongo, LeaguePlayerInfoMongo>() {Sort = sort, Limit = amount});
            while (it.MoveNext())
            {
                if(it.Current == null)
                    yield break;
                foreach (var p in it.Current)
                {
                    yield return p;
                }
            }
        }

        private bool IsBanned(Guid playerId)
        {
            var filter = Builders<LeagueBanMongo>.Filter.Eq(_ => _.PlayerId, playerId);
            return _banCollection.Count(filter) > 0;
        }

        public override LeaguePlayerInfo BanById(Guid playerId)
        {
            _banCollection.InsertOne(new LeagueBanMongo()
            {
                PlayerId = playerId
            });
            return Remove(playerId);
        }

        public override LeaguePlayerInfo BanByScore(long score)
        {
            var filter = Builders<LeaguePlayerInfoMongo>.Filter.Eq(_ => _.Score, score);
            var opts = new FindOptions<LeaguePlayerInfoMongo>() {Limit = 1};
            var cur = _playersCollection.FindSync(filter, opts);
            LeaguePlayerInfo pi = null;
            while (cur.MoveNext())
            {
                if (cur.Current == null || Enumerable.Count<LeaguePlayerInfoMongo>(cur.Current) == 0)
                    return null;
                pi = Enumerable.First<LeaguePlayerInfoMongo>(cur.Current);
                break;
            }
            return BanById(pi.PlayerId);
        }

        public override bool HasBan(Guid playerId)
        {
            var filter = Builders<LeagueBanMongo>.Filter.Eq(_ => _.PlayerId, playerId);
            return _banCollection.Count(filter) > 0;
        }

        public override LeaguePlayerInfo Remove(Guid playerId)
        {
            var pi = GetPlayer(playerId);
            if (pi == null)
                return null;
            var filter = Builders<LeaguePlayerInfoMongo>.Filter.Eq(_ => _.PlayerId, playerId);
            _playersCollection.DeleteOne(filter);
            if (pi.DivisionId != Guid.Empty)
            {
                var dfilter = Builders<DivisionInfo>.Filter.Eq(_ => _.Id, pi.DivisionId);
                var update = pi.IsBot
                    ? Builders<DivisionInfo>.Update.Inc(_ => _.BotsCount, -1)
                    : Builders<DivisionInfo>.Update.Inc(_ => _.Population, -1);
                _divisionsCollection.FindOneAndUpdate(dfilter, update);
            }
            return pi;
        }

        public override void WipeSeasonData(int season)
        {
            var sw = Stopwatch.StartNew();
            var dFilter = Builders<DivisionInfo>.Filter.Eq(_ => _.Season, season);
            var dResult = _divisionsCollection.DeleteMany(dFilter);
            var eFilter = Builders<SeasonInfoMongo>.Filter.And(
                Builders<SeasonInfoMongo>.Filter.Eq(_ => _.Info.Season, season),
                Builders<SeasonInfoMongo>.Filter.Eq(_ => _.IsClaimed, true));
            var eResult = _seasonEndInfoCollection.DeleteMany(eFilter);
            Log.Debug($"Divisions deleted: {dResult.DeletedCount}. season={season}.");
            Log.Debug($"SeasonEndInfos deleted: {eResult.DeletedCount}.");
            if(sw.ElapsedMilliseconds >= _config.SeasonControllerTimeThreshold)
                Log.Warn($"Wipe {season} data took {sw.ElapsedMilliseconds}ms.");
        }

        public override void ScheduleAddScore(Guid playerId, long scoreDelta)
        {
            // in procces of season end
            if (_state.SeasonEnds < DateTime.UtcNow)
            {
                var filter = Builders<PendingAddScore>.Filter.Eq(_ => _.PlayerId, playerId);
                var update = Builders<PendingAddScore>.Update.Inc(_ => _.ScoreDelta, scoreDelta);
                var opts = new FindOneAndUpdateOptions<PendingAddScore>()
                {
                    IsUpsert = true
                };
                _pendingAddScore.FindOneAndUpdate(filter, update, opts);
            }
            else
                AddScore(playerId, scoreDelta);
        }

        private void ScheduleDivisionUpdate(Guid division)
        {
            if(division == Guid.Empty)
                return;
            var filter = Builders<ChangedDivision>.Filter.Eq(_ => _.DivisionId, division);
            var update = Builders<ChangedDivision>.Update.Inc(_ => _.Queries, 1);
            var opts = new FindOneAndUpdateOptions<ChangedDivision>();
            opts.IsUpsert = true;
            try
            {
                _changedDivisionsCollection.FindOneAndUpdate(filter, update, opts);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public override void OnSeasonEnd()
        {
            var sw = Stopwatch.StartNew();

            ProcessDivisionUpdateSchedule();

            var filter = Builders<SeasonInfoMongo>.Filter.And(
                Builders<SeasonInfoMongo>.Filter.Eq(_ => _.Info.Season, _state.Season),
                Builders<SeasonInfoMongo>.Filter.Not(
                    Builders<SeasonInfoMongo>.Filter.Exists(_ => _.Info.GlobalPlace)
            ));
            var opt = new FindOptions<SeasonInfoMongo>();
            const int maxGlobalTopSize = 100000;
            var maxLeaguePlayers = maxGlobalTopSize / _defs.LeaguesAmount;

            var seasonPlayers = maxGlobalTopSize;
            try
            {
                seasonPlayers = (int)Math.Min((long)_playersCollection.Count(Builders<LeaguePlayerInfoMongo>.Filter.Gt(_ => _.LastLogin, _state.SeasonStarts)), maxGlobalTopSize);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            var count = 0;
            var swGlob = Stopwatch.StartNew();
            var globalTop = Enumerable.ToArray<long>(GetGlobalTop(seasonPlayers).Select(_ => _.Score));
            Array.Reverse((Array) globalTop);
            swGlob.Stop();

            var swLeagTop = Stopwatch.StartNew();
            var leaguesTops = new Dictionary<int, long[]>();
            for (var i = 0; i < _defs.LeaguesAmount; i++)
            {
                var leagueTop = Enumerable.ToArray<long>(GetLeagueTop(i, maxLeaguePlayers).Select(_ => _.Score));
                Array.Reverse((Array) leagueTop);
                leaguesTops[i] = leagueTop;
            }
            swLeagTop.Stop();

            var globalPosCacheMiss = 0;
            var leaguePosCacheMiss = 0;
            var bulkOpts = new List<UpdateOneModel<SeasonInfoMongo>>();

            var processed = 0;
            while (true)
            {
                var lastProcessed = processed;
                opt.Skip = processed;
                opt.Limit = 1000;
                var cursor = _seasonEndInfoCollection.FindSync(filter, opt);
                while (cursor.MoveNext())
                {
                    if (cursor.Current == null) break;
                    foreach (var seasonInfo in cursor.Current)
                    {
                        ++processed;
                        var globalPlace = Array.BinarySearch<long>(globalTop, seasonInfo.Info.Score);
                        while (globalPlace > 0 && globalTop[globalPlace - 1] == seasonInfo.Info.Score)
                        {
                            --globalPlace;
                        }

                        if (globalPlace < 0)
                        {
                            globalPlace = GetGlobalRank(seasonInfo.Info.Season, seasonInfo.Info.Score);
                            ++globalPosCacheMiss;
                        }
                        else
                            globalPlace = seasonPlayers - globalPlace;

                        var leagueTop = leaguesTops[seasonInfo.Info.LeagueLevel];
                        var leaguePlace = Array.BinarySearch<long>(leagueTop, seasonInfo.Info.Score);
                        while (leaguePlace > 0 && leagueTop[leaguePlace - 1] == seasonInfo.Info.Score)
                        {
                            --leaguePlace;
                        }

                        if (leaguePlace < 0)
                        {
                            leaguePlace = GetLeagueRank(seasonInfo.Info.Season, seasonInfo.Info.LeagueLevel, seasonInfo.Info.Score);
                            ++leaguePosCacheMiss;
                        }
                        else
                            leaguePlace = leagueTop.Length - leaguePlace;
                        filter = Builders<SeasonInfoMongo>.Filter.Eq(_ => _.Id, seasonInfo.Id);
                        var update = Builders<SeasonInfoMongo>.Update.Combine(
                            Builders<SeasonInfoMongo>.Update.Set(_ => _.Info.GlobalPlace, globalPlace),
                            Builders<SeasonInfoMongo>.Update.Set(_ => _.Info.LeaguePlace, leaguePlace));
                        bulkOpts.Add(new UpdateOneModel<SeasonInfoMongo>(filter, update));
                        ++count;
                    }
                }
                while (bulkOpts.Count > 0)
                {
                    try
                    {
                        _seasonEndInfoCollection.BulkWrite(bulkOpts);
                        bulkOpts.Clear();
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
                }
                if (processed - lastProcessed == 0)
                    break;
            }

            if (sw.ElapsedMilliseconds >= _config.SeasonControllerTimeThreshold)
            {
                Log.Debug($"Season finalize data. season={_state.Season}, time={sw.ElapsedMilliseconds}ms, seasonPlayersCount={seasonPlayers}, globTopTime={swGlob.ElapsedMilliseconds}ms, leagTopsTime={swLeagTop.ElapsedMilliseconds}ms, leaguePosCacheMisses={leaguePosCacheMiss}, globalPosCacheMisses={globalPosCacheMiss}.");
            }
        }

        private void ProcessDivisionUpdateSchedule()
        {
            Stopwatch sw = Stopwatch.StartNew();
            var swIntermediate = Stopwatch.StartNew();

            var toProcess = _changedDivisionsCollection.FindSync(Builders<ChangedDivision>.Filter.Empty).ToList();
            var endSeason = _state.SeasonEnds <= DateTime.UtcNow;

            Log.Debug($"Division schedule size: {toProcess.Count}.");
            int playersCount = 0;
            int divisionsCount = toProcess.Count;
            List<UpdateOneModel<SeasonInfoMongo>> seasonOperations = new List<UpdateOneModel<SeasonInfoMongo>>();
            List<UpdateOneModel<LeaguePlayerInfoMongo>> playerOperations = new List<UpdateOneModel<LeaguePlayerInfoMongo>>();
            List<UpdateOneModel<ChangedDivision>> divisionOperations = new List<UpdateOneModel<ChangedDivision>>();
            var season = _state.Season;
            foreach (var curr in toProcess)
            {
                try
                {
                    var divPlayers = GetDivision(curr.DivisionId).OrderByDescending(_ => _.Score).ToArray();
                    if (divPlayers.Length == 0)
                        continue;
                    var leagueLvl = divPlayers[0].LeagueLevel;
                    var up = (int)(_defs.GetDivisionUpCoeff(leagueLvl) * _defs.DivisionSize);
                    var down = divPlayers.Length - (int)(_defs.GetDivisionDownCoeff(leagueLvl) * divPlayers.Length);
                    playersCount += divPlayers.Length;
                    for (var i = 0; i < divPlayers.Length; ++i)
                    {
                        var pi = divPlayers[i];
                        if (pi.IsBot)
                            continue;
                        if (pi.Score < 1)
                            continue;

                        leagueLvl = pi.LeagueLevel;
                        var pos = i + 1;
                        var leagueChange = 0;
                        if (pos <= up)
                            leagueChange = 1;
                        else if (leagueLvl > 1 && pos >= down)
                            leagueChange = -1;
                        var divisionPlace = pos;
                        var filter = Builders<SeasonInfoMongo>.Filter.And(
                            Builders<SeasonInfoMongo>.Filter.Eq(_ => _.PlayerId, pi.PlayerId),
                            Builders<SeasonInfoMongo>.Filter.Eq(_ => _.Info.Season, season)
                        );
                        var update = Builders<SeasonInfoMongo>.Update.Combine(
                            Builders<SeasonInfoMongo>.Update.Set(_ => _.Division, curr.DivisionId),
                            Builders<SeasonInfoMongo>.Update.Set(_ => _.Info.DivisionPlace, divisionPlace),
                            Builders<SeasonInfoMongo>.Update.Set(_ => _.Info.LeagueLevel, leagueLvl),
                            Builders<SeasonInfoMongo>.Update.Set(_ => _.Info.LeagueChange, leagueChange),
                            Builders<SeasonInfoMongo>.Update.Set(_ => _.Info.Score, pi.Score)
                        );
                        seasonOperations.Add(new UpdateOneModel<SeasonInfoMongo>(filter, update) {IsUpsert = true});
                        if (endSeason)
                            playerOperations.Add(SeasonEndBulk(pi.PlayerId, new SeasonEndInfo() {LeagueLevel = leagueLvl, LeagueChange = leagueChange}));
                    }

                    if (curr.Queries > 0)
                    {
                        var dfilter = Builders<ChangedDivision>.Filter.Eq(_ => _.DivisionId, curr.DivisionId);
                        var dupdate = Builders<ChangedDivision>.Update.Inc(_ => _.Queries, -curr.Queries);
                        divisionOperations.Add(new UpdateOneModel<ChangedDivision>(dfilter, dupdate));
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"Processing division {curr} failed.", e);
                }
            }

            if (swIntermediate.ElapsedMilliseconds > 100)
                Log.Debug($"Division's player's enumeration took {swIntermediate.ElapsedMilliseconds}ms. DivCount={divisionsCount}, Players={playersCount}.");

            swIntermediate.Restart();
            var bulks = new List<Task>();
            if (seasonOperations.Count > 0)
                bulks.Add(_seasonEndInfoCollection.BulkWriteAsync(seasonOperations));

            if (playerOperations.Count > 0)
                bulks.Add(_playersCollection.BulkWriteAsync(playerOperations));

            if(divisionOperations.Count > 0)
                bulks.Add(_changedDivisionsCollection.BulkWriteAsync(divisionOperations));

            if (bulks.Count > 0)
            {
                try
                {
                    Task.WaitAll(bulks.ToArray());
                    _changedDivisionsCollection.DeleteMany(Builders<ChangedDivision>.Filter.Lt(_ => _.Queries, 1));
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }

            if(swIntermediate.ElapsedMilliseconds > 100)
                Log.Debug($"Player's division position update took {swIntermediate.ElapsedMilliseconds}ms. DivCount={divisionsCount}, Players={playersCount}.");

            _divisionUpdateTime = sw.Elapsed;
            _lastDivisionUpdate = DateTime.UtcNow;
            if(_divisionUpdateTime.TotalMilliseconds >= _config.SeasonControllerTimeThreshold)
                Log.Debug($"{toProcess.Count} divisions update took {sw.ElapsedMilliseconds}ms");
        }

        private DateTime _lastDivisionUpdate = DateTime.MinValue;
        private TimeSpan _divisionUpdateTime = TimeSpan.Zero;
        public override void ProcessAddScoreSchedule()
        {
            var sw = Stopwatch.StartNew();
            var endSeason = _state.SeasonEnds <= DateTime.UtcNow;
            var cursor = _pendingAddScore.FindSync(Builders<PendingAddScore>.Filter.Empty);
            var processed = 0;
            while (cursor.MoveNext())
            {
                if(cursor.Current == null)
                    break;
                foreach (var score in cursor.Current)
                {
                    AddScore(score.PlayerId, score.ScoreDelta);
                    var filter = Builders<PendingAddScore>.Filter.Eq(_ => _.PlayerId, score.PlayerId);
                    var update = Builders<PendingAddScore>.Update.Inc(_ => _.ScoreDelta, -score.ScoreDelta);
                    _pendingAddScore.FindOneAndUpdate(filter, update);
                    ++processed;
                }
            }

            if(processed > 0)
            {
                Log.Debug($"Pending add score records processed: {processed}. time={sw.ElapsedMilliseconds}ms.");
                sw.Restart();
                var filter = Builders<PendingAddScore>.Filter.Eq(_ => _.ScoreDelta, 0);
                var r = _pendingAddScore.DeleteMany(filter);
                Log.Debug($"Pending add score records deleted: {r.DeletedCount}. time={sw.ElapsedMilliseconds}ms.");
            }

            if (endSeason || DateTime.UtcNow - _lastDivisionUpdate > _divisionUpdateTime)
                ProcessDivisionUpdateSchedule();
        }

        private void MigrateSeasonInfos()
        {
            Log.Debug("Starting backgound season info redis->mongo migration.");
            const string PlayerInfoKey = "League:UserMap";
            var count = RedisUtils.Cache.HashLength(PlayerInfoKey);
            var infos = RedisUtils.Cache.HashScan(PlayerInfoKey);
            var bulk = new List<UpdateOneModel<SeasonInfoMongo>>();
            var pos = 0;
            var playersMigrated = 0;
            var sw = Stopwatch.StartNew();
            foreach (var hashEntry in infos)
            {
                ++pos;
                if (pos % 10000 == 0)
                {
                    var eta = sw.Elapsed.TotalMilliseconds / pos * (count - pos);
                    Log.Debug($"League migration. {pos} of {count} complete. ETA {TimeSpan.FromMilliseconds(eta)}ms. {playersMigrated} players migrated.");
                }
                if (!hashEntry.Value.HasValue)
                    continue;

                var pi = JsonConvert.DeserializeObject<LeaguePlayerInfo>(hashEntry.Value);
                if (pi.IsBot)
                    continue;
                var existingPlayer = GetPlayer(pi.PlayerId);
                if (existingPlayer != null)
                    continue;
                ++playersMigrated;
                var unclaimed = pi.UnclaimedRewards ?? new List<SeasonEndInfo>();
                pi.UnclaimedRewards = new List<SeasonEndInfo>();
                pi.DivisionId = Guid.Empty;
                while (true)
                {
                    try
                    {
                        SetPlayer(pi, DateTime.UtcNow);
                        break;
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
                }

                foreach (var reward in unclaimed)
                {
                    var filter = Builders<SeasonInfoMongo>.Filter.And(
                            Builders<SeasonInfoMongo>.Filter.Eq(_ => _.PlayerId, pi.PlayerId),
                            Builders<SeasonInfoMongo>.Filter.Eq(_ => _.Info.Season, reward.Season),
                            Builders<SeasonInfoMongo>.Filter.Or(
                                Builders<SeasonInfoMongo>.Filter.Exists(_ => _.Info.Score, false),
                                Builders<SeasonInfoMongo>.Filter.Lt(_ => _.Info.Score, reward.Score)
                                )
                        );
                    var update = Builders<SeasonInfoMongo>.Update.Combine(
                        Builders<SeasonInfoMongo>.Update.Set(_ => _.Info.DivisionPlace, reward.DivisionPlace),
                        Builders<SeasonInfoMongo>.Update.Set(_ => _.Info.LeagueLevel, reward.LeagueLevel),
                        Builders<SeasonInfoMongo>.Update.Set(_ => _.Info.LeagueChange, reward.LeagueChange),
                        Builders<SeasonInfoMongo>.Update.Set(_ => _.Info.GlobalPlace, reward.GlobalPlace),
                        Builders<SeasonInfoMongo>.Update.Set(_ => _.Info.LeaguePlace, reward.LeaguePlace),
                        Builders<SeasonInfoMongo>.Update.Set(_ => _.Info.Score, reward.Score)
                    );
                    bulk.Add(new UpdateOneModel<SeasonInfoMongo>(filter, update) { IsUpsert = true });
                }

                while (bulk.Count > 0)
                {
                    try
                    {
                        _seasonEndInfoCollection.BulkWrite(bulk);
                        bulk.Clear();
                    }
                    catch (Exception e)
                    {
                        Log.Error(hashEntry.Value.ToString(), e);
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
                }
            }
            Log.Debug($"League migration complete. {playersMigrated} players migrated. time={sw.ElapsedMilliseconds}ms.");
        }

        public void SetPlayer(LeaguePlayerInfo playerInfo, DateTime lastLogin = default(DateTime))
        {
            var filter = Builders<LeaguePlayerInfoMongo>.Filter.Eq(_ => _.PlayerId, playerInfo.PlayerId);
            var opts = new UpdateOptions()
            {
                IsUpsert = true
            };
            var r = _playersCollection.ReplaceOne(filter, new LeaguePlayerInfoMongo(playerInfo, lastLogin)
            {
                LastLogin = DateTime.UtcNow
            }, opts);
        }
    }
}
