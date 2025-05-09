using System.Collections.Generic;
using System.Linq;
using ThreeDeePongProto.Shared.HelperClasses;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeDeePongProto.Shared.UI.Menu.ScrollViews
{
    public class ScrollViewController : MonoBehaviour
    {
        private enum EContentFillType
        {
            Filled,
            Instantiated
        }

        private enum EScrollType
        {
            None,
            Vertical,
            Horizontal,
            Auto
        }

        internal enum EScrollDirection
        {
            None,
            Vertical,
            Horizontal
        }

        [SerializeField] internal FillContent m_fillContent;
        [SerializeField] internal AutoScroll m_autoScrolling;

        //GridLayoutGroup: StartCorner: Upper Left, StartAxis: Horizontal, ChildAlignment: Upper Left.
        [SerializeField] private EContentFillType m_contentFillType = EContentFillType.Filled;
        [SerializeField] private EScrollType m_eScrollType = EScrollType.Vertical;
        internal EScrollDirection m_EScrollDirection { get => m_eScrollDirection; }
        /*[SerializeField] */
        private EScrollDirection m_eScrollDirection;
        [SerializeField] private float m_scrollSensitivity = 10.0f;

        [Header("ScrollView Components")]
        [SerializeField] internal ScrollRect m_scrollViewRect;
        [SerializeField] internal RectTransform m_scrollViewContent;
        [SerializeField] internal LayoutGroup m_layoutGroup;

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
        [SerializeField] internal Vector2Int m_gridSize;
        internal GridLayoutGroup m_gridSettings;

        internal List<Selectable> m_ContainedSelectables { get; private set; } = new List<Selectable>();

        private void Awake()
        {
            GetScrollViewComponents();

            m_gotComponents = m_scrollViewRect != null && m_scrollViewContent != null;

            GetLayoutGroupSettings(m_layoutGroup);
            SetContentFillType();
        }

        /// <summary>
        /// Gets ScrollRect, RectTransforms of scrollView, content and the LayoutGroup. 
        /// </summary>
        private void GetScrollViewComponents()
        {
            m_scrollViewRect = GetComponent<ScrollRect>();
            m_scrollViewRect.scrollSensitivity = m_scrollSensitivity;
            m_scrollViewRectTransform = m_scrollViewRect.GetComponent<RectTransform>();
            m_scrollViewContent = m_scrollViewRect.content.GetComponent<RectTransform>();
            m_layoutGroup = m_scrollViewContent.GetComponent<LayoutGroup>();
        }

        private void GetLayoutGroupSettings(LayoutGroup _layoutGroup)
        {
            bool verticalScrolling = m_scrollViewRect.verticalScrollbar != null && m_scrollViewRect.vertical;
            bool horizontalScrolling = m_scrollViewRect.horizontalScrollbar != null && m_scrollViewRect.horizontal;

            bool bothDirectionPossible = verticalScrolling && horizontalScrolling;

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

                    switch (m_eScrollType)
                    {
                        case EScrollType.Auto:
                        {
                            m_eScrollDirection = m_gridSettings.constraint switch
                            {
                                GridLayoutGroup.Constraint.FixedRowCount => EScrollDirection.Vertical,
                                GridLayoutGroup.Constraint.FixedColumnCount => EScrollDirection.Horizontal,
                                GridLayoutGroup.Constraint.Flexible => EScrollDirection.None,   //Or own implementation.
                                _ => EScrollDirection.None,
                            };
                            break;
                        }
                        case EScrollType.Vertical:
                        {
                            SetScrollDirection(verticalScrolling, EScrollType.Vertical);
                            break;
                        }
                        case EScrollType.Horizontal:
                        {
                            SetScrollDirection(horizontalScrolling, EScrollType.Horizontal);
                            break;
                        }
                        case EScrollType.None:
                        default:
                            m_eScrollDirection = EScrollDirection.None;
                            break;
                    }
                    break;
                }
                case VerticalLayoutGroup:
                {
                    var padding = _layoutGroup./*GetComponent<VerticalLayoutGroup>().*/padding;        //Space at LayoutGroup borders.
                    m_topPadding = padding.top;
                    m_bottomPadding = padding.bottom;
                    m_verticalSpacing = _layoutGroup.GetComponent<VerticalLayoutGroup>().spacing;      //Spacing between Elements.

                    if (m_eScrollType == EScrollType.Vertical || m_eScrollType == EScrollType.Auto)
                        SetScrollDirection(verticalScrolling, EScrollType.Vertical);
                    else
                        SetScrollDirection(false, EScrollType.Vertical);
                    break;
                }
                case HorizontalLayoutGroup:
                {
                    var padding = _layoutGroup./*GetComponent<HorizontalLayoutGroup>().*/padding;      //Space at LayoutGroup borders.
                    m_leftPadding = padding.left;
                    m_rightPadding = padding.right;
                    m_horizontalSpacing = _layoutGroup.GetComponent<HorizontalLayoutGroup>().spacing;  //Spacing between Elements.

                    if (m_eScrollType == EScrollType.Horizontal || m_eScrollType == EScrollType.Auto)
                        SetScrollDirection(horizontalScrolling, EScrollType.Horizontal);
                    else
                        SetScrollDirection(false, EScrollType.Horizontal);
                    break;
                }
                default:
                    m_eScrollDirection = EScrollDirection.None;
                    break;
            }

            m_maskedScrollWindow = new Vector2(m_scrollViewRectTransform.rect.width, m_scrollViewRectTransform.rect.height);
            m_fullContentWindow = new Vector2(m_scrollViewContent.rect.width, m_scrollViewContent.rect.height);   //.x - .width, .y - .height.
            
            m_contentChildCount = m_scrollViewContent.childCount;
        }

        private void SetScrollDirection(bool _switch, EScrollType _eScrollType)
        {
            switch (_switch)
            {
                case true:
                {
                    m_eScrollDirection = (EScrollDirection)_eScrollType;
                    break;
                }
                case false:
                    m_eScrollDirection = EScrollDirection.None;
                    break;
            }
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
                m_childrenSpawned = m_scrollViewContent.childCount > 0;
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