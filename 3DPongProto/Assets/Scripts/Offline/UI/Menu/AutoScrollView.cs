using System.Collections.Generic;
using ThreeDeePongProto.Shared.InputActions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// Credit zero3growlithe (See README.md.)
/// sourced from: http://forum.unity3d.com/threads/scripts-useful-4-6-scripts-collection.264161/page-2#post-2011648

namespace ThreeDeePongProto.Offline.UI.Menu
{
    [RequireComponent(typeof(ScrollRect))]
    //[AddComponentMenu("UI/Extensions/AutoScrollView")]
    public class AutoScrollView : MonoBehaviour
    {
        private enum AutoScrollOptions
        {
            None,
            Vertical,
            Horizontal,
            Both
        }

        private enum WindowEdgesSwitch
        {
            MaskedScrollViewRect,
            UnmaskedContentRect
        }

        //TODO: Remove '[SerializeField] ' after development, if it's not needed.
        private PlayerInputActions m_playerInputActions;

        [SerializeField] private AutoScrollOptions m_detectedScrollOption = AutoScrollOptions.Both;
        [SerializeField] private WindowEdgesSwitch m_selectedWindowMask = WindowEdgesSwitch.MaskedScrollViewRect;
        [SerializeField] private float m_scrollSpeed = 60.0f;
        [Space]
        [SerializeField] private ScrollRect m_scrollViewRect;
        [SerializeField] private RectTransform m_scrollContentRT;
        [Space]
        [SerializeField] private LayoutGroup m_layoutGroup;
        [SerializeField] private int m_contentChildCount;
        [Space]
        [SerializeField] private Vector2 m_maskedScrollWindow;  //Fix (masked) Width & Height
        [SerializeField] private Vector2 m_unmaskedContentRT;   //Full Width & Height.
        private Vector2Int m_gridSize;
        private Vector2 m_firstChildRT;
        [Space]
        [SerializeField] private int m_leftPadding;
        [SerializeField] private int m_rightPadding;
        [SerializeField] private int m_topPadding;
        [SerializeField] private int m_bottomPadding;
        [SerializeField] private float m_horizontalSpacing;
        [SerializeField] private float m_verticalSpacing;

        private bool m_canAutoScroll = false, m_scrollContentSet;
        private bool m_edgePosition = false;
        private bool m_startEdgeVer = false, m_endEdgeVer = false;
        private bool m_startEdgeHor = false, m_endEdgeHor = false;

        private bool m_mouseIsInScrollView;
        private Vector2 m_mouseScrollValue, m_mousePosition;

        private GameObject m_lastSelectedGameObject;
        private RectTransform m_scrollViewRectTransform;
        private RectTransform m_childRect;      //ChildRect for each chilc of the Content and it's '.anchoredPosition'.
        private GridLayoutGroup.Constraint m_gridConstraint;

        private Dictionary<GameObject, RectTransform> m_contentChildAnchorPos = new Dictionary<GameObject, RectTransform>();
        private Dictionary<GameObject, Navigation> m_objectNavigation = new Dictionary<GameObject, Navigation>();

        private void Awake()
        {
            m_lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
            m_contentChildAnchorPos.Clear();
            m_objectNavigation.Clear();

            m_scrollViewRect = GetComponent<ScrollRect>();
            m_scrollViewRectTransform = m_scrollViewRect.GetComponent<RectTransform>();
            m_scrollContentRT = m_scrollViewRect.content.GetComponent<RectTransform>();

            m_scrollContentSet = m_scrollViewRect != null && m_scrollContentRT != null;

            GetAutoScrollOptions(m_scrollViewRect);
        }

        private void Start()
        {
            if (m_scrollContentRT != null)
                ContentLevelIterations();

            GetLayoutGroupSettings(m_layoutGroup);  //Requires 'ContentLevelIterations()' '_contentChildCount', if m_totalSpacings are wanted.

            m_playerInputActions = InputManager.m_playerInputActions;
            m_playerInputActions.UI.Enable();
        }

        private void OnDisable()
        {
            m_playerInputActions.UI.Disable();
        }

        private void Update()
        {
            GetMouseValues();
            
            AutoScrollToNextGameObject();
            UpdateCurrentGameObject();
        }

