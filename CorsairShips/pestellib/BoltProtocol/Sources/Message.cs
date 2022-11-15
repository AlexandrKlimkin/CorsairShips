using System;
using System.Collections.Generic;

namespace MasterServerProtocol
{
    #region Common
    public class Message
    {
        public Guid MessageId = Guid.NewGuid();
    }

    public enum ErrorCode
    {
        SERVERS_ARE_FULL,
        COMPATIBLE_MASTER_NOT_FOUND
    }

    public class Error : Message
    {
        public ErrorCode Code;
    }

    public class ReportIdentifier : Message
    {
        public string Identifier;
    }

    public class GameInfo : IEquatable<GameInfo>
    {
        public string GameName; //для отличия разных приложений
        public string GameVersion; //версия приложения

        public bool Equals(GameInfo other)
        {
            return other.GameName == GameName && other.GameVersion == GameVersion;
        }
    }

    public class JoinInfo {
        public string IpAddress;
        public int Port;
        public Guid ReservedSlot;
        public string Map;
    }
    #endregion

    #region Client <-> MasterServer interaction
    public class RequestServer : Message
    {
        public GameInfo GameInfo; //тут имя приложения и версия
        /*
         * произвольная информация для матчмейкинга. Обычно джейсон
         * Тут может быть например игровой уровень (если их несколько в игре)
         * Режим игры (DM/CTF etc.)
         * Тир игроков.
         * Может так же быть какая-то информация о игроке, который ищет матч - уровень и тп
         */
        public string MatchmakingData; 
    }

    public class RequestServerResponse : Message
    {
        public JoinInfo JoinInfo;
    }
    #endregion

    #region GameServer <-> MasterServer interaction
    public class InitializeGameServer : Message
    {
        public string MatchmakingData;
    }
    public class GameServerStateReport : Message
    {
        public int Players;
        public int MaxPlayers;
        public string MatchmakingData;
        public Guid GameServerId;
        public long NoAnyPlayersTimestamp;

        /*
         * Зарезервированные слоты - для тех игроков, которые решили подключиться 
         * к этому серверу, но пока что находятся в процессе подключения или загрузки 
         * уровня
         */
        public List<SlotReservation> ReservedSlots = new List<SlotReservation>();
    }

    public struct SlotReservation
    {
        public Guid ReservationId;
        public long Timestamp;
    }

    public class ReserveSlotRequest : Message
    {
        public Guid GameServerId;
    }

    /*
     * Перед тем как отдать игроку адрес игрового сервера, мастер
     * должен забронировать для этого игрока место.
     * 
     * При бронировании слота игровой сервер возвращает идентификатор бронирования
     * это необходимо т.к. скорость загрузки клиентов отличается, кроме того, какие-то клиенты могут в итоге 
     * не подключиться, в таком случае нужно сбрасывать бронирование по тайм-ауту
     */
    public class ReserveSlotResponse : Message
    {
        public bool Succeed;

        public JoinInfo JoinInfo;
    }
    
    public class ShutdownServer : Message { }

    public class Received : Message { }

    public class RemoveGameServerFromMaster : Message {
        public Guid GameServerId;
    }
    #endregion


    #region LoadBalancing <-> MasterServer interaction
    public class MasterServerReport : Message
    {
        public GameServerStateReport[] GameServers; //данные по игровым серверам, которые управляются данным мастером
        public float CPUUsage; //загрузка CPU на машине, на которой запущен данный сервер от 0.00 (0%) до 1.00 (100%)
        public Guid InstanceId; //уникальный идентификатор данного мастер-сервера
        public GameInfo GameInfo; //тут имя приложения и версия
        public int MasterListenerPort; //номер порта, к которому пытаются подключиться запускаемы этим мастером игровые сервера
    }

    public class CreateGameRequest : Message
    {
        public string MatchmakingData;
    }

    public class CreateGameResponse : Message
    {
        public JoinInfo JoinInfo;
    }

    /*
     * Мастер-сервер при запуске должен получить конфиг от лоад балансера такой, 
     * что бы он не конфликтовал по портам с другими мастер клиентами на данном хосте
     */
    public class MasterConfigurationRequest : Message
    {
        public string IPAdress;
        public MasterServerReport Report;
    }

    public class MasterConfigurationResponse : Message
    {
        public int FirstGameServerPort;
        public int MasterListenerPort;
    }
    #endregion
}
