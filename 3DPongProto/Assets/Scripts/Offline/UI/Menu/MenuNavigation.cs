using System;
using System.Collections.Generic;
using ThreeDeePongProto.Shared.InputActions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ThreeDeePongProto.Offline.UI.Menu
{
    public class MenuNavigation : MonoBehaviour
    {
        [SerializeField] internal MenuOrganisation m_menuOrganisation;

        #region Select First Elements by using the EventSystem.        
        [Header("Select First Elements")]
        //[SerializeField] internal Transform m_firstElement;   //moved to MenuOrganisation.cs!
        private Stack<Transform> m_activeElement = new();
        //private Transform m_lastSelectedTransform;            //moved to MenuOrganisation.cs!

        //Key-/Value-Pair component-arrays to set the selected GameObject for menu navigation with a dictionary.
        [SerializeField] internal Transform[] m_keyTransform;
        [SerializeField] private GameObject[] m_valueGameObject;    //TODO: Internal?
        private Dictionary<Transform, GameObject> m_selectedElement = new Dictionary<Transform, GameObject>();
        #endregion

        #region Alpha-Buttons and SubPages.
        //I could not leave this undone. Only the clicked button shall be dominant, the others should visibly stay in the back.
        [Header("Button Alpha-Values")]
        [SerializeField] private Button[] m_alphaButtons;
        [SerializeField, Range(0.1f, 0.9f)] private float m_reducedAlphaValue = 0.5f;
        [SerializeField, Range(0.5f, 1f)] private float m_maxAlphaValue = 1f;

        //Simply just to (de-)activate the corresponding Transforms for each Settings-Category.
        [SerializeField] private Transform[] m_subPageTransforms;
        #endregion

        private void Awake()
        {
            SetFirstStackElement(m_menuOrganisation.m_firstElement);
        }

        private void Start()
        {
            SetUIElements();
        }

        private void Update()
        {
            if (EventSystem.current.currentSelectedGameObject == null && InputManager.m_playerInputActions.UI.enabled)
            {
                SetEventSystemGameObject(m_menuOrganisation.m_lastSelectedTransform);
            }
        }

        private void SetUIElements()
        {
            for (int i = 0; i < m_keyTransform.Length; i++)
                m_selectedElement.Add(m_keyTransform[i], m_valueGameObject[i]);

            SetEventSystemGameObject(m_menuOrganisation.m_firstElement);
        }

        #region Methods to (de-)activate Menu-Transforms with a stack and to set the active Element in each UI-Window.
        public void NextElement(Transform _next)
        {
            Transform currentElement = m_activeElement.Peek();
            currentElement.gameObject.SetActive(false);

            m_activeElement.Push(_next);
            _next.gameObject.SetActive(true);

            SetEventSystemGameObject(_next);
        }

        public void CloseToPreviousElement()
        {
            Transform currentElement = m_activeElement.Pop();
            currentElement.gameObject.SetActive(false);

            Transform previousElement = m_activeElement.Peek();
            previousElement.gameObject.SetActive(true);

            SetEventSystemGameObject(previousElement);
        }

        /// <summary>
        /// The Transform gets used as the 'Key' to find the correct 'Value' in the dictionary.
        /// </summary>
        /// <param name="_activeTransform"></param>
        internal void SetEventSystemGameObject(Transform _activeTransform)
        {
            m_menuOrganisation.m_lastSelectedTransform = _activeTransform; //Update reselects first selected Button once, if none is selected.
            GameObject selectElement = m_selectedElement[_activeTransform];
            m_menuOrganisation.m_eventSystem.SetSelectedGameObject(selectElement);
        }

        /// <summary>
        /// 'm_activeElement' Stack requires a set element to start with, to prevent a null error.
        /// </summary>
        /// <param name="_firstElement"></param>
        protected void SetFirstStackElement(Transform _firstElement)
        {
            m_activeElement.Push(_firstElement);
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