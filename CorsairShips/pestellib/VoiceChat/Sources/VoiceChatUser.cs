using Byn.Awrtc;
using System;
using log4net;

public class VoiceChatUser
{
    public ConnectionId Id;
    public int GameId;
    public string Nick;
    public ChatRemoteClock Clock;

    public bool Muted
    {
        get
        {
            return _muted;
        }
        set
        {
            _muted = value;
            try
            {
                _call?.SetVolume(VoiceChat.DefaultVolume * (_muted ? 0 : _volume), Id);
            }
            catch (InvalidOperationException)
            {
                Log.Debug($"VOICECHAT {Id}:{Nick} is invalid connection.");
                Invalid = true;
            }
        }
    }

    public float Volume
    {
        get
        {
            return _volume;
        }
        set
        {
            _volume = value;
            try
            {
                _call?.SetVolume(VoiceChat.DefaultVolume * (_muted ? 0 : _volume), Id);
            }
            catch (InvalidOperationException)
            {
                Log.Debug($"VOICECHAT {Id}:{Nick} is invalid connection.");
                Invalid = true;
            }
        }
    }

    public bool Invalid { get; private set; }

    public bool Speaks {
        get { return CheckSpeakingAutoOff(); } set 
        {
            _lastMicInfo = DateTime.UtcNow;
            _speaking = value; 
        } }
    public int Buffered => _call?.GetBufferedAmount(Id, true) ?? 0;

    public VoiceChatUser(ICall call)
    {
        // ICall для каждого юзера будет тот же что и в VoiceChat
        // отличие между юзерами только в Id
        _call = call;
        _volume = 1;
    }

    public void SendMicData(float[] data)
    {
        var msg = VoiceChatProto.WriteMicData(data);
        try
        {
            _call?.Send(msg, true, Id);
        }
        catch (InvalidOperationException)
        {
            Log.Debug($"VOICECHAT {Id}:{Nick} is invalid connection.");
            Invalid = true;
        }
    }

    public void NotifySpeaking(bool speaking)
    {
        var msg = VoiceChatProto.WriteMicInfo(speaking);
        try
        {
            _call?.Send(msg, true, Id);
        }
        catch (InvalidOperationException)
        {
            Log.Debug($"VOICECHAT {Id}:{Nick} is invalid connection.");
            Invalid = true;
        }
    }

    public void Close()
    {
        if (_call != null)
        {
            _call = null;
        }
    }

    private bool CheckSpeakingAutoOff()
    {
        if (!_speaking)
            return false;
        if (DateTime.UtcNow - _lastMicInfo > VoiceChat.SpeakingAutoOffDelay)
        {
            _speaking = false;
            return false;
        }
        return _speaking;
    }

    private DateTime _lastMicInfo;
    private bool _speaking;
    private bool _muted;
    private ICall _call;
    private float _volume;

    private static readonly ILog Log = LogManager.GetLogger(typeof(VoiceChatUser));
}
