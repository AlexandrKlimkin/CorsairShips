using BoltTransport;
using MasterServerProtocol;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace BoltGameServerToMasterServerConnector
{
    public class MasterServerConnectionImplementation : Connection
    {
        private GameServerParameters _gameServerParameters;
        
        /*
         * Функция, отдающая текущее число игроков на сервере.
         * Не хочется тут создавать зависимости от PhotonBolt;
         * Можно инициализировать вот так в конкретной игре:
         * MasterServerConnectionImplementation.GetPlayerCount = () => BoltNetwork.Connections.Count();
         */
        public static Func<int> GetPlayerCount;

        public static List<SlotReservation> _reservedSlots = new List<SlotReservation>();
        public static int ReservedSlotsCount
        {
            get
            {
                lock (_reservedSlots)
                {
                    return _reservedSlots.Count;
                }
            }
        }

        public static volatile bool TimeToLeave;

        public MasterServerConnectionImplementation()
        {
        }

        public MasterServerConnectionImplementation(GameServerParameters gameServerParameters)
        {
            _gameServerParameters = gameServerParameters;
        }

        protected override async Task ProcessMessage(Message message)
        {
            if (message is ShutdownServer)
            {
                TimeToLeave = true;
            } 
            else if (message is ReserveSlotRequest reserve)
            {
                try
                {
                    var maxConnections = _gameServerParameters.MaxConnections.Value;
                    var gameServerPort = _gameServerParameters.GameServerExternalPort.Value;
                    var ipAddress = _gameServerParameters.GameServerIpAddress;
                    var playersAndReserve = ReservedSlotsCount + GetPlayerCount();

                    UnityEngine.Debug.Log("ReservedSlots.Count: " + ReservedSlotsCount + " maxConnections: " +
                                          maxConnections + " bolt connections: " + GetPlayerCount());

                    if (playersAndReserve < maxConnections)
                    {
                        var slot = Guid.NewGuid();

                        lock (_reservedSlots)
                        {
                            _reservedSlots.Add(new SlotReservation
                            {
                                ReservationId = slot,
                                Timestamp = DateTime.UtcNow.Ticks
                            });
                        }

                        var response = new ReserveSlotResponse
                        {
                            MessageId = reserve.MessageId,
                            JoinInfo = new JoinInfo
                            {
                                ReservedSlot = slot,
                                Port = gameServerPort,
                                IpAddress = ipAddress,
                                Map = SceneManager.GetActiveScene().name
                            },
                            Succeed = true
                        };

                        SendMessage(response);
                    }
                    else
                    {
                        var response = new ReserveSlotResponse
                        {
                            MessageId = reserve.MessageId,
                            Succeed = false
                        };

                        SendMessage(response);
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError("MasterServerConnectionImplementation: exception: " + e.Message + " " + e.StackTrace);
                    throw;
                }
            }

            await Task.CompletedTask;
        }
    }
}
