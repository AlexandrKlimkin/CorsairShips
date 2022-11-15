using UnityEngine;
using Byn.Awrtc;
using Byn.Awrtc.Unity;
using System.Collections.Generic;
using System;
using System.Linq;
using Byn.Awrtc.Native;
using log4net;
using PestelLib.Log.Sources.log4net;

public enum VoiceChatMessageType
{ 
    PlayerInfo,
    MicInfo,
    MicData,
    Sync
}

[MessagePack.MessagePackObject]
public class VoiceChatMessage
{
    [MessagePack.Key(0)]
    public VoiceChatMessageType Type;
    [MessagePack.Key(1)]
    public byte[] Data;
}

[MessagePack.MessagePackObject]
public class VoiceChatMessagePlayerInfo
{
    [MessagePack.Key(0)]
    public string Nick;
    [MessagePack.Key(1)]
    public int Id;
    [MessagePack.Key(2)]
    public float Volume;
}

[MessagePack.MessagePackObject]
public class VoiceChatMessageMicInfo
{
    [MessagePack.Key(0)]
    public bool Speaking;
}

[MessagePack.MessagePackObject]
public class VoiceChatMic2ByteData
{
    [MessagePack.Key(0)]
    public short[] Data;
}

[MessagePack.MessagePackObject]
public class VoiceChatSync
{
    [MessagePack.Key(0)]
    public long My;
    [MessagePack.Key(1)]
    public long Remote;
}

public static class VoiceChatProto
{
    public static string Serialize(VoiceChatMessage message)
    {
        var data = MessagePack.MessagePackSerializer.Serialize(message);
        return Convert.ToBase64String(data);
    }

    public static VoiceChatMessage Deserialize(string data)
    {
        var bytes = Convert.FromBase64String(data);
        return MessagePack.MessagePackSerializer.Deserialize<VoiceChatMessage>(bytes);
    }

    public static string WritePlayerInfoMessage(VoiceChatUser user)
    {
        var msg = new VoiceChatMessage()
        {
            Type = VoiceChatMessageType.PlayerInfo,
            Data = MessagePack.MessagePackSerializer.Serialize(
                new VoiceChatMessagePlayerInfo()
                {
                    Nick = user.Nick,
                    Id = user.GameId,
                    Volume = user.Volume
                })
        };
        return Serialize(msg);
    }

    public static VoiceChatMessagePlayerInfo ReadPlayerInfoMessage(VoiceChatMessage msg)
    {
        return MessagePack.MessagePackSerializer.Deserialize<VoiceChatMessagePlayerInfo>(msg.Data);
    }

    public static string WriteMicInfo(bool speaking)
    {
        var msg = new VoiceChatMessage()
        {
            Type = VoiceChatMessageType.MicInfo,
            Data = MessagePack.MessagePackSerializer.Serialize(
                new VoiceChatMessageMicInfo()
                {
                    Speaking = speaking
                })
        };
        return Serialize(msg);
    }

    public static VoiceChatMessageMicInfo ReadMicInfo(VoiceChatMessage msg)
    {
        return MessagePack.MessagePackSerializer.Deserialize<VoiceChatMessageMicInfo>(msg.Data);
    }

    public static string WriteMicData(float[] data)
    {
        var compresed = SimpleAudioCompression.Compress(data);
        var msg = new VoiceChatMessage
        {
            Type = VoiceChatMessageType.MicData,
            Data = MessagePack.MessagePackSerializer.Serialize(new VoiceChatMic2ByteData
            {
                Data = compresed
            })
        };
        SimpleAudioCompression.Push(compresed);
        return Serialize(msg);
    }

    public static void ReadMicData(VoiceChatMessage msg, float[] outData)
    {
        var compressed = MessagePack.MessagePackSerializer.Deserialize<VoiceChatMic2ByteData>(msg.Data);
        SimpleAudioCompression.Decompress(compressed.Data, outData);
    }

    public static string WriteSyncMessage(ChatRemoteClock clock)
    {
        var msg = new VoiceChatMessage()
        {
            Type = VoiceChatMessageType.Sync,
            Data = MessagePack.MessagePackSerializer.Serialize(new VoiceChatSync()
            {
                My = DateTime.UtcNow.Ticks,
                Remote = clock?.RemoteNow ?? DateTime.UtcNow.Ticks,
            })
        };
        return Serialize(msg);
    }

    public static VoiceChatSync ReadSyncMessage(VoiceChatMessage msg)
    {
        return MessagePack.MessagePackSerializer.Deserialize<VoiceChatSync>(msg.Data);
    }
}