        /// <summary>
        /// If Slider, Toggle or Button GameObjects are found in the ScrollView, they will get added to the 'm_scrollViewGameObjects' List.
        /// </summary>
        /// <param name="_transformLevel"></param>
        private void ScrollViewObjectsToDicts(Transform _transformLevel, RectTransform _contentElementAnchorPos)
        {
            bool containsToggle = _transformLevel.TryGetComponent(out Toggle toggle);
            bool containsSlider = _transformLevel.TryGetComponent(out Slider slider);
            bool containsButton = _transformLevel.TryGetComponent(out Button button);

            switch (containsToggle)
            {
                case false:
                    break;
                case true:
                {
                    m_contentChildAnchorPos.Add(toggle.gameObject, _contentElementAnchorPos); //RectTransform with '.anchoredPosition'.
                    m_objectNavigation.Add(toggle.gameObject, toggle.navigation);
                    break;
                }
            }

            switch (containsSlider)
            {
                case false:
                    break;
                case true:
                {
                    m_contentChildAnchorPos.Add(slider.gameObject, _contentElementAnchorPos); //RectTransform with '.anchoredPosition'.
                    m_objectNavigation.Add(slider.gameObject, slider.navigation);
                    break;
                }
            }

            switch (containsButton)
            {
                case false:
                    break;
                case true:
                {
                    m_contentChildAnchorPos.Add(button.gameObject, _contentElementAnchorPos); //RectTransform with '.anchoredPosition'.
                    m_objectNavigation.Add(button.gameObject, button.navigation);
                    break;
                }
            }
        }

