using System.Collections;
using System.Threading.Tasks;
using ThreeDeePongProto.Shared.InputActions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ThreeDeePongProto.Offline.UI.Menu.AutoScrolling
{
    public class AutoScroll : MonoBehaviour
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
        private Vector2 m_mouseScrollValue, m_mousePosition, m_lastDirectionInput;

        private GameObject m_lastSelectedGameObject, m_fallbackGameObject;
        private IEnumerator iEAutoScrolling;

        //TODO: StackOption to keep track of current selected Objects and new Object to scroll to?

        private void OnEnable()
        {
            //iEAutoScrolling = AutoScrollObjects();
        }

        private void OnDisable()
        {
            m_playerInputActions.UI.Disable();
            m_playerInputActions.UI.Navigate.performed -= StoreLastNavigationInput;

            m_autoScrollingEnabled = false;
            //StopCoroutine(iEAutoScrolling);
        }

        private void Start()
        {
            m_playerInputActions = InputManager.m_PlayerInputActions;
            m_playerInputActions.UI.Enable();
            m_playerInputActions.UI.Navigate.performed += StoreLastNavigationInput;

            m_scrollViewController.m_scrollViewRect.scrollSensitivity = m_setScrollSensitivity;

            if (EventSystem.current.currentSelectedGameObject != null)
            {
                m_lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
                m_fallbackGameObject = m_lastSelectedGameObject;
            }

            if (m_scrollViewController.ContentChildrenSet & m_scrollViewController.ObjectNavigationSet)
            {
                m_autoScrollingEnabled = true;
                //StartCoroutine(iEAutoScrolling);
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
                    //Progress();
                    //LerpPosition();
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

        private async void UpdateCurrentObject()
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

                        switch (m_scrollViewController.m_contentChildAnchorPos.ContainsKey(m_lastSelectedGameObject))
                        {
                            case true:
                            {
                                m_selectedObjectInScrollView = true;
                                Task objectTransition = TransitionFromTo(m_lastSelectedGameObject, EventSystem.current.currentSelectedGameObject, m_transitionDuration);
                                await objectTransition;
                                objectTransition.Dispose();
                                break;
                            }
                            case false:
                            {
                                m_selectedObjectInScrollView = false;
                                break;
                            }
                        }

                        m_lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
                        m_fallbackGameObject = m_lastSelectedGameObject;
                    }
                    break;
                }
            }
        }

        private async Task TransitionFromTo(GameObject _oldGOFrom, GameObject _newGOTo, float _duration)   //MoveContentObjectYByAmount?
        {
            ResetVariables();
            await Task.Delay(0);

            if (m_scrollViewController.m_contentChildAnchorPos.ContainsKey(_newGOTo))
            {
                //m_positionFrom = m_scrollViewController.m_scrollViewContent.transform.localPosition;
                m_positionFrom = m_scrollViewController.m_contentChildAnchorPos[_oldGOFrom].anchoredPosition;
                m_positionTo = m_scrollViewController.m_contentChildAnchorPos[_newGOTo].anchoredPosition;
            }

            Debug.Log($"OldGO: {m_lastSelectedGameObject.name} - {m_positionFrom} | NewGO: {EventSystem.current.currentSelectedGameObject.name} - {m_positionTo} | DirInput: {m_lastDirectionInput}");
        }

        private void ResetVariables()
        {
            m_currentPosition = Vector2.zero;
            m_positionFrom = Vector2.zero;
            m_positionTo = Vector2.zero;

            m_duration = 0.0f;
            m_timeElapsed = 0.0f;
            m_progress = 0.0f;

            m_inProgress = false;
        }

        #region AutoScroll Coroutine
        private IEnumerator AutoScrollObjects()     //Equal to 'AutoScrollToNextGameObject' from AutoScrollView
        {
            //TODO: - Only start the process, when 'm_selectedObjectInScrollView = true'
            //if (m_selectedObjectInScrollView && m_lastSelectedGameObject != EventSystem.current.currentSelectedGameObject)
            //{
            //    ResetVariables();
            //    m_positionFrom = m_scrollViewController.m_contentChildAnchorPos[m_lastSelectedGameObject].anchoredPosition;
            //    m_positionTo = m_scrollViewController.m_contentChildAnchorPos[EventSystem.current.currentSelectedGameObject].anchoredPosition;
            //    m_inProgress = true;
            //    //TODO: Do IEnumerator stuff.
            //}

            //if (m_inProgress)
            //{
            //    switch (m_navigateDirection != Vector2.zero)
            //    {
            //        case true:
            //        {
            //            yield return m_currentPosition = Vector2.Lerp(m_positionFrom, m_positionTo, m_timeElapsed += Time.deltaTime / m_duration);
            //            break;
            //        }
            //        case false:
            //            break;
            //    } 
            //}
            yield return null;
        }
        #endregion

        private void StoreLastNavigationInput(InputAction.CallbackContext _callbackContext)
        {
            if (_callbackContext.ReadValue<Vector2>() != Vector2.zero)
            {
                m_lastDirectionInput = _callbackContext.ReadValue<Vector2>();
#if UNITY_EDITOR
                //Debug.Log(m_lastDirectionInput);
#endif
            }
        }
    }
}