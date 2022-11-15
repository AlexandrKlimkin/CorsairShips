using System;
using System.Collections.Generic;
using System.Text;
using MessagePack;
using S;

namespace S
{
    [MessagePackObject()]
    public class ConflictResultPoints
    {
        [Key(0)] public string Team;
        [Key(1)] public int Points;
    }

    [MessagePackObject()]
    public class ConflictResult
    {
        [Key(0)] public string ConflictId;
        [Key(1)] public string WinnerTeam;
        [Key(2)] public string MyTeam;
        [Key(3)] public int MyPlace;
        [Key(4)] public int MySector;                            // MyPlace / (conflict.PrizePlaces / conflict.PrizesCount)
        [Key(5)] public List<ConflictResultPoints> ResultPoints;
        [Key(6)] public string TeamRewardId;
        [Key(7)] public string LeaderboardRewardId;
    }
}

namespace ServerShared.GlobalConflict
{
    public static class GlobalConflictApiTypes
    {
        public static class ConflictsSchedule // 100
        {
            public static readonly int GetCurrentConflict = 100;
        }

        public static class Players // 200
        {
            public static readonly int Register = 200;
            public static readonly int GetPlayer = 201;
            public static readonly int GetTeamPlayersStat = 202;
            public static readonly int SetName = 203;
        }

        public static class DonationStage // 300
        {
            public static readonly int Donate = 300;
        }

        public static class Battle // 400
        {
            public static readonly int RegisterBattleResult = 400;
        }

        public static class Leaderboards // 500
        {
            public static readonly int GetDonationTopMyPosition = 500;
            public static readonly int GetDonationTop = 501;
            public static readonly int GetWinPointsTopMyPosition = 502;
            public static readonly int GetWinPointsTop = 503;
        }

        public static class ConflictResults // 600
        {
            public static readonly int GetResult = 600;
        }

        public static class PointsOfInterest // 700
        {
            public static readonly int GetTeamPointsOfInterest = 700;
            public static readonly int DeployPointOfInterestAsync = 701;
        }

        public static class Debug //800
        {
            public static readonly int AddTime = 800;
            public static readonly int StartConflictById = 801;
            public static readonly int StartConflictByProto = 802;
            public static readonly int ListConflictPrototypes = 803;
        }
    }

    public interface IConflictsSchedule
    {
         void GetCurrentConflict(Action<GlobalConflictState> callback);
#if !UNITY_5_3_OR_NEWER
        GlobalConflictState GetCurrentConflict();
#endif
    }

    public interface IPlayers
    {
        void SetName(string userId, string name, Action callback);
        void Register(string conflictId, string userId, string teamId, Action<PlayerState> callback);
        void GetPlayer(string userId, string conflictId, Action<PlayerState> callback);
        void GetTeamPlayersStat(string conflict, Action<TeamPlayersStat> callback);
#if !UNITY_5_3_OR_NEWER
        PlayerState GetPlayer(string userId, string conflictId);
#endif
    }

    public interface IDebug
    {
        void AddTime(int secondsToAdd, Action callback);
        void StartConflict(string id, Action<string> callback);
        void StartConflict(GlobalConflictState prototype, Action<string> callback);
        void ListConflictPrototypes(Action<string[]> callback);
    }

    public interface IDonationStage
    {
        void Donate(string userId, int amount, Action callback);
    }

    public interface IBattle
    {
        void RegisterBattleResult(string playerId, int nodeId, bool win, decimal winMod, decimal loseMod, Action callback);
    }

    public interface ILeaderboards
    {
        void GetDonationTopMyPosition(string userId, bool myTeamOnly, string conflictId, Action<long> callback);
        void GetDonationTop(string conflictId, string team, int page, int pageSize, Action<PlayerState[]> callback);

        void GetWinPointsTopMyPosition(string userId, bool myTeamOnly, string conflictId, Action<long> callback);
        void GetWinPointsTop(string conflictId, string teamId, int page, int pageSize, Action<PlayerState[]> callback);
    }

    public interface IConflictResults
    {
        void GetResult(string conflictId, Action<ConflictResult> callback);
    }

    public interface IPointOfInterest
    {
        void GetTeamPointsOfInterest(string conflictId, string teamId, Action<PointOfInterest[]> callback);
        void DeployPointOfInterest(string conflictId, string playerId, string team, int nodeId, string poiId, Action<bool> callback);
    }

    public abstract class GlobalConflictApi
    {
        public IConflictsSchedule ConflictsScheduleApi { get; protected set; }
        public IPlayers PlayersApi { get; protected set; }
        public IDonationStage DonationApi { get; protected set; }
        public IBattle BattleApi { get; protected set; }
        public ILeaderboards LeaderboardsApi { get; protected set; }
        public IConflictResults ConflictResultsApi { get; protected set; }
        public IPointOfInterest PointOfInterestApi { get; protected set; }
        public IDebug DebugApi { get; protected set; }
    }

