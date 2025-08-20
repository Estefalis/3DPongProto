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
        [SerializeField] private bool m_excludeMouse = true; //Exclude the Mouse on the Rebind-Process.

        [Header("Player Association")]
        [Tooltip("Determine, if the Action is system-wide, or if it belongs to a specific player.")]
        [SerializeField] private bool m_isGlobalBinding = false;

        [Header("Binding-Informations - DON'T CHANGE ANYTHING HERE!")]
        [SerializeField] private InputBinding m_inputBinding;

        [Tooltip("The player index (0-3) this binding belongs to. Only used if m_isGlobalBinding is false.")]
        [SerializeField, Range(0, 3)] private int m_playerIndex;

        [Header("UI-Fields")]
        //[SerializeField] private TextMeshProUGUI m_actionTitle;
        [SerializeField] private Button m_resetButton;
        [SerializeField] private Image m_buttonImage;
        [SerializeField] private Button m_rebindButton;
        [SerializeField] private TextMeshProUGUI m_rebindButtonText;

        //private int m_bindingIndex;
        //private string m_actionName, m_controlScheme;

        private void Awake()
        {
            if (RebindManager.Instance == null)
            {
                Debug.LogError("RebindManager not found in the scene! Disabling Rebind button.", this);
                m_rebindButton.interactable = false;
                if (m_resetButton != null)
                    m_resetButton.interactable = false;
                return;
            }

            m_rebindButton.onClick.AddListener(StartRebindingProcess);
            m_resetButton.onClick.AddListener(ResetThisRebinding);

            RebindManager.Instance.OnRebindComplete += UpdateBindingDisplay;
            RebindManager.Instance.OnRebindCanceled += UpdateBindingDisplay;

            //Example for a global reset event
            //ControlSettings.ResetPlayerViewRebinds += UpdateBindingDisplay;

            ControlSettings.ResetPlayerViewRebinds += ResetThisRebinding;
        }

        private void OnEnable()
        {
            GetBindingInformation();
            UpdateBindingDisplay(); //Ensure the display is correct when the object is enabled.
        }

        private void OnDisable()
        {
            m_rebindButton.onClick.RemoveListener(StartRebindingProcess);
            m_resetButton.onClick.RemoveListener(ResetThisRebinding);

            //Unsubscribe to prevent errors.
            if (RebindManager.Instance != null)
            {
                RebindManager.Instance.OnRebindComplete -= UpdateBindingDisplay;
                RebindManager.Instance.OnRebindCanceled -= UpdateBindingDisplay;
            }

            //Example for a global reset event
            //ControlSettings.ResetPlayerViewRebinds -= UpdateBindingDisplay;

            ControlSettings.ResetPlayerViewRebinds -= ResetThisRebinding;
        }

        //private void OnValidate()
        //{
        //    if (m_inputActionReference == null || m_inputActionReference.action == null)
        //        return;

        //    // Automatically name the GameObject for better hierarchy overview
        //    var actionName = m_inputActionReference.action.name;
        //    var binding = m_inputActionReference.action.bindings[m_selectedBindingIndex];
        //    gameObject.name = $"Rebind - {actionName} [{binding.groups}]";

        //    GetBindingInformation();
        //    UpdateBindingDisplay();
        //}

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
        /// Updates the button text to show the current binding.
        /// </summary>
        public void UpdateBindingDisplay()
        {
            if (m_rebindButtonText == null || m_inputActionReference == null)
                return;

            string actionName = m_inputActionReference.action.name;
            
            //The RebindManager is now the single source of truth for display strings.
            string displayString = RebindManager.Instance.GetBindingDisplayString(
                actionName,
                m_selectedBindingIndex,
                m_isGlobalBinding ? -1 : m_playerIndex //Pass -1 or another invalid index for global
            );

            m_rebindButtonText.text = displayString;

            //TODO: Add Logic for updating the gamepad icon image here.
            //if (m_buttonImage != null && m_buttonImage.gameObject.activeInHierarchy)
            //{
            //    Image buttonImage = m_buttonImage.GetComponent<Image>();
            //    buttonImage.sprite = RebindManagerSP.GetControllerIcons(m_controlScheme, RebindManagerSP.GetEffectiveBindingPath(m_actionName, m_bindingIndex));
            //    m_buttonImage.sprite = buttonImage.sprite;
            //}
        }

        /// <summary>
        /// Initiates the rebinding process by calling the central RebindManager.
        /// </summary>
        private void StartRebindingProcess()
        {
            if (m_inputActionReference == null || m_inputActionReference.action == null)
                return;

            string actionName = m_inputActionReference.action.name;

            //We pass all context to the manager and let it handle the logic.
            RebindManager.Instance.StartRebinding(
                actionName,
                m_selectedBindingIndex,
                m_rebindButtonText, //Pass the UI element for feedback text ("Press a key...").
                m_excludeMouse,
                m_isGlobalBinding ? -1 : m_playerIndex //Pass player index or -1 for global.
            );
        }

        /// <summary>
        /// Resets this specific binding to its default value via the RebindManager.
        /// </summary>
        private void ResetThisRebinding()
        {
            if (m_inputActionReference == null || m_inputActionReference.action == null)
                return;

            string actionName = m_inputActionReference.action.name;

            RebindManager.Instance.ResetSpecificBinding(
                actionName,
                m_selectedBindingIndex,
                m_isGlobalBinding ? -1 : m_playerIndex
            );

            //The event subscription will handle the UI update automatically, but calling it directly provides instant feedback.
            UpdateBindingDisplay();
        }
    }
}