public interface IMicrophoneRecorder
{
    void StartRecording();
    void StopRecording();
    // Возвращаемое значение может стать невалидным в следующем фрейме, поэтому его нужно скопировать если у него будет отложенное использование
    float[] NextFrame();

    float Gain { get; set; }
}

public partial class VoiceChat : IDisposable
{
    public static readonly TimeSpan SpeakingAutoOffDelay = TimeSpan.FromSeconds(3);
    public List<VoiceChatUser> Users = new List<VoiceChatUser>();

    public int UsersLimit;
    public VoiceChatUser Me;
    public const float DefaultVolume = 1f;
    public bool _useMic;
    public bool Disposed { get; private set; }

    public event Action<VoiceChatUser> OnNewUser = _ => { };
    public event Action<VoiceChatUser> OnUserRemove = _ => { };
    public bool Ignored { get { return _muteAll; } set { MutedAll(value); }  }
    /// <summary>
    /// Вкл/Выкл свой микрофон.
    /// </summary>
    public bool Muted 
    {
        set 
        {
            if (_microphoneRecorder != null)
                if (value)
                    _microphoneRecorder.StopRecording();
                else
                    _microphoneRecorder.StartRecording();
            _muted = value;
            _call?.SetMute(value);
            HandOrCall();
        }
        get {
            return _muted;
        }
    }

    // false - Пользователь отключил микрофон до входа в чат, он будет замьючен для других игроков, но сможет слышать.
    // Изменить этот флаг можно только при повторном подключении к чату RejoinChat.
    public bool MicEnabled => _useMic;

    public float MicVolume
    {
        get 
        {
            return _micVolume; 
        }
        set 
        {
            if (_microphoneRecorder != null)
                _microphoneRecorder.Gain = value;
            _micVolume = value;
        }
    }

    public float PlayersVolume 
    {
        get 
        {
            return _playersVolume;
        }
        set
        {
            _playersVolume = Mathf.Clamp(value, 0, 1);
            foreach (var u in Users)
            {
                u.Volume = _playersVolume;
            }
        }
    }

    public VoiceChat()
    {
    }

    public VoiceChat(IMicrophoneRecorder microphoneRecorder, VoiceChatAudioPlayer audioPlayer, bool vad)
    {
        _microphoneRecorder = microphoneRecorder;
        _audioPlayer = audioPlayer;
        _vad = vad;
    }

    /// <summary>
    /// Вкл/Выкл аудио от всех игроков.
    /// </summary>
    public void MutedAll(bool on)
    {
        _muteAll = on;
        foreach (var u in Users)
        {
            u.Muted = on;
        }
        HandOrCall();
    }

    public void FocusChange(bool hasFocus)
    {
        if (!hasFocus)
        {
            _call?.SetMute(true);
            foreach (var u in Users)
            {
                u.Muted = true;
            }
        }
        else
        {
            _call?.SetMute(_muted);
            foreach (var u in Users)
            {
                u.Muted = _muteAll;
            }
        }
    }

    public static bool HasAudioPermission()
    {
#if UNITY_ANDROID && UNITY_2018_3_OR_NEWER
        if (Application.platform == RuntimePlatform.Android)
        {
            return UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone);
        }
#endif
        //Assume true for all other platforms for now
        return true;
    }

    public static bool RequestAudioPermission()
    {
        Log.Debug("RequestAudioPermission");
#if UNITY_ANDROID && UNITY_2018_3_OR_NEWER
        if (!HasAudioPermission())
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
            var result = HasAudioPermission();
            return result;
        }
