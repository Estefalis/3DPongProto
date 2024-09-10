using System.Collections.Generic;
using ThreeDeePongProto.Shared.InputActions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

//TODO: AutoScroll, once GameObject is hidden.
//CurrentTargetRectTransform == RectTransform of the currently selected element in the ScrollView.
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
        [SerializeField] private float m_scrollSpeed = 175.0f;
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
        [SerializeField] private int m_topPadding;
        [SerializeField] private int m_leftPadding;
        [SerializeField] private int m_rightPadding;
        [SerializeField] private int m_bottomPadding;
        [Space]
        [SerializeField] private float m_verticalSpacing;
        [SerializeField] private float m_horizontalSpacing;
        //[Space]
        //[SerializeField] private float m_contentHeight;
        //[SerializeField] private float m_contentWidth;
        //[SerializeField] float m_firstChildHeight;
        //[SerializeField] float m_firstChildWidth;

        private RectTransform m_scrollViewRectTransform;

        //private float m_scrollWindowHeight;
        //private float m_scrollWindowWidth;
        private bool m_canAutoScroll = false, m_scrollContentSet;

        private GameObject m_lastSelectedGameObject;
        private Vector2 m_lastMoveDirection;
        private Vector2 m_contentAnchoredPos, m_childAnchoredPos;    //OriginPos' of ScrollContent and each Child!
        private GridLayoutGroup.Constraint m_gridConstraint;

        //private List<GameObject> m_scrollViewGameObjects = new List<GameObject>();
        private Dictionary<GameObject, int> m_contentChildID = new Dictionary<GameObject, int>();

        private void Awake()
        {
            //m_scrollViewGameObjects.Clear();
            m_lastSelectedGameObject = null;
            m_contentChildID.Clear();

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
            AutoScrollToNextGameObject();
            UpdateCurrentGameObject();
        }

        /// <summary>
        /// If Slider, Toggle or Button GameObjects are found in the ScrollView, they will get added to the 'm_scrollViewGameObjects' List.
        /// </summary>
        /// <param name="_transformLevel"></param>
        private void ScrollViewObjectsToDict(Transform _transformLevel, int _contentChildIndex)
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
                    //m_scrollViewGameObjects.Add(toggle.gameObject);
                    m_contentChildID.Add(toggle.gameObject, _contentChildIndex);
                    break;
                }
            }

            switch (containsSlider)
            {
                case false:
                    break;
                case true:
                {
                    //m_scrollViewGameObjects.Add(slider.gameObject);
                    m_contentChildID.Add(slider.gameObject, _contentChildIndex);
                    break;
                }
            }

            switch (containsButton)
            {
                case false:
                    break;
                case true:
                {
                    //m_scrollViewGameObjects.Add(button.gameObject);
                    m_contentChildID.Add(button.gameObject, _contentChildIndex);
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
                m_childAnchoredPos = transform.GetComponent<RectTransform>().anchoredPosition;
                Debug.Log($"ChildAnchoredPos: {transform.name} {m_childAnchoredPos} - DistanceA: {m_childAnchoredPos - m_contentAnchoredPos}");
                //Level for X/Y Axis Toggles.
                for (int i = 0; i < transform.childCount; i++)
                {
#if UNITY_EDITOR
                    //Debug.Log(transform.GetChild(i).name);
#endif
                    Transform subLevelOne = transform.GetChild(i);
                    ScrollViewObjectsToDict(subLevelOne, m_contentChildIndex);

                    ////Level for SliderXAxis Lower & Higher Buttons, XSlider itself, it's Toggle.
                    ////Level for SliderYAxis Lower & Higher Buttons, YSlider itself, it's Toggle.
                    ////Level for ResetButtons for KeyRebinds.
                    for (int j = 0; j < subLevelOne.childCount; j++)
                    {
#if UNITY_EDITOR
                        //Debug.Log(subLevelOne.GetChild(j).name);
#endif
                        Transform subLevelTwo = subLevelOne.GetChild(j);
                        ScrollViewObjectsToDict(subLevelTwo, m_contentChildIndex);

                        //Level for Keyboard & Gamepad Rebind Buttons.
                        for (int k = 0; k < subLevelTwo.childCount; k++)
                        {
#if UNITY_EDITOR
                            //Debug.Log(subLevelTwo.GetChild(k).name);
#endif
                            Transform subLevelThree = subLevelTwo.GetChild(k);
                            ScrollViewObjectsToDict(subLevelThree, m_contentChildIndex);
                        }
                    }
                }
            }
#if UNITY_EDITOR
            //for (int i = 0; i < m_scrollViewGameObjects.Count; i++)
            //    Debug.Log(m_scrollViewGameObjects[i].name);
#endif
        }

        private void GetLayoutGroupSettings(LayoutGroup m_layoutGroup)
        {
            switch (m_setAutoScrollOption)
            {
                case AutoScrollOptions.Both:
                {
                    var padding = m_layoutGroup./*GetComponent<GridLayoutGroup>().*/padding;
                    m_topPadding = padding.top;
                    m_bottomPadding = padding.bottom;
                    m_leftPadding = padding.left;
                    m_rightPadding = padding.right;
                    var gridSpacing = m_layoutGroup.GetComponent<GridLayoutGroup>().spacing;            //Spacing between Elements.
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
                    }
                    break;
                }
                case AutoScrollOptions.Vertical:
                {
                    var padding = m_layoutGroup./*GetComponent<VerticalLayoutGroup>().*/padding;
                    m_topPadding = padding.top;
                    m_bottomPadding = padding.bottom;
                    m_verticalSpacing = m_layoutGroup.GetComponent<VerticalLayoutGroup>().spacing;      //Spacing between Elements.
                    break;
                }
                case AutoScrollOptions.Horizontal:
                {
                    var padding = m_layoutGroup./*GetComponent<HorizontalLayoutGroup>().*/padding;
                    m_leftPadding = padding.left;
                    m_rightPadding = padding.right;
                    m_horizontalSpacing = m_layoutGroup.GetComponent<HorizontalLayoutGroup>().spacing;  //Spacing between Elements.
                    break;
                }
            }

            //m_scrollWindowHeight = m_scrollViewRectTransform.rect.height;
            //m_scrollWindowWidth = m_scrollViewRectTransform.rect.width;
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
            if (m_lastSelectedGameObject != EventSystem.current.currentSelectedGameObject)
            {
                m_lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
                m_lastMoveDirection = m_playerInputActions.UI.Navigate.ReadValue<Vector2>();

                #region List switch
                //switch (m_scrollViewGameObjects.Contains(m_lastSelectedGameObject))
                //{
                //    case true:
                //    {
                //        m_canAutoScroll = true;
                //        //TODO: Detect, if the gameobject is masked.
                //        break;
                //    }
                //    case false:
                //    {
                //        m_canAutoScroll = false;
                //        break;
                //    }
                //}
                #endregion

                #region Dict switch
                switch (m_contentChildID.ContainsKey(m_lastSelectedGameObject))
                {
                    case true:
                    {
                        m_canAutoScroll = true;
                        //TODO: Detect, if the gameobject is masked.
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
            //Debug.Log($"LastGO: {m_lastSelectedGameObject.name} - LastMoveDir: {m_lastMoveDirection} - autoScroll: {m_canAutoScroll}");
#endif
        }

        /// <summary>
        /// AutoScrolls to the next element, if the ScrollView and it's content are not null and the next element is part of the ScrollView.
        /// </summary>
        private void AutoScrollToNextGameObject()
        {
            if (!m_scrollContentSet || !m_canAutoScroll)
                return;

            switch (m_setAutoScrollOption)
            {
                case AutoScrollOptions.Vertical:
                    UpdateVerticalScrollPosition(m_scrollContentRT, m_lastMoveDirection);
                    break;
                case AutoScrollOptions.Horizontal:
                    UpdateHorizontalScrollPosition(m_scrollContentRT, m_lastMoveDirection);
                    break;
                case AutoScrollOptions.Both:
                    UpdateVerticalScrollPosition(m_scrollContentRT, m_lastMoveDirection);
                    UpdateHorizontalScrollPosition(m_scrollContentRT, m_lastMoveDirection);
                    break;
                default:
                    break;
            }
        }

        private void UpdateVerticalScrollPosition(RectTransform _scrollContent, Vector2 _moveDirection)
        {
            //move the current scroll rect to correct _variableContentPos           //min: -57 - max: 0
            float variableContentPos = -_scrollContent.anchoredPosition.y - (_scrollContent.rect.height * (1 - _scrollContent.pivot.y) - (m_topPadding + m_bottomPadding));
            float scrollContentHeight = _scrollContent.rect.height;                 //487 - Fullsize Content
            //float scrollContentHeight = m_firstChildHeight;                       //60  - Child Height
            float scrollWindowHeight = m_maskedScrollWindow.y;                      //yVector = height of masked ContentScrollWindow
            float viewRectAnchorPos = m_scrollViewRectTransform.anchoredPosition.y; //0   - fixed ScrollView AnchorPosition

            // get the element offset value depending on the cursor move direction
            float offlimitsValue = GetScrollOffset(variableContentPos, viewRectAnchorPos, scrollContentHeight, scrollWindowHeight);  //917 - 974

            float normalizedPosition = m_scrollViewRect.verticalNormalizedPosition + (offlimitsValue / m_scrollViewRectTransform.rect.height);
            //2,265116 - 3,132558

            normalizedPosition = Mathf.Clamp01(normalizedPosition); //Currently Mouse isn't part of the context.

            if (Keyboard.current.numpadPlusKey.wasPressedThisFrame)
            {
                normalizedPosition -= Mathf.Abs(offlimitsValue) / m_scrollViewRectTransform.rect.height;
                normalizedPosition = Mathf.Clamp01(normalizedPosition);
                // move the target scroll rect
                m_scrollViewRect.verticalNormalizedPosition = Mathf.SmoothStep(m_scrollViewRect.verticalNormalizedPosition, normalizedPosition, Time.unscaledDeltaTime * m_scrollSpeed);
            }

            if (Keyboard.current.numpadMinusKey.wasPressedThisFrame)
            {
                normalizedPosition += Mathf.Abs(offlimitsValue) / m_scrollViewRectTransform.rect.height;
                normalizedPosition = Mathf.Clamp01(normalizedPosition);
                // move the target scroll rect
                m_scrollViewRect.verticalNormalizedPosition = Mathf.SmoothStep(m_scrollViewRect.verticalNormalizedPosition, normalizedPosition, Time.unscaledDeltaTime * m_scrollSpeed);
            }

            float yDirection = _moveDirection.y;
            switch (yDirection)
            {
                case 0:
                default:
                {
                    break;
                }
                //Children GetComponents Method (OnEachChildAdd) at Start for pivot.y(, when no new Children get added on runtime)?
                case -1:    //On MoveDirection Down     
                {
                    //////Vector2 contentAnchorPos = m_scrollContentRT.anchoredPosition;    //OriginPos of ScrollContent<3! <--------------------
                    
                    //TODO: (Lower Children border to lower ScrollView border)
                    //GetComponent Button's' ScrollWindow-ChildTransform and compare it's pivot.y to ScrollwWindow.pivot.y (South border).
                    //If 'ScrollWindow-ChildTransform's pivot.y is below scrollWindow's south border, move up by childTransforms height + south Padding.
                    break;
                }
                case 1:     //On MoveDirection Up
                {
                    //TODO: (Upper Children border to upper ScrollView border)
                    //GetComponent Button's' ScrollWindow-ChildTransform and compare it's pivot.y to ScrollwWindow.pivot.y (North border).
                    //If 'ScrollWindow-ChildTransform's pivot.y is above scrollWindow's north border, move down by childTransforms height + north Padding.
                    break;
                }
            }

            //TODO: - Berechnung Offset-Value, gemessen an der CursorPosition(später, nicht aktuell gebraucht).
        }

        private void UpdateHorizontalScrollPosition(RectTransform _scrollContent, Vector2 _moveDirection)
        {
            float xDirection = _moveDirection.x;
            switch (xDirection)
            {
                case 0:
                default:
                {
                    break;
                }
                //Children GetComponents Method (OnEachChildAdd) at Start for pivot.y(, when no new Children get added on runtime)?
                case -1:
                {
                    //TODO: (Left Children border to left ScrollView border)
                    //GetComponent Button's' ScrollWindow-ChildTransform and compare it's pivot.y to ScrollwWindow.pivot.y (Left border).
                    //If 'ScrollWindow-ChildTransform's pivot.y is left to scrollWindow's left border, move right by childTransforms width + left Padding
                    break;
                }
                case 1:
                {
                    //TODO: (Left Children border to right ScrollView border)
                    //GetComponent Button's' ScrollWindow-ChildTransform and compare it's pivot.y to ScrollwWindow.pivot.y (Right border).
                    //If 'ScrollWindow-ChildTransform's pivot.y is right to scrollWindow's right border, move left by childTransforms width + right Padding
                    break;
                }
            }
        }

        //Debug.Log($"With ContentHeight: {0 + (487 / 2)} - {-57 + (487 / 2)}"); //243 || 186
        //Debug.Log($"With ChildrenHeight: {0 + (60 / 2)} - {-57 + (60 / 2)}"); //30 || -27
        //            GetScrollOffset(ContentPos upper/start,    fix ScrollView AnchorPos,      Fullsize Content,     (masked) ContentScrollView)
        //                           (          -57 - 0        ,            0            ,              487          ,              430         )
        private float GetScrollOffset(float _variableContentPos, float _viewRectAnchorPos, float _scrollContentHeight, float _scrollWindowHeight)
        {
            if (_variableContentPos < _viewRectAnchorPos + (_scrollContentHeight / 2))
            {
#if UNITY_EDITOR
                //Debug.Log(_viewRectAnchorPos + _scrollWindowHeight - (_variableContentPos - _scrollContentHeight));
#endif
                return (_viewRectAnchorPos + _scrollWindowHeight) - (_variableContentPos - _scrollContentHeight);
            }
            else if (_variableContentPos + _scrollContentHeight > _viewRectAnchorPos + _scrollWindowHeight)
            {
#if UNITY_EDITOR
                //Debug.Log(_viewRectAnchorPos + _scrollWindowHeight - (_variableContentPos + _scrollContentHeight));
#endif
                return (_viewRectAnchorPos + _scrollWindowHeight) - (_variableContentPos + _scrollContentHeight);
            }

            return 0;
        }
    }
}