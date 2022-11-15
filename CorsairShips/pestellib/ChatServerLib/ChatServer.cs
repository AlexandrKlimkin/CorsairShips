using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using PestelLib.ChatCommon;
using System.Text.RegularExpressions;
using ChatCommon;
using ChatServer;
using ChatServer.Mongo;
using ChatServer.Transport;
using Lidgren.Network;
using ServerShared;
using log4net;
using MongoDB.Driver;
using PestelLib.ServerCommon.Db.Auth;
using PestelLib.ServerShared;
using ChatProtocol = PestelLib.ChatCommon.ChatProtocol;

namespace PestelLib.ChatServer
{
    public partial class ChatServer : IStatsProvider<ChatServiceStats>, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ChatServer));
        private DateTime _startTime = DateTime.UtcNow;
        private Dictionary<string, ChatUser> _users = new Dictionary<string, ChatUser>();
        private Dictionary<string, List<ChatProtocol>> _messageHistory = new Dictionary<string, List<ChatProtocol>>();
        private ChatServiceStats _stats;
        private ChatServerStatsGatherer _statsGatherer;
        private object _statGatherer = new object();
        private ChatServerConfig _config;
        private BanStorageFactory _banStorageFactory;
        private byte[] _superuserSecret;
        private ITokenStore _tokenStore;
        private ISuperUsersDb _superusersDb;
        public int ChatId { get; }

        public ChatServer(IChatServerTransport transport)
        {
            _stats = new ChatServiceStats();
            ChatId = (int) Crc32.Compute(AppDomain.CurrentDomain.BaseDirectory);
            Log.Debug($"AppDir={AppDomain.CurrentDomain.BaseDirectory}, ChatId={ChatId}.");
            _config = ChatServerConfigCache.Get();
            Guid superuser;
            if (string.IsNullOrEmpty(_config.SuperuserSecret) || !Guid.TryParse(_config.SuperuserSecret, out superuser))
            {
                superuser = Guid.NewGuid();
            }
            var superuserSecretStr = superuser.ToString("N");
            _superuserSecret = Encoding.UTF8.GetBytes(superuserSecretStr);
            Log.Info($"Superuser secret '{superuserSecretStr}'");
            _banStorageFactory = new BanStorageFactory(_config);
            _superusersDb = new MongoSuperUsersDb(MongoUrl.Create(_config.MongoConnectionString));
            GraphiteClient graphiteClient = new GraphiteClient(_config.GraphiteHost, _config.GraphitePort);
            _statsGatherer = new ChatServerStatsGatherer(TimeSpan.FromMinutes(1), graphiteClient, this, _config);
            _server = transport;
            _server.Start();

            if (!string.IsNullOrEmpty(_config.TokenStorageConnectionString))
            {
                // if token store not null when all ClientLoginInform provides token issued by backend
                _tokenStore = TokenStorageFactory.GetStorage(_config.TokenStorageConnectionString);
            }

            _chatServerLoop = new Thread(ChatServerLoop);
            _chatServerLoop.IsBackground = true;
            _chatServerLoop.Name = "ChatServerThread";

            _chatServerLoop.Start();
        }

        public void Dispose()
        {
            if(_server.IsConnected)
                _server.Stop();

            _statsGatherer.Dispose();

            _chatServerLoop.Join();
        }

        private void ChatServerLoop()
        {
            Log.Debug("Enter ChatServerLoop");

            while (_server.IsConnected)
            {
                ChatProtocol im;
                ChatConnection connection;
                while ((im = _server.ReceiveMessage(1000, out connection)) != null)
                {
                    Log.Debug(JsonConvert.SerializeObject(im));
                    ProcessCommand(im, connection);
                    CleanUsers();
                    ProcessBans();
                }
                ProcessBans();
            }

            Log.Debug("Exit ChatServerLoop");
        }

        private void CleanUsers()
        {
            var disconnectedUsers = _users.Values
                .Where(u => !u.Connection.IsConnected).ToArray();

            for (var i = 0; i < disconnectedUsers.Length; ++i)
            {
                var u = disconnectedUsers[i];
                LeaveAllChannels(u);
                Log.Info($"User {u} disconnected");
                _users.Remove(u.Token);
                u.Connection.Close();
            }
        }

        ChatServiceStats IStatsProvider<ChatServiceStats>.GetStats()
        {
            lock (_statGatherer)
            {
                var usersInRooms = new Dictionary<string, int>();
                var bansInRooms = new Dictionary<string, int>();

                foreach (var u in _users.Values)
                {
                    foreach (var c in u.Channels)
                    {
                        if (!usersInRooms.ContainsKey(c))
                            usersInRooms[c] = 1;
                        else
                            ++usersInRooms[c];
                        if (u.FloodTimeout > DateTime.UtcNow)
                        {
                            if (!bansInRooms.ContainsKey(c))
                                bansInRooms[c] = 1;
                            else
                                ++bansInRooms[c];
                        }
                    }
                }

                _stats.CurrentConnections = _users.Values.Count;
                _stats.RoomsCount = usersInRooms.Count;
                _stats.UsersInRooms = usersInRooms;
                _stats.BansInRooms = bansInRooms;
                _stats.Uptime += DateTime.UtcNow - _startTime;
                _stats.FloodBans = _users.Count(kv => kv.Value.FloodTimeout > DateTime.UtcNow);
                _stats.MaxFloodBanLevel = _stats.FloodBans > 0 ? _users.Max(kv => kv.Value.FloodLevel) : 0;

                return _stats;
            }
        }

        private static ChatUser InteropAdmin = new ChatUser()
        {
            SuperUser = true
        };
        private ChatUser GetUser(ChatProtocol command, ChatConnection peer)
        {
            ChatUser chatUser;
            var token = command.ClientInfo.Token;
            var auth = false;
            var superuser = false;
            var authData = command.ClientInfo.AuthData;
            if (_config.AdminInteropIPs.Contains(peer.RemoteEndPoint.Address.ToString()))
            {
                return InteropAdmin;
            }

            if (authData != null)
            {
                superuser = _superuserSecret.SequenceEqual(authData);
                if (superuser && command.CommandType != CommandType.ClientLoginInform)
                    return InteropAdmin;
            }

            if (command.CommandType == CommandType.ClientLoginInform && authData != null)
            {
                auth = superuser;
                auth |= _tokenStore == null;
                if (!auth)
                {
                    try
                    {
                        var authToken = _tokenStore.GetByTokenId(new Guid(authData));
                        if (authToken != null)
                        {
                            auth = authToken.ExpiryTime > DateTime.UtcNow;
                            if(!auth)
                                Log.Warn($"Session '{authToken.TokenId}' expired at {authToken.ExpiryTime}.");
                            else if (_config.TokenIpCheck)
                            {
                                auth = authToken.Address.Contains(peer.RemoteEndPoint.ToString());

                                if(!auth)
                                    Log.Warn($"Wrong IP '{peer.RemoteEndPoint}' for session '{authToken.TokenId}'. validIp='{authToken.Address}'.");
                            }
                        }
                        if (!auth)
                        {
                            Log.Error($"Invalid backend token '{string.Join(":", authData.Select(_ => _.ToString("X")))}'.");
                        }
                        else
                        {
                            authData = authToken.PlayerId.ToByteArray(); // to support bans
                            Log.Debug($"{peer.RemoteEndPoint} auth token check success.");
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error("Token storage error.", e);
                        auth = false;
                    }
                }
                token = command.ClientInfo.Token = FromBytes(authData);
                command.ClientInfo.AuthData = null;
                superuser = _superusersDb.IsSuper(authData);
                if(superuser)
                    Log.Debug($"Superuser login {token}:{command.ClientInfo.Name}");
                if (_users.TryGetValue(token, out chatUser))
                    chatUser.SuperUser = superuser;
            }
            if (token != null && _users.TryGetValue(token, out chatUser))
            {
                if (!chatUser.Connection.Equals(peer))
                {
                    if (command.CommandType != CommandType.ClientLoginInform)
                    {
                        Log.Error($"Connection {peer.RemoteEndPoint} doesnt own token {token}");
                        return null;
                    }
                    Log.Info($"User {chatUser} reconnect");
                    chatUser.Connection = peer;
                }

                if (command.BodyMetadata != null)
                    chatUser.LastMeta = command.BodyMetadata;
                return chatUser;
            }
            if (!auth)
            {
                Log.Error($"Unauthorized access from {peer.RemoteEndPoint}. dump=" + JsonConvert.SerializeObject(command));
                return null;
            }

            _users[token] = chatUser = new ChatUser();
            chatUser.Connection = peer;
            chatUser.Token = token;
            chatUser.Name = command.ClientInfo.Name;
            chatUser.SuperUser = superuser;
            chatUser.PlayerId = command.ClientInfo.PlayerId;
            if (command.BodyMetadata != null)
                chatUser.LastMeta = command.BodyMetadata;
            if (chatUser.SuperUser)
            {
                Log.Debug($"'{chatUser.Name}:{token}:{peer.RemoteEndPoint}' is superuser");
                _server.SendTo(new ChatProtocol()
                {
                    CommandType = CommandType.ServiceMessage,
                    Body = "Privilege escalation"
                }, peer);
            }

            return chatUser;
        }

        private bool ValidateFlooder(ChatUser user, ChatProtocol command)
        {
            if (user.SuperUser)
                return true;

            if (user.FloodTimeout > DateTime.UtcNow)
            {
                if (command.CommandType == CommandType.Message)
                {
                    if (++user.InBanMessages >= Consts.FloodInBanMessageCountLevelUp)
                    {
                        user.InBanMessages = 0;
                        var d = DateTime.UtcNow;
                        user.FloodLevel = Math.Min(user.FloodLevel + 1, Consts.FloodMaxLevel);
                        var period = GetFloodTimeout(user.FloodLevel);
                        var banResult = _banStorageFactory.Get().GrantBan(user, BanReason.Flood, d + period);
                        if (banResult == DateTime.MinValue)
                            banResult = d + period;
                        user.FloodTimeout = banResult;
                        Log.Info($"Ban levelup {user.FloodLevel} to {user}. {period} ({user.FloodTimeout}) from {d}");
                    }
                }

                return false;
            }

            if (user.FloodLevel > 0)
            {
                var lvlT = GetFloodTimeout(user.FloodLevel);
                var dec = (int)((DateTime.UtcNow - user.FloodTimeout).TotalMilliseconds / lvlT.TotalMilliseconds);
                if (dec > 0)
                {
                    dec = Math.Min(dec, user.FloodLevel);
                    user.FloodLevel -= dec;
                    Log.Info($"Ban leveldown {user.FloodLevel} to {user}");
                }
            }

            return true;
        }

        private TimeSpan GetFloodTimeout(int level)
        {
            return TimeSpan.FromSeconds(Math.Exp(level) * 10f);
        }

        private bool ValidateMessage(ChatUser user, ChatProtocol command)
        {
            if (user.SuperUser)
                return true;
            var rgxMultiSpaces = new Regex("\\s+");
            var history = GetMessageHistory(command);
            var lastMessages = history.Where(m => (DateTime.UtcNow - m.Time) < TimeSpan.FromSeconds(30)).ToArray();
            var userMessages = history.Where(m => m.ClientInfo.Token == user.Token && (DateTime.UtcNow - m.Time) < TimeSpan.FromSeconds(30)).OrderBy(m => m.Time).ToArray();
            var userMessagesCount = userMessages.Length;
            if (userMessages.Length == 0)
                return true;
            var lm = userMessages.Last();
            var messagePerc = userMessagesCount > 0 && lastMessages.Length > _config.BadMessageLimit ? (float)userMessagesCount / lastMessages.Length : 0f;
            var historyLastMessage = rgxMultiSpaces.Replace(lastMessages.Last().Body, " ").ToLower();
            var lastMsg = rgxMultiSpaces.Replace(lm.Body, " ").ToLower();
            var curMsg = rgxMultiSpaces.Replace(command.Body, " ").ToLower();
            if (lastMsg == curMsg || (lastMsg.StartsWith(curMsg) && historyLastMessage == lastMsg && curMsg.Length - lastMsg.Length < 4) || messagePerc > Consts.FloodMessagesPerc)
            {
                user.FloodLevel = Math.Min(user.FloodLevel + 1, Consts.FloodMaxLevel);
                var d = DateTime.UtcNow;
                var period = GetFloodTimeout(user.FloodLevel);
                user.FloodTimeout = d + period;
                Log.Info($"Ban granted to {user}. {period} ({user.FloodTimeout}) from {d}");
                _banStorageFactory.Get().GrantBan(user, BanReason.Flood, user.FloodTimeout);
                BanNotify(user, BanReason.Flood, user.FloodTimeout, command.ChannelName);
                return false;
            }

            return true;
        }

        private bool CheckBansAndNotify(ChatUser user, string channel)
        {
            if (_banStorageFactory.Get().IsBanned(user))
            {
                var bans = _banStorageFactory.Get().GetBans(user);
                for (var i = 0; i < bans.Length; ++i)
                {
                    var period = (int)(bans[i].Expiry - DateTime.UtcNow).TotalSeconds;
                    BanNotifyDest(user, bans[i].Reason, period, channel, user.Connection);
                }
                return true;
            }

            return false;
        }

        private void ProcessCommand(ChatProtocol command, ChatConnection peer)
        {
            ChatUser user;
            try
            {
                user = GetUser(command, peer);
            }
            catch (Exception e)
            {
                Log.Error(e);
                return;
            }

            if(user == null)
                return;
            ValidateFlooder(user, command);
            switch (command.CommandType)
            {
                case CommandType.ClientLoginInform:
                    AssignToChannel(command.ChannelName, user);
                    _server.SendTo(new ChatProtocol()
                    {
                        CommandType = CommandType.LoginResult,
                        ChannelName = command.ChannelName,
                        ClientInfo = command.ClientInfo,
                        Body = "true"
                    }, user.Connection);

                    CheckBansAndNotify(user, command.ChannelName);
                    break;
                case CommandType.GetBanInfo:
                    {
                        BanInfo[] bans = _banStorageFactory.Get().GetBans(user);
                        _server.SendTo(new ChatProtocol()
                        { 
                            CommandType = CommandType.BanInfo,
                            ClientInfo = command.ClientInfo,
                            Body = JsonConvert.SerializeObject(bans),
                            ChannelName = command.ChannelName
                        }, user.Connection);
                    }
                    break;
                case CommandType.ClientLogOffInform:
                    break;
                case CommandType.Message:

                    if (_config.BannedChannels?.Contains(command.ChannelName) == true)
                    {
                        break;
                    }

                    bool muted = false;
                    try
                    {
                        Log.Debug($"Message from {peer.RemoteEndPoint} {command.Body}.");

                        if (_banStorageFactory.Get().IsBanned(peer.RemoteEndPoint))
                        {
                            Log.Debug($"IP banned {peer.RemoteEndPoint}. message={JsonConvert.SerializeObject(command)}");
                            muted = true;
                        }
                    }
                    catch
                    {
                    }

                    if (!muted)
                    {
                        if (CheckBansAndNotify(user, command.ChannelName))
                            break;
                        if (!ValidateMessage(user, command))
                            break;
                        if (user.FloodTimeout > DateTime.UtcNow)
                            break;
                        SaveHistory(command);
                    }

                    if (command.ChannelName != null && !user.Channels.Contains(command.ChannelName))
                    break;

                    // broadcast this to all connections, except sender
                    var targChannel = command.ChannelName;
                    var sendBack = command.Version > 0;
                    

                    var all = 
                        GetUsers(command)
                        .Where(c => targChannel == null || c.Channels.Contains(targChannel))
                        .Where(c => !muted || c.Connection.Equals(peer))
                        .Where(c => sendBack || !c.Connection.Equals(peer))
                        .Select(c => c.Connection)
                        .ToArray();

                    if (all.Length > 0)
                    {
                        _server.SendTo(command, all);
                    }
                    break;
                case CommandType.SendClientList:
                    _server.SendTo(new ChatProtocol()
                    {
                        CommandType = CommandType.SendClientList,
                        ChannelName = command.ChannelName,
                        Clients = _users.Values
                            .Where(u => u.Channels.Contains(command.ChannelName))
                            .Cast<ClientInfo>()
                            .ToArray(),
                        Tag = command.Tag
                    }, user.Connection);
                    break;
                case CommandType.SendHistory:
                    _server.SendTo(new ChatProtocol()
                    {
                        CommandType = CommandType.SendHistory,
                        ChannelName = command.ChannelName,
                        SendTo = command.SendTo,
                        MessageHistory = GetMessageHistory(command).ToArray(),
                        Tag = command.Tag
                    }, user.Connection);
                    break;
                case CommandType.SendPrivateHistory:
                    var history = GetPrivateMessagesHistory(user.Token);
                    foreach (var kv in history)
                    {
                        _server.SendTo(new ChatProtocol()
                        {
                            CommandType = CommandType.SendPrivateHistory,
                            ClientInfo = kv.from,
                            MessageHistory = kv.history
                        }, user.Connection);
                    }
                    break;
                case CommandType.JoinChannel:
                    AssignToChannel(command.ChannelName, user);
                    _server.SendTo(new ChatProtocol()
                    {
                        CommandType = CommandType.JoinedChannel,
                        ChannelName = command.ChannelName,
                        Tag = command.Tag
                    }, user.Connection);
                    break;
                case CommandType.LeaveChannel:
                    LeaveChannel(command.ChannelName, user);
                    _server.SendTo(new ChatProtocol()
                    {
                        CommandType = CommandType.LeaveChannel,
                        ChannelName = command.ChannelName,
                        Tag = command.Tag
                    }, user.Connection);
                    break;
                case CommandType.MessageFilterReport:
                    var strikesCount = user.FilterStrikes.Add(command.BanReason);
                    if (strikesCount >= _config.BadWordCount)
                    {
                        var notifyAll = !_banStorageFactory.Get().IsBanned(user);
                        foreach (var reason in user.FilterStrikes.Resons)
                        {
                            var expiry = _banStorageFactory.Get().GrantBan(user, reason, TimeSpan.FromSeconds(_config.BadWordBanTime));
                            if (notifyAll)
                                BanNotify(user, reason, expiry, command.ChannelName);
                        }
                        user.FilterStrikes.Clear();
                    }
                    break;
                case CommandType.ListChannels:
                    var allChannels = _users.SelectMany(_ => _.Value.Channels).Distinct().ToArray();
                    _server.SendTo(new ChatProtocol()
                    {
                        CommandType = CommandType.ListChannels,
                        Body = JsonConvert.SerializeObject(allChannels),
                        Tag = command.Tag
                    }, user.Connection);
                    break;
                case CommandType.BanUser:
                    if (user.SuperUser)
                    {
                        if (int.TryParse(command.Body, out var time) && command.Clients != null)
                        {
                            foreach (var client in command.Clients)
                            {
                                var expiry = _banStorageFactory.Get().GrantBan(client, BanReason.AdminBan, TimeSpan.FromSeconds(time));
                                if (_users.TryGetValue(client.Token, out var banedUser))
                                {
                                    BanNotify(banedUser, BanReason.AdminBan, expiry, null);
                                }
                            }
                        }
                    }
                    else
                    {
                        _server.SendTo(new ChatProtocol()
                        {
                            CommandType = CommandType.ServiceMessage,
                            Body = "Access denied",
                            Tag = command.Tag
                        }, peer);
                    }

                    break;
                case CommandType.AddSuperByAuthData:
                    if (user.SuperUser)
                    {
                        byte[] authData = StringUtils.HexToBytes(command.Body);
                        bool r = AddSuper(authData);
                        Log.Debug($"Add superuser {command.Body} {r}");
                    }
                    break;
                case CommandType.RemoveSuperByAuthData:
                    if (user.SuperUser)
                    {
                        byte[] authData = StringUtils.HexToBytes(command.Body);
                        bool r = RemoveSuper(authData);
                        Log.Debug($"Remove superuser {command.Body} {r}");
                    }
                    break;
            }
        }

        private bool AddSuper(byte[] authData)
        {
            var token = FromBytes(authData);
            if(_users.TryGetValue(token, out var user))
                user.SuperUser = true;
            return _superusersDb.AddSuper(authData);
        }

        private bool RemoveSuper(byte[] authData)
        {
            var token = FromBytes(authData);
            if (_users.TryGetValue(token, out var user))
                user.SuperUser = false;
            return _superusersDb.RemoveSuper(authData);
        }

        private void BanNotifyDest(ChatUser user, BanReason reason, DateTime expiry, string channel, params ChatConnection[] dest)
        {
            BanNotifyDest(user, reason, (int)(expiry - DateTime.UtcNow).TotalSeconds, channel, dest);
        }

        private void BanNotifyDest(ChatUser user, BanReason reason, int period, string channel, params ChatConnection[] dest)
        {
            var msg = new ChatProtocol()
            {
                CommandType = CommandType.BanGranted,
                ClientInfo = user,
                BanReason = reason,
                ChannelName = channel,
                Body = period > 0 ? period.ToString() : "0",
                BodyMetadata = user.LastMeta
            };
            _server.SendTo(msg, dest);
        }

        private void BanNotify(ChatUser user, BanReason reason, DateTime expiry, string channel)
        {
            Log.Info($"Global notify. User '{user}' banned for {reason}. Ban expires at {expiry}. Channel {channel}.");
            var period = (int)(expiry - DateTime.UtcNow).TotalSeconds;
            if(period < 1)
                return;
            var dest = _users
                .Where(_ => _.Value.Channels.Intersect(user.Channels).Any())
                .Select(_ => _.Value.Connection)
                .Distinct()
                .ToArray();
            if (dest.Length == 0)
                return;

            BanNotifyDest(user, reason, period, channel, dest);
        }

        private void SaveHistory(ChatProtocol cmd)
        {
            var history = GetMessageHistory(cmd);
            history.Add(cmd);
            if (history.Count > _config.ChatChannelHistorySize)
                history.RemoveAt(0);
        }

        private string GetKey(ChatProtocol cmd)
        {
            if (cmd.SendTo != null)
            {
                var from = cmd.ClientInfo.Token;
                var to = cmd.SendTo;
                if (string.Compare(from, to, StringComparison.Ordinal) <= 0)
                {
                    return from + to;
                }
                return to + from;
            }

            return cmd.ChannelName;
        }

        private List<(ClientInfo from, ChatProtocol[] history)> GetPrivateMessagesHistory(string user)
        {
            var keys = _messageHistory.Keys.Where(_ => _.Contains(user)).ToArray();
            var result = new List<(ClientInfo from, ChatProtocol[] history)>();
            foreach (var key in keys)
            {
                string from;
                if (key.StartsWith(user))
                    from = key.Substring(user.Length);
                else
                    from = key.Replace(user, "");
                if (_users.TryGetValue(from, out var sendTo))
                {
                    result.Add((sendTo, _messageHistory[key].ToArray()));
                }
            }
            return result;
        }

        private List<ChatProtocol> GetMessageHistory(ChatProtocol cmd)
        {
            List<ChatProtocol> history;
            var key = GetKey(cmd);
            if (_messageHistory.TryGetValue(key, out history))
                return history;

            return _messageHistory[key] = new List<ChatProtocol>();
        }

        private IEnumerable<ChatUser> GetUsers(ChatProtocol cmd)
        {
            if (cmd.SendTo != null)
            {
                var token = cmd.ClientInfo.Token;
                if (_users.TryGetValue(cmd.ClientInfo.Token, out var chatUser))
                {
                    yield return chatUser;
                }

                if (_users.TryGetValue(cmd.SendTo, out chatUser))
                {
                    yield return chatUser;
                }
                else
                {
                    Log.Warn($"Can't send private message. User {token} not found");
                }
            }
            else
            {
                foreach (var chatUser in UsersOnChannel(cmd.ChannelName))
                {
                    yield return chatUser;
                }
            }
        }

        private IEnumerable<ChatUser> UsersOnChannel(string channel)
        {
            return _users.Values.Where((p) => p.Channels.Contains(channel));
        }

        private void LeaveAllChannels(ChatUser user)
        {
            foreach (var c in user.Channels.ToArray())
            {
                LeaveChannel(c, user);
            }
        }

        private void AssignToChannel(string channel, ChatUser user)
        {
            if(!_config.MultiChannel && user.Channels.Count > 0)
                LeaveAllChannels(user);

            Log.Info($"User {user} assigned to a channel '{channel}'");
            user.Channels.Add(channel);
            ChannelClientsChange(channel, user);
        }

        private void LeaveChannel(string channel, ChatUser user)
        {
            Log.Info($"User {user} leaves a channel '{channel}'");
            user.Channels.Remove(channel);
            ChannelClientsChange(channel, user);
            _server.SendTo(
            new ChatProtocol()
            {
                CommandType = CommandType.LeftChannel,
                ChannelName = channel
            }, user.Connection);
        }

        private void ChannelClientsChange(string channel, ChatUser user)
        {
            var conns = UsersOnChannel(channel).Select(u => u.Connection).ToArray();
            if(conns.Length < 1)
                return;
            _server.SendTo(new ChatProtocol()
            {
                CommandType = CommandType.ClientsChanged,
                ChannelName = channel
            }, conns);
        }

        private IChatServerTransport _server;
        private Thread _chatServerLoop;
    }
}
