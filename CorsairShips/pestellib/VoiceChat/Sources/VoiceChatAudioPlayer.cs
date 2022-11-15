using System;
using System.Collections.Generic;
using UnityEngine;
using UnityDI;

public class VoiceChatAudioPlayer : MonoBehaviour
{

    void Start()
    {
        ContainerHolder.Container.BuildUp(this);
    }

    public void UserLeft(VoiceChatUser user)
    {
        if (_sources.TryGetValue(user.Id.id, out var source))
        {
            Destroy(source.AudioSource);
            _sources.Remove(user.Id.id);
        }
    }

    public void EnqueueAudio(VoiceChatUser from, float[] data)
    {
        if (from.Muted) return;
        if (!_sources.TryGetValue(from.Id.id, out var source))
        {
            var s = gameObject.AddComponent<AudioSource>();
            s.playOnAwake = true;
            var audioGroup = _audioMixerSettings.VoiceChatAudioMixerGroup;
            if (audioGroup != null)
                s.outputAudioMixerGroup = audioGroup;
            s.loop = true;
            _sources[from.Id.id] = source = new VoiceChatAudioSource(s);
        }
        source.Volume = from.Volume * VoiceChat.DefaultVolume * 5;
        source.EnqueueAudio(data);
    }

    private Dictionary<int, VoiceChatAudioSource> _sources = new Dictionary<int, VoiceChatAudioSource>();

    class VoiceChatAudioSource
    {
        public float Volume = 1f;

        public AudioSource AudioSource { get; private set; }
        public VoiceChatAudioSource(AudioSource source)
        {
            AudioSource = source;
            _lastAdd = DateTime.UtcNow;
            AudioSource.clip = AudioClip.Create("vcclip", MicrophoneRecorder.RecordBufferSize, 1, MicrophoneRecorder.SamplesPerSecond, true, ReadDataCallback);
            AudioSource.Play();
        }

        public void EnqueueAudio(float[] data)
        {
            _lastAdd = DateTime.UtcNow;
            for (var i = 0; i < data.Length; ++i)
            {
                data[i] *= Volume;
            }
            lock (_sync)
            {
                if (!_bufferFull && _newData == _data.Length)
                {
                    Debug.LogWarning("Buffer overrun. Equeued data will be lost.");
                    _bufferFull = true;
                    return;
                }

                var pos = 0;
                var sz = data.Length;
                var freeSpace = _data.Length - _newData;
                var toEnd = _data.Length - _writePos;
                freeSpace = freeSpace > toEnd ? toEnd : freeSpace;
                var toCopy = freeSpace > sz ? sz : freeSpace;
                Buffer.BlockCopy(data, pos * sizeof(float), _data, _writePos * sizeof(float), toCopy * sizeof(float));
                _writePos = (_writePos + toCopy) % _data.Length;
                pos += toCopy;
                _newData += toCopy;
                _buffered += toCopy;
                sz -= toCopy;

                freeSpace = _data.Length - _newData;
                toEnd = _data.Length - _writePos;
                freeSpace = freeSpace > toEnd ? toEnd : freeSpace;
                toCopy = freeSpace > sz ? sz : freeSpace;
                Buffer.BlockCopy(data, pos * sizeof(float), _data, _writePos * sizeof(float), toCopy * sizeof(float));
                _writePos = (_writePos + toCopy) % _data.Length;
                _newData += toCopy;
                _buffered += toCopy;
            }
        }

        protected void ReadDataCallback(float[] data)
        {
            var pos = 0;
            if (_buffered == 0 || (_buffered < _bufferSamples && DateTime.UtcNow - _lastAdd < _bufferSecs))
            {
                Array.Clear(data, 0, data.Length);
                return;
            }
            lock (_sync)
            {
                _bufferFull = data.Length == 0;
                var sz = data.Length;
                var availData = _data.Length - _readPos;
                availData = availData > _newData ? _newData : availData;
                var toCopy = availData > sz ? sz : availData;
                Buffer.BlockCopy(_data, _readPos * sizeof(float), data, pos * sizeof(float), toCopy * sizeof(float));
                _readPos = (_readPos + toCopy) % _data.Length;
                pos += toCopy;
                sz -= toCopy;
                _newData -= toCopy;

                availData = _data.Length - _readPos;
                availData = availData > _newData ? _newData : availData;
                toCopy = availData > sz ? sz : availData;
                Buffer.BlockCopy(_data, _readPos * sizeof(float), data, pos * sizeof(float), toCopy * sizeof(float));
                _readPos = (_readPos + toCopy) % _data.Length;
                pos += toCopy;
                sz -= toCopy;
                _newData -= toCopy;

                Array.Clear(data, pos, sz);
                if (pos < data.Length)
                {
                    _buffered = 0;
                    Debug.Log("VOICECHAT buffer drop");
                }
            }
        }

        private object _sync = new object();
        private bool _bufferFull;
        private int _readPos;
        private int _writePos;
        private int _newData;
        private float[] _data = new float[MicrophoneRecorder.RecordBufferSize];
        private int _buffered;
        private const int _bufferSamples = 4096;
        private static readonly TimeSpan _bufferSecs = TimeSpan.FromSeconds((double)_bufferSamples / MicrophoneRecorder.SamplesPerSecond);
        private DateTime _lastAdd;
    }

    [Dependency] private AudioMixerSettings _audioMixerSettings;
}