    public enum NodeStatus
    {
        Neutral,
        Base,
        Captured
    }

    [MessagePackObject()]
    public class TeamPlayersStat
    {
        [Key(0)] public string ConflictId;
        [Key(1)] public string[] Teams;
        [Key(2)] public int[] PlayersCount;
        [Key(3)] public int[] GeneralsCount;
    }

    [Serializable]
    [MessagePackObject()]
    public class DonationBonusLevels
    {
        [Key(0)] public int Level;
        [Key(1)] public bool Team;
        [Key(2)] public int Threshold;       // должно быть больше значения предыдущего уровня.
    }

    public enum DonationBonusType
    {
        Unpecified,
        TeamPointsBuff,
    }

    [Serializable]
    [MessagePackObject()]
    public class DonationBonus
    {
        [Key(0)] public string ClientType;
        [Key(1)] public DonationBonusType ServerType;
        [Key(2)] public bool Team;       // true - командный бонус, иначе личный
        [Key(3)] public decimal Value;     // однотипные бонусы сладываются
        [Key(4)] public int Level;
    }

    [Serializable]
    [MessagePackObject()]
    public class NodeState
    {
        // desc
        [Key(0) ] public int Id;
        [Key(1) ] public string GameMode;                     // В каком режиме будет запущен бой (может быть null)
        [Key(2) ] public string GameMap;                      // Возможность указать массив стрингов (айдишников карт) для клиента
        [Key(3) ] public string BaseForTeam;                  // Для какой команды нод является базой (нод-базу невозможно захватить)
        [Key(4) ] public int WinPoints;                       // !![В одной из реализаций этот параметр рассчитывается исходя из кастомных данных] Сколько TeamPoints получит команда, в случае победы игрока. Может модифицироваться параметрами: бонус от этапа подготовки, временные усиления нода, усиления от соседних нодов, экипировка игрока.
        [Key(5) ] public int LosePoints;                      // !![В одной из реализаций этот параметр рассчитывается исходя из кастомных данных] Сколько TeamPoints получит команда, в случае поражения игрока
        [Key(6) ] public int NeutralThreshold;                // Минимальное количество очков, необходимых для захвата нода, Первая команда, набравшая TeamPoints == NeutralThreshold захватывает нод, переводя его из состояния “нейтральный” в состояние “захваченный”.
        [Key(7) ] public int CaptureThreshold;                // минимальная разница между очками (TeamPoints) команд для перезахвата точки. Используется, если не активна система раундов
        [Key(8) ] public int CaptureBonus;                    // Количество очков (WinPoints), которые добавятся команде захватившей точку
        [Key(9) ] public int CaptureLimit;                    // Максимальная сумма очков нода. Если суммарные очки команд в ноде равны лимиту, то при наборе очков одной из команд, очки других команд уменьшаются на значение, превышающее лимит. Если команд на точке больше 2, то уменьшаем очки пропорционально долям команд в ноде.
        [Key(10)] public int ResultPoints;                    // Сколько очков в итоговый счет команды приносит нод. Победа команды считается по сумме ResultPoints.
        [Key(11)] public float SupportBonus;                  // 0..1 Множитель очков захвата (WinPoints), получаемых в боях на соседних нодах.
        [Key(12)] public float BattleBonus;                   // Глобальный модификатор параметров боя (увеличенное здоровье, скорость или урон) 
        [Key(13)] public float RewardBonus;                   // Глобальный модификатор наград за бой (валюты айтемы)
        [Key(14)] public string ContentBonus;                 // Доступ к уникальному контенту (возможность брать особый юнит, скин или экипировку)
        [Key(15)] public float PositionX;                     // 0..1 отношение позиции к ширине карты
        [Key(16)] public float PositionY;                     // 0..1 отношение позиции к высоте карты
        [Key(17)] public int[] LinkedNodes;                   // соседние ноды
        // var
        [Key(18)] public NodeStatus NodeStatus;                 // Состояние (стартовый, нейтральный, захваченный)
        [Key(19)] public Dictionary<string, int> TeamPoints;  // Количество очков, каждой команды
        [Key(20)] public string Owner;                        // Текущий владелец точки. При длительности раунда равной 0 выбирается исходя из TeamPoints, иначе - меняется раз в CaptureTime
        [Key(21)] public string PointOfInterestId;
    }

    public enum TeamAssignType
    {
        None,
        BasicAuto
    }

    [Serializable]
    [MessagePackObject()]
    public class MapState
    {
        [Key(0)] public string TextureId;
        [Key(1)] public NodeState[] Nodes = new NodeState[] {};
    }

    public enum StageType
    {
        Unknown,
        Cooldown,
        Donation,
        Battle,
        Final,
    }