#endif
        return false;
    }

    public void RejoinChat(string nick = null, string channelName = null, bool? useMic = null)
    {
        JoinChat(nick ?? Me.Nick, channelName ?? _address, useMic ?? _useMic);
    }

    // playerId - обычно пишем сюда PhotonNetwork.player.ID
    public void JoinChat(string nick, string channelName, bool useMic = true, int playerId = 0)
    {
        if (_call != null)
        {
            Close();
        }
        Log.Debug($"VOICECHAT {nick} joining {channelName}.");
        _useMic = useMic;
        _address = channelName;
        Me = new VoiceChatUser(_call) { Nick = nick, GameId = playerId, Volume = _micVolume };
        if (CanCall)
            AwaitCall();
    }

    private bool CanCall => _address != null && (!_muted || !_muteAll);

    private void HandOrCall()
    {
        if (_call == null && CanCall)
            AwaitCall();
        else if (_call != null && _muted && _muteAll)
            Close();
    }

    private void AwaitCall()
    {
        if (_call != null)
        {
            Close();
        }

        NetworkConfig networkConfig = new NetworkConfig();
        NativeMediaConfig nativeMediaConfig = new NativeMediaConfig();
        nativeMediaConfig.AudioOptions.noise_suppression = true;
        nativeMediaConfig.AudioOptions.echo_cancellation = true;
        nativeMediaConfig.AudioOptions.auto_gain_control = true;
        nativeMediaConfig.Audio = _microphoneRecorder == null && _useMic;
        nativeMediaConfig.Video = false;
        networkConfig.IceServers.AddRange(iceServerDescs_);
        networkConfig.SignalingUrl = uSignalingUrl;
        networkConfig.IsConference = true;

        _call = UnityCallFactory.Instance.Create(networkConfig);
        _call.CallEvent += OnCallEvent;
        _call.Configure(nativeMediaConfig);
        _call.SetMute(_muted);
    }

    private void OnCallEvent(object src, CallEventArgs args)
    {
        ICall call = src as ICall;

        Log.DebugLog($"VOICECHAT {Me.Nick}@{_address} message {args.Type}.");
        if (args.Type == CallEventType.ConfigurationComplete)
        {
            call.Listen(_address);
        }
        else if (args.Type == CallEventType.CallAccepted)
        {
            var acceptedArgs = args as CallAcceptedEventArgs;
            Log.DebugLog($"VOICECHAT {Me.Nick}@{_address} {call.GetHashCode()}: Call from {acceptedArgs.ConnectionId.id}.");
            var existing = Users.FirstOrDefault(_ => _.Id == acceptedArgs.ConnectionId);
            if (existing != null)
            {
                Log.Error($"Already connected. {Me.Nick}@{_address} con_id={existing.Id.id}, nick={existing.Nick}.");
                return;
            }
            var user = new VoiceChatUser(call) { Id = acceptedArgs.ConnectionId };
            user.Muted = true;
            Users.Add(user);
            call.Send(VoiceChatProto.WritePlayerInfoMessage(Me));
        }
        else if (args.Type == CallEventType.CallEnded)
        {
            var endArgs = args as CallEndedEventArgs;
            Log.DebugLog($"VOICECHAT {Me.Nick}@{_address} {call.GetHashCode()} call ended {endArgs.ConnectionId.id}.");
            foreach (var r in Users.Where(_ => _.Id == endArgs.ConnectionId).ToArray())
            {
                Log.DebugLog($"VOICECHAT {Me.Nick}@{_address} quit {r.Id}:{r.Nick}.");
                OnUserRemove(r);
                _audioPlayer?.UserLeft(r);
                Users.Remove(r);
            }
        }
        else if (args.Type == CallEventType.Message)
        {
            MessageEventArgs msgArgs = args as MessageEventArgs;
            try
            {
                var msg = VoiceChatProto.Deserialize(msgArgs.Content);
                if (msg.Type == VoiceChatMessageType.PlayerInfo)
                {
                    var infoMsg = VoiceChatProto.ReadPlayerInfoMessage(msg);
                    VoiceChatUser user = Users.Find(_ => _.Id == msgArgs.ConnectionId);
                    if (user.Nick == infoMsg.Nick && user.GameId == infoMsg.Id)
                    {
                        return;
                    }
                    user.Nick = infoMsg.Nick;
                    user.GameId = infoMsg.Id;
                    user.Volume = _playersVolume * infoMsg.Volume;
                    user.Muted = _muteAll;
                    OnNewUser(user);
                    Log.DebugLog($"VOICECHAT {Me.Nick} NICK: {user.Id.id}: {user.Nick}.");
                }
                else if (msg.Type == VoiceChatMessageType.MicInfo)
                {
                }
                else if (msg.Type == VoiceChatMessageType.MicData)
                {
                    if (_audioPlayer != null)
                    {
                        var user = Users.FirstOrDefault(_ => _.Id == msgArgs.ConnectionId);
                        if (user != null)
                        {
                            user.Speaks = true;
                            VoiceChatProto.ReadMicData(msg, netAudioBuffer);
                            _audioPlayer.EnqueueAudio(user, netAudioBuffer);
                        }
                    }
                }
                else if (msg.Type == VoiceChatMessageType.Sync)
                {
                    var syncData = VoiceChatProto.ReadSyncMessage(msg);
                    var user = Users.FirstOrDefault(_ => _.Id == msgArgs.ConnectionId);
                    if (user.Clock == null)
                        user.Clock = new ChatRemoteClock(syncData.My);
                    else
                    {
                        user.Clock.UpdateDelta(syncData.My, syncData.Remote);
                        var latency = user.Clock.Latency;
                        Log.DebugLog($"VOICECHAT {user.Nick} ping {TimeSpan.FromTicks(latency).TotalMilliseconds}ms");
                    }
                }
                else
                {
                    Log.Warn($"VOICECHAT. unknown message " + msgArgs.Content);
                }
            }
            catch (Exception e)
            {
                Log.Error($"VOICECHAT. Error while reading message {msgArgs.Content}." + e);
            }
        }
        else if (args.Type == CallEventType.ConfigurationFailed ||
            args.Type == CallEventType.ConnectionFailed ||
            args.Type == CallEventType.ListeningFailed)
        {
            var errArgs = args as ErrorEventArgs;
            Log.Error(errArgs.Info.ErrorMessage);
        }
    }
    public void Update()
    {
        if (_call == null)
            return;
        if (DateTime.UtcNow - _lastPing > _pingInterval)
        {
            foreach (var u in Users)
            {
                var syncData = VoiceChatProto.WriteSyncMessage(u.Clock);
                _call.Send(syncData, true, u.Id);
            }
            _lastPing = DateTime.UtcNow;
        }
        if (DateTime.UtcNow - _lastVoiceDetection > VoiceDetectionDelay)
        {
            _lastVoiceDetection = DateTime.UtcNow;
            var remCount = Users.RemoveAll(_ => _.Invalid);
            if (remCount > 0)
                Log.DebugLog($"VOICECHAT {Me.Nick}@{_address} removed invalid connections: " + remCount);
            //TODO: плагин войсчата не дает доступ к данным с микрофона, непонятно как сделать vad
            //if (_voiceDetected)
            {
                foreach (var u in Users)
                {
                    u.NotifySpeaking(true);
                }
            }
        }
        var time = Time.realtimeSinceStartup;
        try
        {
            ReadMicrophone();
            _call.Update();
        }
        catch (InvalidOperationException e)
        {
            Log.Error("VOICECHAT invalid. " + e);
        }
        var updateTime = Time.realtimeSinceStartup - time;
        if (updateTime > 1)
            Log.Warn($"Call update took {updateTime:3}s.");
    }

    private void ReadMicrophone()
    {
        var frame = _microphoneRecorder?.NextFrame();
        if (frame == null)
            return;
        Log.DebugLog("VOICECHAT mic data");
        if (_vad && !VoiceActivityDetectorRaw.DetectVoice(frame))
            return;
        Log.DebugLog("VOICECHAT voice detected");
        foreach (var u in Users)
        {
            u.SendMicData(frame);
        }
    }

    private void Close()
    {
        if (_call != null)
        {
            var removedUsers = string.Join(",", Users.Select(_ => $"{_?.Id}:{_?.Nick}"));
            Log.DebugLog($"VOICECHAT {Me.Nick}@{_address} hang up. users={removedUsers}.");
            foreach (var u in Users)
            {
                u.Close();
            }
            Users.Clear();
            _call.CallEvent -= OnCallEvent;
            _call.Dispose();
            _call = null;
        }
    }

    public void Dispose()
    {
        Disposed = true;
        Close();
        _address = null;
    }

    private float[] netAudioBuffer = new float[MicrophoneRecorder.SamplesPerFrame];
    private ICall _call;
    private bool _muted;
    private bool _muteAll;
    private float _micVolume;
    private float _playersVolume;
    private DateTime _lastVoiceDetection;
    private IMicrophoneRecorder _microphoneRecorder;
    private VoiceChatAudioPlayer _audioPlayer;

    private static readonly TimeSpan VoiceDetectionDelay = TimeSpan.FromSeconds(5);
    private string uSignalingUrl = "ws://voip-signaling.planetcommander.ru:12776/conferenceapp";
    //private string uSecureSignalingUrl = "wss://voip-signaling.planetcommander.ru:12777/conferenceapp";
    private IceServer[] iceServerDescs_ = new[] {
        new IceServer("stun:voip-signaling.planetcommander.ru:3478"),
        new IceServer("turns:voip-signaling.planetcommander.ru:5349", "gdcompany", "nfTs6un9atSnrQF1wcB3E$Ss%&")
    };
    private bool _vad;
    private string _address;
    private DateTime _lastPing;
    private static readonly ILog Log = LogManager.GetLogger(typeof(VoiceChat));
    private static readonly TimeSpan _pingInterval = TimeSpan.FromSeconds(5);
}
