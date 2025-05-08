using System.Collections.Generic;
using System.Linq;
using ThreeDeePongProto.Shared.HelperClasses;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeDeePongProto.Shared.UI.Menu.ScrollViews
{
    internal enum EScrollLayout
    {
        None,
        Vertical,
        Horizontal,
        Grid
    }

    internal enum EMouseScrolling
    {
        None,
        Vertical,
        Horizontal,
        Auto
    }

    public class ScrollViewController : MonoBehaviour
    {
        private enum EContentFillType
        {
            Filled,
            Instantiated
        }

        [SerializeField] internal FillContent m_fillContent;
        [SerializeField] internal AutoScroll m_autoScrolling;

        //GridLayoutGroup: StartCorner: Upper Left, StartAxis: Horizontal, ChildAlignment: Upper Left.
        [SerializeField] private EContentFillType m_contentFillType = EContentFillType.Filled;
        [SerializeField] internal EScrollLayout m_scrollLayout = EScrollLayout.Vertical;
        [SerializeField] private float m_scrollSensitivity = 10.0f;

        [Header("ScrollView Components")]
        [SerializeField] internal ScrollRect m_scrollViewRect;
        [SerializeField] internal RectTransform m_scrollViewContent;
        [SerializeField] private LayoutGroup m_layoutGroup;

        //[SerializeField] private bool m_borderLoop = false;  //Required for instantiated ScrollView contents?

        internal int m_leftPadding;
        internal int m_rightPadding;
        internal int m_topPadding;
        internal int m_bottomPadding;
        internal float m_horizontalSpacing;
        internal float m_verticalSpacing;

        //private bool m_setNavigationStarted = false;  //Required for instantiated ScrollView contents?
        internal bool m_childrenSpawned = false;
        private bool m_gotComponents;
        internal bool ContentChildrenSet { get => m_contentChildrenSet; }
        private bool m_contentChildrenSet = false;
        internal bool ObjectNavigationSet { get => m_objectNavigationSet; }
        private bool m_objectNavigationSet = false;

        /*[SerializeField] */
        private Vector2 m_maskedScrollWindow;      //Fix (masked) Width & Height
        /*[SerializeField] */
        private Vector2 m_fullContentWindow;       //Full Width & Height.
        /*[SerializeField] */
        internal int m_contentChildCount;

        internal RectTransform m_scrollViewRectTransform;
        private RectTransform m_childRect;      //Rect for each child of the Content and it's '.anchoredPosition'.
        internal Vector2 m_firstChildRT;

        [Header("Grid")]
        [SerializeField] private EMouseScrolling m_eMouseScrolling = EMouseScrolling.Vertical;
        [SerializeField] internal Vector2Int m_gridSize;
        internal GridLayoutGroup m_gridSettings;

        internal List<Selectable> m_ContainedSelectables { get; private set; } = new List<Selectable>();

        private void Awake()
        {
            GetScrollViewComponents();
            GetScrollOptionAndLayout(m_scrollViewRect);

            m_gotComponents = m_scrollViewRect != null && m_scrollViewContent != null;

            GetLayoutGroupSettings(m_layoutGroup);
            SetContentFillType();
        }

        private void GetScrollViewComponents()
        {
            m_scrollViewRect = GetComponent<ScrollRect>();
            m_scrollViewRect.scrollSensitivity = m_scrollSensitivity;
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

            bool bothDirectionPossible = verticalScrolling && horizontalScrolling;

            switch (bothDirectionPossible)
            {
                case true:
                {
                    switch (m_eMouseScrolling)
                    {
                        case EMouseScrolling.Vertical:
                            m_scrollLayout = EScrollLayout.Vertical;
                            break;
                        case EMouseScrolling.Horizontal:
                            m_scrollLayout = EScrollLayout.Horizontal;
                            break;
                        case EMouseScrolling.Auto:
                            m_scrollLayout = EScrollLayout.Grid;
                            break;
                        case EMouseScrolling.None:
                        default:
                            break;
                    }
                    break;
                }
                case false:
                {
                    switch (verticalScrolling)
                    {
                        case true:
                        {
                            m_scrollLayout = EScrollLayout.Vertical;
                            break;
                        }
                        case false:
                        {
                            switch (horizontalScrolling)
                            {
                                case true:
                                {
                                    m_scrollLayout = EScrollLayout.Horizontal;
                                    break;
                                }
                                case false:
                                {
                                    m_scrollLayout = EScrollLayout.None;
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

                    if (m_contentFillType == EContentFillType.Filled)
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
                switch (m_contentChildCount == 0 && !m_childrenSpawned && m_fillContent.m_spawnPrefab != null)
                {
                    case true:
                    {
                        m_contentFillType = EContentFillType.Instantiated;
                        if (m_fillContent != null)
                            m_fillContent.SpawnContentChildren();
                        break;
                    }
                    case false:
                    {
                        m_contentFillType = EContentFillType.Filled;
                        CacheSelectableChildren();
                        break;
                    }
                }
            }
        }

        internal void CacheSelectableChildren()
        {
            if (m_scrollViewContent != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(m_scrollViewContent);
                //Find all Selectable-Components (Button, Toggle, Slider, InputField...), even if they are inactive. (true)
                m_ContainedSelectables = m_scrollViewContent.GetComponentsInChildren<Selectable>(true).ToList();
#if UNITY_EDITOR
                //Debug.Log($"ScrollViewController: Found {m_ContainedSelectables.Count} Selectables in Content.");
#endif
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError("ScrollViewController: Content RectTransform is null!", this);
#endif
            }
        }
    }
}