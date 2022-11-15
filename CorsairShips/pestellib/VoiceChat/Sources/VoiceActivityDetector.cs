#if false
using System;
using UnityEngine;

public class VoiceActivityDetector: MonoBehaviour
{
    public event Action OnVoiceDetected
    {
        add { vad.OnVoiceDetected += value; }
        remove { vad.OnVoiceDetected -= value; }
    }
    public bool VoiceDetected => vad.VoiceDetected;

    private void Start()
    {
        vad = new VoiceActivityDetectorRaw();
    }

    private void Update()
    {
        vad.Update();
    }

    private VoiceActivityDetectorRaw vad;
}
#endif
