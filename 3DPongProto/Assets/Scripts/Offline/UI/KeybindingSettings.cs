using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ThreeDeePongProto.Shared.InputActions
{
    public class KeybindingSettings : MonoBehaviour
    {
        [SerializeField] private InputActionReference m_InputActionReference;   //Befindet sich auf dem ScriptableObject.
                                                                                //Exkludieren der Maus beim Rebind-Prozess.
        [SerializeField] private bool m_excludeMouse = true;

        //Einstellungen und DisplayOptionen im Inspector, die fuer den Rebind-Prozess erforderlich sind. Inklusive der UI-Elemente.
        [Range(0f, 10), SerializeField] private int m_selectedBinding;
        [SerializeField] private InputBinding.DisplayStringOptions m_displayStringOptions;

        [Header("Binding-Informations - DON'T CHANGE ANYTHING HERE!")]
        [SerializeField] private InputBinding m_inputBinding;

        [Header("UI-Fields")]
        [SerializeField] private EButtonControlScheme m_buttonControlScheme;
        //[SerializeField] private TextMeshProUGUI m_actionTitle;
        [SerializeField] private Button m_rebindButton;
        [SerializeField] private TextMeshProUGUI m_rebindText;
        [SerializeField] private Button m_resetButton;

        private int m_bindingIndex;
        private string m_actionName;

        /// <summary>
        /// Hinzufuegen von Listenern, die bei UI-Button-Klicks Methoden ausfuehren und einschreiben auf Actions des Rebind-Prozesses im InputManager, um das UI zu aktualisieren.
        /// </summary>
        private void OnEnable()
        {
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
        }

        /// <summary>
        /// Ausschreiben aus Actions des Rebind-Prozesses im InputManager.
        /// </summary>
        private void OnDisable()
        {
            InputManager.m_RebindComplete -= UpdateUI;
            InputManager.m_RebindCanceled -= UpdateUI;
        }

        private void OnValidate()
        {
            if (m_InputActionReference == null)
                return;

            GetBindingInfomation();
            UpdateUI();
        }

        /// <summary>
        /// Methode zur Aktualisierung der InputBindings und Indices der m_InputActionReference, sobald die UI-Elemente in der Hierarchie aktiv sind.
        /// </summary>
        private void GetBindingInfomation()
        {
            if (m_InputActionReference.action != null)
                m_actionName = m_InputActionReference.action.name;

            if (this.gameObject.activeInHierarchy)
            {
                if (m_InputActionReference.action.bindings.Count > m_selectedBinding)
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
                //Set the buttonText.
                if (Application.isPlaying)
                {
                    //m_rebindText.text = InputControlPath.ToHumanReadableString(m_InputActionReference.action.bindings[m_bindingIndex].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
                    m_rebindText.text = InputManager.GetBindingName(m_actionName, m_bindingIndex);
                }
                else
                {
                    m_rebindText.text = InputControlPath.ToHumanReadableString(m_InputActionReference.action.bindings[m_bindingIndex].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
                    //m_rebindText.text = m_InputActionReference.action.GetBindingDisplayString(m_bindingIndex);
                }
            }
        }

        /// <summary>
        /// Execute rebind on each UI-Button. 
        /// </summary>
        private void ExecuteKeyRebind()
        {
            InputManager.StartRebindProcess(m_actionName, m_bindingIndex, m_rebindText, m_excludeMouse, m_buttonControlScheme);
        }

        /// <summary>
        /// Reset keyRebinding by the corresponding UI-ResetButton.
        /// </summary>
        private void ResetRebinding()
        {
            InputManager.ResetRebinding(m_actionName, m_bindingIndex);
            UpdateUI();
        }
    } 
}