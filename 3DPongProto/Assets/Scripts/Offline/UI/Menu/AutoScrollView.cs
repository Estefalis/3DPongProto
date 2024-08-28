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
        [SerializeField] private RectTransform m_scrollWindow;

        private GameObject m_lastSelectedGameObject;

        //[SerializeField] private LayoutGroup m_layoutGroup;
        private void Awake()
        {
            m_targetScrollRect = GetComponent<ScrollRect>();
            m_scrollWindow = m_targetScrollRect.GetComponent<RectTransform>();
            GetAutoScrollOptions(m_targetScrollRect);
            //m_layoutGroup = GetComponentInChildren<LayoutGroup>();
            MenuNavigation.ALastSelectedGameObject += UpdateCurrentGameObject;
        }

        private void OnDisable()
        {
            MenuNavigation.ALastSelectedGameObject -= UpdateCurrentGameObject;
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
        }

        private void UpdateCurrentGameObject(GameObject _gameObject)
        {
            m_lastSelectedGameObject = _gameObject;
        }
    }
}