        /// <summary>
        /// Sets the AutoScrollOption and gets the LayoutGroup automatic, depending on the availabilities of connected scrollbars and their corresponding bool states.
        /// </summary>
        /// <param name="_targetScrollRect"></param>
        private void GetAutoScrollOptions(ScrollRect _targetScrollRect)
        {
            bool verticalScrolling = _targetScrollRect.verticalScrollbar != null && _targetScrollRect.vertical;
            bool horizontalScrolling = _targetScrollRect.horizontalScrollbar != null && _targetScrollRect.horizontal;

            bool bothScrollDirections = verticalScrolling && horizontalScrolling;

            switch (bothScrollDirections)
            {
                case true:
                {
                    m_detectedScrollOption = AutoScrollOptions.Both;
                    m_layoutGroup = m_scrollContentRT.GetComponent<GridLayoutGroup>();
                    break;
                }
                case false:
                {
                    switch (verticalScrolling)
                    {
                        case true:
                        {
                            m_detectedScrollOption = AutoScrollOptions.Vertical;
                            m_layoutGroup = m_scrollContentRT.GetComponent<VerticalLayoutGroup>();
                            break;
                        }
                        case false:
                        {
                            switch (horizontalScrolling)
                            {
                                case true:
                                {
                                    m_detectedScrollOption = AutoScrollOptions.Horizontal;
                                    m_layoutGroup = m_scrollContentRT.GetComponent<HorizontalLayoutGroup>();
                                    break;
                                }
                                case false:
                                {
                                    m_detectedScrollOption = AutoScrollOptions.None;
                                    m_layoutGroup = null;
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    break;
                }
            }

            //if '(_layoutGroup == null)' and you want to add one (WITH detailed memberValues to set it's dimensions):
            //'AddMissingLayoutGroup()' - 'switch (m_detectedScrollOption)' - 'case AutoScrollOptions.Both/.Vertical/.Horizontal' -
            //'_layoutGroup = m_scrollContentRT.AddComponent<GridLayoutGroup/VerticalLayoutGroup/HorizontalLayoutGroup>()' and
            //'default: break;' - for savety. (It would currently go too far.)
        }

        /// <summary>
        /// Searches Content of the currently active ScrollView with nested for-loops, to fill Dictionaries with AnchoredPositions and Navigation Informations of the contained GameObjects.
        /// </summary>
        private void ContentLevelIterations()
        {
            m_contentChildCount = 0;
            foreach (Transform transform in m_scrollContentRT.transform)
            {
                m_contentChildCount += 1;
                m_childRect = transform.GetComponent<RectTransform>();

                //Level for X/Y Axis Toggles.
                for (int i = 0; i < transform.childCount; i++)
                {
#if UNITY_EDITOR
                    //Debug.Log(transform.GetChild(i).name);
#endif
                    Transform subLevelOne = transform.GetChild(i);
                    ScrollViewObjectsToDicts(subLevelOne, m_childRect);            //'m_childRect.anchoredPosition' instead of i.

                    ////Level for SliderXAxis Lower & Higher Buttons, XSlider itself, it's Toggle.
                    ////Level for SliderYAxis Lower & Higher Buttons, YSlider itself, it's Toggle.
                    ////Level for ResetButtons for KeyRebinds.
                    for (int j = 0; j < subLevelOne.childCount; j++)
                    {
#if UNITY_EDITOR
                        //Debug.Log(subLevelOne.GetChild(j).name);
#endif
                        Transform subLevelTwo = subLevelOne.GetChild(j);
                        ScrollViewObjectsToDicts(subLevelTwo, m_childRect);        //'m_childRect.anchoredPosition' instead of i.

                        //Level for Keyboard & Gamepad Rebind Buttons.
                        for (int k = 0; k < subLevelTwo.childCount; k++)
                        {
#if UNITY_EDITOR
                            //Debug.Log(subLevelTwo.GetChild(k).name);
#endif
                            Transform subLevelThree = subLevelTwo.GetChild(k);
                            ScrollViewObjectsToDicts(subLevelThree, m_childRect);  //'m_childRect.anchoredPosition' instead of i.
                        }
                    }
                }
            }
        }

        private void GetLayoutGroupSettings(LayoutGroup _layoutGroup)
        {
            switch (m_detectedScrollOption)
            {
                case AutoScrollOptions.Both:
                {
                    var gridSettings = _layoutGroup.GetComponent<GridLayoutGroup>();
                    //Space at LayoutGroup borders.
                    m_topPadding = gridSettings.padding.top;
                    m_bottomPadding = gridSettings.padding.bottom;
                    m_leftPadding = gridSettings.padding.left;
                    m_rightPadding = gridSettings.padding.right;
                    //Spacing between elements.
                    m_horizontalSpacing = gridSettings.spacing.x;
                    m_verticalSpacing = gridSettings.spacing.y;

                    m_gridConstraint = gridSettings.constraint;
                    var constraintCount = _layoutGroup.GetComponent<GridLayoutGroup>().constraintCount;

                    switch (m_gridConstraint)   //TODO: 'GridLayoutGroup.Constraint.Flexible'.
                    {
                        case GridLayoutGroup.Constraint.Flexible:
                            break;
                        case GridLayoutGroup.Constraint.FixedColumnCount:
                            m_gridSize.x = constraintCount;
                            m_gridSize.y = GetOtherAxisCount(m_contentChildCount, m_gridSize.x);
                            break;
                        case GridLayoutGroup.Constraint.FixedRowCount:
                            m_gridSize.y = constraintCount;
                            m_gridSize.x = GetOtherAxisCount(m_contentChildCount, m_gridSize.y);
                            break;
                        default:
                            break;
                    }
                    break;
                }
                case AutoScrollOptions.Vertical:
                {
                    var padding = _layoutGroup./*GetComponent<VerticalLayoutGroup>().*/padding;        //Space at LayoutGroup borders.
                    m_topPadding = padding.top;
                    m_bottomPadding = padding.bottom;
                    m_verticalSpacing = _layoutGroup.GetComponent<VerticalLayoutGroup>().spacing;      //Spacing between Elements.
                    break;
                }
                case AutoScrollOptions.Horizontal:
                {
                    var padding = _layoutGroup./*GetComponent<HorizontalLayoutGroup>().*/padding;      //Space at LayoutGroup borders.
                    m_leftPadding = padding.left;
                    m_rightPadding = padding.right;
                    m_horizontalSpacing = _layoutGroup.GetComponent<HorizontalLayoutGroup>().spacing;  //Spacing between Elements.
                    break;
                }
                case AutoScrollOptions.None:
                default:
                    break;
            }

            m_maskedScrollWindow = new Vector2(m_scrollViewRectTransform.rect.width, m_scrollViewRectTransform.rect.height);

            var contentRect = m_scrollContentRT.GetComponent<RectTransform>().rect;
            m_unmaskedContentRT = new Vector2(contentRect.width, contentRect.height);   //.x - .width, .y - .height.
            var firstChildRect = m_scrollContentRT.GetChild(0).GetComponent<RectTransform>().rect;
            m_firstChildRT = new Vector2(firstChildRect.width, firstChildRect.height);
        }

        /// <summary>
        /// Updates AutoScrolling bool 'm_canAutoScroll', depending on 'm_scrollViewGameObjects' List entries.
        /// </summary>
        /// <param name="_gameObject"></param>
        private void UpdateCurrentGameObject()
        {
            switch (m_lastSelectedGameObject == null)
            {
                case true:  //Dicts don't allow null on keys. And just 'return;' disables AutoScrolling.
                    if (EventSystem.current.currentSelectedGameObject != null)
                        m_lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
                    break;
                case false:
                {
                    if (m_lastSelectedGameObject != EventSystem.current.currentSelectedGameObject)
                    {
                        m_lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;

                        #region Dict switch
                        switch (m_contentChildAnchorPos.ContainsKey(m_lastSelectedGameObject))
                        {
                            case true:
                            {
                                switch (m_selectedWindowMask)
                                {
                                    case WindowEdgesSwitch.MaskedScrollViewRect:
                                    {
                                        m_edgePosition = MaskedScrollRectEdgeCheck(m_scrollViewRectTransform, m_contentChildAnchorPos[m_lastSelectedGameObject].anchoredPosition);
                                        break;
                                    }
                                    case WindowEdgesSwitch.UnmaskedContentRect:
                                    {
                                        m_edgePosition = UnmaskedContentEdgeCheck(m_scrollContentRT.anchoredPosition, m_contentChildAnchorPos[m_lastSelectedGameObject].anchoredPosition);
                                        break;
                                    }
                                    default:
                                        break;
                                }
#if UNITY_EDITOR
                                //Debug.Log($"AtEdgePosition: {m_edgePosition}");
#endif

                                #region Work Around on incomplete autoScrolling with MouseWheel on m_startEdge & m_endEdge positions.
                                switch (m_edgePosition)
                                {
                                    case true:
                                        m_scrollViewRect.scrollSensitivity = 0.0f;
                                        break;
                                    case false:
                                        m_scrollViewRect.scrollSensitivity = 10.0f;
                                        break;
                                }
                                #endregion

                                m_canAutoScroll = true;
                                break;
                            }
                            case false:
                            {
                                m_canAutoScroll = false;
                                break;
                            }
                        }
                    }
                    #endregion
#if UNITY_EDITOR
                    //Debug.Log($"LastGO: {m_lastSelectedGameObject.name} - autoScroll: {m_canAutoScroll}");
#endif
                    break;
                }
            }
        }

        private bool MaskedScrollRectEdgeCheck(RectTransform _scrollViewRect, Vector2 _lastGOAnchor)
        {
            switch (m_detectedScrollOption)
            {
                case AutoScrollOptions.Vertical:
                {
                    m_startEdgeVer = _lastGOAnchor.y + (m_verticalSpacing * m_contentChildCount - 1) + m_topPadding + m_scrollContentRT.anchoredPosition.y >= _scrollViewRect.anchoredPosition.y;
                    m_endEdgeVer = -(_lastGOAnchor.y - m_firstChildRT.y - (m_verticalSpacing * m_contentChildCount - 1) - m_bottomPadding + m_scrollContentRT.anchoredPosition.y) >= _scrollViewRect.rect.height;
                    break;
                }
                case AutoScrollOptions.Horizontal:
                {
                    m_startEdgeHor = _lastGOAnchor.x + (m_horizontalSpacing * m_contentChildCount - 1) + m_leftPadding + m_scrollContentRT.anchoredPosition.x >= _scrollViewRect.anchoredPosition.x;
                    m_endEdgeHor = -(_lastGOAnchor.x - m_firstChildRT.x - (m_horizontalSpacing * m_contentChildCount - 1) - m_rightPadding + m_scrollContentRT.anchoredPosition.x) >= _scrollViewRect.rect.width;
                    break;
                }
                case AutoScrollOptions.Both:
                {
                    //TODO: MaskedScrollRectEdgeCheck AutoScrollOptions.Both
                    break;
                }
                case AutoScrollOptions.None:
                default:
                    break;
            }

            switch (m_startEdgeVer ^ m_endEdgeVer || m_startEdgeHor ^ m_endEdgeHor)
            {
                case true:
                    return true;
                case false:
                    return false;
            }
        }

        private bool UnmaskedContentEdgeCheck(Vector2 _contentAnchor, Vector2 _lastGOAnchor)
        {
            switch (m_detectedScrollOption)
            {
                case AutoScrollOptions.Vertical:
                {
                    var zeroedContentRTAnchorY = _contentAnchor.y - _contentAnchor.y; //On entering ScrollView from below, AnchorPos is not 0.
                    m_startEdgeVer = _lastGOAnchor.y + m_verticalSpacing + m_topPadding >= zeroedContentRTAnchorY;
                    m_endEdgeVer = _lastGOAnchor.y - m_firstChildRT.y - m_verticalSpacing - m_bottomPadding <= zeroedContentRTAnchorY - m_maskedScrollWindow.y;
                    break;
                }
                case AutoScrollOptions.Horizontal:
                {
                    var zeroedContentRTAnchorX = _contentAnchor.x - _contentAnchor.x;  //On entering ScrollView from right, AnchorPos is not 0.
                    m_startEdgeHor = _lastGOAnchor.x + m_horizontalSpacing + m_leftPadding >= zeroedContentRTAnchorX;
                    m_endEdgeHor = _lastGOAnchor.x - m_firstChildRT.x - m_horizontalSpacing - m_rightPadding <= zeroedContentRTAnchorX - m_maskedScrollWindow.x;
                    break;
                }
                case AutoScrollOptions.Both:
                {
                    //TODO: Confirm MaskedScrollRectEdgeCheck AutoScrollOptions.Both
                    var zeroedContentRTAnchorY = _contentAnchor.y - _contentAnchor.y; //On entering ScrollView from below, AnchorPos is not 0.
                    m_startEdgeVer = _lastGOAnchor.y + m_verticalSpacing + m_topPadding >= zeroedContentRTAnchorY;
                    m_endEdgeVer = _lastGOAnchor.y - m_firstChildRT.y - m_verticalSpacing - m_bottomPadding <= zeroedContentRTAnchorY - m_maskedScrollWindow.y;

                    var zeroedContentRTAnchorX = _contentAnchor.x - _contentAnchor.x;  //On entering ScrollView from right, AnchorPos is not 0.
                    m_startEdgeHor = _lastGOAnchor.x + m_horizontalSpacing + m_leftPadding >= zeroedContentRTAnchorX;
                    m_endEdgeHor = _lastGOAnchor.x - m_firstChildRT.x - m_horizontalSpacing - m_rightPadding <= zeroedContentRTAnchorX - m_maskedScrollWindow.x;
                    break;
                }
                case AutoScrollOptions.None:
                default:
                    break;
            }

            switch (m_startEdgeVer ^ m_endEdgeVer || m_startEdgeHor ^ m_endEdgeHor)
            {
                case true:
                    return true;
                case false:
                    return false;
            }
        }

        private void GetMouseValues()
        {
            m_mouseScrollValue = m_playerInputActions.UI.ScrollWheel.ReadValue<Vector2>();
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

            #region Tests
            //m_mouseYInScrollView = m_mousePosition.y + m_scrollViewRectTransform.rect.min.y > 0 && m_mousePosition.y + m_scrollViewRectTransform.rect.min.y < m_scrollViewRectTransform.rect.height;

            //m_mouseXInScrollView = m_mousePosition.x + m_scrollViewRectTransform.rect.min.x > -425 && m_mousePosition.x + m_scrollViewRectTransform.rect.min.x < m_scrollViewRectTransform.rect.width - 425; //Why Y0, but X -425?
            //var restSpaceX = (m_resolution.width - m_scrollViewRectTransform.rect.width) * 0.5f;
            //m_mouseXInScrollView = !(m_mousePosition.x < restSpaceX) && !(m_mousePosition.x > m_scrollViewRectTransform.rect.width + restSpaceX);
            //Debug.Log($"MouseXIn: {m_mouseXInScrollView} - MouseYIn: {m_mouseYInScrollView}");
            #endregion
        }

        private bool MouseIsInScrollView(Vector2 _mousePosition)
        {
            //Thanks to CodeGPT!
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_scrollViewRectTransform, _mousePosition, null, out Vector2 localMousePosition);

            Vector2 rectSize = m_scrollViewRectTransform.rect.size;

            return localMousePosition.x >= -rectSize.x / 2 && localMousePosition.x <= rectSize.x / 2 &&
                localMousePosition.y >= -rectSize.y / 2 && localMousePosition.y <= rectSize.y / 2;            
        }

        /// <summary>
        /// AutoScrolls to the next element, if the ScrollView and it's content are not null and the next element is part of the ScrollView.
        /// </summary>
        private void AutoScrollToNextGameObject()
        {
            if (!m_scrollContentSet || !m_canAutoScroll/* || Cursor.lockState == CursorLockMode.None*/)
                return;

            //TODO: Implement Mouse (In)Visibility change.
            ScrollSelectNextGameObject();

            switch (m_detectedScrollOption)
            {
                case AutoScrollOptions.Vertical:
                    UpdateVerticalScrollPosition(m_contentChildAnchorPos[m_lastSelectedGameObject]);
                    break;
                case AutoScrollOptions.Horizontal:
                    UpdateHorizontalScrollPosition(m_contentChildAnchorPos[m_lastSelectedGameObject]);
                    break;
                case AutoScrollOptions.Both:
                    UpdateVerticalScrollPosition(m_contentChildAnchorPos[m_lastSelectedGameObject]);
                    UpdateHorizontalScrollPosition(m_contentChildAnchorPos[m_lastSelectedGameObject]);
                    break;
                case AutoScrollOptions.None:
                default:
                    break;
            }
        }

        private void UpdateVerticalScrollPosition(RectTransform _selectedElement)
        {
            //Move the current scroll rect to correct elementPosition           //min: -57 - max: 0
            float elementPosition = -_selectedElement.anchoredPosition.y - (_selectedElement.rect.height * (1 - _selectedElement.pivot.y) - (m_topPadding + m_bottomPadding + m_verticalSpacing));

            float viewRectAnchorPos = m_scrollViewRectTransform.anchoredPosition.y; //0   - fixed ScrollView AnchorPosition
            float contentElementHeight = _selectedElement.rect.height;              //Child Height
            float maskedWindowHeight = m_maskedScrollWindow.y;                      //yVector = height of masked ContentScrollWindow

            //Get the element offset value depending on the cursor move direction.
            float offlimitsValue = GetScrollOffset(elementPosition, viewRectAnchorPos, contentElementHeight, maskedWindowHeight);

            //Get the normalized  position, based on the TargetScrollRect's height.
            float normalizedPosition = m_scrollViewRect.verticalNormalizedPosition + (offlimitsValue / m_scrollViewRectTransform.rect.height);
            normalizedPosition = Mathf.Clamp01(normalizedPosition);
#if UNITY_EDITOR
            //Debug.Log($"OffValue: {offlimitsValue} - NormalizedPos: {normalizedPosition} - ElementPos: {elementPosition}");
#endif

            if (offlimitsValue < 0)
            {
                normalizedPosition -= Mathf.Abs(offlimitsValue) / m_scrollViewRectTransform.rect.height;    //Scroll up.
            }

            if (offlimitsValue > 0)
            {
                normalizedPosition += offlimitsValue / m_scrollViewRectTransform.rect.height;               //Scroll down.
            }

            //Clamp the normalized Position to ensure, that it stays within the valid bound of (0 ... 1).
            normalizedPosition = Mathf.Clamp01(normalizedPosition);
            //Move the targetScrollRect to the new position with 'SmoothStep'.
            m_scrollViewRect.verticalNormalizedPosition = Mathf.SmoothStep(m_scrollViewRect.verticalNormalizedPosition, normalizedPosition, Time.unscaledDeltaTime * m_scrollSpeed);
        }

        private void UpdateHorizontalScrollPosition(RectTransform _selectedElement)
        {
            //Move the current scroll rect to correct elementPosition           //min: -57 - max: 0
            float elementPosition = -_selectedElement.anchoredPosition.x - (_selectedElement.rect.width * (1 - _selectedElement.pivot.x) - (m_leftPadding + m_rightPadding + m_horizontalSpacing));

            float viewRectAnchorPos = m_scrollViewRectTransform.anchoredPosition.x; //0   - fixed ScrollView AnchorPosition.
            float contentElementWidth = _selectedElement.rect.width;                //Child Width.
            float maskedWindowWidth = m_maskedScrollWindow.x;                       //xVector = width of masked ContentScrollWindow.

            //Get the element offset value depending on the cursor move direction.
            float offlimitsValue = GetScrollOffset(elementPosition, viewRectAnchorPos, contentElementWidth, maskedWindowWidth);

            //Get the normalized  position, based on the TargetScrollRect's height.
            float normalizedPosition = m_scrollViewRect.horizontalNormalizedPosition + (offlimitsValue / m_scrollViewRectTransform.rect.width);
            normalizedPosition = Mathf.Clamp01(normalizedPosition);
#if UNITY_EDITOR
            //Debug.Log($"OffValue: {offlimitsValue} - NormalizedPos: {normalizedPosition} - ElementPos: {elementPosition}");
#endif

            if (offlimitsValue < 0)
            {
                normalizedPosition -= Mathf.Abs(offlimitsValue) / m_scrollViewRectTransform.rect.width;    //Scroll left.
            }

            if (offlimitsValue > 0)
            {
                normalizedPosition += offlimitsValue / m_scrollViewRectTransform.rect.width;               //Scroll right.
            }

            //Clamp the normalized Position to ensure, that it stays within the valid bound of (0 ... 1).
            normalizedPosition = Mathf.Clamp01(normalizedPosition);
            //Move the targetScrollRect to the new position with 'SmoothStep'.
            m_scrollViewRect.horizontalNormalizedPosition = Mathf.SmoothStep(m_scrollViewRect.horizontalNormalizedPosition, normalizedPosition, Time.unscaledDeltaTime * m_scrollSpeed);
        }

        private float GetScrollOffset(float _elementPosition, float _viewRectAnchorPos, float _contentElementHeight, float _maskedWindowHeight)
        {
            if (_elementPosition < _viewRectAnchorPos + (_contentElementHeight / 2))
            {
                return _viewRectAnchorPos + _maskedWindowHeight - (_elementPosition - _contentElementHeight);
            }
            else if (_elementPosition + _contentElementHeight > _viewRectAnchorPos + _maskedWindowHeight)
            {
                return _viewRectAnchorPos + _maskedWindowHeight - (_elementPosition + _contentElementHeight);
            }

            return 0;
        }

        private static int GetOtherAxisCount(int _contentChildCount, int _constraintAxisCount)
        {
            float lambdaSwitch = (float)_contentChildCount / _constraintAxisCount - _constraintAxisCount;
            float addedCount = lambdaSwitch - (lambdaSwitch % 1);
            int otherAxisCount = lambdaSwitch <= 0 ? _constraintAxisCount + (int)addedCount : _constraintAxisCount + (int)addedCount + 1;
            return otherAxisCount;
        }

        private void ScrollSelectNextGameObject()
        {
            if (m_mouseScrollValue.y != 0 && m_mouseIsInScrollView)
            {
                switch (m_detectedScrollOption)
                {
                    case AutoScrollOptions.Vertical:
                    {
                        switch (m_mouseScrollValue.y > 0)
                        {
                            case true:
                            {
                                MoveToNextObject(m_objectNavigation[m_lastSelectedGameObject].selectOnUp);
                                break;
                            }
                            case false:
                            {
                                MoveToNextObject(m_objectNavigation[m_lastSelectedGameObject].selectOnDown);
                                break;
                            }
                        }
                        break;
                    }
                    case AutoScrollOptions.Horizontal:
                    {
                        switch (m_mouseScrollValue.y > 0)
                        {
                            case true:
                            {
                                MoveToNextObject(m_objectNavigation[m_lastSelectedGameObject].selectOnLeft);
                                break;
                            }
                            case false:
                            {
                                MoveToNextObject(m_objectNavigation[m_lastSelectedGameObject].selectOnRight);
                                break;
                            }
                        }
                        break;
                    }
                    case AutoScrollOptions.Both:
                    {
                        //TODO: MoveToNextObject on AutoScrollOptions.Both.
                        break;
                    }
                    case AutoScrollOptions.None:
                    default:
                        break;
                }
            }
        }

        private void MoveToNextObject(Selectable _nextObject)
        {
            EventSystem.current.SetSelectedGameObject(_nextObject.gameObject);
        }
    }
}