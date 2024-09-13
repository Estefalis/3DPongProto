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
        internal enum AutoScrollOptions
        {
            None,
            Vertical,
            Horizontal,
            Both
        }
        //TODO: Remove '[SerializeField] ' after development, if it's not needed.
        private PlayerInputActions m_playerInputActions;

        [SerializeField] private AutoScrollOptions m_setAutoScrollOption = AutoScrollOptions.Both;
        [SerializeField] private float m_scrollSpeed = 50.0f;
        [Space]
        [SerializeField] private ScrollRect m_scrollViewRect;
        [SerializeField] private RectTransform m_scrollContentRT;
        [Space]
        [SerializeField] private LayoutGroup m_layoutGroup;
        [SerializeField] private int m_contentChildIndex;
        [SerializeField] private Vector2 m_unmaskedContentRT;
        [SerializeField] private Vector2 m_maskedScrollWindow;
        [SerializeField] private Vector2 m_firstChildRT;
        [Space]
        [SerializeField] private int m_leftPadding;
        [SerializeField] private int m_rightPadding;
        [SerializeField] private int m_topPadding;
        [SerializeField] private int m_bottomPadding;
        [Space]
        [SerializeField] private float m_horizontalSpacing;
        [SerializeField] private float m_verticalSpacing;

        private bool m_canAutoScroll = false, m_scrollContentSet;

        private Vector2 m_mouseScrollValue, m_mousePosition;

        private GameObject m_lastSelectedGameObject;
        private RectTransform m_scrollViewRectTransform;
        private Vector2 m_contentAnchoredPos;   //OriginPos' of ScrollContent.
        private RectTransform m_childRect;      //ChildRect for each chilc of the Content and it's '.anchoredPosition'.
        private GridLayoutGroup.Constraint m_gridConstraint;

        private Dictionary<GameObject, RectTransform> m_contentChildAnchorPos = new Dictionary<GameObject, RectTransform>();

        private void Awake()
        {
            m_lastSelectedGameObject = null;
            m_contentChildAnchorPos.Clear();

            m_scrollViewRect = GetComponent<ScrollRect>();
            m_scrollViewRectTransform = m_scrollViewRect.GetComponent<RectTransform>();
            m_scrollContentRT = m_scrollViewRect.content.GetComponent<RectTransform>();
            m_contentAnchoredPos = m_scrollContentRT.anchoredPosition;

            m_scrollContentSet = m_scrollViewRect != null && m_scrollContentRT != null;

            GetAutoScrollOptions(m_scrollViewRect);
        }

        private void Start()
        {
            if (m_scrollContentRT != null)
            {
                ContentLevelIterations();
            }

            GetLayoutGroupSettings(m_layoutGroup);  //Requires 'ContentLevelIterations()' 'm_contentChildIndex', if m_totalSpacings are wanted.

            m_playerInputActions = InputManager.m_playerInputActions;
            m_playerInputActions.UI.Enable();
        }

        private void OnDisable()
        {
            m_playerInputActions.UI.Disable();
        }

        private void Update()
        {
            //m_mouseScrollValue = m_playerInputActions.UI.ScrollWheel.ReadValue<Vector2>();
            //m_mousePosition = m_playerInputActions.UI.MousePosition.ReadValue<Vector2>();
            //Debug.Log(m_mousePosition);
            AutoScrollToNextGameObject();
            UpdateCurrentGameObject();
        }

        /// <summary>
        /// If Slider, Toggle or Button GameObjects are found in the ScrollView, they will get added to the 'm_scrollViewGameObjects' List.
        /// </summary>
        /// <param name="_transformLevel"></param>
        private void ScrollViewObjectsToDict(Transform _transformLevel, RectTransform _contentElementAnchorPos)
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
                    m_setAutoScrollOption = AutoScrollOptions.Both;
                    m_layoutGroup = GetComponent<GridLayoutGroup>();
                    break;
                }
                case false:
                {
                    switch (verticalScrolling)
                    {
                        case true:
                        {
                            m_setAutoScrollOption = AutoScrollOptions.Vertical;
                            m_layoutGroup = m_scrollContentRT.GetComponent<VerticalLayoutGroup>();
                            break;
                        }
                        case false:
                        {
                            switch (horizontalScrolling)
                            {
                                case true:
                                {
                                    m_setAutoScrollOption = AutoScrollOptions.Horizontal;
                                    m_layoutGroup = m_scrollContentRT.GetComponent<HorizontalLayoutGroup>();
                                    break;
                                }
                                case false:
                                {
                                    m_setAutoScrollOption = AutoScrollOptions.None;
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

            //if '(m_layoutGroup == null)' and you want to add one (WITH detailed memberValues to set it's dimensions):
            //'AddMissingLayoutGroup()' - 'switch (m_setAutoScrollOption)' - 'case AutoScrollOptions.Both/.Vertical/.Horizontal' -
            //'m_layoutGroup = m_scrollContentRT.AddComponent<GridLayoutGroup/VerticalLayoutGroup/HorizontalLayoutGroup>()' and
            //'default: break;' - for savety. (It would currently go too far.)
        }

        /// <summary>
        /// Searches Content, of the currently active ScrollView, for GameObjects in each Parent/Child Level of the Hierarchy.
        /// </summary>
        private void ContentLevelIterations()
        {
            m_contentChildIndex = 0;
            foreach (Transform transform in m_scrollContentRT.transform)
            {
                m_contentChildIndex += 1;
                m_childRect = transform.GetComponent<RectTransform>();

                //Level for X/Y Axis Toggles.
                for (int i = 0; i < transform.childCount; i++)
                {
#if UNITY_EDITOR
                    //Debug.Log(transform.GetChild(i).name);
#endif
                    Transform subLevelOne = transform.GetChild(i);
                    ScrollViewObjectsToDict(subLevelOne, m_childRect);            //'m_childRect.anchoredPosition' instead of i.

                    ////Level for SliderXAxis Lower & Higher Buttons, XSlider itself, it's Toggle.
                    ////Level for SliderYAxis Lower & Higher Buttons, YSlider itself, it's Toggle.
                    ////Level for ResetButtons for KeyRebinds.
                    for (int j = 0; j < subLevelOne.childCount; j++)
                    {
#if UNITY_EDITOR
                        //Debug.Log(subLevelOne.GetChild(j).name);
#endif
                        Transform subLevelTwo = subLevelOne.GetChild(j);
                        ScrollViewObjectsToDict(subLevelTwo, m_childRect);        //'m_childRect.anchoredPosition' instead of i.

                        //Level for Keyboard & Gamepad Rebind Buttons.
                        for (int k = 0; k < subLevelTwo.childCount; k++)
                        {
#if UNITY_EDITOR
                            //Debug.Log(subLevelTwo.GetChild(k).name);
#endif
                            Transform subLevelThree = subLevelTwo.GetChild(k);
                            ScrollViewObjectsToDict(subLevelThree, m_childRect);  //'m_childRect.anchoredPosition' instead of i.
                        }
                    }
                }
            }
        }

        private void GetLayoutGroupSettings(LayoutGroup m_layoutGroup)
        {
            switch (m_setAutoScrollOption)
            {
                case AutoScrollOptions.Both:
                {
                    var padding = m_layoutGroup./*GetComponent<GridLayoutGroup>().*/padding;            //Space at LayoutGroup borders.
                    m_topPadding = padding.top;
                    m_bottomPadding = padding.bottom;
                    m_leftPadding = padding.left;
                    m_rightPadding = padding.right;
                    var gridSpacing = m_layoutGroup.GetComponent<GridLayoutGroup>().spacing;            //Spacing between elements.
                    m_horizontalSpacing = gridSpacing.x;
                    m_verticalSpacing = gridSpacing.y;

                    m_gridConstraint = m_layoutGroup.GetComponent<GridLayoutGroup>().constraint;
                    switch (m_gridConstraint)   //TODO: Get Column/Row childCounts, depending on 'm_gridConstraint's.
                    {
                        case GridLayoutGroup.Constraint.Flexible:
                            break;
                        case GridLayoutGroup.Constraint.FixedColumnCount:
                            break;
                        case GridLayoutGroup.Constraint.FixedRowCount:
                            break;
                        default:
                            break;
                    }
                    break;
                }
                case AutoScrollOptions.Vertical:
                {
                    var padding = m_layoutGroup./*GetComponent<VerticalLayoutGroup>().*/padding;        //Space at LayoutGroup borders.
                    m_topPadding = padding.top;
                    m_bottomPadding = padding.bottom;
                    m_verticalSpacing = m_layoutGroup.GetComponent<VerticalLayoutGroup>().spacing;      //Spacing between Elements.
                    break;
                }
                case AutoScrollOptions.Horizontal:
                {
                    var padding = m_layoutGroup./*GetComponent<HorizontalLayoutGroup>().*/padding;      //Space at LayoutGroup borders.
                    m_leftPadding = padding.left;
                    m_rightPadding = padding.right;
                    m_horizontalSpacing = m_layoutGroup.GetComponent<HorizontalLayoutGroup>().spacing;  //Spacing between Elements.
                    break;
                }
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
            if (m_lastSelectedGameObject == null)   //Dicts don't allow null on keys. And just 'return;' disables AutoScrolling.
                m_lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;

            if (m_lastSelectedGameObject != EventSystem.current.currentSelectedGameObject)
            {
                m_lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;

                #region Dict switch
                switch (m_contentChildAnchorPos.ContainsKey(m_lastSelectedGameObject))
                {
                    case true:
                    {
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
        }

        /// <summary>
        /// AutoScrolls to the next element, if the ScrollView and it's content are not null and the next element is part of the ScrollView.
        /// </summary>
        private void AutoScrollToNextGameObject()
        {
            if (!m_scrollContentSet || !m_canAutoScroll/* || Cursor.lockState == CursorLockMode.None*/) //TODO: Implement Mouse (In)Visibility change.
                return;

            switch (m_setAutoScrollOption)
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
    }
}