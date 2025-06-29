using ThreeDeePongProto.Shared.Managers;
using ThreeDeePongProto.Shared.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ThreeDeePongProto.Shared.Rebinding
{
    public class KeyRebinding : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private InputActionReference m_inputActionReference;
        [SerializeField, Range(0, 16)] private int m_selectedBindingIndex;
        //[SerializeField] private InputControlPath.HumanReadableStringOptions m_humanReadableStringOptions;
        [SerializeField] private InputBinding.DisplayStringOptions m_displayStringOptions;
        [Tooltip("System-wide Action, that does not belong to a specific player.")]
        [SerializeField] private bool m_isGlobalBinding = false;
        [SerializeField] private bool m_excludeMouse = true; //Exclude the Mouse on the Rebind-Process.

#if UNITY_EDITOR
        [Header("Binding-Informations - DON'T CHANGE ANYTHING HERE!")]
        [SerializeField] private InputBinding m_inputBinding;
#endif

        [Header("UI-Fields")]
        //[SerializeField] private TextMeshProUGUI m_actionTitle;
        [SerializeField] private Button m_resetButton;
        [SerializeField] private Image m_buttonImage;
        [SerializeField] private Button m_rebindButton;
        [SerializeField] private TextMeshProUGUI m_rebindButtonText;

        [Header("Player-Reference")]
        [Tooltip("If the targeted Action belongs to specific player, then isSystemBinding = false.")]
        [SerializeField] private PlayerInput m_targetPlayerInput;

        //private int m_bindingIndex;
        //private string m_actionName, m_controlScheme;

        private void Awake()
        {
            if (RebindManager.Instance == null)
            {
                Debug.LogError("No RebindManager found in the Scene!");
                m_rebindButton.interactable = false;
                m_resetButton.interactable = false;
                return;
            }

            m_rebindButton.onClick.AddListener(StartRebindingProcess);
            m_resetButton.onClick.AddListener(ResetThisRebinding);

            RebindManager.Instance.OnRebindComplete += UpdateBindingDisplay;
            RebindManager.Instance.OnRebindCanceled += UpdateBindingDisplay;

            ControlSettings.ResetPlayerViewRebinds += ResetThisRebinding;
        }

        private void OnEnable()
        {
            UpdateBindingDisplay();
        }

        private void OnDestroy()
        {
            m_rebindButton.onClick.RemoveListener(StartRebindingProcess);
            m_resetButton.onClick.RemoveListener(ResetThisRebinding);

            //Unsubscribe to prevent errors.
            if (RebindManager.Instance != null)
            {
                RebindManager.Instance.OnRebindComplete -= UpdateBindingDisplay;
                RebindManager.Instance.OnRebindCanceled -= UpdateBindingDisplay;
            }

            ControlSettings.ResetPlayerViewRebinds -= ResetThisRebinding;
        }

        private void OnValidate()
        {
            if (m_inputActionReference == null)
                return;

            if (m_inputActionReference != null && m_inputActionReference.action != null)
            {
                //Update the name of the gameObject in Unity's hierarchy for a better overview.
                gameObject.name = $"Rebind - {m_inputActionReference.action.name} ({m_selectedBindingIndex})";
                GetBindingInformation();
                UpdateBindingDisplay();
            }
        }

        /// <summary>
        /// Update shown Button-Text.
        /// </summary>
        public void UpdateBindingDisplay()
        {
            if (m_rebindButtonText != null)
            {
                InputAction action = GetAction();
                if (action == null)
                    return;

                if (action.name != string.Empty)
                {
                    string displayString = RebindManager.Instance.GetBindingDisplayString(
                            action.name,
                            m_selectedBindingIndex,
                            m_targetPlayerInput,
                            m_isGlobalBinding);

                    //Debug.Log($"ActionName sent: {action.name} | DisplayName received: {displayString}");
                    m_rebindButtonText.text = displayString;
                }

                //m_rebindButtonText.text = action.GetBindingDisplayString(m_selectedBindingIndex, m_displayStringOptions);
            }

            //if (m_buttonImage != null && m_buttonImage.gameObject.activeInHierarchy)
            //{
            //    Image buttonImage = m_buttonImage.GetComponent<Image>();
            //    buttonImage.sprite = RebindManagerSP.GetControllerIcons(m_controlScheme, RebindManagerSP.GetEffectiveBindingPath(m_actionName, m_bindingIndex));
            //    m_buttonImage.sprite = buttonImage.sprite;
            //}
        }

        private void GetBindingInformation()
        {
            if (m_inputActionReference.action == null)
                return;

            m_selectedBindingIndex = Mathf.Clamp(m_selectedBindingIndex, 0, m_inputActionReference.action.bindings.Count - 1);

            if (gameObject.activeInHierarchy)
            {
                if (m_inputActionReference.action.bindings.Count > m_selectedBindingIndex)   //prevents ArgumentOutOfRangeException.
                {
                    m_inputBinding = m_inputActionReference.action.bindings[m_selectedBindingIndex];
                    //m_bindingIndex = m_selectedBinding;
                    //m_controlScheme = m_inputActionReference.action.bindings[m_selectedBindingIndex].groups;
                }
            }
        }

        /// <summary>
        /// Call the central RebindManager, to start the Rebinding-Process.
        /// </summary>
        private void StartRebindingProcess()
        {
            if (m_inputActionReference == null || m_inputActionReference.action == null)
                return;

            InputAction action = GetAction();
            if (action == null)
                return;

            if (action.name == string.Empty)
                return;

            if (m_isGlobalBinding)
            {
                RebindManager.Instance.StartGlobalRebinding(action.name, m_selectedBindingIndex, m_rebindButtonText, m_excludeMouse);
            }
            else
            {
                if (m_targetPlayerInput == null)
                {
                    Debug.LogError("Player need a PlayerInput-Reference assigned for KeyRebinding!", this.gameObject);
                    return;
                }

                RebindManager.Instance.StartPlayerRebinding(m_targetPlayerInput, action.name, m_selectedBindingIndex, m_rebindButtonText, m_excludeMouse);
            }
        }

        /// <summary>
        /// Reset the button setup back to default value.
        /// </summary>
        private void ResetThisRebinding()
        {
            if (m_inputActionReference == null || m_inputActionReference.action == null)
                return;

            InputAction action = GetAction();
            if (action == null)
                return;

            if (action.name != string.Empty)
                return;

            RebindManager.Instance.ResetBinding(action.name, m_selectedBindingIndex, m_targetPlayerInput, m_isGlobalBinding);

            UpdateBindingDisplay();
        }

        /// <summary>
        /// Helper-method to either load a global or player-specific action-instance. Essential for UI-Display.
        /// </summary>
        private InputAction GetAction()
        {
            if (m_inputActionReference == null || m_inputActionReference.action == null)
                return null;

            string actionName = m_inputActionReference.action.name;

            if (m_isGlobalBinding)
            {
                if (RebindManager.Instance == null)
                    return null;

                return RebindManager.Instance.GetGlobalAction(actionName);
            }
            else
            {
                if (m_targetPlayerInput == null)
                    return null;

                return m_targetPlayerInput.actions.FindAction(actionName);
            }
        }
    }
}