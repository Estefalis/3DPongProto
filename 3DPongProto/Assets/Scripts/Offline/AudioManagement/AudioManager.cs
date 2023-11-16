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
        Custom
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

        [SerializeField] private List<AudioFiles> m_AudioFileLists;     //Expandable List for each desired AudioEffect-Type List.

        //Tasks: Audiotypes (key), Task (value). (Coroutines, IEnumerator)?
        //TODO: Connection Emitter -> EAudioType (AudioSource & AudioSource-Settings) -> AudioFiles
        private Dictionary<ESoundEmittingObjects, Dictionary<AudioSource, List<AudioFiles>>> m_objectAudioConnection;

        [SerializeField] private List<AudioSource> m_availableAudiosources = new();
        private static Queue<AudioSource> m_QueueRegisterAudioSource;
        private static Queue<AudioSource> m_QueueRemoveAudioSource;
        private static event Action<Queue<AudioSource>> m_ActionRegisterAudioSource;     //Action to savely register AudioSources at begin. (Queue, then add to List.)
        private static event Action<Queue<AudioSource>> m_ActionRemoveAudioSource;       //Action to savely remove AudioSources. (Queue, then add to List.)

        private void Awake()
        {
            m_ActionRegisterAudioSource += AddIncomingAudioSource;
            m_ActionRemoveAudioSource += RemoveIncomingAudioSources;

            m_QueueRegisterAudioSource = new();
            m_QueueRemoveAudioSource = new();
        }

        private void OnDisable()
        {
            m_ActionRegisterAudioSource -= AddIncomingAudioSource;
            m_ActionRemoveAudioSource -= RemoveIncomingAudioSources;
        }

        //private void Start()
        //{
        //    SetupAudioSources(m_availableAudiosources);
        //}

        //private void SetupAudioSources(List<AudioSource> _audioSourceList)
        //{
        //    for (int i = 0; i < _audioSourceList.Count; i++)
        //    {
        //        switch(m_availableAudiosources[i].name)
        //        {
        //            case "Ball":
        //            {
        //                SetupSFXAudioSource(m_availableAudiosources[i]);
        //                break;
        //            }
        //        }
        //    }
        //}

        //private void SetupSFXAudioSource(AudioSource _audioSource)
        //{
        //    _audioSource = new AudioSource();
        //    _audioSource.outputAudioMixerGroup = m_sFXAudioMixer;
        //    _audioSource.loop = false;
        //}

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

        private void RemoveIncomingAudioSources(Queue<AudioSource> _queue)
        {
            AudioSource audioSource = _queue.Peek();

            if (m_availableAudiosources.Contains(audioSource))
            {
                m_availableAudiosources.Remove(audioSource);
            }
        }
        #endregion
    }
}