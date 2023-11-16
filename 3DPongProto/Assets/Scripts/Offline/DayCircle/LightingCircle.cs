using UnityEngine;

namespace ThreeDeePongProto.Shared.DayCircle
{
    [ExecuteAlways]
    public class LightingCircle : MonoBehaviour
    {
        [SerializeField] private Light m_directionalLight;
        [SerializeField] private LightingConditions m_lightingConditions;
        [SerializeField, Range(0, 24)] private float m_timeOfDay;
        [SerializeField, Min(0.1f)] private float m_adjustDayLength;    //Ein höherer Float-Wert bei 'm_adjustDayLength' verlängert den Tag-/Nacht-Wechsel und umgekehrt.

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
}