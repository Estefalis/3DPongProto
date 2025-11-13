using System.Collections.Generic;
using System.Linq;
using ThreeDeePongProto.Shared.HelperClasses;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeDeePongProto.Shared.UI.Menu.ScrollViews
{
    internal enum EScrollDirection
    {
        None,
        Vertical,
        Horizontal
    }

    internal enum ESetScrollType
    {
        None,
        Vertical,
        Horizontal,
        Auto
    }

    internal enum ELoopOption
    {
        None,
        LastToFirstCount,
        LastToFirstRowColumn
    }

    public class ScrollViewController : MonoBehaviour
    {
        private enum EContentFillType
        {
            Filled,
            Instantiated
        }

        [SerializeField] internal InstantiateContent m_instantiateContent;
        [SerializeField] internal AutoScroll m_autoScrolling;

        [Header("ScrollView Components")]
        [SerializeField] internal ScrollRect m_scrollViewRect;
        [SerializeField] internal RectTransform m_scrollViewContent;
        [SerializeField] internal LayoutGroup m_layoutGroup;

        [Header("Content Navigation")]
        [SerializeField] private EContentFillType m_contentFillType = EContentFillType.Filled;
        [SerializeField] internal ESetScrollType m_eSetScrollType = ESetScrollType.Vertical;
        [SerializeField] internal ELoopOption m_eLoopOption = ELoopOption.LastToFirstCount;
        [SerializeField] private float m_scrollSensitivity = 10.0f;
        [SerializeField] internal bool m_loopNavigation = false;

        //[Header("Filled Debug")]
        //[SerializeField] private Vector2 m_maskedScrollWindow;      //Fix (masked) Width & Height
        //[SerializeField] private Vector2 m_fullContentWindow;       //Full Width & Height.
        //[SerializeField] private Vector2Int m_gridSize;

        internal int ConstraintCount { get => m_constraintCount; }
        private int m_constraintCount;

        internal int m_leftPadding, m_rightPadding, m_topPadding, m_bottomPadding;
        internal float m_horizontalSpacing, m_verticalSpacing;

        private bool m_gotComponents;
        internal bool ContentChildrenSet { get => m_contentChildrenSet; }
        private bool m_contentChildrenSet = false;
        internal bool m_ChildrenNavigationSet = false;

        internal EScrollDirection EScrollDirection { get => m_eScrollDirection; }
        private EScrollDirection m_eScrollDirection;

        internal RectTransform m_scrollViewRectTransform;
        private GridLayoutGroup m_gridSettings;
        internal List<Selectable> ContainedSelectables { get; private set; } = new List<Selectable>();

        private void Awake()
        {
            GetScrollViewComponents();

            m_gotComponents = m_scrollViewRect != null && m_scrollViewContent != null;
            m_contentChildrenSet = m_gotComponents && m_scrollViewContent.childCount > 0;

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

                    if (m_gridSettings.constraint == GridLayoutGroup.Constraint.FixedColumnCount || m_gridSettings.constraint == GridLayoutGroup.Constraint.FixedRowCount)
                        m_constraintCount = m_gridSettings.constraintCount <= 0 ? m_gridSettings.constraintCount = 1 : m_gridSettings.constraintCount;

                    switch (m_eSetScrollType)
                    {
                        case ESetScrollType.Auto:
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
                        case ESetScrollType.Vertical:
                        {
                            m_eScrollDirection = EScrollDirection.Vertical;
                            break;
                        }
                        case ESetScrollType.Horizontal:
                        {
                            m_eScrollDirection = EScrollDirection.Horizontal;
                            break;
                        }
                        case ESetScrollType.None:
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

                    if (m_eSetScrollType == ESetScrollType.Vertical || m_eSetScrollType == ESetScrollType.Auto)
                        m_eScrollDirection = EScrollDirection.Vertical;
                    else
                        m_eScrollDirection = EScrollDirection.None;
                    break;
                }
                case HorizontalLayoutGroup:
                {
                    var padding = _layoutGroup./*GetComponent<HorizontalLayoutGroup>().*/padding;      //Space at LayoutGroup borders.
                    m_leftPadding = padding.left;
                    m_rightPadding = padding.right;
                    m_horizontalSpacing = _layoutGroup.GetComponent<HorizontalLayoutGroup>().spacing;  //Spacing between Elements.

                    if (m_eSetScrollType == ESetScrollType.Horizontal || m_eSetScrollType == ESetScrollType.Auto)
                        m_eScrollDirection = EScrollDirection.Horizontal;
                    else
                        m_eScrollDirection = EScrollDirection.None;
                    break;
                }
                default:
                    m_eScrollDirection = EScrollDirection.None;
                    break;
            }

            //if (m_contentFillType == EContentFillType.Filled)
            //{
            //    m_maskedScrollWindow = new Vector2(m_scrollViewRectTransform.rect.width, m_scrollViewRectTransform.rect.height);
            //    m_fullContentWindow = new Vector2(m_scrollViewContent.rect.width, m_scrollViewContent.rect.height);   //.x - .width, .y - .height.
            //    m_gridSize = CustomGridLayoutSetup.GetGridSize(m_gridSettings);
            //}
        }

        private void SetContentFillType()
        {
            if (m_gotComponents)
            {
                switch (!m_contentChildrenSet && m_instantiateContent.m_spawnPrefab != null)
                {
                    case true:
                    {
                        m_contentFillType = EContentFillType.Instantiated;
                        if (m_instantiateContent != null)
                            m_instantiateContent.SpawnContentChildren();
                        break;
                    }
                    case false:
                    {
                        m_contentFillType = EContentFillType.Filled;
                        CacheSelectableChildren();
                        m_ChildrenNavigationSet = true;
                        break;
                    }
                }
            }
        }

        internal void CacheSelectableChildren()
        {
            if (m_gotComponents)
            {
                m_contentChildrenSet = m_scrollViewContent.childCount > 0;
                LayoutRebuilder.ForceRebuildLayoutImmediate(m_scrollViewContent);
                //Find all Selectable-Components (Button, Toggle, Slider, InputField...), even if they are inactive. (true)
                ContainedSelectables = m_scrollViewContent.GetComponentsInChildren<Selectable>(true).ToList();
                GetLayoutGroupSettings(m_layoutGroup);
#if UNITY_EDITOR
                //Debug.Log($"ScrollViewController: Found {ContainedSelectables.Count} Selectables in Content.");
#endif
                if (m_contentFillType == EContentFillType.Instantiated)
                    m_instantiateContent.SetupExplicitNavigation(m_contentChildrenSet);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError("ScrollViewController: Content RectTransform is null!", this);
#endif
            }
        }

        public void SetLoopNavigation(bool loop)
        {
            if (m_loopNavigation == loop)
                return;

            m_loopNavigation = loop;
            CacheSelectableChildren();
            m_instantiateContent.SetupExplicitNavigation(m_contentChildrenSet); //Update children Navigation.
#if UNITY_EDITOR
            Debug.Log($"Loop Navigation {(loop ? "activated" : "deactivated")}.");
#endif
        }
    }
}