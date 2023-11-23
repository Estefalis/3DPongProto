using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ThreeDeePongProto.Offline.AudioManagement
{
    public enum EAudiotrackControlOptions
    {
        None,
        Start,
        Pause,
        Stop,
        Restart
    }

    public enum EAudioType
    {
        NonDiegetic,    //2D - worldwide hearable AudioSources
        Diegetic,       //3D - AudioSources related to gameObject-positions in the world
        Custom          //For values between 0 - 1 to mix 2D with 3D.
    }

    public enum ESoundEmittingObjects
    {
        UiElements,
        Ball,
        Player
    }

    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioMixer m_masterAudioMixer;
        [SerializeField] private AudioMixerGroup m_bGMAudioMixer;
        [SerializeField] private AudioMixerGroup m_sFXAudioMixer;

        [Header("AudioSource Settings")]
        [SerializeField] private bool m_mute = false;
        [SerializeField] private bool m_bypassEffects = false, m_bypassListenerEffects = false, m_bypassReverbZones = false;
        [SerializeField] private bool m_playOnAwake = false, m_loop = false;
        [SerializeField, Range(0, 256)] private int m_priority = 128;
        [SerializeField, Range(0, 1)] private float m_volume = 1.0f;
        [SerializeField, Range(-3.0f, 3.0f)] private float m_pitch = 1.0f;
        [SerializeField, Range(-1.0f, 1.0f)] private float m_stereoPan = 0.0f;
        [SerializeField, Range(0, 1)] private float m_spatialBlend = 0.0f;
        [SerializeField, Range(0, 1.1f)] private float m_reverbZoneMix = 1.0f;

        [Header("3D Sound Settings")]
        [SerializeField, Range(0, 5)] private float m_dopplerLevel3D = 1.0f;
        [SerializeField, Range(0, 360)] private float m_spread3D = 0.0f;
        [SerializeField] private AudioRolloffMode m_audioRolloffMode3D = AudioRolloffMode.Logarithmic;
        [SerializeField, Min(0)] private float m_rollOffMinDistance, m_rollOffMaxDistance;
        [Space]
        [SerializeField] private List<AudioFiles> m_AudioFileLists;     //Expandable List for each desired AudioEffect-Type List.

        //Tasks: Audiotypes (key), Task (value). (Coroutines, IEnumerator)?
        //TODO: Connection Emitter -> EAudioType (AudioSource & AudioSource-Settings) -> AudioFiles
        //Reverb Zones implementieren?
        //private Dictionary<ESoundEmittingObjects, Dictionary<AudioSource, List<AudioFiles>>> m_sourceEmittsAudioDict = new();

        [SerializeField] private List<AudioSource> m_availableAudiosources = new();
        private static Queue<AudioSource> m_QueueRegisterAudioSource;
        private static Queue<AudioSource> m_QueueRemoveAudioSource;
        private static event Action<Queue<AudioSource>> m_ActionRegisterAudioSource;     //Action to savely register AudioSources at begin. (Queue, then add to List.)
        private static event Action<Queue<AudioSource>> m_ActionRemoveAudioSource;       //Action to savely remove AudioSources. (Queue, then add to List.)

        private EAudioType m_audioType;

        private void Awake()
        {
            m_ActionRegisterAudioSource += AddIncomingAudioSource;
            m_ActionRemoveAudioSource += RemoveAddedAudioSources;

            m_QueueRegisterAudioSource = new();
            m_QueueRemoveAudioSource = new();
        }

        private void OnDisable()
        {
            m_ActionRegisterAudioSource -= AddIncomingAudioSource;
            m_ActionRemoveAudioSource -= RemoveAddedAudioSources;
        }

        #region Add AudioSources
        public static void LetsRegisterAudioSources(AudioSource _audioSource)
        {
            //Add incoming AudioSources into a Queue to prevent exceptions. Sent by Objects with attached AudioSource.
            m_QueueRegisterAudioSource.Enqueue(_audioSource);
            //Invoke Event to add AudioSources to List one by one, so the AudioSourceList is visible in the Inspector.
            m_ActionRegisterAudioSource?.Invoke(m_QueueRegisterAudioSource);
        }

        private void AddIncomingAudioSource(Queue<AudioSource> _queue)
        {
            if (_queue.Count > 0)
            {
                AudioSource audioSource = _queue.Peek();
                m_availableAudiosources.Add(audioSource);
                _queue.Dequeue();
            }
        }
        #endregion

        #region Remove AudioSources
        public static void LetsRemoveAudioSources(AudioSource _audioSource)
        {
            m_QueueRemoveAudioSource.Enqueue(_audioSource);
            m_ActionRemoveAudioSource?.Invoke(m_QueueRemoveAudioSource);
        }

        private void RemoveAddedAudioSources(Queue<AudioSource> _queue)
        {
            AudioSource audioSource = _queue.Peek();

            if (m_availableAudiosources.Contains(audioSource))
            {
                m_availableAudiosources.Remove(audioSource);
                _queue.Dequeue();
            }
        }
        #endregion

        private void SetupNonDiegeticAudio(AudioSource _audioSource)
        {
            _audioSource.mute = m_mute;
            _audioSource.bypassEffects = m_bypassEffects;
            _audioSource.bypassListenerEffects = m_bypassListenerEffects;
            _audioSource.bypassReverbZones = m_bypassReverbZones;
            _audioSource.playOnAwake = m_playOnAwake;
            _audioSource.loop = m_loop;
            _audioSource.priority = m_priority;
            _audioSource.volume = m_volume;
            _audioSource.pitch = m_pitch;
            _audioSource.panStereo = m_stereoPan;
            _audioSource.spatialBlend = 0.0f;
            _audioSource.reverbZoneMix = m_reverbZoneMix;

            _audioSource.dopplerLevel = m_dopplerLevel3D;
            _audioSource.spread = m_spread3D;
            _audioSource.rolloffMode = m_audioRolloffMode3D;    //Default AudioRolloffMode.Logarithmic.

            ////AudioSource instead of void?
            //return _audioSource;
        }

        private void SetupDiegeticAudio(AudioSource _audioSource)
        {
            _audioSource.mute = m_mute;
            _audioSource.bypassEffects = m_bypassEffects;
            _audioSource.bypassListenerEffects = m_bypassListenerEffects;
            _audioSource.bypassReverbZones = m_bypassReverbZones;
            _audioSource.playOnAwake = m_playOnAwake;
            _audioSource.loop = m_loop;
            _audioSource.priority = m_priority;
            _audioSource.volume = m_volume;
            _audioSource.pitch = m_pitch;
            _audioSource.panStereo = m_stereoPan;
            _audioSource.spatialBlend = 1.0f;
            _audioSource.reverbZoneMix = m_reverbZoneMix;

            _audioSource.dopplerLevel = m_dopplerLevel3D;
            _audioSource.spread = m_spread3D;
            _audioSource.rolloffMode = m_audioRolloffMode3D;    //Default AudioRolloffMode.Logarithmic.

            ////AudioSource instead of void?
            //return _audioSource;
        }

        //private void SetupCustomAudio(AudioSource _audioSource)
        //{

        //}
    }
}