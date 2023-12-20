#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.InputSystem;

public class InGameCamTest : MonoBehaviour
{
    [SerializeField] private Camera cam1;
    [SerializeField] private Camera cam2;

    private bool m_onOffCams;

    private void Awake()
    {
        m_onOffCams = true;
        SwitchCams(m_onOffCams);
    }

    private void Update()
    {
        if (Keyboard.current.rightAltKey.wasPressedThisFrame)
        {
            m_onOffCams = !m_onOffCams;
            SwitchCams(m_onOffCams);
        }
    }

    private void SwitchCams(bool _onOffCams)
    {
        switch (_onOffCams)
        {
            case true:
            {
                cam1.gameObject.SetActive(true);
                cam2.gameObject.SetActive(false);
                break;
            }
            case false:
            {
                cam1.gameObject.SetActive(false);
                cam2.gameObject.SetActive(true);
                break;
            }
        }
    }
}
#endif