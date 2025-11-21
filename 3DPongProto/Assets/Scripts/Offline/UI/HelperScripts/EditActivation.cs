using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ThreeDeePongProto.Offline.UI.Menu
{
    [RequireComponent(typeof(TMP_InputField))]
    public class EditActivation : MonoBehaviour, IPointerClickHandler
    {
        private TMP_InputField m_ownInputField;
        private MenuManager m_menuManager;

        private void Awake()
        {
            m_ownInputField = GetComponent<TMP_InputField>();
            m_menuManager = FindObjectOfType<MenuManager>();
        }

        public void OnPointerClick(PointerEventData _eventData)
        {
            if (_eventData.button != PointerEventData.InputButton.Left)
                return;
            
            //Single-Click
            if (_eventData.clickCount == 1)
            {
                //MenuManager deactivates the InputField by an internal stored variable.
                if (EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(m_ownInputField.gameObject);
                }
            }
            //Double-Click
            else if (_eventData.clickCount >= 2)
            {
                m_ownInputField.interactable = true;
                m_ownInputField.ActivateInputField();

                if (m_menuManager != null)
                {
                    m_menuManager.ClickActivation(m_ownInputField.gameObject);
                }
            }
        }
    }
}