using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlSettings : MonoBehaviour
{
    [Header("Axis Inversion")]
    [SerializeField] private Toggle[] m_axisToggles;    //X = 0 - Movement, Y = 1 - Rotation.

    [Header("Axis Sensitivity")]
    [SerializeField] private Toggle[] m_customToggles;
    [SerializeField] private Slider[] m_sensitivitySliders;
    [SerializeField] private float m_adjustSliderStep = 0.05f;
    private Dictionary<Toggle, Slider> m_toggleSliderConnection = new Dictionary<Toggle, Slider>();

    [SerializeField] private TMP_Dropdown m_playerDropdown;

    private void Awake()
    {

    }

    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }

    public void LoadDefaultControlSettings()
    {

    }

    public void LowerSliderValue()
    {

    }

    public void IncreaseSliderValue()
    {

    }

    public void ReSetDefault()
    {

    }
}
