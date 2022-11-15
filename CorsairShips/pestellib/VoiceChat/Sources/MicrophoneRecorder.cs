using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public class MicrophoneRecorder : MonoBehaviour, IMicrophoneRecorder
{
    public const int SamplesPerSecond = 8000;
    public const int SamplesPerFrame = SamplesPerSecond / 10; // 100ms
    public const int ClipBufferSeconds = 5;
    public const int RecordBufferSize = ClipBufferSeconds * SamplesPerSecond;

    public float Gain { get { return _gain; } set { _gain = value; } }

    private void  Start()
    {
        _device = Microphone.devices[0];

        for (var i = 0; i < MaxFramesInQueue; ++i)
        {
            _freeFrames.Enqueue(new float[SamplesPerFrame]);
        }
    }

    public void StartRecording()
    {
        _clip = Microphone.Start(_device, true, ClipBufferSeconds, SamplesPerSecond);
        _readPos = 0;
    }
    public void StopRecording()
    {
        Microphone.End(_device);
        _clip = null;
    }

    // Возвращаемое значение может стать невалидным в следующем фрейме, поэтому его нужно скопировать если у него будет отложенное использование
    public float[] NextFrame()
    {
        if (_frames.Count == 0)
            return null;
        var result = _frames.Dequeue();
        _freeFrames.Enqueue(result);
        return result;
    }

    void ReadFrames()
    {
        var freeFrames = _freeFrames.Count();
        var framesInQueue = _frames.Count();
        Assert.IsTrue(framesInQueue <= MaxFramesInQueue);
        if (_noFreeFrames)
        {
            if (freeFrames > 0)
                _noFreeFrames = false;
            else
            {
                _readPos = Microphone.GetPosition(_device);
                return;
            }
        }
        if (freeFrames == 0)
        {
            Debug.LogError("Microphone recorded frames not consumed. Loosing all extra frames.");
            _noFreeFrames = true;
            _readPos = Microphone.GetPosition(_device);
            return;
        }
        var pos = Microphone.GetPosition(_device);
        int recordedSamples;
        // wrapped around
        if (pos < _readPos)
        {
            var fromBack = RecordBufferSize - _readPos;
            recordedSamples = fromBack + pos;
        }
        else
        {
            recordedSamples = pos - _readPos;
        }
        var framesReady = recordedSamples / SamplesPerFrame;
        if (framesReady == 0)
            return;

        framesReady = framesReady > freeFrames ? freeFrames : framesReady;
        for (var i = 0; i < framesReady; ++i)
        {
            var buff = _freeFrames.Dequeue();
            _clip.GetData(buff, _readPos);
            _readPos += SamplesPerFrame;
            if (_readPos >= RecordBufferSize)
                _readPos -= RecordBufferSize;
            for (var s = 0; s < buff.Length; ++s)
            {
                buff[s] = buff[s] * _gain;
            }
            _frames.Enqueue(buff);
        }
    }

    private void Update()
    {
        if (_clip == null)
            return;
        ReadFrames();
    }

    private const int MaxFramesInQueue = RecordBufferSize / SamplesPerFrame;
    private int _readPos;
    private Queue<float[]> _freeFrames = new Queue<float[]>();
    private bool _noFreeFrames;
    private Queue<float[]> _frames = new Queue<float[]>();
    private AudioClip _clip;
    private string _device;
    private float _gain = 1f;
}