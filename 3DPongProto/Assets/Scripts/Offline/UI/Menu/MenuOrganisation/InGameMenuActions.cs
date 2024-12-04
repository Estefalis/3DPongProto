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
        [SerializeField] internal MenuManager m_menuOrganisation;

        //[SerializeField] private Button m_hiddenFinishButton;

        ////MatchManager unpauses the Game. - PlayerController restarts Coroutines and Inputsystem.PlayerActions.
        //public static event Action CloseInGameMenu;
        ////MatchManager unpauses the Game.
        //public static event Action RestartGameLevel;
        ////MatchManager unpauses the Game.
        //public static event Action OnLoadMainScene;

        //public static event Action EndInfiniteMatch;

        private const string m_startMenuScene = "StartMenuScene";

        private void OnDisable()
        {
            m_menuActions.PlayerActions.Disable();
            //m_menuActions.PlayerActions.ToggleGameMenu.performed -= SetSelectedMenuButton;
        }

        /// <summary>
        /// PlayerController and UIControls need to be moved into 'Start()' and the PlayerInputActions of the InputManager into 'Awake()', to prevent Exceptions.
        /// </summary>
        private void Start()
        {
            m_menuActions = InputManager.m_PlayerInputActions;
            m_menuActions.PlayerActions.Enable();
            //m_menuActions.PlayerActions.ToggleGameMenu.performed += SetSelectedMenuButton;

            //if (m_hiddenFinishButton != null)
            //    InVisibleButton(m_menuOrganisation.GetMatchUIStates.InfiniteMatch); //m_matchUIStates get load in LoadSettingsValues > Awake().
        }

        //private void InVisibleButton(bool _infiniteMatch)
        //{
        //    switch (_infiniteMatch)
        //    {
        //        case true:
        //            m_hiddenFinishButton.gameObject.SetActive(true);
        //            break;
        //        case false:
        //            m_hiddenFinishButton.gameObject.SetActive(false);
        //            break;
        //    }
        //}

        //private void SetSelectedMenuButton(InputAction.CallbackContext _callbackContext)
        //{
        //    if (!m_menuOrganisation.FirstElement.gameObject.activeInHierarchy)
        //    {
        //        m_menuOrganisation.FirstElement.gameObject.SetActive(true);
        //        m_menuOrganisation.SetNavigationGameObject(m_menuOrganisation.FirstElement);
        //    }
        //}

        //public void ResumeGame()
        //{
        //    CloseInGameMenu?.Invoke();
        //    m_menuOrganisation.FirstElement.gameObject.SetActive(false);
        //    InputManager.ToggleActionMaps(InputManager.m_PlayerInputActions.PlayerActions);
        //}

        //public void RestartLevel()
        //{
        //    RestartGameLevel?.Invoke();
        //    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        //    InputManager.ToggleActionMaps(InputManager.m_PlayerInputActions.PlayerActions);
        //}

        //public void ReturnToMainScene()
        //{
        //    //Action to reset timescale inside the Matchmanager. And other possible settings on returning to the main menu scene.
        //    OnLoadMainScene?.Invoke();
        //    SceneManager.LoadScene(m_startMenuScene);
        //}

//        public void QuitGameIngame()
//        {
//#if UNITY_EDITOR
//            UnityEditor.EditorApplication.isPlaying = false;
//#else
//            Application.Quit();
//#endif
//        }

        //public void EndOfInfiniteMatch()
        //{
        //    if (m_menuOrganisation.GetMatchValues.TotalPointsTPOne > 0 || m_menuOrganisation.GetMatchValues.TotalPointsTPTwo > 0)
        //    {
        //        EndInfiniteMatch?.Invoke();
        //        m_menuOrganisation.m_keyTransform[0].gameObject.SetActive(false);
        //    }
        //}
    }
}