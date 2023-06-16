using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using ThreeDeePongProto.Player.Inputs;
using ThreeDeePongProto.Managers;

namespace ThreeDeePongProto.UI
{
    public class MenuOrganisation : MonoBehaviour
    {
        private PlayerInputActions m_menuActions;
        [SerializeField] private string m_loadWinScene;
        [SerializeField] private string m_loadMenuScene;
        [SerializeField] private EventSystem m_eventSystem;

        #region Select First Elements by using the EventSystem.
        // The stack of active (Transform-)elements for menu-navigation needs to have a Start-Transform to prevent an error. It gets set active in 'Awake()'.
        [Header("Select First Elements")]
        [SerializeField] private Transform m_firstElement;
        private Stack<Transform> m_activeElement = new Stack<Transform>();

        //Key-/Value-Pair component-arrays to set the selected GameObject for menu navigation with a dictionary.
        [SerializeField] private Transform[] m_keyTransform;
        [SerializeField] private GameObject[] m_valueGameObject;
        private Dictionary<Transform, GameObject> m_selectedElement = new Dictionary<Transform, GameObject>();
        #endregion

        #region Alpha-Buttons and SubPages.
        //I could not leave this undone. Only the clicked button shall be highlighted, the others should visibly stay inactive.
        [Header("Button Alpha-Values")]
        [SerializeField] private Button[] m_alphaButtons;
        [SerializeField, Range(0.1f, 0.9f)] private float m_reducedAlphaValue = 0.5f;
        [SerializeField, Range(0.5f, 1f)] private float m_maxAlphaValue = 1f;

        //Simply just to (de-)activate the corresponding Transforms for each Settings-Category.
        [SerializeField] private Transform[] m_subPageTransforms;
        #endregion

        /// <summary>
        /// PlayerMovement and UIControls need to be moved into 'Start()' and the PlayerInputActions of the UserInputManager into 'Awake()', to prevent Exceptions.
        /// </summary>
        private void Start()
        {
            m_menuActions = UserInputManager.m_playerInputActions;
            m_menuActions?.Enable();
            m_menuActions.PlayerActions.ToggleGameMenu.performed += EnableNavigation;
        }

        private void OnDisable()
        {
            m_menuActions?.Disable();
            m_menuActions.PlayerActions.ToggleGameMenu.performed -= EnableNavigation;
        }

        private void Awake()
        {
            SetUIElements();
        }

        private void EnableNavigation(InputAction.CallbackContext _callbackContext)
        {
            if (!m_firstElement.gameObject.activeInHierarchy && !GameManager.Instance.GameIsPaused)
            {
                m_firstElement.gameObject.SetActive(true);
            }
        }

        public void ResumeGame()
        {
            ResetPauseAndTimescale();
            //UserInputManager.ResetPauseAndTimescale();
            m_firstElement.gameObject.SetActive(false);
        }

        private void SetUIElements()
        {
            SetFirstElement(m_firstElement);

            for (int i = 0; i < m_keyTransform.Length; i++)
                m_selectedElement.Add(m_keyTransform[i], m_valueGameObject[i]);

            SetSelectedElement(m_firstElement);
        }

        private void ResetPauseAndTimescale()
        {
            Time.timeScale = 1f;
            GameManager.Instance.GameIsPaused = false;
        }

        public void RestartLevel()
        {
            UserInputManager.ResetPauseAndTimescale();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ReturnToMainScene()
        {
            UserInputManager.ResetPauseAndTimescale();

            if (m_loadMenuScene != null || m_loadMenuScene != string.Empty)
                if (GameManager.Instance.EGameConnectionModi == EGameModi.LocalPC)
                    SceneManager.LoadScene(m_loadMenuScene);
            //TODO: Also disconnect from Network in Online-Seasons.
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #region Methods to (de-)activate Menu-Transforms with a stack and to set the active Element in each UI-Window.
        public void NextElement(Transform _next)
        {
            Transform currentElement = m_activeElement.Peek();
            currentElement.gameObject.SetActive(false);

            m_activeElement.Push(_next);
            _next.gameObject.SetActive(true);

            SetSelectedElement(_next);
        }

        public void CloseToPreviousElement()
        {
            Transform currentElement = m_activeElement.Pop();
            currentElement.gameObject.SetActive(false);

            Transform previousElement = m_activeElement.Peek();
            previousElement.gameObject.SetActive(true);

            SetSelectedElement(previousElement);
        }

        /// <summary>
        /// The Transform gets used as the 'Key' to find the correct 'Value' in the dictionary.
        /// </summary>
        /// <param name="_activeTransform"></param>
        protected void SetSelectedElement(Transform _activeTransform)
        {
            GameObject selectElement = m_selectedElement[_activeTransform];
            m_eventSystem.SetSelectedGameObject(selectElement);
        }

        protected void SetFirstElement(Transform _firstElement)
        {
            m_activeElement.Push(_firstElement);
            if (SceneManager.GetActiveScene().name == m_loadMenuScene)
                _firstElement.gameObject.SetActive(true);
        }

        protected void ResetFirstElement()
        {
            m_activeElement.Clear();
            m_activeElement.Push(m_firstElement);
        }
        #endregion

        /// <summary>
        /// Each Button pressed sets the visibly activated/deactivated Button and enables/disables the corresponding Settings-SubPage.
        /// </summary>
        /// <param name="_sender"></param>
        public void SetButtonAlpha(Button _sender)
        {
            for (int i = 0; i < m_alphaButtons.Length; i++)
            {
                if (_sender == m_alphaButtons[i])
                {
                    m_subPageTransforms[i].gameObject.SetActive(true);
                    Color tempAlpha1 = m_alphaButtons[i].image.color;
                    tempAlpha1.a = m_maxAlphaValue;
                    m_alphaButtons[i].image.color = tempAlpha1;
                }
                else
                {
                    m_subPageTransforms[i].gameObject.SetActive(false);
                    Color tempAlpha05 = m_alphaButtons[i].image.color;
                    tempAlpha05.a = m_reducedAlphaValue;
                    m_alphaButtons[i].image.color = tempAlpha05;
                }
            }
        }
    }
}