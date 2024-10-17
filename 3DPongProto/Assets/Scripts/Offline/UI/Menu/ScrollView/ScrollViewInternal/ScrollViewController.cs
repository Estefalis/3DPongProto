using System;
using System.Collections.Generic;
using ThreeDeePongProto.Shared.HelperClasses;
using UnityEngine;
using UnityEngine.UI;


namespace ThreeDeePongProto.Offline.UI.Menu.AutoScrolling
{
    internal enum ScrollDirection
    {
        None,
        Vertical,
        Horizontal,
        Both
    }

    public class ScrollViewController : MonoBehaviour
    {

        private enum ContentFillType
        {
            Filled,
            Instantiated
        }

        [SerializeField] internal AutoScroll m_autoScrolling;

        [SerializeField] internal ScrollDirection m_scrollDirection = ScrollDirection.Both;
        [SerializeField] private ContentFillType m_contentFillType = ContentFillType.Filled;

        [Header("ScrollView Components")]
        [SerializeField] internal ScrollRect m_scrollViewRect;
        [SerializeField] internal RectTransform m_scrollViewContent;
        [SerializeField] internal LayoutGroup m_layoutGroup;
        [SerializeField] internal int m_contentChildCount;
        [Space]
        [SerializeField] internal Vector2 m_maskedScrollWindow;      //Fix (masked) Width & Height
        [SerializeField] internal Vector2 m_fullContentWindow;       //Full Width & Height.

        [Header("Prefab")]
        [SerializeField] internal bool m_instantiatedContent = false;
        [SerializeField] private GameObject m_spawnablePrefab = null;
        [SerializeField] private int m_setChildAmount = 50;
        [SerializeField] internal bool m_childsSpawned = false;

        internal int m_leftPadding;
        internal int m_rightPadding;
        internal int m_topPadding;
        internal int m_bottomPadding;
        internal float m_horizontalSpacing;
        internal float m_verticalSpacing;

        private bool m_gotComponents;
        internal bool ContentChildrenSet { get => m_contentChildrenSet; }
        internal bool ObjectNavigationSet { get => m_objectNavigationSet; }
        private bool m_contentChildrenSet = false, m_objectNavigationSet = false;  //or 'internal static Action<bool> ContentFilled/Set;'

        private Vector2Int m_gridSize;
        internal Vector2 m_firstChildRT;

        internal RectTransform m_scrollViewRectTransform/*, m_viewportRectTransform*/;
        private RectTransform m_childRect;      //Rect for each child of the Content and it's '.anchoredPosition'.

        internal Dictionary<GameObject, RectTransform> m_contentChildAnchorPos = new Dictionary<GameObject, RectTransform>();
        internal Dictionary<GameObject, Navigation> m_objectNavigation = new Dictionary<GameObject, Navigation>();

        private void Awake()
        {
            GetScrollViewComponents();
            GetScrollOptionAndLayout(m_scrollViewRect);

            m_gotComponents = m_scrollViewRect != null && m_scrollViewContent != null;

            SetContentFillType();
            GetLayoutGroupSettings(m_layoutGroup);

            m_contentChildAnchorPos.Clear();
            m_objectNavigation.Clear();
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

        #region ScrollView Preparation
        private void GetScrollViewComponents()
        {
            m_scrollViewRect = GetComponent<ScrollRect>();
            m_scrollViewRectTransform = m_scrollViewRect.GetComponent<RectTransform>();
            //m_viewportRectTransform = m_scrollViewRect.viewport.GetComponent<RectTransform>();
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
                    m_scrollDirection = ScrollDirection.Both;
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

        private void SetContentFillType()
        {
            if (m_gotComponents)
            {
                switch (m_instantiatedContent && !m_childsSpawned && m_spawnablePrefab != null)
                {
                    case true:
                    {
                        SpawnContentChildren();
                        m_contentFillType = ContentFillType.Instantiated;
                        break;
                    }
                    case false:
                    {
                        m_contentFillType = ContentFillType.Filled;
                        break;
                    }
                }

                m_contentChildCount = m_scrollViewContent.childCount;
                switch (m_contentChildCount > 0)
                {
                    case true:
                        m_contentChildrenSet = true;
                        break;
                    case false:
                        m_contentChildrenSet = false;
                        break;
                }
            }
        }

        private void SpawnContentChildren()
        {
            for (int i = 0; i < m_setChildAmount; i++)
                Instantiate(m_spawnablePrefab, m_scrollViewContent);

            m_childsSpawned = true;
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

            if (m_contentChildCount > 0)
            {
                var firstChildRect = m_scrollViewContent.GetChild(0).GetComponent<RectTransform>().rect;
                m_firstChildRT = new Vector2(firstChildRect.width, firstChildRect.height);
            }
        }

        /// <summary>
        /// Searches Content of the currently active ScrollView with nested for-loops, to fill Dictionaries with AnchoredPositions and Navigation Informations of the contained GameObjects.
        /// </summary>
        private void ContentLevelIterations()
        {
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
                    break;
                }
                default:
                    break;
            }
        }
        #endregion
    }
}