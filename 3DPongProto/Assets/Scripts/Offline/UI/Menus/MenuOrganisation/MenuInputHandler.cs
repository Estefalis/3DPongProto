using ThreeDeePongProto.Shared.InputActions;
using ThreeDeePongProto.Shared.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ThreeDeePongProto.Offline.UI.Menu
{
    internal class MenuInputHandler : MonoBehaviour
    {
        private PlayerInputActions m_inputActions;

        private InputAction m_navigateAction;
        private InputAction m_submitAction;
        private InputAction m_cancelAction;

        [SerializeField] private EventSystem m_eventSystem;
        [SerializeField] private MenuManager m_menuManager;

        [Header("Navigation Settings")]
        [SerializeField] private float m_navigationRepeatDelay = 0.1f;
        [SerializeField] private float m_navigationInitialDelay = 0.2f;
        private float m_nextMoveTime = 0.0f;
        private bool m_isHoldingNavigation = false;

        private void Awake()
        {            
            InitialConfiguration();
            m_inputActions.UserInterface.CloseGameMenu.performed += OnCloseGameMenu;
        }

        private void OnDisable()
        {
            m_inputActions.UserInterface.CloseGameMenu.performed -= OnCloseGameMenu;
        }

        private void OnDestroy()
        {
            m_inputActions.UserInterface.CloseGameMenu.performed -= OnCloseGameMenu;
#if UNITY_EDITOR
            Debug.Log("Disabling Maps and InputActions onDestroy.");
#endif
            m_inputActions?.Disable();
        }

        private void Update()
        {
            //Only execute code, if UI-Actions are enabled.
            if (m_inputActions == null || !m_navigateAction.enabled || m_inputActions.PlayerActions.enabled)
                return;

            Vector2 navInput = m_navigateAction.ReadValue<Vector2>();
            bool submit = m_submitAction.WasPressedThisFrame();
            bool cancel = m_cancelAction.WasPressedThisFrame();

            //Execute manual Submit-Handler.
            if (submit && m_eventSystem.currentSelectedGameObject != null)
                ExecuteManualSubmit();
            //Execute manual Cancel-Actions.
            else if (cancel) //'else if' prevents other Cancel-Action(s) within the same Frame as Submit.
                ExecuteManualCancel();
            //Execute manual navigation only when Submit/Cancel was not pressed.
            else if (Time.unscaledTime >= m_nextMoveTime && (Mathf.Abs(navInput.x) > 0.5f || Mathf.Abs(navInput.y) > 0.5f))
                ExecuteManualNavigation(navInput);
            else if (Mathf.Abs(navInput.x) < 0.1f && Mathf.Abs(navInput.y) < 0.1f)
                //InitialDelayReset after navigation stopped. No reset for nextMoveTime needed, because Time.unscaledTime >= nextMoveTime.
                m_isHoldingNavigation = false;
        }

        private void InitialConfiguration()
        {
            m_inputActions = new PlayerInputActions();
            m_inputActions.Enable();

            if (m_inputActions == null)
            {
#if UNITY_EDITOR
                Debug.LogError("Error! Could not create a PlayerInputActions Instance!", this);
#endif
                enabled = false;
                return;
            }

            var uiActionMap = m_inputActions.UserInterface;

            m_navigateAction = uiActionMap.Navigate;    //or FindAction("Navigate")
            m_submitAction = uiActionMap.Submit;        //or FindAction("Submit")
            m_cancelAction = uiActionMap.Cancel;        //or FindAction("Cancel")

            if (m_navigateAction == null || m_submitAction == null || m_cancelAction == null)
            {
#if UNITY_EDITOR
                Debug.LogError("One or more UI-Actions could not be found!", this);
#endif
                enabled = false;
                return;
            }

            if (m_eventSystem == null)
                m_eventSystem = EventSystem.current;

            if (m_eventSystem == null)
            {
#if UNITY_EDITOR
                Debug.LogError("No EventSystem found!", this);
#endif
                enabled = false;
                return;
            }

            //DeactivateMenuInput();
        }

        private void ExecuteManualSubmit()
        {
            GameObject selectedObject = m_eventSystem.currentSelectedGameObject;
#if UNITY_EDITOR
            //Debug.Log($"Executing manual Submit to: {selectedObject.name}.");
#endif
            ExecuteEvents.Execute(selectedObject, new BaseEventData(m_eventSystem), ExecuteEvents.submitHandler);
            m_nextMoveTime = Time.unscaledTime + m_navigationInitialDelay;  //navigationDelay.
        }

        private void ExecuteManualCancel()
        {
#if UNITY_EDITOR
            Debug.Log("Manual Cancel detected.");
#endif
            //Try to get Cancel-Handler on the same Object.
            GameObject cancelHandlerObj = ExecuteEvents.GetEventHandler<ICancelHandler>(m_eventSystem.currentSelectedGameObject);

            if (cancelHandlerObj != null)
            {
#if UNITY_EDITOR
                Debug.Log($"Execute Cancel on Handler: {cancelHandlerObj.name}.");
#endif
                ExecuteEvents.Execute(cancelHandlerObj, new BaseEventData(m_eventSystem), ExecuteEvents.cancelHandler);
            }
            else if (m_menuManager != null) //Fallback to previous element.
            {
#if UNITY_EDITOR
                //Debug.Log("Going back to previous Element, because no Cancel-Handler is found.");
#endif
                m_menuManager.CloseToPreviousElement();
            }

            //Prevents immediate repetitions.
            m_nextMoveTime = Time.unscaledTime + m_navigationInitialDelay;
        }

        private void ExecuteManualNavigation(Vector2 _navInput)
        {
#if UNITY_EDITOR
            //Debug.Log($"Navigating NOW - Time: {Time.unscaledTime}, NextMoveTime was: {m_nextMoveTime}, Input: {_navInput}");
#endif

            GameObject currentSelected = m_eventSystem.currentSelectedGameObject;
            if (currentSelected != null)
            {
                if (currentSelected.TryGetComponent<Selectable>(out var currentSelectable))
                {
                    Selectable nextSelectable = null;
                    //Horizontal Navigation prioritized.
                    if (Mathf.Abs(_navInput.x) >= Mathf.Abs(_navInput.y))
                    {
                        nextSelectable = (_navInput.x > 0.5f) ? currentSelectable.FindSelectableOnRight() : ((_navInput.x < -0.5f) ? currentSelectable.FindSelectableOnLeft() : null);
                    }

                    //Vertical Navigation.
                    if (nextSelectable == null && Mathf.Abs(_navInput.y) > 0.5f)
                    {
                        nextSelectable = (_navInput.y > 0.5f) ? currentSelectable.FindSelectableOnUp() : currentSelectable.FindSelectableOnDown();
                    }
                    
                    if (nextSelectable != null && nextSelectable.gameObject.activeInHierarchy)  //There's a hidden button! O-HA!
                    {
#if UNITY_EDITOR
                        //Debug.Log($"Manual Navigation to: {nextSelectable.gameObject.name}.");
#endif
                        m_eventSystem.SetSelectedGameObject(nextSelectable.gameObject);
                        //Setting delay for next movement.
                        m_nextMoveTime = Time.unscaledTime + (m_isHoldingNavigation ? m_navigationRepeatDelay : m_navigationInitialDelay);
                        m_isHoldingNavigation = true; //Save button press.
                    }
                    else
                    {
                        m_isHoldingNavigation = false; //Button press released.
                    }
                }
                else
                {
                    m_isHoldingNavigation = false; //Current Object is no Selectable.
                }
            }
            //Try to get the default element from MenuManager, if nothing is selected.
            else if (Mathf.Abs(_navInput.x) < 0.1f && Mathf.Abs(_navInput.y) < 0.1f)
            {
                m_isHoldingNavigation = false;
            }
            else
            {
                m_isHoldingNavigation = false; //No selection or default element available.
            }
        }

        public void ActivateMenuInput()
        {
            if (m_inputActions == null)
                return;

            //Activate global UI-Map (for manual Logic and UI-Module)
            m_inputActions.UserInterface.Enable();

            foreach (PlayerInput pi in PlayerInput.all) //Get all active PlayerInput Instances.
            {
                var playerActionsMap = pi.actions.FindActionMap("PlayerActions");   //Find PlayerActions-Map of this Instance.
                var uiActionsMap = pi.actions.FindActionMap("UserInterface");
                if (playerActionsMap != null && playerActionsMap.enabled)
                {
                    uiActionsMap.Enable();
                    playerActionsMap.Disable();
#if UNITY_EDITOR
                    Debug.Log($"PlayerActions Map deaktiviert für Spieler-Objekt: {pi.gameObject.name}");
#endif
                }
            }

            //Reset Navigation Delay Timer.
            m_nextMoveTime = Time.unscaledTime + m_navigationInitialDelay;
            m_isHoldingNavigation = false;
        }

        public void DeactivateMenuInput()
        {
            if (m_inputActions == null)
                return;

            m_inputActions.UserInterface.Disable();

            //Activate PlayerActions-Map on all Player Instances.
            foreach (PlayerInput pi in PlayerInput.all)
            {
                var playerActionsMap = pi.actions.FindActionMap("PlayerActions");    //Find PlayerActions-Map of this Instance.
                var uiActionsMap = pi.actions.FindActionMap("UserInterface");
                if (playerActionsMap != null)
                {
                    uiActionsMap.Disable();
                    pi.SwitchCurrentActionMap("PlayerActions");
#if UNITY_EDITOR
                    Debug.Log($"PlayerActions Map wieder aktiviert für Spieler-Objekt: {pi.gameObject.name}");
#endif
                }
            }
        }

        private void OnCloseGameMenu(InputAction.CallbackContext _callbackContext)
        {
            if (NewUserInputManager.SetActionMap == EInputActionMaps.UserInterface.ToString())
                NewUserInputManager.ToggleActionMaps(EInputActionMaps.PlayerActions.ToString());
        }
    }
}