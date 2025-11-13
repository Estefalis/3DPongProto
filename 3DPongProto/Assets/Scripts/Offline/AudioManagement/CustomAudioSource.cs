using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class CustomAudioSource
{
    public AudioSource AudioSource;
    public AudioMixerGroup OutputMixerGroup;
    public bool Mute, BypassEffects, BypassListenerEffects, BypassReverbZones;
    public bool PlayOnAwake, Loop;
    [SerializeField, Range(0, 256)] public int Priority;
    [SerializeField, Range(0, 1)] public float Volume, Pitch, StereoPan, SpatialBlend;
    [SerializeField, Range(0, 1.1f)] public float ReverbZoneMix;
    [SerializeField, Range(0, 5)] public float DopplerLevel3D;
    [SerializeField, Range(0, 360)] public float Spread3D;
    [SerializeField, Range(0, 2)] public AudioRolloffMode AudioRolloffMode3D;
    [SerializeField, Min(0)] public float RollOffMinDistance, RollOffMaxDistance;

    public CustomAudioSource(AudioSource _audioSource, AudioMixerGroup _outputAudioMixerGroup)
    {
        AudioSource = _audioSource;
        OutputMixerGroup = _outputAudioMixerGroup;
        Mute = _audioSource.mute;
        BypassEffects = _audioSource.bypassEffects;
        BypassListenerEffects = _audioSource.bypassListenerEffects;
        BypassReverbZones = _audioSource.bypassReverbZones;
        PlayOnAwake = _audioSource.playOnAwake;
        Loop = _audioSource.loop;
        Priority = _audioSource.priority;
        Volume = _audioSource.volume;
        Pitch = _audioSource.pitch;
        StereoPan = _audioSource.panStereo;
        SpatialBlend = _audioSource.spatialBlend;
        ReverbZoneMix = _audioSource.reverbZoneMix;
        DopplerLevel3D = _audioSource.dopplerLevel;
        Spread3D = _audioSource.spread;
        AudioRolloffMode3D = _audioSource.rolloffMode;
        RollOffMinDistance = _audioSource.minDistance;
        RollOffMaxDistance = _audioSource.maxDistance;
    }

    public CustomAudioSource(AudioSource _audioSource, AudioMixerGroup _outputAudioMixerGroup, bool _mute, bool _bypassEffects, bool _bypassListenerEffects, bool _bypassReverbZones, bool _playOnAwake, bool _loop, int _priority, float _volume, float _pitch, float _stereoPan, float _spatialBlend, float _reverbZoneMix, float _dopplerLevel3D, float _spread3D, AudioRolloffMode _audioRolloffMode3D, float _rollOffMinDistance, float _rollOffMaxDistance)
    {
        AudioSource = _audioSource;
        OutputMixerGroup = _outputAudioMixerGroup;
        Mute = _mute;
        BypassEffects = _bypassEffects;
        BypassListenerEffects = _bypassListenerEffects;
        BypassReverbZones = _bypassReverbZones;
        PlayOnAwake = _playOnAwake;
        Loop = _loop;
        Priority = _priority;
        Volume = _volume;
        Pitch = _pitch;
        StereoPan = _stereoPan;
        SpatialBlend = _spatialBlend;
        ReverbZoneMix = _reverbZoneMix;
        DopplerLevel3D = _dopplerLevel3D;
        Spread3D = _spread3D;
        AudioRolloffMode3D = _audioRolloffMode3D;
        RollOffMinDistance = _rollOffMinDistance;
        RollOffMaxDistance = _rollOffMaxDistance;
    }
}