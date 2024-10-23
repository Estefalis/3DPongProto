using System.Collections.Generic;
using ThreeDeePongProto.Shared.HelperClasses;
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
        private enum DetectedScrollOption
        {
            None,
            Vertical,
            Horizontal,
            Both
        }

        //TODO: Remove '[SerializeField] ' after development, if it's not needed.
        private PlayerInputActions m_playerInputActions;

        [SerializeField] private DetectedScrollOption m_detectedScrollOption = DetectedScrollOption.Both;
        [Header("ScrollView Components")]
        [SerializeField] private ScrollRect m_scrollViewRect;
        [SerializeField] private RectTransform m_scrollViewContent;
        [SerializeField] private LayoutGroup m_layoutGroup;
        [SerializeField] private int m_contentChildCount;
        [Space]
        [SerializeField] private Vector2 m_maskedScrollWindow;      //Fix (masked) Width & Height
        [SerializeField] private Vector2 m_fullContentWindow;       //Full Width & Height.
        [Space]
        [SerializeField] private float m_scrollSpeed = 60.0f;
        [SerializeField] private float m_setScrollSensitivity = 10.0f;

        [Header("Prefab")]
        [SerializeField] private bool m_instantiatedContent = false;
        [SerializeField] private GameObject m_spawnablePrefab = null;
        [SerializeField] private int m_setChildAmount = 50;
        [SerializeField] private bool m_childsSpawned = false;

        private int m_leftPadding;
        private int m_rightPadding;
        private int m_topPadding;
        private int m_bottomPadding;
        private float m_horizontalSpacing;
        private float m_verticalSpacing;

        private bool m_canAutoScroll = false, m_scrollContentSet;
        private bool m_edgePosition = false;
        private bool m_startEdgeVer = false, m_endEdgeVer = false;
        private bool m_startEdgeHor = false, m_endEdgeHor = false;

        private bool m_mouseIsInScrollView;
        private Vector2 m_mouseScrollValue, m_mousePosition;

        private Vector2Int m_gridSize;
        private Vector2 m_firstChildRT;

        private GameObject m_lastSelectedGameObject, m_fallbackGameObject;
        internal RectTransform m_scrollViewRectTransform;
        private RectTransform m_childRect;      //Rect for each child of the Content and it's '.anchoredPosition'.

        private Dictionary<GameObject, RectTransform> m_contentChildAnchorPos = new Dictionary<GameObject, RectTransform>();
        private Dictionary<GameObject, Navigation> m_objectNavigation = new Dictionary<GameObject, Navigation>();

        private void Awake()
        {
            GetScrollViewComponents();
            GetScrollOptionAndLayout(m_scrollViewRect);

            m_scrollContentSet = m_scrollViewRect != null && m_scrollViewContent != null;
            if (m_scrollContentSet && m_instantiatedContent && !m_childsSpawned && m_spawnablePrefab != null)
                SpawnContentChildren();

            GetLayoutGroupSettings(m_layoutGroup);     //Gets childCount AFTER Instantiation.

            m_contentChildAnchorPos.Clear();
            m_objectNavigation.Clear();
            m_lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
            m_fallbackGameObject = m_lastSelectedGameObject;
        }

        private void Start()
        {
            m_scrollViewRect.scrollSensitivity = m_setScrollSensitivity;

            m_playerInputActions = InputManager.m_PlayerInputActions;
            m_playerInputActions.UI.Enable();

            if (m_scrollViewContent != null && m_contentChildCount > 0)
                ContentLevelIterations();
        }

        private void OnDisable()
        {
            m_playerInputActions.UI.Disable();
        }

        private void Update()
        {
            GetMouseValues();

            UpdateCurrentGameObject();
            AutoScrollToNextGameObject();
        }

        private void GetScrollViewComponents()
        {
            m_scrollViewRect = GetComponent<ScrollRect>();
            m_scrollViewRectTransform = m_scrollViewRect.GetComponent<RectTransform>();
            m_scrollViewContent = m_scrollViewRect.content.GetComponent<RectTransform>();
        }

        private void SpawnContentChildren()
        {
            for (int i = 0; i < m_setChildAmount; i++)
                Instantiate(m_spawnablePrefab, m_scrollViewContent);

            m_childsSpawned = true;
        }

        /// <summary>
        /// Gets ScrollOption and LayoutGroup automatic. ScrollOption depends on the availabilities of connected scrollbars and their corresponding bool states.
        /// </summary>
        /// <param name="_targetScrollRect"></param>
        private void GetScrollOptionAndLayout(ScrollRect _targetScrollRect)
        {
            bool verticalScrolling = _targetScrollRect.verticalScrollbar != null && _targetScrollRect.vertical;
            bool horizontalScrolling = _targetScrollRect.horizontalScrollbar != null && _targetScrollRect.horizontal;

            bool bothScrollDirections = verticalScrolling && horizontalScrolling;

            switch (bothScrollDirections)
            {
                case true:
                {
                    m_detectedScrollOption = DetectedScrollOption.Both;
                    break;
                }
                case false:
                {
                    switch (verticalScrolling)
                    {
                        case true:
                        {
                            m_detectedScrollOption = DetectedScrollOption.Vertical;
                            break;
                        }
                        case false:
                        {
                            switch (horizontalScrolling)
                            {
                                case true:
                                {
                                    m_detectedScrollOption = DetectedScrollOption.Horizontal;
                                    break;
                                }
                                case false:
                                {
                                    m_detectedScrollOption = DetectedScrollOption.None;
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    break;
                }
            }

            m_layoutGroup = m_scrollViewContent.GetComponent<LayoutGroup>();
        }

        private void GetLayoutGroupSettings(LayoutGroup _layoutGroup)
        {
            switch (_layoutGroup)
            {
                case GridLayoutGroup:
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

                    m_gridSize = CustomGridLayoutSetup.GetGridSize(gridSettings);
                    break;
                }
                case VerticalLayoutGroup:
                {
                    var padding = _layoutGroup./*GetComponent<VerticalLayoutGroup>().*/padding;        //Space at LayoutGroup borders.
                    m_topPadding = padding.top;
                    m_bottomPadding = padding.bottom;
                    m_verticalSpacing = _layoutGroup.GetComponent<VerticalLayoutGroup>().spacing;      //Spacing between Elements.
                    break;
                }
                case HorizontalLayoutGroup:
                {
                    var padding = _layoutGroup./*GetComponent<HorizontalLayoutGroup>().*/padding;      //Space at LayoutGroup borders.
                    m_leftPadding = padding.left;
                    m_rightPadding = padding.right;
                    m_horizontalSpacing = _layoutGroup.GetComponent<HorizontalLayoutGroup>().spacing;  //Spacing between Elements.
                    break;
                }
                default:
                    break;
            }

            m_maskedScrollWindow = new Vector2(m_scrollViewRectTransform.rect.width, m_scrollViewRectTransform.rect.height);
            m_fullContentWindow = new Vector2(m_scrollViewContent.rect.width, m_scrollViewContent.rect.height);   //.x - .width, .y - .height.
            m_contentChildCount = m_layoutGroup.transform.childCount;

            if (m_contentChildCount > 0)
            {
                var firstChildRect = m_scrollViewContent.GetChild(0).GetComponent<RectTransform>().rect;
                m_firstChildRT = new Vector2(firstChildRect.width, firstChildRect.height);
            }
        }

        #region ContentLevelIterations
        /// <summary>
        /// Searches Content of the currently active ScrollView with nested for-loops, to fill Dictionaries with AnchoredPositions and Navigation Informations of the contained GameObjects.
        /// </summary>
        private void ContentLevelIterations()
        {
            //m_contentChildCount = 0;
            foreach (Transform transform in m_scrollViewContent.transform)
            {
                //m_contentChildCount += 1;
                m_childRect = transform.GetComponent<RectTransform>();
                //Very first childLevel of 'm_scrollViewContent.transform'.
                ScrollViewObjectsToDicts(transform, m_childRect);

                //Level for X/Y Axis Toggles.
                for (int i = 0; i < transform.childCount; i++)
                {
#if UNITY_EDITOR
                    //Debug.Log(transform.GetChild(i).name);
#endif
                    Transform subLevelOne = transform.GetChild(i);
                    ScrollViewObjectsToDicts(subLevelOne, m_childRect);             //'m_childRect.anchoredPosition' instead of i.

                    ////Level for SliderXAxis Lower & Higher Buttons, XSlider itself, it's Toggle.
                    ////Level for SliderYAxis Lower & Higher Buttons, YSlider itself, it's Toggle.
                    ////Level for ResetButtons for KeyRebinds.
                    for (int j = 0; j < subLevelOne.childCount; j++)
                    {
#if UNITY_EDITOR
                        //Debug.Log(subLevelOne.GetChild(j).name);
#endif
                        Transform subLevelTwo = subLevelOne.GetChild(j);
                        ScrollViewObjectsToDicts(subLevelTwo, m_childRect);         //'m_childRect.anchoredPosition' instead of i.

                        //Level for Keyboard & Gamepad Rebind Buttons.
                        for (int k = 0; k < subLevelTwo.childCount; k++)
                        {
#if UNITY_EDITOR
                            //Debug.Log(subLevelTwo.GetChild(k).name);
#endif
                            Transform subLevelThree = subLevelTwo.GetChild(k);
                            ScrollViewObjectsToDicts(subLevelThree, m_childRect);   //'m_childRect.anchoredPosition' instead of i.
                        }
                    }
                }
            }
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

                    switch (m_instantiatedContent)
                    {
                        case true:
                        {
                            //TODO:
                            //SetupObjectNavigation(toggle.gameObject);
                            //- contentTransform.childCount fuer Navigation an EndObject bei Index 0 und umgekehrt (inkl. .GetComponent)
                            //- je nach Hor/Ver/Grid, Up/Down &| Left/Right mit eigenen Methoden/Code. (Grid mit constaintAxisCount)
                            break;
                        }
                        case false:
                        {
                            m_objectNavigation.Add(toggle.gameObject, toggle.navigation);
                            break;
                        }
                    }
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

                    switch (m_instantiatedContent)
                    {
                        case true:
                        {
                            //TODO:
                            //SetupObjectNavigation(slider.gameObject);
                            //- contentTransform.childCount fuer Navigation an EndObject bei Index 0 und umgekehrt (inkl. .GetComponent)
                            //- je nach Hor/Ver/Grid, Up/Down &| Left/Right mit eigenen Methoden/Code. (Grid mit constaintAxisCount)
                            break;
                        }
                        case false:
                        {
                            m_objectNavigation.Add(slider.gameObject, slider.navigation);
                            break;
                        }
                    }
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

                    switch (m_instantiatedContent)
                    {
                        case true:
                        {
                            //TODO:
                            //SetupObjectNavigation(button.gameObject);

                            //- contentTransform.childCount fuer Navigation an EndObject bei Index 0 und umgekehrt (inkl. .GetComponent)
                            //- je nach Hor/Ver/Grid, Up/Down &| Left/Right mit eigenen Methoden/Code. (Grid mit constaintAxisCount)
                            break;
                        }
                        case false:
                        {
                            m_objectNavigation.Add(button.gameObject, button.navigation);
                            break;
                        }
                    }
                    break;
                }
            }
        }
        #endregion

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
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_scrollViewRectTransform, _mousePosition, null, out Vector2 localMousePosition);

            Vector2 rectSize = m_scrollViewRectTransform.rect.size;

            return localMousePosition.x >= -rectSize.x / 2 && localMousePosition.x <= rectSize.x / 2 &&
                localMousePosition.y >= -rectSize.y / 2 && localMousePosition.y <= rectSize.y / 2;
        }
        #endregion

        #region AutoScrollToNextGameObject
        /// <summary>
        /// AutoScrolls to the next element, if the ScrollView and it's content are not null and the next element is part of the ScrollView.
        /// </summary>
        private void AutoScrollToNextGameObject()
        {
            if (!m_scrollContentSet || !m_canAutoScroll/* || Cursor.lockState == CursorLockMode.None*/)
            {
                if (m_scrollViewRect.scrollSensitivity != m_setScrollSensitivity)
                    m_scrollViewRect.scrollSensitivity = m_setScrollSensitivity;
                return;
            }

            //TODO: Implement Gamepad Mouse.
            ScrollSelectNextGameObject();

            switch (m_detectedScrollOption)
            {
                case DetectedScrollOption.Vertical:
                    UpdateVerticalScrollPosition(m_contentChildAnchorPos[m_lastSelectedGameObject]);
                    break;
                case DetectedScrollOption.Horizontal:
                    UpdateHorizontalScrollPosition(m_contentChildAnchorPos[m_lastSelectedGameObject]);
                    break;
                case DetectedScrollOption.Both:
                    UpdateVerticalScrollPosition(m_contentChildAnchorPos[m_lastSelectedGameObject]);
                    UpdateHorizontalScrollPosition(m_contentChildAnchorPos[m_lastSelectedGameObject]);
                    break;
                case DetectedScrollOption.None:
                default:
                    break;
            }
        }

        private void ScrollSelectNextGameObject()
        {
            if (m_mouseScrollValue.y != 0 && m_mouseIsInScrollView)
            {
                switch (m_detectedScrollOption)
                {
                    case DetectedScrollOption.Vertical:
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
                    case DetectedScrollOption.Horizontal:
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
                    case DetectedScrollOption.Both:
                    {
                        //TODO: MoveToNextObject on DetectedScrollOption.Both.
                        break;
                    }
                    case DetectedScrollOption.None:
                    default:
                        break;
                }
            }
        }

        private void MoveToNextObject(Selectable _nextObject)
        {
            switch (_nextObject == null)
            {
                case true:
                    return;
                case false:
                    EventSystem.current.SetSelectedGameObject(_nextObject.gameObject);
                    break;
            }
        }

        private void UpdateVerticalScrollPosition(RectTransform _selectedElement)
        {
            //Move the current scroll rect to correct elementPosition           //min: -57 - max: 0
            float elementPosition = -_selectedElement.anchoredPosition.y - (_selectedElement.rect.height * (1 - _selectedElement.pivot.y) - m_topPadding - m_bottomPadding - m_verticalSpacing);

            float contentElementHeight = _selectedElement.rect.height;              //Child Height
            float maskedWindowHeight = m_maskedScrollWindow.y;                      //yVector = height of masked ContentScrollWindow
            float viewRectAnchorPos = m_scrollViewRectTransform.anchoredPosition.y; //0 - fixed ScrollView AnchorPosition

            //Get the element offset value depending on the cursor move direction.
            float offlimitsValue = GetScrollOffset(elementPosition, contentElementHeight, maskedWindowHeight, viewRectAnchorPos);

            //Get the normalized position, based on the TargetScrollRect's height.
            float normalizedPosition = m_scrollViewRect.verticalNormalizedPosition + (offlimitsValue / m_scrollViewRectTransform.rect.height);

            if (offlimitsValue < 0)
            {
                normalizedPosition -= Mathf.Abs(offlimitsValue) / m_scrollViewRectTransform.rect.height;    //Scroll down.
            }

            if (offlimitsValue > 0)
            {
                normalizedPosition += offlimitsValue / m_scrollViewRectTransform.rect.height;               //Scroll up.
            }

            //Clamp the normalized Position to ensure, that it stays within the valid bound of (0 ... 1).
            normalizedPosition = Mathf.Clamp01(normalizedPosition);
            //Move the targetScrollRect to the new position with 'SmoothStep'.
            m_scrollViewRect.verticalNormalizedPosition = Mathf.SmoothStep(m_scrollViewRect.verticalNormalizedPosition, normalizedPosition, Time.unscaledDeltaTime * m_scrollSpeed);
#if UNITY_EDITOR
            //Debug.Log($"OffValue: {offlimitsValue} | NormalizedPos: {normalizedPosition} | Calc: {offlimitsValue / m_scrollViewRectTransform.rect.height} | AbsCalc: {Mathf.Abs(offlimitsValue) / m_scrollViewRectTransform.rect.height}");
#endif
        }

        private void UpdateHorizontalScrollPosition(RectTransform _selectedElement)
        {
            #region Own
            //            //Move the current scroll rect to correct elementPosition           //min: -57 - max: 0
            //            float elementPosition = _selectedElement.anchoredPosition.x - (_selectedElement.rect.width * (1 - _selectedElement.pivot.x) - m_leftPadding - m_rightPadding - m_horizontalSpacing);

            //            float viewRectAnchorPos = m_scrollViewRectTransform.anchoredPosition.x; //0   - fixed ScrollView AnchorPosition.
            //            float contentElementWidth = _selectedElement.rect.width;                //Child Width.
            //            float maskedWindowWidth = m_maskedScrollWindow.x;                       //xVector = width of masked ContentScrollWindow.

            //            //Get the element offset value depending on the cursor move direction.
            //            float offlimitsValue = GetScrollOffset(elementPosition, viewRectAnchorPos, contentElementWidth, maskedWindowWidth);

            //            //Get the normalized position, based on the TargetScrollRect's width.
            //            float normalizedPosition = m_scrollViewRect.horizontalNormalizedPosition/* + (offlimitsValue / m_scrollViewRectTransform.rect.width)*/;

            //            if (offlimitsValue > 0)
            //            {
            //                normalizedPosition -= offlimitsValue / m_scrollViewRectTransform.rect.width;               //Scroll Left.
            //            }

            //            if (offlimitsValue < 0)
            //            {
            //                normalizedPosition += Mathf.Abs(offlimitsValue) / m_scrollViewRectTransform.rect.width;    //Scroll right.
            //            }

            //            //Clamp the normalized Position to ensure, that it stays within the valid bound of (0 ... 1).
            //            normalizedPosition = Mathf.Clamp01(normalizedPosition);
            //            //Move the targetScrollRect to the new position with 'SmoothStep'.
            //            m_scrollViewRect.horizontalNormalizedPosition = Mathf.SmoothStep(m_scrollViewRect.horizontalNormalizedPosition, normalizedPosition, Time.unscaledDeltaTime * m_scrollSpeed);
            //#if UNITY_EDITOR
            //            //AbsCalc +, while normal Calc -.
            //            Debug.Log($"OffValue: {offlimitsValue} | HoriBar: {m_scrollViewRect.horizontalNormalizedPosition} | NormalizedPos: {normalizedPosition} | Calc: {offlimitsValue / m_scrollViewRectTransform.rect.width} | AbsCalc: {Mathf.Abs(offlimitsValue) / m_scrollViewRectTransform.rect.width}");
            //#endif
            #endregion

            //Move the current scroll rect to correct elementPosition                   //min: -57 - max: 0
            float elementPosition = -_selectedElement.anchoredPosition.x - (_selectedElement.rect.width * (1 - _selectedElement.pivot.x) - m_leftPadding - m_rightPadding - m_horizontalSpacing);

            float elementWidth = _selectedElement.rect.width;                           //Child Width
            float maskedWindowWidth = m_maskedScrollWindow.x;                           //xVector = width of masked ContentScrollWindow
            float viewRectAnchorPos = -m_scrollViewRectTransform.anchoredPosition.x;    //0 - fixed ScrollView AnchorPosition

            //Get the element offset value depending on the cursor move direction.
            float offlimitsValue = -GetScrollOffset(elementPosition, elementWidth, maskedWindowWidth, viewRectAnchorPos);

            //Get the normalized position, based on the TargetScrollRect's height.
            float normalizedPosition = m_scrollViewRect.horizontalNormalizedPosition + (offlimitsValue / m_scrollViewRectTransform.rect.width);

            if (offlimitsValue < 0)
            {
                normalizedPosition -= Mathf.Abs(offlimitsValue) / m_scrollViewRectTransform.rect.width;    //Scroll ?.
            }

            if (offlimitsValue > 0)
            {
                normalizedPosition += offlimitsValue / m_scrollViewRectTransform.rect.width;               //Scroll ?.
            }

            //Clamp the normalized Position to ensure, that it stays within the valid bound of (0 ... 1).
            normalizedPosition = Mathf.Clamp01(normalizedPosition);

            //Move the targetScrollRect to the new position with 'SmoothStep'.
            m_scrollViewRect.horizontalNormalizedPosition = Mathf.SmoothStep(m_scrollViewRect.horizontalNormalizedPosition, normalizedPosition, Time.unscaledDeltaTime * m_scrollSpeed);
        }

        private float GetScrollOffset(float _elementPosition, float _elementWidthOrHeight, float _windowWidthOrHeight, float _viewRectAnchorPos)
        {
            if (_elementPosition < _viewRectAnchorPos + (_elementWidthOrHeight / 2))
            {
                return _viewRectAnchorPos + _windowWidthOrHeight - (_elementPosition - _elementWidthOrHeight);
            }
            else if (_elementPosition + _elementWidthOrHeight > _viewRectAnchorPos + _windowWidthOrHeight)
            {
                return _viewRectAnchorPos + _windowWidthOrHeight - (_elementPosition + _elementWidthOrHeight);
            }

            return 0;
        }
        #endregion

        #region UpdateCurrentGameObject
        /// <summary>
        /// Updates bool 'm_selectedObjectInScrollView', depending on GameObject related Dicts.
        /// </summary>
        /// <param name="_gameObject"></param>
        private void UpdateCurrentGameObject()
        {
            switch (m_lastSelectedGameObject == null)
            {
                case true:  //Dicts don't allow null on keys. And just 'return;' disables the autoscrolling.
                    if (EventSystem.current.currentSelectedGameObject != null)
                    {
                        m_lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
                        m_fallbackGameObject = m_lastSelectedGameObject;
                    }
                    else
                        m_lastSelectedGameObject = m_fallbackGameObject;
                    break;
                case false:
                {
                    if (m_lastSelectedGameObject != EventSystem.current.currentSelectedGameObject && EventSystem.current.currentSelectedGameObject != null)
                    {
                        m_lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
                        m_fallbackGameObject = m_lastSelectedGameObject;

                        #region Dict switch
                        switch (m_contentChildAnchorPos.ContainsKey(m_lastSelectedGameObject))
                        {
                            case true:
                            {
                                m_edgePosition = MaskedScrollRectEdgeCheck(m_scrollViewRectTransform, m_contentChildAnchorPos[m_lastSelectedGameObject].anchoredPosition);
#if UNITY_EDITOR
                                //Debug.Log($"AtEdgePosition: {m_edgePosition}");
#endif

                                #region Switch helps to keep AutoScroll smooth, while using 'MoveToNextObject()' MouseScrolling.
                                switch (m_edgePosition)
                                {
                                    case true:
                                        m_scrollViewRect.scrollSensitivity = 0.0f;
                                        break;
                                    case false:
                                        m_scrollViewRect.scrollSensitivity = m_setScrollSensitivity;
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
                        #endregion
                    }
#if UNITY_EDITOR
                    //Debug.Log($"LastGO: {m_lastSelectedGameObject.name} - autoScroll: {m_selectedObjectInScrollView}");
#endif
                    break;
                }
            }
        }

        private bool MaskedScrollRectEdgeCheck(RectTransform _scrollViewRect, Vector2 _lastGOAnchor)
        {
            switch (m_detectedScrollOption)
            {
                case DetectedScrollOption.Vertical:
                {
                    m_startEdgeVer = _lastGOAnchor.y + (m_verticalSpacing * m_contentChildCount - 1) + m_topPadding + m_scrollViewContent.anchoredPosition.y >= _scrollViewRect.anchoredPosition.y;
                    m_endEdgeVer = -(_lastGOAnchor.y - m_firstChildRT.y - (m_verticalSpacing * m_contentChildCount - 1) - m_bottomPadding + m_scrollViewContent.anchoredPosition.y) >= _scrollViewRect.rect.height;
                    break;
                }
                case DetectedScrollOption.Horizontal:
                {
                    //var zeroedContentAnchorPos = -m_scrollViewContent.anchoredPosition.x + m_scrollViewContent.anchoredPosition.x;
                    //var fullRectStartEdgeHor = _lastGOAnchor.x - m_firstChildRT.x - (m_horizontalSpacing * m_contentChildCount - 1) - m_leftPadding <= _scrollViewRect.anchoredPosition.x;
                    //var fullRectEndEdgeHor = _lastGOAnchor.x + m_firstChildRT.x + (m_horizontalSpacing * m_contentChildCount - 1) + m_rightPadding >= m_scrollViewContent.rect.width;

                    //TODO: m_startEdgeHor;
                    //TODO: m_endEdgeHor;
                    break;
                }
                case DetectedScrollOption.Both:
                {
                    //TODO: MaskedScrollRectEdgeCheck DetectedScrollOption.Both                    
                    break;
                }
                case DetectedScrollOption.None:
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
        #endregion
    }
}