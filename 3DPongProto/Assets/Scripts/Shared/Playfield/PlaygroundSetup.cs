using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaygroundSetup : MonoBehaviour
{
    [SerializeField] private GameObject m_playGround;
    [SerializeField] private Transform m_middleLine;

    private const float m_playGroundWidthScale = 0.1f, m_playGroundLengthScale = 0.1f, m_middleLineScale = 0.00025f;

    private MatchSettingsData m_matchData;  //Field-Dimensions

    public void InitializePlayground(float _fieldWidth, float _fieldLength)
    {
        
    }
}