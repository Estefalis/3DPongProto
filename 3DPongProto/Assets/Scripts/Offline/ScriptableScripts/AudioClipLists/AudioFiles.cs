using System;
using System.Collections.Generic;
using ThreeDeePongProto.Offline.AudioManagement;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableObjects/AudioClipLists", menuName = "AudioClipList")]
[Serializable] // ^ to AudioManager
public class AudioFiles : ScriptableObject
{
    public EAudioType AudioType => audioType;
    [SerializeField] private EAudioType audioType;
    public List<AudioClip> m_audioClipLists => m_clipList;
    [SerializeField] private List<AudioClip> m_clipList;
}