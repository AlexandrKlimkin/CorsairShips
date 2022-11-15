using System.Linq;

public static class VoiceActivityDetectorRaw
{
    public static bool DetectVoice(float[] data)
    {
        var framesCount = data.Length / samplesPerFrame;
        float avgEnergy = 0;
        float[] energy = new float[framesCount];
        for (var i = 0; i < framesCount; ++i)
        {
            var from = i * samplesPerFrame;
            var to = (i + 1) * samplesPerFrame;
            float sum = 0;
            for (var j = from; j < to; ++j)
            {
                sum += data[j] * data[j];
            }
            energy[i] = sum / samplesPerFrame;
            avgEnergy += energy[i];
        }
        avgEnergy /= framesCount;
        historyAverageEnergy = (historyAverageEnergy + avgEnergy) / 2;
        for (var i = 0; i < framesCount; ++i)
        {
            history[(frameNo + i) % bufferSamples] = energy[i] - historyAverageEnergy >= detectionThreshold;
        }
        frameNo += framesCount;
        var detected = history.Count(_ => _);
        if (frameNo < bufferSamples)
            return detected > 0;
        UnityEngine.Debug.Log($"");
        return detected > detectedFramesThreshold;
    }

    private static bool[] history = new bool[bufferSamples];
    private static int frameNo;
    private static float historyAverageEnergy;
    private const int bufferSamples = framesCountPerSec * 3;
    private const int framesCountPerSec = 100;
    private const int samplesPerFrame = MicrophoneRecorder.SamplesPerSecond / framesCountPerSec;
    private const float detectionThreshold = 0.003f;
    private const int detectedFramesThreshold = 2;
}