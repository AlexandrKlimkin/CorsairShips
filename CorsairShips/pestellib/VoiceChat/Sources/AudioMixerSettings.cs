using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "AudioMixer settings")]
public class AudioMixerSettings : ScriptableObject
{
    public AudioMixerGroup DefaultAudioMixerGroup;
    public AudioMixerGroup VoiceChatAudioMixerGroup;
    public string DefaultGroupVolumeParamName;
    public string VoiceChatGroupVolumeParamName;
}
