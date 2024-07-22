using System;
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

        private event Action<string, Image> m_updateTargetComponent;
        private string m_targetPath;
        private Transform m_initialTransform;
        private Image m_targetImage;

        private void OnEnable()
        {
            m_rebindButton.onClick.AddListener(() => ExecuteKeyRebind());
            m_resetButton.onClick.AddListener(() => ResetRebinding());

            if (m_InputActionReference != null)
            {
                GetBindingInfomation();
                InputManager.LoadKeyBindingOverride(m_actionName);  //MUST be below 'GetBindingInfomation()'! Else Exception!
                UpdateInvolvedTargets(m_InputActionReference.action.bindings[m_bindingIndex].effectivePath, m_buttonImage);
                UpdateUI(m_InputActionReference.action.bindings[m_bindingIndex].effectivePath, m_buttonImage);
            }

            InputManager.m_RebindComplete += UpdateUI;
            InputManager.m_RebindCanceled += UpdateUI;

            m_updateTargetComponent += UpdateInvolvedTargets;
        }

        private void OnDisable()
        {
            InputManager.m_RebindComplete -= UpdateUI;
            InputManager.m_RebindCanceled -= UpdateUI;

            m_updateTargetComponent -= UpdateInvolvedTargets;
        }

        //private void OnValidate()
        //{
        //    if (m_InputActionReference == null)
        //        return;

        //    GetBindingInfomation();
        //    UpdateUI();
        //}

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

        private void UpdateUI(string _effectivePath, Image _targetImage)
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
                UpdatePadSprite(_effectivePath, _targetImage);
            }
        }

        private void UpdatePadSprite(string _effectivePath, Image _targetImage)
        {
            if (m_buttonImage == _targetImage)
                m_buttonImage.sprite = InputManager.GetControllerIcons(m_buttonControlScheme, _effectivePath);
        }

        private void UpdateInvolvedTargets(string _effectivepath, Image _targetImage)
        {
#if UNITY_EDITOR
            //Debug.Log($"Path: {_effectivepath} - Image: {_targetImage}");
#endif
            m_targetPath = _effectivepath;
            m_targetImage = _targetImage;
        }

        /// <summary>
        /// Execute rebind on each UI-Button. 
        /// </summary>
        private void ExecuteKeyRebind()
        {
            m_updateTargetComponent?.Invoke(m_InputActionReference.action.bindings[m_bindingIndex].effectivePath, m_targetImage);
            InputManager.StartRebindProcess(m_actionName, m_bindingIndex, m_rebindText, m_excludeMouse, m_buttonControlScheme, m_targetImage);
        }

        /// <summary>
        /// Reset keyRebinding by the corresponding UI-ResetButton.
        /// </summary>
        private void ResetRebinding()
        {
            InputManager.ResetRebinding(m_actionName, m_bindingIndex);
            UpdateUI(m_InputActionReference.action.bindings[m_bindingIndex].effectivePath, m_buttonImage);
        }
    }
}