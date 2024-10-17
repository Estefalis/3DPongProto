using System;
using System.Collections;
using ThreeDeePongProto.Shared.InputActions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        private Vector2 m_mouseScrollValue, m_mousePosition;

        private GameObject m_lastSelectedGameObject, m_fallbackGameObject;

        //TODO: StackOption to keep track of current selected Objects and new Object to scroll to?

        private void OnDisable()
        {
            m_playerInputActions.UI.Disable();
            m_autoScrollingEnabled = false;
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

            if (m_scrollViewController.ContentChildrenSet & m_scrollViewController.ObjectNavigationSet)
                m_autoScrollingEnabled = true;
        }

        private void Update()
        {
            GetMouseValues();
            UpdateCurrentObject();

            TransitionProgress();
            CalculatePosition();

            if (m_inProgress)
                m_scrollViewController.m_scrollViewContent.localPosition = m_currentPosition;
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

                        switch (m_scrollViewController.m_contentChildAnchorPos.ContainsKey(EventSystem.current.currentSelectedGameObject))
                        {
                            case true:
                            {
                                //TODO: edgePosition & scrollSensitivity again
                                AutoScrollToNextGameObject(m_lastSelectedGameObject);
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

        #region AutoScroll to next GameObject
        /// <summary>
        /// AutoScrolls to the next element, if the ScrollView and it's content are not null and the next element is part of the ScrollView.
        /// </summary>
        /// <param name="_newGOTo"></param>
        /// <param name="_taskDelay"></param>
        private void AutoScrollToNextGameObject(GameObject _nextGameObject)
        {
            if (!m_autoScrollingEnabled || !m_selectedObjectInScrollView/* || Cursor.lockState == CursorLockMode.None*/)
            {
                if (m_scrollViewController.m_scrollViewRect.scrollSensitivity != m_setScrollSensitivity)
                    m_scrollViewController.m_scrollViewRect.scrollSensitivity = m_setScrollSensitivity;
                return;
            }

            float scrollViewTopBorderLocalY = GetLocalTopBorder(m_scrollViewController.m_scrollViewRectTransform.gameObject);
            float scrollViewBottomBorderLocalY = GetLocalBottomBorder(m_scrollViewController.m_scrollViewRectTransform.gameObject);
            
            //top
            float nextGOTopBorderY = GetRelativeTopBorder(_nextGameObject);
            float nextGOTopOffsetY = nextGOTopBorderY + scrollViewTopBorderLocalY;
            
            //bottom
            float nextGOBottomBorderY = GetRelativeBottomBorder(_nextGameObject);
            float nextGOBottomOffsetY = nextGOBottomBorderY - scrollViewBottomBorderLocalY;

            //topDifference
            float topDifference = nextGOTopOffsetY - scrollViewTopBorderLocalY;
            if (topDifference > 0)
            {
                MoveContentObjectYByAmount((topDifference) + m_scrollViewController.m_topPadding);
            }

            //bottomDifference
            float bottomDifference = nextGOBottomOffsetY - scrollViewBottomBorderLocalY;

            if (bottomDifference < 0)
            {
                MoveContentObjectYByAmount((bottomDifference) - m_scrollViewController.m_bottomPadding);
            }

            ////TODO: Implement Gamepad Mouse.
            //ScrollSelectNextGameObject();

            //switch (m_scrollViewController.m_scrollDirection)
            //{
            //    case ScrollDirection.Vertical:
            //        UpdateVerticalScrollPosition(m_scrollViewController.m_contentChildAnchorPos[m_lastSelectedGameObject]);
            //        break;
            //    case ScrollDirection.Horizontal:
            //        UpdateHorizontalScrollPosition(m_scrollViewController.m_contentChildAnchorPos[m_lastSelectedGameObject]);
            //        break;
            //    case ScrollDirection.Both:
            //        UpdateVerticalScrollPosition(m_scrollViewController.m_contentChildAnchorPos[m_lastSelectedGameObject]);
            //        UpdateHorizontalScrollPosition(m_scrollViewController.m_contentChildAnchorPos[m_lastSelectedGameObject]);
            //        break;
            //    case ScrollDirection.None:
            //    default:
            //        break;
            //}
        }

        private float GetRelativeTopBorder(GameObject _object)
        {
            float contentY = m_scrollViewController.m_scrollViewContent.transform.localPosition.y;
            float elementTopBorderLocalY = GetLocalTopBorder(_object);
            float elementTopBorderRelativeY = elementTopBorderLocalY + contentY;
            
            return elementTopBorderRelativeY;
        }

        private float GetLocalTopBorder(GameObject _object)
        {
            Vector2 localPos = _object.transform.localPosition;

            return localPos.y;
        }

        private float GetRelativeBottomBorder(GameObject _object)
        {
            float contentY = m_scrollViewController.m_scrollViewContent.transform.localPosition.y;
            float elementBottomBorderLocalY = GetLocalBottomBorder(_object);
            float elementBottomBorderRelativeY = elementBottomBorderLocalY + contentY;

            return elementBottomBorderRelativeY;
        }

        private float GetLocalBottomBorder(GameObject _object)
        {
            Vector2 rectSize = m_scrollViewController.m_scrollViewRectTransform.rect.size;
            Vector2 localPos = _object.transform.localPosition;
            localPos.y -= rectSize.y;

            return localPos.y;
        }

        //        private void ScrollSelectNextGameObject()
        //        {
        //            if (m_mouseScrollValue.y != 0 && m_mouseIsInScrollView)
        //            {
        //                switch (m_scrollViewController.m_scrollDirection)
        //                {
        //                    case ScrollDirection.Vertical:
        //                    {
        //                        switch (m_mouseScrollValue.y > 0)
        //                        {
        //                            case true:
        //                            {
        //                                MoveToNextObject(m_scrollViewController.m_objectNavigation[m_lastSelectedGameObject].selectOnUp);
        //                                break;
        //                            }
        //                            case false:
        //                            {
        //                                MoveToNextObject(m_scrollViewController.m_objectNavigation[m_lastSelectedGameObject].selectOnDown);
        //                                break;
        //                            }
        //                        }

        //                        break;
        //                    }
        //                    case ScrollDirection.Horizontal:
        //                    {
        //                        switch (m_mouseScrollValue.y > 0)
        //                        {
        //                            case true:
        //                            {
        //                                MoveToNextObject(m_scrollViewController.m_objectNavigation[m_lastSelectedGameObject].selectOnLeft);
        //                                break;
        //                            }
        //                            case false:
        //                            {
        //                                MoveToNextObject(m_scrollViewController.m_objectNavigation[m_lastSelectedGameObject].selectOnRight);
        //                                break;
        //                            }
        //                        }

        //                        break;
        //                    }
        //                    case ScrollDirection.Both:
        //                    {
        //                        //TODO: MoveToNextObject on DetectedScrollOption.Both.
        //                        break;
        //                    }
        //                    case ScrollDirection.None:
        //                    default:
        //                        break;
        //                }
        //            }
        //        }

        //        private void MoveToNextObject(Selectable _nextObject)
        //        {
        //            switch (_nextObject == null)
        //            {
        //                case true:
        //                    return;
        //                case false:
        //                    EventSystem.current.SetSelectedGameObject(_nextObject.gameObject);
        //                    break;
        //            }
        //        }

        //        private void UpdateVerticalScrollPosition(RectTransform _selectedElement)
        //        {
        //            //Move the current scroll rect to correct elementPosition           //min: -57 - max: 0
        //            float elementPosition = -_selectedElement.anchoredPosition.y - (_selectedElement.rect.height * (1 - _selectedElement.pivot.y) - m_scrollViewController.m_topPadding - m_scrollViewController.m_bottomPadding - m_scrollViewController.m_verticalSpacing);

        //            float elementHeight = _selectedElement.rect.height;                                 //Child Height
        //            float maskedWindowHeight = m_scrollViewController.m_maskedScrollWindow.y;           //yVector = height of masked ContentScrollWindow
        //            float viewRectAnchorPos = m_scrollViewController.m_scrollViewRectTransform.anchoredPosition.y; //0 - fixed ScrollView AnchorPosition

        //            //Get the element offset value depending on the cursor move direction.
        //            float offlimitsValue = GetScrollOffset(elementPosition, elementHeight, maskedWindowHeight, viewRectAnchorPos);

        //            //Get the normalized position, based on the TargetScrollRect's height.
        //            float normalizedPosition = m_scrollViewController.m_scrollViewRect.verticalNormalizedPosition + (offlimitsValue / m_scrollViewController.m_scrollViewRectTransform.rect.height);

        //            if (offlimitsValue < 0)
        //            {
        //                normalizedPosition -= Mathf.Abs(offlimitsValue) / m_scrollViewController.m_scrollViewRectTransform.rect.height;    //Scroll down.
        //            }
        //            else if (offlimitsValue > 0)
        //            {
        //                normalizedPosition += offlimitsValue / m_scrollViewController.m_scrollViewRectTransform.rect.height;               //Scroll up.
        //            }

        //            //Clamp the normalized Position to ensure, that it stays within the valid bound of (0 ... 1).
        //            normalizedPosition = Mathf.Clamp01(normalizedPosition);
        //            //Move the targetScrollRect to the new position with 'SmoothStep'.
        //            m_scrollViewController.m_scrollViewRect.verticalNormalizedPosition = Mathf.SmoothStep(m_scrollViewController.m_scrollViewRect.verticalNormalizedPosition, normalizedPosition, Time.unscaledDeltaTime * m_scrollSpeed);
        //#if UNITY_EDITOR
        //            //Debug.Log($"OffValue: {offlimitsValue} | NormalizedPos: {normalizedPosition} | Calc: {offlimitsValue / m_scrollViewController.m_scrollViewRectTransform.rect.height} | AbsCalc: {Mathf.Abs(offlimitsValue) / m_scrollViewController.m_scrollViewRectTransform.rect.height}");
        //#endif
        //        }

        //        private void UpdateHorizontalScrollPosition(RectTransform _selectedElement)
        //        {
        //            #region Own
        //            //            //Move the current scroll rect to correct elementPosition           //min: -57 - max: 0
        //            //            float elementPosition = _selectedElement.anchoredPosition.x - (_selectedElement.rect.width * (1 - _selectedElement.pivot.x) - m_leftPadding - m_rightPadding - m_horizontalSpacing);

        //            //            float viewRectAnchorPos = m_scrollViewRectTransform.anchoredPosition.x; //0   - fixed ScrollView AnchorPosition.
        //            //            float contentElementWidth = _selectedElement.rect.width;                //Child Width.
        //            //            float maskedWindowWidth = m_maskedScrollWindow.x;                       //xVector = width of masked ContentScrollWindow.

        //            //            //Get the element offset value depending on the cursor move direction.
        //            //            float offlimitsValue = GetScrollOffset(elementPosition, viewRectAnchorPos, contentElementWidth, maskedWindowWidth);

        //            //            //Get the normalized position, based on the TargetScrollRect's width.
        //            //            float normalizedPosition = m_scrollViewRect.horizontalNormalizedPosition/* + (offlimitsValue / m_scrollViewRectTransform.rect.width)*/;

        //            //            if (offlimitsValue > 0)
        //            //            {
        //            //                normalizedPosition -= offlimitsValue / m_scrollViewRectTransform.rect.width;               //Scroll Left.
        //            //            }

        //            //            if (offlimitsValue < 0)
        //            //            {
        //            //                normalizedPosition += Mathf.Abs(offlimitsValue) / m_scrollViewRectTransform.rect.width;    //Scroll right.
        //            //            }

        //            //            //Clamp the normalized Position to ensure, that it stays within the valid bound of (0 ... 1).
        //            //            normalizedPosition = Mathf.Clamp01(normalizedPosition);
        //            //            //Move the targetScrollRect to the new position with 'SmoothStep'.
        //            //            m_scrollViewRect.horizontalNormalizedPosition = Mathf.SmoothStep(m_scrollViewRect.horizontalNormalizedPosition, normalizedPosition, Time.unscaledDeltaTime * m_scrollSpeed);
        //            //#if UNITY_EDITOR
        //            //            //AbsCalc +, while normal Calc -.
        //            //            Debug.Log($"OffValue: {offlimitsValue} | HoriBar: {m_scrollViewRect.horizontalNormalizedPosition} | NormalizedPos: {normalizedPosition} | Calc: {offlimitsValue / m_scrollViewRectTransform.rect.width} | AbsCalc: {Mathf.Abs(offlimitsValue) / m_scrollViewRectTransform.rect.width}");
        //            //#endif
        //            #endregion

        //            //Move the current scroll rect to correct elementPosition                   //min: -57 - max: 0
        //            float elementPosition = -_selectedElement.anchoredPosition.x - (_selectedElement.rect.width * (1 - _selectedElement.pivot.x) - m_scrollViewController.m_leftPadding - m_scrollViewController.m_rightPadding - m_scrollViewController.m_horizontalSpacing);

        //            float viewRectAnchorPos = -m_scrollViewController.m_scrollViewRectTransform.anchoredPosition.x;    //0 - fixed ScrollView AnchorPosition
        //            float elementWidth = _selectedElement.rect.width;                           //Child Width
        //            float maskedWindowWidth = m_scrollViewController.m_maskedScrollWindow.x;                           //xVector = width of masked ContentScrollWindow

        //            //Get the element offset value depending on the cursor move direction.
        //            float offlimitsValue = -GetScrollOffset(elementPosition, elementWidth, maskedWindowWidth, viewRectAnchorPos);

        //            //Get the normalized position, based on the TargetScrollRect's height.
        //            float normalizedPosition = m_scrollViewController.m_scrollViewRect.horizontalNormalizedPosition + (offlimitsValue / m_scrollViewController.m_scrollViewRectTransform.rect.width);

        //            if (offlimitsValue > 0)
        //            {
        //                normalizedPosition += offlimitsValue / m_scrollViewController.m_scrollViewRectTransform.rect.width;               //Scroll ?.
        //            }

        //            if (offlimitsValue < 0)
        //            {
        //                normalizedPosition -= Mathf.Abs(offlimitsValue) / m_scrollViewController.m_scrollViewRectTransform.rect.width;    //Scroll ?.
        //            }

        //            //Clamp the normalized Position to ensure, that it stays within the valid bound of (0 ... 1).
        //            normalizedPosition = Mathf.Clamp01(normalizedPosition);

        //            //Move the targetScrollRect to the new position with 'SmoothStep'.
        //            m_scrollViewController.m_scrollViewRect.horizontalNormalizedPosition = Mathf.SmoothStep(m_scrollViewController.m_scrollViewRect.horizontalNormalizedPosition, normalizedPosition, Time.unscaledDeltaTime * m_scrollSpeed);
        //        }

        //        private float GetScrollOffset(float _elementPosition, float _elementWidthOrHeight, float _windowWidthOrHeight, float _viewRectAnchorPos)
        //        {
        //            if (_elementPosition < _viewRectAnchorPos + (_elementWidthOrHeight / 2))
        //            {
        //                return _viewRectAnchorPos + _windowWidthOrHeight - (_elementPosition - _elementWidthOrHeight);
        //            }
        //            else if (_elementPosition + _elementWidthOrHeight > _viewRectAnchorPos + _windowWidthOrHeight)
        //            {
        //                return _viewRectAnchorPos + _windowWidthOrHeight - (_elementPosition + _elementWidthOrHeight);
        //            }

        //            return 0;
        //        }
        #endregion

        private void TransitionProgress()
        {
            if (m_inProgress == false)
                return;

            m_timeElapsed += Time.unscaledDeltaTime;
            m_progress = m_timeElapsed / m_duration;

            if (m_progress > 1.0f)
            {
                m_inProgress = false;
                m_progress = 1.0f;
            }
        }

        private void CalculatePosition()
        {
            m_currentPosition.x = Mathf.Lerp(m_positionFrom.x, m_positionTo.x, m_progress);
            m_currentPosition.y = Mathf.Lerp(m_positionFrom.y, m_positionTo.y, m_progress);
        }

        private void MoveContentObjectYByAmount(float _distanceY)
        {
            Vector2 scrollPosFrom = m_scrollViewController.m_scrollViewContent.localPosition;
            Vector2 scrollPosTo = scrollPosFrom;
            scrollPosTo.y -= _distanceY;
            TransitionFromTo(scrollPosFrom, scrollPosTo, m_transitionDuration);
        }

        private void TransitionFromTo(Vector2 _positionFrom, Vector2 _positionTo, float _duration)
        {
            ResetVariables();

            m_positionFrom = _positionFrom;
            m_positionTo = _positionTo;
            m_duration = _duration;

            m_inProgress = true;
        }

        private void ResetVariables()
        {
            m_duration = 0.0f;
            m_timeElapsed = 0.0f;
            m_progress = 0.0f;
            m_inProgress = false;
        }
    }
}