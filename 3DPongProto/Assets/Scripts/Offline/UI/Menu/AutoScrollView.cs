using System.Collections.Generic;
using ThreeDeePongProto.Shared.InputActions;
using UnityEngine;
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

        private PlayerInputActions m_playerInputActions;
        [SerializeField] private AutoScrollOptions m_setAutoScrollOption = AutoScrollOptions.Both;
        [SerializeField] private float m_scrollSpeed = 50.0f;

        [SerializeField] private ScrollRect m_scrollRect;
        [SerializeField] private RectTransform m_scrollContent;
        [SerializeField] private LayoutGroup m_layoutGroup;

        private bool m_canAutoScroll = false, m_scrollContentSet;

        private RectTransform m_scrollWindow;
        private GameObject m_lastSelectedGameObject;
        private Vector2 m_moveDirection;
        private List<GameObject> m_scrollViewGameObjects = new List<GameObject>();

        private void Awake()
        {
            m_scrollViewGameObjects.Clear();

            m_scrollRect = GetComponent<ScrollRect>();
            m_scrollWindow = m_scrollRect.GetComponent<RectTransform>();
            m_scrollContent = m_scrollRect.content.GetComponent<RectTransform>();
            m_layoutGroup = m_scrollContent.GetComponent<LayoutGroup>();

            m_scrollContentSet = m_scrollRect != null && m_scrollContent != null;

            GetAutoScrollOptions(m_scrollRect);

            MenuNavigation.ALastSelectedGameObject += UpdateCurrentGameObject;
        }

        private void Start()
        {
            if (m_scrollContent != null)
            {
                ContentLevelIterations();
            }

            m_playerInputActions = InputManager.m_playerInputActions;
            m_playerInputActions.UI.Enable();
        }

        private void OnDisable()
        {
            MenuNavigation.ALastSelectedGameObject -= UpdateCurrentGameObject;
            m_playerInputActions.UI.Disable();
        }

        private void Update()
        {
            AutoScrollToNextGameObject();
        }

        /// <summary>
        /// Switch solution to autoset available AutoScroll options.
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
                    break;
                }
                case false:
                {
                    switch (verticalScrolling)
                    {
                        case true:
                        {
                            m_setAutoScrollOption = AutoScrollOptions.Vertical;
                            break;
                        }
                        case false:
                        {
                            switch (horizontalScrolling)
                            {
                                case true:
                                {
                                    m_setAutoScrollOption = AutoScrollOptions.Horizontal;
                                    break;
                                }
                                case false:
                                {
                                    m_setAutoScrollOption = AutoScrollOptions.None;
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
            //'m_layoutGroup = m_scrollContent.AddComponent<GridLayoutGroup/VerticalLayoutGroup/HorizontalLayoutGroup>()' and
            //'default: break;' - for savety. (It would currently go too far.)
        }

        /// <summary>
        /// Searches Content, of the currently active ScrollView, for GameObjects in each Parent/Child Level of the Hierarchy.
        /// </summary>
        private void ContentLevelIterations()
        {
            foreach (Transform transform in m_scrollContent.transform)
            {
                //Level for X/Y Axis Toggles.
                for (int i = 0; i < transform.childCount; i++)
                {
#if UNITY_EDITOR
                    //Debug.Log(transform.GetChild(i).name);
#endif
                    Transform subLevelOne = transform.GetChild(i);
                    GetToggleSliderButtons(subLevelOne);

                    ////Level for SliderXAxis Lower & Higher Buttons, XSlider itself, it's Toggle.
                    ////Level for SliderYAxis Lower & Higher Buttons, YSlider itself, it's Toggle.
                    ////Level for ResetButtons for KeyRebinds.
                    for (int j = 0; j < subLevelOne.childCount; j++)
                    {
#if UNITY_EDITOR
                        //Debug.Log(subLevelOne.GetChild(j).name);
#endif
                        Transform subLevelTwo = subLevelOne.GetChild(j);
                        GetToggleSliderButtons(subLevelTwo);

                        //Level for Keyboard & Gamepad Rebind Buttons.
                        for (int k = 0; k < subLevelTwo.childCount; k++)
                        {
#if UNITY_EDITOR
                            //Debug.Log(subLevelTwo.GetChild(k).name);
#endif
                            Transform subLevelThree = subLevelTwo.GetChild(k);
                            GetToggleSliderButtons(subLevelThree);
                        }
                    }
                }
            }
#if UNITY_EDITOR
            //for (int i = 0; i < m_scrollViewGameObjects.Count; i++)
            //    Debug.Log(m_scrollViewGameObjects[i].name);
#endif
        }

        /// <summary>
        /// If Slider, Toggle or Button GameObjects are found in the ScrollView, they will get added to the 'm_scrollViewGameObjects' List.
        /// </summary>
        /// <param name="_transformLevel"></param>
        private void GetToggleSliderButtons(Transform _transformLevel)
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
                    m_scrollViewGameObjects.Add(toggle.gameObject);
                    break;
                }
            }

            switch (containsSlider)
            {
                case false:
                    break;
                case true:
                {
                    m_scrollViewGameObjects.Add(slider.gameObject);
                    break;
                }
            }

            switch (containsButton)
            {
                case false:
                    break;
                case true:
                {
                    m_scrollViewGameObjects.Add(button.gameObject);
                    break;
                }
            }
        }

        /// <summary>
        /// Updates AutoScrolling bool 'm_canAutoScroll', depending on 'm_scrollViewGameObjects' List entries.
        /// </summary>
        /// <param name="_gameObject"></param>
        private void UpdateCurrentGameObject(GameObject _gameObject)
        {
            m_lastSelectedGameObject = _gameObject;
            m_moveDirection = m_playerInputActions.UI.Navigate.ReadValue<Vector2>();
            
            //for (int i = 0; i < m_scrollViewGameObjects.Count; i++)
            //{
            switch (m_scrollViewGameObjects.Contains(m_lastSelectedGameObject))
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
            //}
#if UNITY_EDITOR
            //Debug.Log($"ASW MoveDir: {m_moveDirection} - AutoScroll: {m_canAutoScroll} - CurrentGO: {m_lastSelectedGameObject}");
#endif
        }

        /// <summary>
        /// AutoScrolls to the next element, if the ScrollView and it's content are not null and the next element is part of the ScrollView.
        /// </summary>
        private void AutoScrollToNextGameObject()   //TODO:
        {
            if (!m_scrollContentSet || !m_canAutoScroll)
                return;

            switch (m_setAutoScrollOption)
            {
                case AutoScrollOptions.Vertical:
                    UpdateVerticalScrollPosition();
                    break;
                case AutoScrollOptions.Horizontal:
                    UpdateHorizontalScrollPosition();
                    break;
                case AutoScrollOptions.Both:
                    UpdateVerticalScrollPosition();
                    UpdateHorizontalScrollPosition();
                    break;
                default:
                    break;
            }
        }

        private void UpdateVerticalScrollPosition()
        {
            //TODO:
            //Berechnung der ScrollView element position, zu der gescrollt werden muss:
            //      - element.anchorPosition, element.rect.width/height, element.pivot.y.
            //Berechnung Offset-Value, gemessen an der CursorPosition(sp�ter, nicht aktuell gebraucht):
            //      - element.height/width, scrollView.height/width, scrollView.anchorPosition.y.
            //      - Berechnung per Methode
            //      - mit 'TargetScrollRect.verticalNormalizedPosition' Scrollbar/ScrollView scollen
        }

        private void UpdateHorizontalScrollPosition()
        {

        }
    }
}