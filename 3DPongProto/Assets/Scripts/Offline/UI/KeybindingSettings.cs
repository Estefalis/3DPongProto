using System;
using System.Collections.Generic;
using ThreeDeePongProto.Offline.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ThreeDeePongProto.Shared.InputActions
{
    public class KeybindingSettings : MonoBehaviour
    {
        [SerializeField] private InputActionReference m_InputActionReference; //ScriptableObject.

        [Range(0f, 10), SerializeField] private int m_selectedBinding;
        [SerializeField] private InputBinding.DisplayStringOptions m_displayStringOptions;
        [SerializeField] private bool m_excludeMouse = true; //Exclude the Mouse on the Rebind-Prozess.

        [Header("Binding-Informations - DON'T CHANGE ANYTHING HERE!")]
        [SerializeField] private InputBinding m_inputBinding;

        [Header("UI-Fields")]
        [SerializeField] private EButtonControlScheme m_buttonControlScheme;
        //[SerializeField] private TextMeshProUGUI m_actionTitle;
        [SerializeField] private Image m_buttonImage;
        [SerializeField] private Button m_rebindButton;
        [SerializeField] private TextMeshProUGUI m_rebindText;
        [SerializeField] private Button m_resetButton;

        private int m_bindingIndex;
        private string m_actionName;
        private Transform m_initialRebindTransform;

        private ControlSettings m_controlSettings;
        Dictionary<Transform, Image> m_rebindImages = new Dictionary<Transform, Image>();

        private void OnEnable()
        {
            m_rebindImages.Clear(); //Clears on another script start.
            m_rebindImages.Add(transform, m_buttonImage);

            m_rebindButton.onClick.AddListener(() => ExecuteKeyRebind());
            m_resetButton.onClick.AddListener(() => ResetRebinding());

            if (m_InputActionReference != null)
            {
                InputManager.LoadKeyBindingOverride(m_actionName);
                GetBindingInfomation();
                UpdateUI();
            }

            InputManager.m_RebindComplete += UpdateUI;
            InputManager.m_RebindCanceled += UpdateUI;
            InputManager.m_refreshRebindIcon += UpdateIcon;
        }

        private void OnDisable()
        {
            InputManager.m_RebindComplete -= UpdateUI;
            InputManager.m_RebindCanceled -= UpdateUI;
            InputManager.m_refreshRebindIcon -= UpdateIcon;
        }

        private void OnValidate()
        {
            if (m_InputActionReference == null)
                return;

            GetBindingInfomation();
            UpdateUI();
        }

        private void GetBindingInfomation()
        {
            if (m_InputActionReference.action != null)
                m_actionName = m_InputActionReference.action.name;

            m_selectedBinding = Mathf.Clamp(m_selectedBinding, 0, m_InputActionReference.action.bindings.Count);

            if (this.gameObject.activeInHierarchy)
            {
                if (m_InputActionReference.action.bindings.Count > m_selectedBinding)   //prevents ArgumentOutOfRangeException.
                {
                    m_inputBinding = m_InputActionReference.action.bindings[m_selectedBinding];
                    m_bindingIndex = m_selectedBinding;
                }
            }
        }

        private void UpdateUI()
        {
            if (m_rebindText != null)
            {
                if (Application.isPlaying)
                {
                    //m_rebindText.text = InputControlPath.ToHumanReadableString(m_InputActionReference.action.bindings[m_bindingIndex].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
                    m_rebindText.text = InputManager.GetBindingName(m_actionName, m_bindingIndex);
                    //m_rebindText.text = m_InputActionReference.action.GetBindingDisplayString(m_bindingIndex);
                }
                else
                {
                    m_rebindText.text = InputControlPath.ToHumanReadableString(m_InputActionReference.action.bindings[m_bindingIndex].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
                    //m_rebindText.text = InputManager.GetBindingName(m_actionName, m_bindingIndex);
                    //m_rebindText.text = m_InputActionReference.action.GetBindingDisplayString(m_bindingIndex);
                }
            }

            if (gameObject.activeInHierarchy && m_buttonImage != null)
            {
                UpdatePadSprite(m_InputActionReference.action.bindings[m_bindingIndex].effectivePath);
            }
        }

        private void UpdatePadSprite(string _effectivePath)
        {
            m_buttonImage.sprite = InputManager.GetControllerIcons(m_buttonControlScheme, _effectivePath);
        }

        private void UpdateIcon(string _effectivePath, Transform _startTransform)
        {
            if (transform == m_initialRebindTransform)
                UpdatePadSprite(_effectivePath);
        }

        /// <summary>
        /// Execute rebind on each UI-Button. 
        /// </summary>
        private void ExecuteKeyRebind()
        {
            InputManager.StartRebindProcess(m_actionName, m_bindingIndex, m_rebindText, m_excludeMouse, m_buttonControlScheme, transform);
            m_initialRebindTransform = transform;
        }

        /// <summary>
        /// Reset keyRebinding by the corresponding UI-ResetButton.
        /// </summary>
        private void ResetRebinding()
        {
            m_initialRebindTransform = null;
            InputManager.ResetRebinding(m_actionName, m_bindingIndex);
            UpdateUI();
        }
    }
}