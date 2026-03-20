using System;
using ThreeDeePongProto.Shared.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace ThreeDeePongProto.Offline.UI.Menu
{
    [RequireComponent(typeof(MenuNavigation))]
    [RequireComponent(typeof(MenuInput))]
    public class MenuManager : MonoBehaviour
    {        
        [SerializeField] internal MenuInput m_menuInput;
        [SerializeField] internal MenuNavigation m_menuNavigation;

        [SerializeField] internal EventSystem m_eventSystem;

        // internal static GameObject LastSelectedGameObject { get => MenuNavigation.m_lastSelectedGameObject; }

        #region Actions_and_Functions
        internal static event Action AEndInfiniteMatch;         //LocalMatchManager ends an InfiniteMatch.
        #endregion

        private void Awake()
        {
            if(m_menuInput == null)
                m_menuInput = GetComponent<MenuInput>();
            if(m_menuNavigation == null)
                m_menuNavigation = GetComponent<MenuNavigation>();

            //If MenuManager's firstElement is active (MainMenu), toggle UserInterface Map. Else (GameScene) PlayerActions.
            if (m_menuNavigation.m_firstTransformElement.gameObject.activeInHierarchy)
                UserInputManager.Instance.ToggleActionMaps(EInputActionMaps.UserInterface.ToString());
            else
                UserInputManager.Instance.ToggleActionMaps(EInputActionMaps.PlayerActions.ToString());
        }

        private void OnEnable()
        {
            UserInputManager.AChangeActiveActionMap += OnChangeActiveActionMap;
        }

        private void OnDisable()    //Copy content into 'OnDestroy()', if needed.
        {
            UserInputManager.AChangeActiveActionMap -= OnChangeActiveActionMap;
        }

        #region Subscriptions not related to the InputSystem
        /// <summary>
        /// Method to react on ActionMap changes triggered via static method 'ToggleActionMaps'.
        /// </summary>
        /// <param name="_actionMap"></param>
        private void OnChangeActiveActionMap(string _actionMap)
        {
            if (!Application.isFocused)
                return;

            var sceneIndex = SceneManager.GetActiveScene().buildIndex;
            if (sceneIndex == (int)ESceneNames.StartMenu)   //Or in other menuOnly Scenes.
                return;

            if (_actionMap == EInputActionMaps.UserInterface.ToString())
            {
                m_menuInput.OnOpenMenu();
            }
            else if (_actionMap == EInputActionMaps.PlayerActions.ToString())
            {
                m_menuInput.OnCloseMenu();
            }
        }
        #endregion

        #region Custom-Methods        
        internal int CurrentSceneIndex()
        {
            return SceneManager.GetActiveScene().buildIndex;
        }
        #endregion

        #region MenuButton_Methods
        public void ResumeTheGame()
        {
            //Toggle ActionMap-switch to PlayerActions.
            UserInputManager.Instance.ToggleActionMaps(EInputActionMaps.PlayerActions.ToString());
        }

        public void RestartGameScene()
        {
            var sceneIndex = SceneManager.GetActiveScene().buildIndex;
            UserInputManager.Instance.ToggleActionMaps(EInputActionMaps.PlayerActions.ToString());
            ReLoadScene(sceneIndex);
        }

        public void ReturnToMainMenu()
        {
            ReLoadScene((int)ESceneNames.StartMenu);
        }

        public void EndInfiniteMatch()
        {
            AEndInfiniteMatch?.Invoke();    //Sends request to end infinite Matches. Skips HighScoreBoard while noone gained a point.
        }

        /// <summary>
        /// Method to receive the buildIndex of the scene that shall be reloaded.
        /// </summary>
        /// <param name="_sceneIndex"></param>
        internal void ReLoadScene(int _sceneIndex)
        {
            if (_sceneIndex > 0) //Exclude BootScene with DDOL-Managers.
            {
                if (_sceneIndex < SceneManager.sceneCountInBuildSettings - 1)   //-1 marks last SceneIndex.
                    SceneManager.LoadScene(_sceneIndex);
            }
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        #endregion
    }
}