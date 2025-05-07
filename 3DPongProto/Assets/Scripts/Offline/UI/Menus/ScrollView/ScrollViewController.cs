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
        Horizontal,
        Both
    }

    internal enum EMouseScrolling
    {
        None,
        Vertical,
        Horizontal,
        Both
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

        [SerializeField] private EContentFillType m_contentFillType = EContentFillType.Filled;
        //GridLayoutGroup: StartCorner: Upper Left, StartAxis: Horizontal, ChildAlignment: Upper Left.
        [SerializeField] internal EScrollDirection m_scrollDirection = EScrollDirection.Vertical;
        [SerializeField] private float m_scrollSensitivity = 10.0f;

        [Header("ScrollView Components")]
        [SerializeField] internal ScrollRect m_scrollViewRect;
        [SerializeField] internal RectTransform m_scrollViewContent;
        [SerializeField] private LayoutGroup m_layoutGroup;

        //[SerializeField] private bool m_borderLoop = false;

        //internal Selectable m_simpleSelectable;
        internal int m_leftPadding;
        internal int m_rightPadding;
        internal int m_topPadding;
        internal int m_bottomPadding;
        internal float m_horizontalSpacing;
        internal float m_verticalSpacing;
        //private Vector2 m_cellSize;

        //private bool m_setNavigationStarted = false;
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

        #region Lists_and_Dictionaries
        #region Old manual scrolling before rework 2025
        //private List<GameObject> m_dictKeys; 
        #endregion
        internal List<Selectable> m_ContainedSelectables { get; private set; } = new List<Selectable>();
        #region Old manual scrolling before rework 2025
        //internal Dictionary<GameObject, RectTransform> m_ContentChildAnchorPos = new();
        //internal Dictionary<GameObject, Navigation> m_ObjectNavigation = new(); 
        #endregion
        #endregion

        private void Awake()
        {
            #region Old manual scrolling before rework 2025
            //m_ContentChildAnchorPos.Clear();
            //m_ObjectNavigation.Clear();
            //m_dictKeys = new();
            //m_dictKeys.Clear(); 
            #endregion

            GetScrollViewComponents();
            GetScrollOptionAndLayout(m_scrollViewRect);

            m_gotComponents = m_scrollViewRect != null && m_scrollViewContent != null;

            GetLayoutGroupSettings(m_layoutGroup);
            SetContentFillType();
        }

        //private void OnEnable()
        //{
        //    ContentLevelIterations();
        //}

        //private void OnDisable()
        //{
        //    //m_ContentChildAnchorPos.Clear();
        //    //m_ObjectNavigation.Clear();
        //}

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
                            m_scrollDirection = EScrollDirection.Vertical;
                            break;
                        case EMouseScrolling.Horizontal:
                            m_scrollDirection = EScrollDirection.Horizontal;
                            break;
                        case EMouseScrolling.Both:
                            m_scrollDirection = EScrollDirection.Both;
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
                            m_scrollDirection = EScrollDirection.Vertical;
                            break;
                        }
                        case false:
                        {
                            switch (horizontalScrolling)
                            {
                                case true:
                                {
                                    m_scrollDirection = EScrollDirection.Horizontal;
                                    break;
                                }
                                case false:
                                {
                                    m_scrollDirection = EScrollDirection.None;
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

        /// <summary>
        /// Searches Content of the currently active ScrollView with nested for-loops, to fill Dictionaries with AnchoredPositions and Navigation Information of the contained GameObjects.
        /// </summary>
        //private void ContentLevelIterations()
        //{
        //    switch (m_contentChildCount > 0)
        //    {
        //        case true:
        //        {
        //            var firstChildRect = m_scrollViewContent.GetChild(0).GetComponent<RectTransform>().rect;
        //            m_firstChildRT = new Vector2(firstChildRect.width, firstChildRect.height);

        //            m_contentChildrenSet = true;
        //            break;
        //        }
        //        case false:
        //            m_contentChildrenSet = false;
        //            return;
        //    }

        //    foreach (Transform transform in m_scrollViewContent.transform)
        //    {
        //        m_childRect = transform.GetComponent<RectTransform>();

        //        //Very 1st childLevel.
        //        GetScrollViewObjects(transform, m_childRect);

        //        //for-loop for 2nd childLevel.
        //        for (int i = 0; i < transform.childCount; i++)
        //        {
        //            Transform subLevelOne = transform.GetChild(i);
        //            GetScrollViewObjects(subLevelOne, m_childRect);

        //            //for-loop for 3rd childLevel.
        //            for (int j = 0; j < subLevelOne.childCount; j++)
        //            {
        //                Transform subLevelTwo = subLevelOne.GetChild(j);
        //                GetScrollViewObjects(subLevelTwo, m_childRect);

        //                //for-loop for 4th childLevel.
        //                for (int k = 0; k < subLevelTwo.childCount; k++)
        //                {
        //                    Transform subLevelThree = subLevelTwo.GetChild(k);
        //                    GetScrollViewObjects(subLevelThree, m_childRect);
        //                }
        //            }
        //        }
        //    }

        //    m_objectNavigationSet = true;
        //}

        //private void GetScrollViewObjects(Transform _transformLevel, RectTransform _contentElementAnchorPos)
        //{
        //    bool containsToggle = _transformLevel.TryGetComponent(out Toggle toggle);
        //    bool containsSlider = _transformLevel.TryGetComponent(out Slider slider);
        //    bool containsButton = _transformLevel.TryGetComponent(out Button button);

        //    switch (m_contentFillType)
        //    {
        //        case EContentFillType.Filled:
        //        {
        //            if (containsToggle)
        //            {
        //                m_ContentChildAnchorPos.Add(toggle.gameObject, _contentElementAnchorPos);
        //                m_ObjectNavigation.Add(toggle.gameObject, toggle.navigation);
        //            }

        //            if (containsSlider)
        //            {
        //                m_ContentChildAnchorPos.Add(slider.gameObject, _contentElementAnchorPos);
        //                m_ObjectNavigation.Add(slider.gameObject, slider.navigation);
        //            }

        //            if (containsButton)
        //            {
        //                m_ContentChildAnchorPos.Add(button.gameObject, _contentElementAnchorPos);
        //                m_ObjectNavigation.Add(button.gameObject, button.navigation);
        //            }
        //            break;
        //        }
        //        case EContentFillType.Instantiated:
        //        {
        //            if (containsToggle)
        //                m_ContentChildAnchorPos.Add(toggle.gameObject, _contentElementAnchorPos);

        //            if (containsSlider)
        //                m_ContentChildAnchorPos.Add(slider.gameObject, _contentElementAnchorPos);

        //            if (containsButton)
        //                m_ContentChildAnchorPos.Add(button.gameObject, _contentElementAnchorPos);

        //            if (m_scrollViewContent.childCount < 2)
        //                return;

        //            //Looping until this point!

        //            if (m_ContentChildAnchorPos.Keys.Count == m_createChildAmount && !m_setNavigationStarted)
        //            {
        //                m_setNavigationStarted = true;                  //Blocks 2nd access to start the following code only once.
        //                //SetInstantiateNavigation();
        //            }
        //            break;
        //        }
        //        default:
        //            break;
        //    }
        //}
    }
}