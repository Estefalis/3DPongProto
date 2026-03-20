using ThreeDeePongProto.Offline.UI.Menu;
using ThreeDeePongProto.Shared.InputActions;
using ThreeDeePongProto.Shared.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ThreeDeePongProto.Shared.UI.Menu.ScrollViews
{
    internal class AutoScroll : MonoBehaviour
    {
        private enum EScrollTarget
        {
            None,
            ContentChild,
            Selectable
        }

        private PlayerInputActions m_inputActions;
        [SerializeField] internal ScrollViewController m_scrollViewController;
        [SerializeField] private EScrollTarget m_eScrollTarget = EScrollTarget.Selectable;

        #region AutoScroll Transition
        [Header("Transition")]
        [SerializeField] private float m_transitionDuration = 0.1f;
        private float m_duration = 0.0f;
        private float m_timeElapsed = 0.0f;
        private float m_progress = 0.0f;

        private bool m_inProgress = false;

        private Vector2 m_curNormalizedPos;
        private Vector2 m_normalizedPosFrom;
        private Vector2 m_normalizedPosTo;
        #endregion

        [Header("Mouse Scrolling")]
        [SerializeField] private float m_mouseScrollThreshold = 0.1f;
        [SerializeField] private float m_mouseNavigationDelay = 0.1f;
        private float m_nextMouseWheelNavTime = 0f;

        private bool m_childrenNavigationSet;
        private bool m_mouseIsInScrollView;
        private Vector2 m_mouseScrollValue, m_mousePosition;

        private GameObject m_lastCheckedSelectedObject = null, m_lastSelectedObject = null;

        private void Start()
        {
            var centralInputAction = UserInputManager.Instance.GetCentralActions();
            m_inputActions = centralInputAction;

            ResetVariables();

            m_scrollViewController.m_scrollViewRect.scrollSensitivity = 0.0f;
            m_childrenNavigationSet = m_scrollViewController.ContentChildrenSet /*&& m_scrollViewController.m_ChildrenNavigationSet*/;
        }

        private void Update()
        {
            if (!m_childrenNavigationSet)
                return;

            m_lastSelectedObject = MenuNavigation.m_lastSelectedGameObject;
            SetScrollTarget();      //AutoScroll with keyboard, gamepad, etc and ScrollCalculations.

            if (m_inputActions != null && m_inputActions.UserInterface.enabled)
            {
                GetMouseValues();
                MouseScrolling();   //AutoScroll with MouseWheel.
            }

            TransitionProgress();
        }

        #region Scroll-Preparation
        #region GetMouseValues
        private void GetMouseValues()
        {
            m_mouseScrollValue = m_inputActions.UserInterface.ScrollWheel.ReadValue<Vector2>();
            m_mousePosition = m_inputActions.UserInterface.Point.ReadValue<Vector2>();

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
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_scrollViewController.m_scrollViewRectTransform, _mousePosition, null, out Vector2 localMousePosition);

            Vector2 rectSize = m_scrollViewController.m_scrollViewRectTransform.rect.size;

            return localMousePosition.x >= -rectSize.x / 2 && localMousePosition.x <= rectSize.x / 2 &&
                localMousePosition.y >= -rectSize.y / 2 && localMousePosition.y <= rectSize.y / 2;
        }
        #endregion

        private void SetScrollTarget()
        {
            if (m_lastSelectedObject != null && m_lastSelectedObject != m_lastCheckedSelectedObject)
            {
                bool isContentChild = m_lastSelectedObject.transform.IsChildOf(m_scrollViewController.m_scrollViewContent);

                switch (m_eScrollTarget)
                {
                    case EScrollTarget.Selectable:  //Get selected Object.
                    {
                        if (isContentChild)
                        {
                            RectTransform selectedRect = m_lastSelectedObject.GetComponent<RectTransform>();
                            CalculateAndScroll(selectedRect);
                        }
                        break;
                    }
                    case EScrollTarget.ContentChild:    //Get direct childObject of scrollViewContent we are in.
                    {
                        if (isContentChild)
                        {
                            RectTransform rectToScrollTo = FindDirectContentChild(m_lastSelectedObject.transform);
                            if (rectToScrollTo != null)
                            {
                                //Use current contentChild Transform for calculation.
                                CalculateAndScroll(rectToScrollTo);
                            }
                            else
                            {
#if UNITY_EDITOR
                                Debug.LogWarning($"No direct contentChild object found. Using Fallback.", this);
#endif
                                CalculateAndScroll(m_lastSelectedObject.GetComponent<RectTransform>());   //Fallback.
                            }
                        }
                        break;
                    }
                    case EScrollTarget.None:
                    default:
                        break;
                }

                m_lastCheckedSelectedObject = m_lastSelectedObject; //Save for next frame.
            }
        }

        private void MouseScrolling()
        {
            if (!m_mouseIsInScrollView || Time.unscaledTime < m_nextMouseWheelNavTime || m_scrollViewController.EScrollDirection == EScrollDirection.None)
                return;

            if (m_mouseScrollValue.y != 0)
            {
                float scrollInputY = m_mouseScrollValue.y;

                if (Mathf.Abs(scrollInputY) > m_mouseScrollThreshold)
                {
                    //Check for Navigation-Objects.
                    if (!m_lastSelectedObject.TryGetComponent<Selectable>(out var currentSelectable))
                        return;

                    if (!currentSelectable.transform.IsChildOf(m_scrollViewController.m_scrollViewContent.transform))
                        return;

                    Selectable nextSelectable = null;

                    if (scrollInputY > 0)   //Scrolling Up/Left!
                    {
                        if (m_scrollViewController.EScrollDirection == EScrollDirection.Vertical)
                            nextSelectable = currentSelectable.FindSelectableOnUp();
                        else if (m_scrollViewController.EScrollDirection == EScrollDirection.Horizontal)
                            nextSelectable = currentSelectable.FindSelectableOnLeft();
                    }
                    else                    //Scrolling Down/Right!
                    {
                        if (m_scrollViewController.EScrollDirection == EScrollDirection.Vertical)
                            nextSelectable = currentSelectable.FindSelectableOnDown();
                        else if (m_scrollViewController.EScrollDirection == EScrollDirection.Horizontal)
                            nextSelectable = currentSelectable.FindSelectableOnRight();
                    }

                    if (nextSelectable != null && nextSelectable.gameObject.activeInHierarchy)
                    {
#if UNITY_EDITOR
                        //Debug.Log($"MouseScrolling to: {nextSelectable.gameObject.name}");
#endif
                        EventSystem.current.SetSelectedGameObject(nextSelectable.gameObject);
                        m_nextMouseWheelNavTime = Time.unscaledTime + m_mouseNavigationDelay;
                    }
                }
            }
        }

        /// <summary>
        /// Checks Hierarchy upwards to find childTransform of the scrollView content.
        /// </summary>
        /// <param name="_selectedTransform">Selectable Transform.</param>
        /// <returns>RectTransform of direct scrollView content child or null.</returns>
        private RectTransform FindDirectContentChild(Transform _selectedTransform)
        {
            if (_selectedTransform == null || m_scrollViewController.m_scrollViewContent == null)
                return null;

            Transform currentParent = _selectedTransform.parent;
            Transform directChild = _selectedTransform; //Start with submitted element.

            //Check Hierarchy upwards until '.parent' is not the scrollView Content Transform.
            while (currentParent != null && currentParent != m_scrollViewController.m_scrollViewContent)
            {
                directChild = currentParent; //Move up a level.
                currentParent = directChild.parent;
            }

            //Direct child is found, when objectParent is contentTransform.
            if (currentParent == m_scrollViewController.m_scrollViewContent)
                return directChild.GetComponent<RectTransform>();

            //If selected element already is a direct child.
            if (_selectedTransform.parent == m_scrollViewController.m_scrollViewContent)
                return _selectedTransform.GetComponent<RectTransform>();

            //If nothing is found. For safety.
            return null;
        }
        #endregion

        private void CalculateAndScroll(RectTransform _selectedRect)
        {
            if (_selectedRect == null || m_scrollViewController.m_scrollViewRect == null)
            {
                Debug.LogWarning("AutoScroll: SelectedRect or ScrollViewRect are null.");
                return;
            }

            ScrollRect scrollRect = m_scrollViewController.m_scrollViewRect;
            RectTransform viewportRect = scrollRect.viewport; //Viewport RectTransform.
            if (viewportRect == null)
                viewportRect = m_scrollViewController.m_scrollViewRectTransform; //Fallback, if Viewport is not explicitly set.

            RectTransform contentRect = m_scrollViewController.m_scrollViewContent;

            //Current normalized ScrollRect Position.
            Vector2 currentNormalizedPos = scrollRect.normalizedPosition;
            //Set targetPosition on currentPosition to start with.
            Vector2 targetNormalizedPos = currentNormalizedPos;
            
            //Calculate normalized Y-position, to move the element into a visible position.
            if (scrollRect.vertical) //Vertical scrolling must be enabled.
            {
                targetNormalizedPos.y = CalculateNormalizedTargetPositionY(viewportRect, contentRect, _selectedRect);
            }

            //Calculate normalized X-position, to move the element into a visible position.
            if (scrollRect.horizontal) //Horizontal scrolling must be enabled.
            {
                targetNormalizedPos.x = CalculateNormalizedTargetPositionX(viewportRect, contentRect, _selectedRect);
            }

            //Start the transition, if the currentPosition and targetPosition differ.
            if (!Mathf.Approximately(currentNormalizedPos.x, targetNormalizedPos.x) ||
                !Mathf.Approximately(currentNormalizedPos.y, targetNormalizedPos.y))
            {
#if UNITY_EDITOR
                //Debug.Log($"AutoScroll: Scrolling required! Current: {currentNormalizedPos}, Target: {targetNormalizedPos}");
#endif
                NormalizedTransitionFromTo(currentNormalizedPos, targetNormalizedPos, m_transitionDuration);
            }
#if UNITY_EDITOR
            else
            {
                //Debug.Log("AutoScroll: No scrolling required. Element is visible.");
            }
#endif
        }

        private float CalculateNormalizedTargetPositionY(RectTransform _viewport, RectTransform _content, RectTransform _item)
        {
            ScrollRect scrollRect = m_scrollViewController.m_scrollViewRect;
            float currentNormalizedY = scrollRect.verticalNormalizedPosition;

            //World position of the corners.
            Vector3[] viewportCorners = new Vector3[4];
            _viewport.GetWorldCorners(viewportCorners); //BottomLeft = 0, TopLeft = 1, TopRight = 2, BottomRight = 3.
            Vector3[] itemCorners = new Vector3[4];
            _item.GetWorldCorners(itemCorners);

            //Item bounds in world space.
            float itemLowerBound = itemCorners[0].y;
            float itemUpperBound = itemCorners[1].y;

            //Viewport bounds in world space.
            float viewportLowerBound = viewportCorners[0].y;
            float viewportUpperBound = viewportCorners[1].y;

            //Calculate the amount the _item is outside the _viewport bounds.
            float deltaLower = viewportLowerBound - itemLowerBound; //Positive if _item is below _viewport bottom.
            float deltaUpper = itemUpperBound - viewportUpperBound; //Positive if _item is above _viewport top.

            //Calculate scroll amount needed in world units (if any).
            float scrollDeltaWorld = 0.0f;
            if (deltaLower > 0.0f)
            {
                //If _item bottom is below _viewport bottom, scroll _content UP by this amount.
                scrollDeltaWorld = deltaLower;
                scrollDeltaWorld += m_scrollViewController.m_bottomPadding; //Include padding to calculation.
            }
            else if (deltaUpper > 0.0f)
            {
                //If _item top is above _viewport top, scroll _content DOWN by this amount.
                scrollDeltaWorld = -deltaUpper;
                scrollDeltaWorld -= m_scrollViewController.m_topPadding; //Include padding to calculation.
            }

            //Convert world scroll delta to normalized scroll delta.
            if (Mathf.Abs(scrollDeltaWorld) > 0.01f) //Scroll only if needed.
            {
                float contentHeight = _content.rect.height;
                float viewportHeight = _viewport.rect.height;
                float scrollableHeight = contentHeight - viewportHeight;

                if (scrollableHeight > 0.0f)
                {
                    //How much normalized position changes per scroll step. (Scroll content up DECREASES normalized position.)
                    float normalizedDelta = -scrollDeltaWorld / scrollableHeight;
                    currentNormalizedY += normalizedDelta;
                }
            }
            
            return Mathf.Clamp01(currentNormalizedY);
        }

        private float CalculateNormalizedTargetPositionX(RectTransform _viewport, RectTransform _content, RectTransform _item)
        {
            ScrollRect scrollRect = m_scrollViewController.m_scrollViewRect;
            float currentNormalizedX = scrollRect.horizontalNormalizedPosition;

            //World position of the corners.
            Vector3[] viewportCorners = new Vector3[4];
            _viewport.GetWorldCorners(viewportCorners); //BottomLeft = 0, TopLeft = 1, TopRight = 2, BottomRight = 3.
            Vector3[] itemCorners = new Vector3[4];
            _item.GetWorldCorners(itemCorners);

            //Item bounds in world space.
            float itemLeftBound = itemCorners[0].x;
            float itemRightBound = itemCorners[3].x;

            //Viewport bounds in world space.
            float viewportLeftBound = viewportCorners[0].x;
            float viewportRightBound = viewportCorners[3].x;

            //Calculate the amount the _item is outside the _viewport bounds.
            float deltaLeft = viewportLeftBound - itemLeftBound;   //Positive if _item is below _viewport left.
            float deltaRight = itemRightBound - viewportRightBound; //Positive if _item is above _viewport right.

            //Calculate scroll amount needed in world units (if any).
            float scrollDeltaWorld = 0.0f;
            if (deltaLeft > 0.0f)
            {
                //If _item bottom is below _viewport left, scroll _content RIGHT by this amount.
                scrollDeltaWorld = deltaLeft;
                scrollDeltaWorld += m_scrollViewController.m_leftPadding; //Include padding to calculation.
            }
            else if (deltaRight > 0.0f)
            {
                //If _item top is above _viewport right, scroll _content LEFT by this amount.
                scrollDeltaWorld = -deltaRight;
                scrollDeltaWorld -= m_scrollViewController.m_rightPadding; //Include padding to calculation.
            }

            //Convert world scroll delta to normalized scroll delta.
            if (Mathf.Abs(scrollDeltaWorld) > 0.01f) //Scroll only if needed.
            {
                float contentWidth = _content.rect.width;
                float viewportWidth = _viewport.rect.width;
                float scrollableWidth = contentWidth - viewportWidth;

                if (scrollableWidth > 0.0f)
                {
                    //How much normalized position changes per scroll step. (Scroll content left DECREASES normalized position.)
                    float normalizedDelta = -scrollDeltaWorld / scrollableWidth;
                    currentNormalizedX += normalizedDelta;
                }
            }

            return Mathf.Clamp01(currentNormalizedX);
        }

        private void TransitionProgress()
        {
            if (m_inProgress)
            {
                m_timeElapsed += Time.unscaledDeltaTime;
                m_progress = m_timeElapsed / m_duration;

                if (m_progress >= 1.0f)
                {
                    m_progress = 1.0f;
                    m_inProgress = false;

                    m_curNormalizedPos = m_normalizedPosTo;
                    ApplyFinalPosition();
                }
                else
                {
                    CalculateNormalizedPosition();
                    ApplyFinalPosition();
                }
            }
        }

        private void CalculateNormalizedPosition()
        {
            m_curNormalizedPos.x = Mathf.Lerp(m_normalizedPosFrom.x, m_normalizedPosTo.x, m_progress);
            m_curNormalizedPos.y = Mathf.Lerp(m_normalizedPosFrom.y, m_normalizedPosTo.y, m_progress);
            //m_curNormalizedPos = Vector2.Lerp(m_normalizedPosFrom, m_normalizedPosTo, m_progress);
        }

        private void NormalizedTransitionFromTo(Vector2 _normalizedPosFrom, Vector2 _normalizedPosTo, float _duration)
        {
            //REQUIRES A RESOLUTION OF FULL HAD (1920 x 1080) IN UNITY! [Damn GlobalDefenseInitiative!]
            //Prevent micro movement.
            if (!(Vector2.Distance(_normalizedPosFrom, _normalizedPosTo) > 0.0001f))
            {
                m_curNormalizedPos = _normalizedPosTo;
                ApplyFinalPosition();
                ResetVariables();       //Clean Up
                return;
            }

            ResetVariables();           //Clean start

            m_normalizedPosFrom = _normalizedPosFrom;
            m_normalizedPosTo = _normalizedPosTo;
            m_duration = Mathf.Max(_duration, 0.001f);  //Duration must not be 0.

            m_curNormalizedPos = _normalizedPosFrom;
            m_inProgress = true;
        }

        private void ApplyFinalPosition()
        {
            if (m_scrollViewController.m_scrollViewRect != null)
                m_scrollViewController.m_scrollViewRect.normalizedPosition = m_curNormalizedPos;
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