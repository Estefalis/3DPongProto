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
        [SerializeField] private Image m_buttonImage;
        [SerializeField] private Button m_rebindButton;
        [SerializeField] private TextMeshProUGUI m_rebindButtonText;
        [SerializeField] private Button m_resetButton;
        //[SerializeField] private GameObject m_rebindOverlay;      //If an extra Overlay is required.

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
            if (m_rebindButtonText != null && m_inputActionReference.action != null)
            {
                if (!m_inputActionReference.action.bindings[m_selectedBindingIndex].isComposite)
                {
                    InputAction action = GetAction();
                    if (action == null)
                        return;

                    m_rebindButtonText.text = action.GetBindingDisplayString(m_selectedBindingIndex, m_displayStringOptions);
                }
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
            InputAction action = GetAction();
            if (action == null)
                return;

            //m_rebindOverlay.SetActive(true);
            //TextMeshProUGUI statusText = m_rebindOverlay.GetComponentInChildren<TextMeshProUGUI>();

            if (m_isGlobalBinding)
            {
                RebindManager.Instance.StartGlobalRebinding(action, m_selectedBindingIndex, /*statusText, */ m_excludeMouse);
            }
            else
            {
                if (m_targetPlayerInput == null)
                {
                    Debug.LogError("Player need a PlayerInput-Reference assigned for KeyRebinding!", this.gameObject);
                    //m_rebindOverlay.SetActive(false);
                    return;
                }
                RebindManager.Instance.StartPlayerRebinding(m_targetPlayerInput, action, m_selectedBindingIndex, /*statusText, */ m_excludeMouse);
            }
        }

        /// <summary>
        /// Reset the button setup back to default value.
        /// </summary>
        private void ResetThisRebinding()
        {
            InputAction action = GetAction();
            if (action == null)
                return;

            action.RemoveBindingOverride(m_selectedBindingIndex);

            //Save the current state after reset.
            if (m_isGlobalBinding)
            {
                RebindManager.Instance.SaveGlobalBindings();
            }
            else
            {
                if (m_targetPlayerInput == null)
                    return;
                RebindManager.Instance.SavePlayerBindings(m_targetPlayerInput);
            }

            UpdateBindingDisplay();
        }

        /// <summary>
        /// Helper-method to either load a global or player-specific action-instance.
        /// </summary>
        private InputAction GetAction()
        {
            if (m_inputActionReference == null || m_inputActionReference.action == null)
                return null;

            if (m_isGlobalBinding)
            {
                return RebindManager.Instance.GetGlobalAction(m_inputActionReference.action.name);
            }
            else
            {
                if (m_targetPlayerInput == null)
                    return null;
                return m_targetPlayerInput.actions.FindAction(m_inputActionReference.action.name);
            }
        }
    }
}