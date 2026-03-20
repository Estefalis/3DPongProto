using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace ThreeDeePongProto.Offline.UI.Menu
{
    [RequireComponent(typeof(MenuManager))]
    public class MenuNavigation : MonoBehaviour
    {
        private enum EButtonTransition
        {
            None,
            Alpha,
            Color
        }

        [SerializeField] internal MenuManager m_menuManager;

        #region Select First Elements by using the EventSystem.
        [Header("Select First Elements")]
        [SerializeField] internal Transform m_firstTransformElement; //To prevent confusion: DictKey, not dictValue.
        [Space]
        //Key-/Value-Pair component-arrays to set the selected GameObject for menu navigation with a dictionary.
        [SerializeField] internal Transform[] m_navigationKey;
        [SerializeField] internal GameObject[] m_navigationValue;

        internal readonly Stack<Transform> m_activeElement = new();
        private readonly Dictionary<Transform, GameObject> m_targetNavigationElement = new();
        #endregion

        #region Alpha-Buttons and SubPages.
        //I could not leave this undone. Only the clicked button shall be dominant, the others should visibly stay in the back.
        [Header("Button Transition")]
        [SerializeField] private EButtonTransition m_eButtonTransition = EButtonTransition.Color;
        [SerializeField, Range(0.1f, 0.9f)] private float m_reducedAlphaValue = 0.5f;
        [SerializeField, Range(0.5f, 1f)] private float m_maxAlphaValue = 1f;
        [Space]
        [SerializeField] private Button[] m_categoryButtons;
        //Simply just to (de-)activate the corresponding Transforms for each Settings-Category.
        [SerializeField] private Transform[] m_subPageTransforms;
        [SerializeField] internal TMP_Dropdown[] m_uIDropdowns;

        internal static GameObject m_lastSelectedGameObject;
        private TMP_InputField m_lastSelectedInputField = null;
        #endregion

        private void Awake()
        {
            if(m_menuManager == null)
                m_menuManager = GetComponent<MenuManager>();

            m_lastSelectedGameObject = null;
            m_targetNavigationElement.Clear();

            SetFirstStackElement(m_firstTransformElement);
            SetUIElements();    //Also sets the lastSelectedElement.
        }

        private void Update()
        {
            if (!Application.isFocused)
                return;

            UpdateLastSelectedObject();
        }

        #region Prepare Navigation-Stack and (de-)activate Menu-Transforms to navigate.
        /// <summary>
        /// 'm_activeElement' Stack requires a set element to start with, to prevent a null error.
        /// </summary>
        /// <param name="_firstElement"></param>
        private void SetFirstStackElement(Transform _firstElement)
        {
            m_activeElement.Push(_firstElement);
        }

        private void SetUIElements()
        {
            for (int navMenu = 0; navMenu < m_navigationKey.Length; navMenu++)
                m_targetNavigationElement.Add(m_navigationKey[navMenu], m_navigationValue[navMenu]);

            SetNavigationGameObject(m_firstTransformElement);
        }

        public void NextElement(Transform _next)
        {
            if (!Application.isFocused)
                return;

            Transform currentElement = m_activeElement.Peek();
            currentElement.gameObject.SetActive(false);

            m_activeElement.Push(_next);
            _next.gameObject.SetActive(true);

            SetNavigationGameObject(_next);
        }

        public void CloseToPreviousElement()
        {
            if (!Application.isFocused)
                return;

            Transform currentElement = m_activeElement.Pop();
            currentElement.gameObject.SetActive(false);

            if (m_activeElement.Count == 0)
                SetFirstStackElement(m_firstTransformElement);

            Transform previousElement = m_activeElement.Peek();
            previousElement.gameObject.SetActive(true);

            SetNavigationGameObject(previousElement);
        }

        /// <summary>
        /// Sets the lastSelected Transform and GameObject from the dictionary required for navigation in each new enabled Transform.
        /// </summary>
        /// <param name="_activeTransform"></param>
        internal void SetNavigationGameObject(Transform _activeTransform)
        {
            switch (_activeTransform == null)
            {
                case false:
                {
                    GameObject selectElement = m_targetNavigationElement[_activeTransform];
                    m_lastSelectedGameObject = selectElement;   //BEFORE active Transform switch, new selectElement replaces the old!
                    m_menuManager.m_eventSystem.SetSelectedGameObject(selectElement);
                    break;
                }
                case true:
                {
                    Debug.LogWarning("FocusTarget is null!");
                    return;
                }
            }
        }
        #endregion

        internal void NavigateToNextObject(Vector2 _navigationVector)
        {
            Selectable nextSelectable = null;

            if (_navigationVector.y != 0)
            {
                switch (_navigationVector.y < 0)
                {
                    case true:  //-1 Down
                    {
                        if (m_lastSelectedGameObject.TryGetComponent<Selectable>(out var currentSelectable))
                            nextSelectable = currentSelectable.FindSelectableOnDown();
                    }
                    break;
                    case false: //+1 Up
                    {
                        if (m_lastSelectedGameObject.TryGetComponent<Selectable>(out var currentSelectable))
                            nextSelectable = currentSelectable.FindSelectableOnUp();
                    }
                    break;
                }
            }
            else if (_navigationVector.x != 0)
            {
                switch (_navigationVector.x < 0)
                {
                    case true:  //-1 Left
                    {
                        if (m_lastSelectedGameObject.TryGetComponent<Selectable>(out var currentSelectable))
                            nextSelectable = currentSelectable.FindSelectableOnLeft();
                    }
                    break;
                    case false: //+1 Right
                    {
                        if (m_lastSelectedGameObject.TryGetComponent<Selectable>(out var currentSelectable))
                            nextSelectable = currentSelectable.FindSelectableOnRight();
                    }
                    break;
                }
            }

            if (nextSelectable != null && nextSelectable.gameObject.activeInHierarchy)
                m_menuManager.m_eventSystem.SetSelectedGameObject(nextSelectable.gameObject);
        }

        private void UpdateLastSelectedObject()
        {
            switch (m_menuManager.m_eventSystem.currentSelectedGameObject == null)
            {
                case false:
                {
                    //Moment, when the previous saved GO still differs from the 'up to date' selected Object from the eventSystem.
                    switch (m_lastSelectedGameObject != m_menuManager.m_eventSystem.currentSelectedGameObject)
                    {
                        case true:
                        {
                            //1st check for TMP Input Field before replacing the latest selected FallBack-GameObject.
                            if (m_lastSelectedGameObject != null)
                                InputFieldCheck(m_lastSelectedGameObject);

                            //Moment, when the previous saved GO is made equal to the selected Object from the eventSystem.
                            m_lastSelectedGameObject = m_menuManager.m_eventSystem.currentSelectedGameObject;
                            //CallbackContext-OnNavigationInput sets new selected GameObject!

                            //2nd has to be in the same frame, or it blinks!
                            if(m_lastSelectedGameObject.TryGetComponent(out Button button))
                                CategoryButtonTransition(button);
                            break;
                        }
                        case false:
                        {
                            //2nd check for TMP Input Field after replacing the latest selected FallBack-GameObject.
                            InputFieldCheck(m_lastSelectedGameObject);
                            break;
                        }
                    }
                    break;
                }
                case true:
                {
                    switch (m_lastSelectedGameObject == null)
                    {
                        case true:
                            break;
                        case false:
                            m_menuManager.m_eventSystem.SetSelectedGameObject(m_lastSelectedGameObject);
                            break;
                    }
                    break;
                }
            }
        }

        private void InputFieldCheck(GameObject _incomingGameObject)
        {
            //If '_incomingGameObject' is a TMP_InputField.
            if (_incomingGameObject.TryGetComponent<TMP_InputField>(out var inputField))
            {
                //true == newest G0 from InputFieldCheck() || false == previous saved GO from UpdateLastSelectedObject().
                switch (inputField.gameObject == m_menuManager.m_eventSystem.currentSelectedGameObject)
                {
                    case true:  //GO is the newest, current selected Object AND a TMP_InputField. InputField's eventData activates itself in EditActivation.cs.
                    {
                        if (m_lastSelectedInputField == null)
                        {
                            m_lastSelectedInputField = inputField;  //Save the latest selected TMP_InputField for later changes.
                            m_lastSelectedInputField.image.color = m_lastSelectedInputField.colors.selectedColor;
                        }
                        break;
                    }
                    case false: //GO is the TMP_InputField we just left. Deactivate the old InputField, before setting current IF.
                    {
                        if (m_lastSelectedInputField != null)
                        {
                            if(m_lastSelectedInputField.interactable)
                                {
                                    m_lastSelectedInputField.interactable = false;
                                    m_lastSelectedInputField.DeactivateInputField();
                                }
                                
                            inputField.image.color = inputField.colors.normalColor;
                            m_lastSelectedInputField = null;
                        }
                        break;
                    }
                }
            }
        }

        private void CategoryButtonTransition(Button _incomingButton)
        {
            if(m_categoryButtons.Contains(_incomingButton))         //Only act, if button is in array.
                CategorySwitch(_incomingButton);
        }

        /// <summary>
        /// Each Button pressed sets the visibly activated/deactivated Button and enables/disables the corresponding Settings-SubPage.
        /// </summary>
        /// <param name="_sender"></param>
        public void CategorySwitch(Button _sender)
        {
            for (int cb = 0; cb < m_categoryButtons.Length; cb++)
            {
                if (_sender == m_categoryButtons[cb])
                {
                    m_subPageTransforms[cb].gameObject.SetActive(true);
                    switch (m_eButtonTransition)
                    {
                        case EButtonTransition.Alpha:
                        {
                            Color maxAlpha = m_categoryButtons[cb].image.color;
                            maxAlpha.a = m_maxAlphaValue;
                            m_categoryButtons[cb].image.color = maxAlpha;
                            break;
                        }
                        case EButtonTransition.Color:
                        {
                            m_categoryButtons[cb].image.color = m_categoryButtons[cb].colors.selectedColor;
                            break;
                        }
                        default:
                            break;
                    }
                }
                else
                {
                    m_subPageTransforms[cb].gameObject.SetActive(false);
                    switch (m_eButtonTransition)
                    {
                        case EButtonTransition.Alpha:
                        {
                            Color reducedAlpha = m_categoryButtons[cb].image.color;
                            reducedAlpha.a = m_reducedAlphaValue;
                            m_categoryButtons[cb].image.color = reducedAlpha;
                            break;
                        }
                        case EButtonTransition.Color:
                        {
                            m_categoryButtons[cb].image.color = m_categoryButtons[cb].colors.normalColor;
                            break;
                        }
                        default:
                            break;
                    }
                }
            }
        }
    }
}