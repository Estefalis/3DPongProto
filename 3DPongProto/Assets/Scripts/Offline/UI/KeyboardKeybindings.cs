using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ThreeDeePongProto.Shared.InputActions
{
    public class KeyboardKeybindings : MonoBehaviour
    {
        [SerializeField] private InputActionReference m_inputActionReference; //ScriptableObject.

        [Range(0f, 10), SerializeField] private int m_selectedBinding;
        [SerializeField] private InputBinding.DisplayStringOptions m_displayStringOptions;
        [SerializeField] private InputControlPath.HumanReadableStringOptions m_humanReadableStringOptions;
        [SerializeField] private bool m_excludeMouse = true; //Exclude the Mouse on the Rebind-Prozess.

        [Header("Binding-Informations - DON'T CHANGE ANYTHING HERE!")]
        [SerializeField] private InputBinding m_inputBinding;

        [Header("UI-Fields")]
        //[SerializeField] private TextMeshProUGUI m_actionTitle;
        [SerializeField] private Image m_buttonImage;
        [SerializeField] private Button m_rebindButton;
        [SerializeField] private TextMeshProUGUI m_rebindText;
        [SerializeField] private Button m_resetButton;

        private int m_bindingIndex;
        private string m_actionName, m_controlScheme;
        private Guid m_bindingId;

        private void OnEnable()
        {
            m_rebindButton.onClick.AddListener(() => ExecuteKeyRebind());
            m_resetButton.onClick.AddListener(() => ResetRebinding());

            InputManager.m_RebindComplete += UpdateRebindUI;
            InputManager.m_RebindCanceled += UpdateRebindUI;

            if (m_inputActionReference != null)
            {
                GetBindingInformation();
                InputManager.LoadKeyboardOverrides(m_actionName, m_bindingId);    //MUST be below 'GetBindingInformation()'! Else Exception!
                UpdateRebindUI();
            }
        }

        private void OnDisable()
        {
            InputManager.m_RebindComplete -= UpdateRebindUI;
            InputManager.m_RebindCanceled -= UpdateRebindUI;
        }

        private void OnValidate()
        {
            if (m_inputActionReference == null)
                return;

            GetBindingInformation();
            UpdateRebindUI();
        }

        private void GetBindingInformation()
        {
            if (m_inputActionReference.action != null)
                m_actionName = m_inputActionReference.action.name;

            m_selectedBinding = Mathf.Clamp(m_selectedBinding, 0, m_inputActionReference.action.bindings.Count - 1);

            if (gameObject.activeInHierarchy)
            {
                if (m_inputActionReference.action.bindings.Count > m_selectedBinding)   //prevents ArgumentOutOfRangeException.
                {
                    m_inputBinding = m_inputActionReference.action.bindings[m_selectedBinding];
                    m_bindingIndex = m_selectedBinding;
                    m_bindingId = m_inputActionReference.action.bindings[m_bindingIndex].id;
                    m_controlScheme = m_inputActionReference.action.bindings[m_bindingIndex].groups;
                }
            }
        }

        private void UpdateRebindUI()
        {
            if (m_rebindText != null)
            {
                if (Application.isPlaying)
                {
                    //m_rebindText.text = InputManager.GetBindingName(m_actionName, m_bindingIndex).ToUpper();
                    m_rebindText.text = InputControlPath.ToHumanReadableString(InputManager.GetBindingName(m_actionName, m_bindingIndex), m_humanReadableStringOptions).ToUpper();
                }
                else
                {
                    m_rebindText.text = InputManager.GetBindingName(m_actionName, m_bindingIndex).ToUpper();
                    //m_rebindText.text = InputControlPath.ToHumanReadableString(InputManager.GetBindingName(m_actionName, m_bindingIndex), options: InputControlPath.HumanReadableStringOptions.OmitDevice).ToUpper();
                }
            }

            if (m_buttonImage != null && m_buttonImage.gameObject.activeInHierarchy)
            {
                Image buttonImage = m_buttonImage.GetComponent<Image>();
                buttonImage.sprite = InputManager.GetControllerIcons(m_controlScheme, InputManager.GetEffectiveBindingPath(m_actionName, m_bindingIndex).ToUpper());
                m_buttonImage.sprite = buttonImage.sprite;
            }
        }

        /// <summary>
        /// Execute rebind on each UI-Button. 
        /// </summary>
        private void ExecuteKeyRebind()
        {
            InputManager.StartRebindProcess(m_actionName, m_bindingIndex, m_rebindText, m_controlScheme, m_excludeMouse, m_bindingId);
        }

        /// <summary>
        /// Reset keyRebinding by the corresponding UI-ResetButton.
        /// </summary>
        private void ResetRebinding()
        {
            InputManager.ResetRebinding(m_actionName, m_bindingIndex, m_controlScheme, m_bindingId);
            UpdateRebindUI();
        }
    }
}