    [Serializable]
    [MessagePackObject()]
    public class StageDef
    {
        [Key(0)] public StageType Id;
        [Key(1)] public TimeSpan Period;             // продолжительность этапа
    }

    public enum PointOfInterestServerLogic
    {
        None,
        CaptureInterest                              // множитель очков захвата (WinPoints)
    }

    [Serializable]
    [MessagePackObject()]
    public class PointOfInterestBonus
    {
        [Key(0)] public string ClientType;
        [Key(1)] public PointOfInterestServerLogic ServerType;
        [Key(2)] public decimal Value;
    }

    [Serializable]
    [MessagePackObject()]
    public class PointOfInterest
    {
        [Key(0)] public string Id;                  // уникальный ид в пределах конфликта
        [Key(1)] public int NodeId;                 // переменная, выставляется сервером
        [Key(2)] public string OwnerTeam;           // оставье пустым если точка не привязана к команде
        [Key(3)] public TimeSpan BonusTime;         // период активности бонуса, если не задан берется дефолтный из конфликта
        [Key(4)] public TimeSpan DeployCooldown;    // максимальная частота деплоя точки, если не задан берется дефолтная
        [Key(5)] public DateTime BonusExpiryDate;
        [Key(6)] public DateTime NextDeploy;
        [Key(7)] public int GeneralLevel;           // если точка генеральская, то в параметре указан уровень геренрала, который ей управляет
        [Key(8)] public bool Auto;
        [Key(9)] public PointOfInterestBonus[] Bonuses;
        [Key(10)] public string Type;
    }

    [Serializable]
    [MessagePackObject()]
    public class NodeQuest
    {
        [Key(0)] public string Id;
        [Key(1)] public int QuestLevel;                      // если точка квестовая. среди вчех точек с одним QuestLevel будет выбираться случайная
        [Key(2)] public string ClientType;
        [Key(3)] public bool Auto;
        [Key(4)] public TimeSpan ActiveTime;
        [Key(5)] public TimeSpan DeployCooldown;
        [Key(6)] public string RewardId;
        [Key(7)] public int Weight;
    }

    [MessagePackObject()]
    public class PlayerState
    {
        [Key(0)] public string Id;                   // игровой id юзера
        [Key(1)] public string ConflictId;
        [Key(2)] public string TeamId;
        [Key(3)] public int WinPoints;
        [Key(4)] public int DonationPoints;
        [Key(5)] public int GeneralLevel;
        [Key(7)] public DonationBonus[] DonationBonuses = new DonationBonus[] {};
        [Key(8)] public DateTime RegisterTime = DateTime.UtcNow;
        [Key(9)] public string Name;
    }

    [Serializable]
    [MessagePackObject()]
    public class TeamState
    {
        [Key(0)] public string Id;
        [Key(1)] public int WinPoints;
        [Key(2)] public int ResultPoints;
        [Key(3)] public int DonationPoints;
        [Key(4)] public DonationBonus[] DonationBonuses = new DonationBonus[] { };
    }

    [Serializable]
    [MessagePackObject()]
    public class GlobalConflictState
    {
        // mostly consts
        [Key(0)] public int PrizePlaces = 100;       // топ 100 получает какие либо призы
        [Key(1)] public int PrizesCount = 5;         // градация призов по секторам PrizePlaces / PrizesCount (в базовом варианте 20 мест)
        // desc
        [Key(2)] public string Id;                        // идентификатор события
        [Key(3)] public string[] Teams = new string[] {};              // id команд
        [Key(4)] public TeamAssignType AssignType;   // способ распределения по командам
        [Key(5)] public int CaptureTime;             // Секунды. Длительность раунда, может быть равна 0, тогда перезахват происходит сразу по выполнению условия.
        [Key(6)] public int LastRound;
        [Key(7)] public int BattleCost;              // Базовая стоимость битвы за любую точку
        [Key(8)] public MapState Map;                // Описание карты
        [Key(9)] public DonationBonusLevels[] DonationBonusLevels = new DonationBonusLevels[] {};
        [Key(10)] public DonationBonus[] DonationBonuses = new DonationBonus[] {};
        [Key(11)] public StageDef[] Stages;
        [Key(12)] public int GeneralsCount;            // количество генералов
        [Key(13)] public PointOfInterest[] PointsOfInterest = new PointOfInterest[] {}; // прототипы всех командных точек интереса
        [Key(14)] public int MaxPointsOfInterestAtNode;            // максимального числа точек в ноде
        [Key(15)] public int MaxSameTypePointsOfInterestAtNode;    // ограничение на установку точек одного типа в ноде
        [Key(16)] public NodeQuest[] Quests = new NodeQuest[] {};           // прототипы персональных квестов игрока
        // var
        [Key(17)] public TeamState[] TeamsStates = new TeamState[] {};
        [Key(18)] public DateTime StartTime;
        [Key(19)] public DateTime EndTime;
    }
}
