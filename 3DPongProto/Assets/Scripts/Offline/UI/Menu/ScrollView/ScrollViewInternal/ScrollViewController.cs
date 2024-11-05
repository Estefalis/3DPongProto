using System.Collections.Generic;
using ThreeDeePongProto.Shared.HelperClasses;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeDeePongProto.Offline.UI.Menu.ScrollViews
{
    internal enum ScrollDirection
    {
        None,
        Vertical,
        Horizontal,
        Grid
    }

    internal enum NavigationLevel
    {
        None,
        Simple,
        Nested
    }

    public class ScrollViewController : MonoBehaviour
    {
        private enum ContentFillType
        {
            Filled,
            Instantiated
        }

        [SerializeField] internal AutoScroll m_autoScrolling;

        [SerializeField] internal ScrollDirection m_scrollDirection = ScrollDirection.Grid;
        [SerializeField] private ContentFillType m_contentFillType = ContentFillType.Filled;
        [SerializeField] private NavigationLevel m_navigationLevel = NavigationLevel.Simple;

        [Header("ScrollView Components")]
        [SerializeField] internal ScrollRect m_scrollViewRect;
        [SerializeField] internal RectTransform m_scrollViewContent;
        [SerializeField] private LayoutGroup m_layoutGroup;

        [Header("Prefab Instantiation")]
        [SerializeField] private GameObject m_spawnablePrefab = null;
        [SerializeField] private int m_setChildAmount = 50;
        [SerializeField] private bool m_loopNavigation = false;

        private bool m_setNavigationStarted = false;
        private bool m_childsSpawned = false;
        private Selectable m_simpleSelectable;
        private List<GameObject> m_dictKeys;

        internal int m_leftPadding;
        internal int m_rightPadding;
        internal int m_topPadding;
        internal int m_bottomPadding;
        internal float m_horizontalSpacing;
        internal float m_verticalSpacing;
        //private Vector2 m_cellSize;

        private bool m_gotComponents;
        internal bool ContentChildrenSet { get => m_contentChildrenSet; }
        internal bool ObjectNavigationSet { get => m_objectNavigationSet; }
        private bool m_contentChildrenSet = false, m_objectNavigationSet = false;  //or 'internal static Action<bool> ContentFilled/Set;'

        /*[SerializeField] */
        private Vector2 m_maskedScrollWindow;      //Fix (masked) Width & Height
        /*[SerializeField] */
        private Vector2 m_fullContentWindow;       //Full Width & Height.
        /*[SerializeField] */
        private int m_contentChildCount;

        internal RectTransform m_scrollViewRectTransform;
        private RectTransform m_childRect;      //Rect for each child of the Content and it's '.anchoredPosition'.
        internal Vector2 m_firstChildRT;

        private GridLayoutGroup m_gridSettings;
        //[SerializeField]
        private Vector2Int m_gridSize;

        internal Dictionary<GameObject, RectTransform> m_contentChildAnchorPos = new Dictionary<GameObject, RectTransform>();
        internal Dictionary<GameObject, Navigation> m_objectNavigation = new Dictionary<GameObject, Navigation>();

        private void Awake()
        {
            m_contentChildAnchorPos.Clear();
            m_objectNavigation.Clear();

            GetScrollViewComponents();
            GetScrollOptionAndLayout(m_scrollViewRect);

            m_gotComponents = m_scrollViewRect != null && m_scrollViewContent != null;

            GetLayoutGroupSettings(m_layoutGroup);
            SetContentFillType();
        }

        private void OnEnable()
        {
            ContentLevelIterations();
        }

        private void OnDisable()
        {
            m_contentChildAnchorPos.Clear();
            m_objectNavigation.Clear();
        }

        private void GetScrollViewComponents()
        {
            m_scrollViewRect = GetComponent<ScrollRect>();
            m_scrollViewRectTransform = m_scrollViewRect.GetComponent<RectTransform>();
            m_scrollViewContent = m_scrollViewRect.content.GetComponent<RectTransform>();
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
                    m_scrollDirection = ScrollDirection.Grid;
                    break;
                }
                case false:
                {
                    switch (verticalScrolling)
                    {
                        case true:
                        {
                            m_scrollDirection = ScrollDirection.Vertical;
                            break;
                        }
                        case false:
                        {
                            switch (horizontalScrolling)
                            {
                                case true:
                                {
                                    m_scrollDirection = ScrollDirection.Horizontal;
                                    break;
                                }
                                case false:
                                {
                                    m_scrollDirection = ScrollDirection.None;
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
                    m_gridSettings = _layoutGroup.GetComponent<GridLayoutGroup>();
                    //Space at LayoutGroup borders.
                    m_topPadding = m_gridSettings.padding.top;
                    m_bottomPadding = m_gridSettings.padding.bottom;
                    m_leftPadding = m_gridSettings.padding.left;
                    m_rightPadding = m_gridSettings.padding.right;
                    //Spacing between elements.
                    m_horizontalSpacing = m_gridSettings.spacing.x;
                    m_verticalSpacing = m_gridSettings.spacing.y;
                    //m_cellSize = m_gridSettings.cellSize;

                    if (m_contentFillType == ContentFillType.Filled)
                        m_gridSize = CustomGridLayoutSetup.GetGridSize(m_gridSettings);
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

            m_contentChildCount = m_scrollViewContent.childCount;
        }

        private void SetContentFillType()
        {
            if (m_gotComponents)
            {
                switch (m_contentChildCount == 0 && !m_childsSpawned && m_spawnablePrefab != null)
                {
                    case true:
                    {
                        m_contentFillType = ContentFillType.Instantiated;
                        SpawnContentChildren();
                        break;
                    }
                    case false:
                    {
                        m_contentFillType = ContentFillType.Filled;
                        break;
                    }
                }
            }
        }

        private void SpawnContentChildren()
        {
            int navObjCount = 0;

            bool containsToggle = m_spawnablePrefab.TryGetComponent(out Toggle toggle);
            bool containsSlider = m_spawnablePrefab.TryGetComponent(out Slider slider);
            bool containsButton = m_spawnablePrefab.TryGetComponent(out Button button);

            foreach (Transform child in m_spawnablePrefab.transform)
            {
                if (containsToggle)
                    navObjCount++;
                if (containsSlider)
                    navObjCount++;
                if (containsButton)
                    navObjCount++;
            }

            switch (navObjCount)    //determine how many NavigationObjects the contentChild Prefab has on each one.
            {
                case 0:
                    m_navigationLevel = NavigationLevel.None;
                    return;
                case 1:
                {
                    m_navigationLevel = NavigationLevel.Simple;

                    if (containsToggle)
                        m_simpleSelectable = toggle;
                    if (containsSlider)
                        m_simpleSelectable = slider;
                    if (containsButton)
                        m_simpleSelectable = button;

                    break;
                }
                default:
                {
                    m_navigationLevel = NavigationLevel.Nested;
                    break;
                }
            }

            for (int i = 0; i < m_setChildAmount; i++)
            {
                m_spawnablePrefab.GetComponentInChildren<TextMeshProUGUI>().text = $"Btn-ID {i}";
                Instantiate(m_spawnablePrefab, m_scrollViewContent);
            }

            m_childsSpawned = true;
            m_contentChildCount = m_scrollViewContent.childCount;
        }

        /// <summary>
        /// Searches Content of the currently active ScrollView with nested for-loops, to fill Dictionaries with AnchoredPositions and Navigation Informations of the contained GameObjects.
        /// </summary>
        private void ContentLevelIterations()
        {
            switch (m_contentChildCount > 0)
            {
                case true:
                {
                    var firstChildRect = m_scrollViewContent.GetChild(0).GetComponent<RectTransform>().rect;
                    m_firstChildRT = new Vector2(firstChildRect.width, firstChildRect.height);

                    m_contentChildrenSet = true;
                    break;
                }
                case false:
                    m_contentChildrenSet = false;
                    return;
            }

            foreach (Transform transform in m_scrollViewContent.transform)
            {
                m_childRect = transform.GetComponent<RectTransform>();

                //Very 1st childLevel.
                GetScrollViewObjects(transform, m_childRect);

                //for-loop for 2nd childLevel.
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform subLevelOne = transform.GetChild(i);
                    GetScrollViewObjects(subLevelOne, m_childRect);

                    //for-loop for 3rd childLevel.
                    for (int j = 0; j < subLevelOne.childCount; j++)
                    {
                        Transform subLevelTwo = subLevelOne.GetChild(j);
                        GetScrollViewObjects(subLevelTwo, m_childRect);

                        //for-loop for 4th childLevel.
                        for (int k = 0; k < subLevelTwo.childCount; k++)
                        {
                            Transform subLevelThree = subLevelTwo.GetChild(k);
                            GetScrollViewObjects(subLevelThree, m_childRect);
                        }
                    }
                }
            }

            m_objectNavigationSet = true;
        }

        private void GetScrollViewObjects(Transform _transformLevel, RectTransform _contentElementAnchorPos)
        {
            bool containsToggle = _transformLevel.TryGetComponent(out Toggle toggle);
            bool containsSlider = _transformLevel.TryGetComponent(out Slider slider);
            bool containsButton = _transformLevel.TryGetComponent(out Button button);

            switch (m_contentFillType)
            {
                case ContentFillType.Filled:
                {
                    if (containsToggle)
                    {
                        m_contentChildAnchorPos.Add(toggle.gameObject, _contentElementAnchorPos);
                        m_objectNavigation.Add(toggle.gameObject, toggle.navigation);
                    }

                    if (containsSlider)
                    {
                        m_contentChildAnchorPos.Add(slider.gameObject, _contentElementAnchorPos);
                        m_objectNavigation.Add(slider.gameObject, slider.navigation);
                    }

                    if (containsButton)
                    {
                        m_contentChildAnchorPos.Add(button.gameObject, _contentElementAnchorPos);
                        m_objectNavigation.Add(button.gameObject, button.navigation);
                    }
                    break;
                }
                case ContentFillType.Instantiated:
                {
                    //TODO: Set Up/Down/Left/Right - ObjectNavigation for Hor/Ver/Grid Layouts. Including loop between index 0 - lastChild index.
                    if (containsToggle)
                        m_contentChildAnchorPos.Add(toggle.gameObject, _contentElementAnchorPos);

                    if (containsSlider)
                        m_contentChildAnchorPos.Add(slider.gameObject, _contentElementAnchorPos);

                    if (containsButton)
                        m_contentChildAnchorPos.Add(button.gameObject, _contentElementAnchorPos);

                    if (m_scrollViewContent.childCount < 2)
                        return;

                    //Looping until this point!

                    if (m_contentChildAnchorPos.Keys.Count == m_setChildAmount && !m_setNavigationStarted)
                    {
                        m_setNavigationStarted = true;                  //Blocks 2nd access to start the following code only once.
                        SetInstaniateNavigation(m_navigationLevel);
                    }
                    break;
                }
                default:
                    break;
            }
        }

        #region InstaniateNavigation
        private void SetInstaniateNavigation(NavigationLevel _navigationLevel)
        {
            Navigation navigation;

            switch (_navigationLevel)
            {
                case NavigationLevel.None:
                default:
                    break;
                case NavigationLevel.Simple:
                {
                    switch (m_scrollDirection)
                    {
                        case ScrollDirection.None:
                        default:
                            break;
                        case ScrollDirection.Vertical:
                        {
                            m_dictKeys = new(m_contentChildAnchorPos.Keys);

                            for (int i = 0; i < m_contentChildAnchorPos.Count; i++)
                            {
                                navigation = GetSimpleSelectableNavigation(m_simpleSelectable, i);

                                navigation.selectOnUp = GetTopNavigation(i);
                                navigation.selectOnDown = GetBottomNavigation(i);

                                SetSimpleSelectableNavigation(m_simpleSelectable, i, navigation);
                            }

                            break;
                        }
                        case ScrollDirection.Horizontal:
                        {
                            m_dictKeys = new(m_contentChildAnchorPos.Keys);

                            for (int i = 0; i < m_contentChildAnchorPos.Count; i++)
                            {
                                navigation = GetSimpleSelectableNavigation(m_simpleSelectable, i);

                                navigation.selectOnLeft = GetTopNavigation(i);
                                navigation.selectOnRight = GetBottomNavigation(i);

                                SetSimpleSelectableNavigation(m_simpleSelectable, i, navigation);
                            }

                            break;
                        }
                        case ScrollDirection.Grid:  //Instantiate case gets set in 'GetScrollViewObjects()'.
                        {
                            //TODO: Get Naviation to all 4 MoveDirections, depending on GridConstraints.
                            m_dictKeys = new(m_contentChildAnchorPos.Keys);
                            m_gridSize = CustomGridLayoutSetup.GetGridSize(m_gridSettings);

                            for (int i = 0; i < m_scrollViewContent.childCount; i++)
                            {
                                navigation = GetSimpleSelectableNavigation(m_simpleSelectable, i);

                                SetGridSlotNavigation(m_simpleSelectable, i, navigation);
                            }
                            break;
                        }
                    }
                    break;
                }
                case NavigationLevel.Nested:
                {
                    break;
                }
            }
        }

        private Navigation GetSimpleSelectableNavigation(Selectable _simpleSelectable, int _index)
        {
            Navigation navigation = default;

            switch (_simpleSelectable)
            {
                case Toggle:
                {
                    navigation = m_dictKeys[_index].GetComponent<Toggle>().navigation;
                    break;
                }
                case Slider:
                {
                    navigation = m_dictKeys[_index].GetComponent<Slider>().navigation;
                    break;
                }
                case Button:
                {
                    navigation = m_dictKeys[_index].GetComponent<Button>().navigation;
                    break;
                }
            }

            return navigation;
        }

        private void SetSimpleSelectableNavigation(Selectable _simpleSelectable, int _index, Navigation _navigation)
        {
            switch (_simpleSelectable)
            {
                case Toggle:
                {
                    m_dictKeys[_index].GetComponent<Toggle>().navigation = _navigation;
                    break;
                }
                case Slider:
                {
                    m_dictKeys[_index].GetComponent<Slider>().navigation = _navigation;
                    break;
                }
                case Button:
                {
                    m_dictKeys[_index].GetComponent<Button>().navigation = _navigation;
                    break;
                }
            }
#if UNITY_EDITOR
            //Debug.Log(m_contentChildAnchorPos[m_scrollViewContent.transform.GetChild(_index).gameObject].name);
#endif
        }

        private void SetGridSlotNavigation(Selectable _simpleSelectable, int _index, Navigation _navigation)
        {
            bool topBorderIndex = _index < m_gridSettings.constraintCount;
            bool leftBorderIndex = _index % m_gridSettings.constraintCount == 0;
            bool rightBorderIndex = _index % m_gridSettings.constraintCount == m_gridSettings.constraintCount - 1;
            bool bottomBorderIndex = _index / m_gridSettings.constraintCount == m_gridSize.y - 1;

            //if (bottomBorderIndex)
            //    Debug.Log(_index);

            #region Considered steps while progressing
            //switch (m_loopNavigation)
            //{
            //    case false:
            //    {
            //        break;
            //    }
            //    case true:
            //    {
            //        break;
            //    }
            //}

            ////SetSimpleSelectableNavigation-Part.
            //switch (_simpleSelectable)
            //{
            //    case Toggle:
            //    {
            //        m_dictKeys[_index].GetComponent<Toggle>().navigation = _navigation;
            //        break;
            //    }
            //    case Slider:
            //    {
            //        m_dictKeys[_index].GetComponent<Slider>().navigation = _navigation;
            //        break;
            //    }
            //    case Button:
            //    {
            //        m_dictKeys[_index].GetComponent<Button>().navigation = _navigation;
            //        break;
            //    }
            //}
            #endregion
        }

        private Selectable GetTopNavigation(int _objectIndex)
        {
            Toggle selectableToggle;
            Slider selectableSlider;
            Button selectableButton;

            if (_objectIndex == 0 && m_loopNavigation)
            {
                switch (m_simpleSelectable)
                {
                    case Toggle:
                    {
                        selectableToggle =
                            m_scrollViewContent.transform.GetChild(m_scrollViewContent.transform.childCount - 1).GetComponent<Toggle>();
                        return selectableToggle.GetComponent<Selectable>();
                    }
                    case Slider:
                    {
                        selectableSlider =
                            m_scrollViewContent.transform.GetChild(m_scrollViewContent.transform.childCount - 1).GetComponent<Slider>();
                        return selectableSlider.GetComponent<Selectable>();
                    }
                    case Button:
                    {
                        selectableButton =
                            m_scrollViewContent.transform.GetChild(m_scrollViewContent.transform.childCount - 1).GetComponent<Button>();
                        return selectableButton.GetComponent<Selectable>();
                    }
                }
            }
            else if (_objectIndex == 0 && !m_loopNavigation)
            {
                return null;
            }
            else
            {
                switch (m_simpleSelectable)
                {
                    case Toggle:
                    {
                        selectableToggle =
                            m_scrollViewContent.transform.GetChild(_objectIndex - 1).GetComponent<Toggle>();
                        return selectableToggle.GetComponent<Selectable>();
                    }
                    case Slider:
                    {
                        selectableSlider =
                            m_scrollViewContent.transform.GetChild(_objectIndex - 1).GetComponent<Slider>();
                        return selectableSlider.GetComponent<Selectable>();
                    }
                    case Button:
                    {
                        selectableButton =
                            m_scrollViewContent.transform.GetChild(_objectIndex - 1).GetComponent<Button>();
                        return selectableButton.GetComponent<Selectable>();
                    }
                }
            }

            return null;
        }

        private Selectable GetBottomNavigation(int _objectIndex)
        {
            Toggle selectableToggle;
            Slider selectableSlider;
            Button selectableButton;

            if (_objectIndex == m_scrollViewContent.transform.childCount - 1 && m_loopNavigation)
            {
                switch (m_simpleSelectable)
                {
                    case Toggle:
                    {
                        selectableToggle =
                            m_scrollViewContent.transform.GetChild(0).GetComponent<Toggle>();
                        return selectableToggle.GetComponent<Selectable>();
                    }
                    case Slider:
                    {
                        selectableSlider =
                            m_scrollViewContent.transform.GetChild(0).GetComponent<Slider>();
                        return selectableSlider.GetComponent<Selectable>();
                    }
                    case Button:
                    {
                        selectableButton =
                            m_scrollViewContent.transform.GetChild(0).GetComponent<Button>();
                        return selectableButton.GetComponent<Selectable>();
                    }
                }
            }
            else if (_objectIndex == m_scrollViewContent.transform.childCount - 1 && !m_loopNavigation)
            {
                return null;
            }
            else if (_objectIndex < m_scrollViewContent.transform.childCount - 1)
            {
                switch (m_simpleSelectable)
                {
                    case Toggle:
                    {
                        selectableToggle =
                            m_scrollViewContent.transform.GetChild(_objectIndex + 1).GetComponent<Toggle>();
                        return selectableToggle.GetComponent<Selectable>();
                    }
                    case Slider:
                    {
                        selectableSlider =
                            m_scrollViewContent.transform.GetChild(_objectIndex + 1).GetComponent<Slider>();
                        return selectableSlider.GetComponent<Selectable>();
                    }
                    case Button:
                    {
                        selectableButton =
                            m_scrollViewContent.transform.GetChild(_objectIndex + 1).GetComponent<Button>();
                        return selectableButton.GetComponent<Selectable>();
                    }
                }
            }

            return null;
        }
        #endregion
    }
}