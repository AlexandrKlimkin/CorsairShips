using MasterServerProtocol;
using System;
using System.Threading.Tasks;
using BoltTransport;


namespace BoltMasterServer
{
    internal class GameServer : IGameServer
    {
        private ITimeProvider _timeProvider;
        private readonly IConnection _connection;
        private readonly Settings _settings;
        private GameServerStateReport _state;
        private DateTime _updateTimestamp;
        
        public GameServer(IConnection connection, GameServerStateReport state, Settings settings, ITimeProvider timeProvider = null)
        {
            _settings = settings;
            _timeProvider = timeProvider ?? new DateTimeProvider();
            _connection = connection;
            _updateTimestamp = _timeProvider.UtcNow;
            State = state;
        }

        /// <returns>   Количество игроков, подключенных к игровому серверу</returns>
        public int Players => State.Players;

        /// <summary>
        /// Мастер-сервер идентифицирует различные игровые серверы через их GameServerId. Он их знает сразу,
        /// при запуске процесса. Игровой сервер получает свой идентификатор через аргументы командной
        /// строки
        /// </summary>
        ///
        /// <returns>   The identifier of the process. </returns>
        public Guid GameServerId => State.GameServerId;

        /// <summary>   Порт для подключения игроков к игровому серверу. </summary>
        ///
        /// <returns>   The port. </returns>
        public int Port { get; set; }

        public string IPAddress { get; set; }

        /// <summary>
        /// Сколько слотов в данный момент зарезервировано. Часто нужно учитывать не только количество
        /// уже подключенных игроков, но и зарезервированные слоты.
        /// </summary>
        ///
        /// <returns>   Кол-во зарезервированных слотов. </returns>
        public int Reserved => State.ReservedSlots.Count;

        public TimeSpan TimeSinceUpdate => _timeProvider.UtcNow - _updateTimestamp;

        /// <summary>
        /// статус сервера целиком приходит с игрового сервера каждые несколько секунд.
        /// </summary>
        ///
        /// <returns>   The state. </returns>
        public GameServerStateReport State { 
            get => _state;
            set {
                _updateTimestamp = _timeProvider.UtcNow;
                _state = value;
            }
        }

        /// <summary>
        /// Если игровой сервер в течении InstanceShutdownTimeout работает без игроков, его нужно
        /// выключать.
        /// </summary>
        ///
        /// <returns>   True if iterator time to shutdown server, false if not. </returns>
        public bool IsItTimeToShutdownServer()
        {
            return IsServerOutsideAllowedPeriodWithoutPlayers(_settings.InstanceShutdownTimeout);
        }

        /// <summary>
        /// Сервер перестаёт принимать новых игроков после того, как на нём в течении какого-то времени
        /// нет ни одного игрока.
        /// </summary>
        ///
        /// <returns>
        /// Вернёт true, если сервер в течении InstanceBlockTimeout работает без игроков.
        /// </returns>
        public bool IsServerGoingToClose()
        {
            return IsServerOutsideAllowedPeriodWithoutPlayers(_settings.InstanceBlockTimeout);
        }

        public Task<ReserveSlotResponse> ReserveSlot(ReserveSlotRequest request)
        {
            return _connection.SendMessageAsync<ReserveSlotResponse>(
                new ReserveSlotRequest { MessageId = request.MessageId }
            );
        }

        /// <summary>
        /// Остановка игрового сервера с помощью отправки ему сообщения о необходимости остановки. У него
        /// будет 5 секунд на то, чтобы завершить работу. Если за 5 секунд не успеет - его процесс будет
        /// остановлен.
        /// </summary>
        public void Shutdown()
        {
            _connection.SendMessage(new ShutdownServer());
        }
        private bool IsServerOutsideAllowedPeriodWithoutPlayers(TimeSpan allowedPeriod)
        {
            var noAnyPlayersTimestamp = new DateTime(_state.NoAnyPlayersTimestamp, DateTimeKind.Utc);
            var endOfAllowedPeriodWithoutPlayers = noAnyPlayersTimestamp + allowedPeriod;

            var isAllowedByTime = _timeProvider.UtcNow < endOfAllowedPeriodWithoutPlayers;

            var serverIsTooLongWithoutPlayers = (!isAllowedByTime && Players == 0);

            return serverIsTooLongWithoutPlayers;
        }
    }
}
