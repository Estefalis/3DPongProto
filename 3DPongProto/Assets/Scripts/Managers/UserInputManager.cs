using System;
using ThreeDeePongProto.Player.Movement;
using ThreeDeePongProto.Menu.Actions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UserInputManager : MonoBehaviour
{
    //Reference to the PlayerAction-InputAsset.
    public static PlayerInputActions m_playerInputActions;

    //ActionEvent to switch between ActionMaps within the new InputActionAsset.
    public static event Action<InputActionMap> m_changeActiveActionMap;

    private void OnEnable()
    {
        if (m_playerInputActions == null)
        {
            m_playerInputActions = new PlayerInputActions();
        }
    }

    private void Start()
    {
        //int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        string sceneName = SceneManager.GetActiveScene().name;

        switch (sceneName)
        {
            case "StartMenuScene":
                ToggleActionMaps(m_playerInputActions.UI);
                break;
            case "LocalGameScene":
                ToggleActionMaps(m_playerInputActions.PlayerActions);
                break;
            case "LanGameScene":
                ToggleActionMaps(m_playerInputActions.PlayerActions);
                break;
            case "NetGameScene":
                ToggleActionMaps(m_playerInputActions.PlayerActions);
                break;
            case "WinScene":
                ToggleActionMaps(m_playerInputActions.UI);
                break;
            default:
                ToggleActionMaps(m_playerInputActions.UI);
                break;
        }
    }

    /// <summary>
    /// Methode zum Wechsel der ActionMaps. Solange die uebergebene ActionMap dieselbe ist, passiert nichts.
    /// </summary>
    /// <param name="_actionMap"></param>
    #region Action-Maps
    public static void ToggleActionMaps(InputActionMap _actionMap)
    {
        //If you try to change to the same ActionMap skip the rest.
        if (_actionMap.enabled)
            return;

        //else disable the current ActionMap to switch to the next.
        m_playerInputActions.Disable();
        m_changeActiveActionMap?.Invoke(_actionMap);
        _actionMap.Enable();
#if UNITY_EDITOR
        Debug.Log(_actionMap.name);
#endif
    }
    #endregion

    public static void ResetPauseAndTimescale()
    {
        Time.timeScale = 1f;
        GameManager.Instance.GameIsPaused = false;
    }
}