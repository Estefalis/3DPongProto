using System;
using ThreeDeePongProto.Shared.InputActions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThreeDeePongProto.Offline.UI.Menu
{
    public class InGameMenuActions : MonoBehaviour
    {
        private PlayerInputActions m_menuActions;
        [SerializeField] internal MenuOrganisation m_menuOrganisation;

        [SerializeField] private Button m_hiddenFinishButton;

        //MatchManager unpauses the Game. - PlayerController restarts Coroutines and Inputsystem.PlayerActions.
        public static event Action CloseInGameMenu;
        //MatchManager unpauses the Game.
        public static event Action RestartGameLevel;
        //MatchManager unpauses the Game.
        public static event Action OnLoadMainScene;

        public static event Action EndInfiniteMatch;

        private const string m_startMenuScene = "StartMenuScene";

        private void OnDisable()
        {
            m_menuActions?.Disable();
            m_menuActions.PlayerActions.ToggleGameMenu.performed -= EnableMenuNavigation;
        }

        /// <summary>
        /// PlayerController and UIControls need to be moved into 'Start()' and the PlayerInputActions of the InputManager into 'Awake()', to prevent Exceptions.
        /// </summary>
        private void Start()
        {
            m_menuActions = InputManager.m_playerInputActions;
            m_menuActions?.Enable();
            m_menuActions.PlayerActions.ToggleGameMenu.performed += EnableMenuNavigation;

            if (m_hiddenFinishButton != null)
                InVisibleButton(m_menuOrganisation.m_matchUIStates.InfiniteMatch); //m_matchUIStates get load in LoadSettingsValues > Awake().
        }

        private void InVisibleButton(bool _infiniteMatch)
        {
            switch (_infiniteMatch)
            {
                case true:
                    m_hiddenFinishButton.gameObject.SetActive(true);
                    break;
                case false:
                    m_hiddenFinishButton.gameObject.SetActive(false);
                    break;
            }
        }

        private void EnableMenuNavigation(InputAction.CallbackContext _callbackContext)
        {
            if (!m_menuOrganisation.m_firstElement.gameObject.activeInHierarchy)
            {
                m_menuOrganisation.m_firstElement.gameObject.SetActive(true);
                m_menuOrganisation.m_menuNavigation.SetEventSystemGameObject(m_menuOrganisation.m_firstElement);
            }
        }

        public void ResumeGame()
        {
            CloseInGameMenu?.Invoke();
            m_menuOrganisation.m_firstElement.gameObject.SetActive(false);
            InputManager.ToggleActionMaps(InputManager.m_playerInputActions.PlayerActions);
        }

        public void RestartLevel()
        {
            RestartGameLevel?.Invoke();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            InputManager.ToggleActionMaps(InputManager.m_playerInputActions.PlayerActions);
        }

        public void ReturnToMainScene()
        {
            //Action to reset timescale inside the Matchmanager. And other possible settings on returning to the main menu scene.
            OnLoadMainScene?.Invoke();
            SceneManager.LoadScene(m_startMenuScene);
        }

        public void QuitGameIngame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void EndOfInfiniteMatch()
        {
            if (m_menuOrganisation.m_matchValues.TotalPointsTPOne > 0 || m_menuOrganisation.m_matchValues.TotalPointsTPTwo > 0)
            {
                EndInfiniteMatch?.Invoke();
                m_menuOrganisation.m_menuNavigation.m_keyTransform[0].gameObject.SetActive(false);
            }
        }
    }
}