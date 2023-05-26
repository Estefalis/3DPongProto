using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class LightingCircle : MonoBehaviour
{
    [SerializeField] Light m_directionalLight;
    [SerializeField] LightingConditions m_lightingConditions;
    [SerializeField, Range(0, 24)] float m_timeOfDay;
    [SerializeField] float m_adjustDayLength;

    private void OnValidate()
    {
        if (m_directionalLight != null)
            return;

        if (RenderSettings.sun != null)
            m_directionalLight = RenderSettings.sun;
        else
        {
            Light[] lights = GameObject.FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    m_directionalLight = light;
                    return;
                }
            }
        }
    }

    private void Update()
    {
        if (m_lightingConditions == null)
            return;

        if (Application.isPlaying)
        {
            //Ein höherer Float-Wert bei 'm_adjustDayLength' verlängert den Tag-/Nacht-Wechsel und umgekehrt.
            m_timeOfDay += Time.deltaTime / m_adjustDayLength;
            m_timeOfDay %= 24;              //Clamp zw. 0-24
            UpdateLighting(m_timeOfDay / 24f);
        }
        else
            UpdateLighting(m_timeOfDay / 24f);
    }

    private void UpdateLighting(float _timePercent)
    {
        RenderSettings.ambientLight = m_lightingConditions.m_ambientColor.Evaluate(_timePercent);
        RenderSettings.fogColor = m_lightingConditions.m_fogcolor.Evaluate(_timePercent);

        if (m_directionalLight != null)
        {
            m_directionalLight.color = m_lightingConditions.m_directionColor.Evaluate(_timePercent);
            m_directionalLight.transform.localRotation = Quaternion.Euler(new Vector3((_timePercent * 360f) - 90f, -90f, 170f));
        }
    }
}
