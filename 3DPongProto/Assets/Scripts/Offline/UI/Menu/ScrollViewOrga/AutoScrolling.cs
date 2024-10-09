using System.Collections;
using ThreeDeePongProto.Shared.InputActions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ThreeDeePongProto.Offline.UI.Menu.AutoScrolling
{
    public class AutoScrolling : MonoBehaviour
    {
        private PlayerInputActions m_playerInputActions;
        [SerializeField] internal ScrollViewController m_scrollViewController;

        [SerializeField] private float m_scrollSpeed = 60.0f;
        [SerializeField] private float m_setScrollSensitivity = 10.0f;

        #region AutoScroll Transition
        [Header("Transition")]
        [SerializeField] private float m_transitionDuration = 0.2f;
        private float m_duration = 0.0f;
        private float m_timeElapsed = 0.0f;
        private float m_progress = 0.0f;

        private bool m_inProgress = false;

        private Vector2 m_currentPosition;
        private Vector2 m_positionFrom;
        private Vector2 m_positionTo;
        #endregion

        private bool m_selectedObjectInScrollView = false, m_autoScrollingEnabled = false;
        private bool m_mouseIsInScrollView;
        private Vector2 m_mouseScrollValue, m_mousePosition;

        private GameObject m_lastSelectedGameObject, m_fallbackGameObject;
        private IEnumerator iEAutoScrolling;

        //TODO: StackOption to keep track of current selected Objects and new Object to scroll to?

        private void OnEnable()
        {
            iEAutoScrolling = AutoScrollObjects();
            ScrollViewController.ScrollPreparationsDone += EnableScrolling;
        }

        private void OnDisable()
        {
            m_playerInputActions.UI.Disable();

            m_autoScrollingEnabled = false;
            StopCoroutine(iEAutoScrolling);
            ScrollViewController.ScrollPreparationsDone -= EnableScrolling;
        }

        private void Start()
        {
            m_playerInputActions = InputManager.m_PlayerInputActions;
            m_playerInputActions.UI.Enable();

            m_scrollViewController.m_scrollViewRect.scrollSensitivity = m_setScrollSensitivity;

            if (EventSystem.current.currentSelectedGameObject != null)
            {
                m_lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
                m_fallbackGameObject = m_lastSelectedGameObject;
            }
        }

        private void Update()
        {
            GetMouseValues();
            UpdateCurrentObject();

            switch (m_autoScrollingEnabled && m_selectedObjectInScrollView)
            {
                case true:
                {
                    //Ticks();
                    //CalculatePosition();
                    break;
                }
                case false:
                    break;
            }
        }

        #region GetMouseValues
        private void GetMouseValues()
        {
            m_mouseScrollValue = m_playerInputActions.UI.ScrollWheel.ReadValue<Vector2>();
            m_mouseScrollValue.Normalize();
            m_mousePosition = m_playerInputActions.UI.MousePosition.ReadValue<Vector2>();

            switch (MouseIsInScrollView(m_mousePosition))
            {
                case true:
                {
                    m_mouseIsInScrollView = true;
                    break;
                }
                case false:
                {
                    m_mouseIsInScrollView = false;
                    break;
                }
            }
        }

        private bool MouseIsInScrollView(Vector2 _mousePosition)
        {
            //Thanks to CodeGPT!
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_scrollViewController.m_scrollViewRectTransform, _mousePosition, null, out Vector2 localMousePosition);

            Vector2 rectSize = m_scrollViewController.m_scrollViewRectTransform.rect.size;

            return localMousePosition.x >= -rectSize.x / 2 && localMousePosition.x <= rectSize.x / 2 &&
                localMousePosition.y >= -rectSize.y / 2 && localMousePosition.y <= rectSize.y / 2;
        }
        #endregion

        private void UpdateCurrentObject()
        {
            switch (m_lastSelectedGameObject == null)
            {
                case true:  //Dicts don't allow null on keys. And just 'return;' disables AutoScrolling.
                {
                    if (EventSystem.current.currentSelectedGameObject != null)
                    {
                        m_lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
                        m_fallbackGameObject = m_lastSelectedGameObject;
                    }
                    else
                        m_lastSelectedGameObject = m_fallbackGameObject;
                    break;
                }
                case false:
                {
                    if (m_lastSelectedGameObject != EventSystem.current.currentSelectedGameObject && EventSystem.current.currentSelectedGameObject != null)
                    {
                        m_lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
                        m_fallbackGameObject = m_lastSelectedGameObject;

                        switch (m_scrollViewController.m_contentChildAnchorPos.ContainsKey(m_lastSelectedGameObject))
                        {
                            case true:
                            {
                                m_selectedObjectInScrollView = true;
                                break;
                            }
                            case false:
                            {
                                m_selectedObjectInScrollView = false;
                                break;
                            }
                        }
                    }
                    break;
                }
            }
        }

        #region AutoScroll Coroutine
        private void EnableScrolling()
        {
            Debug.Log("Received command from Controller!");
            StartCoroutine(iEAutoScrolling);
            m_autoScrollingEnabled = true;
        }

        private IEnumerator AutoScrollObjects()     //Equal to 'AutoScrollToNextGameObject' from AutoScrollView
        {
            //TODO: Implement 'm_selectedObjectInScrollView' in IEnumerator.
            yield return null;
        }
        #endregion
    }
}