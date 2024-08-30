using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//TODO: Get available scroll options;
//      Update next GameObject for AutoScrolling based on InputActionAsset MovementInput, current GameObject.
//      AutoScroll, once GameObject is hidden.
namespace ThreeDeePongProto.Offline.UI.Menu
{
    [RequireComponent(typeof(ScrollRect))]
    //[AddComponentMenu("UI/Extensions/AutoScrollView")]
    public class AutoScrollView : MonoBehaviour
    {
        internal enum AutoScrollOptions
        {
            None,
            Both,
            Vertical,
            Horizontal
        }

        [SerializeField] private AutoScrollOptions m_setAutoScrollOption = AutoScrollOptions.Both;
        [SerializeField] private float m_scrollSpeed = 50.0f;

        [SerializeField] private ScrollRect m_targetScrollRect;
        [SerializeField] private RectTransform m_scrollWindow;      //Used in rect.height.
        [SerializeField] private GameObject m_scrollRectContent;
        [SerializeField] private LayoutGroup m_layoutGroup;         //Used in group.anchoredPosition (.x and .y).

        private GameObject m_lastSelectedGameObject;
        private List<GameObject> m_scrollViewGameObjects = new List<GameObject>();
        private bool m_canAutoScroll = false;

        private void Awake()
        {
            m_scrollViewGameObjects.Clear();

            m_targetScrollRect = GetComponent<ScrollRect>();
            m_scrollWindow = m_targetScrollRect.GetComponent<RectTransform>();
            m_scrollRectContent = m_targetScrollRect.content.gameObject;
            m_layoutGroup = m_scrollRectContent.GetComponent<LayoutGroup>();

            GetAutoScrollOptions(m_targetScrollRect);

            MenuNavigation.ALastSelectedGameObject += UpdateCurrentGameObject;  //or 'EventSystem.current.currentSelectedGameObject'.
        }

        private void Start()
        {
            if (m_scrollRectContent != null)
            {
                ContentLevelIterations();
            }
        }

        private void OnDisable()
        {
            MenuNavigation.ALastSelectedGameObject -= UpdateCurrentGameObject;  //or 'EventSystem.current.currentSelectedGameObject'.
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
            //'m_layoutGroup = m_scrollRectContent.AddComponent<GridLayoutGroup/VerticalLayoutGroup/HorizontalLayoutGroup>()' and
            //'default: break;' - for savety. (It would currently go too far.)
        }

        /// <summary>
        /// Searches Content, of the currently active ScrollView, for GameObjects in each Parent/Child Level of the Hierarchy.
        /// </summary>
        private void ContentLevelIterations()
        {
            foreach (Transform transform in m_scrollRectContent.transform)
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
            //Debug.Log(m_canAutoScroll);
#endif
        }

        private void AutoScrollToNextGameObject()   //TODO:
        {
            if (!m_canAutoScroll)
                return;
        }
    }
}