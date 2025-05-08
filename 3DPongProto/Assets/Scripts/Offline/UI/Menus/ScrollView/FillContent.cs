using TMPro;
using UnityEngine;

namespace ThreeDeePongProto.Shared.UI.Menu.ScrollViews
{
    internal class FillContent : MonoBehaviour
    {
        [SerializeField] internal ScrollViewController m_scrollViewController;

        [Header("Prefab Instantiation")]
        [SerializeField] internal GameObject m_spawnPrefab = null;
        [SerializeField] private int m_createChildAmount = 5;

        internal void SpawnContentChildren()
        {
            for (int i = 0; i < m_createChildAmount; i++)
            {
                m_spawnPrefab.name = $"Btn-ID {i}";
                m_spawnPrefab.GetComponentInChildren<TextMeshProUGUI>().text = $"Btn-ID {i}";
                Instantiate(m_spawnPrefab, m_scrollViewController.m_scrollViewContent);
            }

            m_scrollViewController.m_childrenSpawned = true;
            m_scrollViewController.m_contentChildCount = m_scrollViewController.m_scrollViewContent.childCount;
            m_scrollViewController.CacheSelectableChildren();
        }

        #region InstaniateNavigation
        //private void SetInstantiateNavigation()
        //{
        //Navigation navigation;

        //switch (m_scrollLayout)
        //{
        //    case EScrollLayout.None:
        //    default:
        //        break;
        //    case EScrollLayout.Vertical:
        //    {
        //        m_dictKeys = new(m_ContentChildAnchorPos.Keys);

        //        for (int i = 0; i < m_ContentChildAnchorPos.Count; i++)
        //        {
        //            navigation = GetSimpleSelectableNavigation(m_simpleSelectable, i);

        //            navigation.selectOnUp = GetTopNavigation(m_simpleSelectable, i);
        //            navigation.selectOnDown = GetBottomNavigation(m_simpleSelectable, i);

        //            SetSimpleScrollViewNavigation(m_simpleSelectable, i, navigation);
        //        }

        //        break;
        //    }
        //    case EScrollLayout.Horizontal:
        //    {
        //        m_dictKeys = new(m_ContentChildAnchorPos.Keys);

        //        for (int i = 0; i < m_ContentChildAnchorPos.Count; i++)
        //        {
        //            navigation = GetSimpleSelectableNavigation(m_simpleSelectable, i);

        //            navigation.selectOnLeft = GetTopNavigation(m_simpleSelectable, i);
        //            navigation.selectOnRight = GetBottomNavigation(m_simpleSelectable, i);

        //            SetSimpleScrollViewNavigation(m_simpleSelectable, i, navigation);
        //        }

        //        break;
        //    }
        //    case EScrollLayout.Grid:  //Instantiate case gets set in 'GetScrollViewObjects()'.
        //    {
        //        m_dictKeys = new(m_ContentChildAnchorPos.Keys);
        //        m_gridSize = CustomGridLayoutSetup.GetGridSize(m_gridSettings);

        //        for (int i = 0; i < m_scrollViewContent.childCount; i++)
        //        {
        //            navigation = GetSimpleSelectableNavigation(m_simpleSelectable, i);

        //            navigation.selectOnUp = GetGridNavigationUp(m_simpleSelectable, i);
        //            navigation.selectOnDown = GetGridNavigationDown(m_simpleSelectable, i);
        //            navigation.selectOnLeft = GetGridNavigationLeft(m_simpleSelectable, i);
        //            navigation.selectOnRight = GetGridNavigationRight(m_simpleSelectable, i);

        //            SetSimpleScrollViewNavigation(m_simpleSelectable, i, navigation);
        //        }
        //        break;
        //    }
        //}
        //break;
        //}

        //private Navigation GetSimpleSelectableNavigation(Selectable _simpleSelectable, int _index)
        //{
        //    Navigation navigation = default;

        //    switch (_simpleSelectable)
        //    {
        //        case Toggle:
        //        {
        //            navigation = m_dictKeys[_index].GetComponent<Toggle>().navigation;
        //            break;
        //        }
        //        case Slider:
        //        {
        //            navigation = m_dictKeys[_index].GetComponent<Slider>().navigation;
        //            break;
        //        }
        //        case Button:
        //        {
        //            navigation = m_dictKeys[_index].GetComponent<Button>().navigation;
        //            break;
        //        }
        //    }

        //    return navigation;
        //}

        //        private void SetSimpleScrollViewNavigation(Selectable _simpleSelectable, int _index, Navigation _navigation)
        //        {
        //            switch (_simpleSelectable)
        //            {
        //                case Toggle:
        //                {
        //                    m_dictKeys[_index].GetComponent<Toggle>().navigation = _navigation;
        //                    break;
        //                }
        //                case Slider:
        //                {
        //                    m_dictKeys[_index].GetComponent<Slider>().navigation = _navigation;
        //                    break;
        //                }
        //                case Button:
        //                {
        //                    m_dictKeys[_index].GetComponent<Button>().navigation = _navigation;
        //                    break;
        //                }
        //            }

        //            m_ObjectNavigation.Add(m_dictKeys[_index], _navigation);
        //#if UNITY_EDITOR
        //            //Debug.Log(m_ContentChildAnchorPos[m_scrollViewContent.transform.GetChild(_index).gameObject].name);
        //#endif
        //        }

        #region ScrollView Navigation Directions
        #region Vertical & Horizontal Navigation
        //private Selectable GetTopNavigation(Selectable _simpleSelectable, int _objectIndex)
        //{
        //    if (_objectIndex == 0 && m_borderLoop)
        //    {
        //        return _ = GetSelectableComponent(_simpleSelectable, m_scrollViewContent.transform.childCount - 1);
        //    }
        //    else if (_objectIndex == 0 && !m_borderLoop)
        //    {
        //        return null;
        //    }
        //    else
        //    {
        //        return _ = GetSelectableComponent(_simpleSelectable, _objectIndex - 1);
        //    }
        //}

        //private Selectable GetBottomNavigation(Selectable _simpleSelectable, int _objectIndex)
        //{
        //    if (_objectIndex == m_scrollViewContent.transform.childCount - 1 && m_borderLoop)
        //    {
        //        return _ = GetSelectableComponent(_simpleSelectable, 0);
        //    }
        //    else if (_objectIndex == m_scrollViewContent.transform.childCount - 1 && !m_borderLoop)
        //    {
        //        return null;
        //    }
        //    else /*if (_objectIndex < m_scrollViewContent.transform.childCount - 1)*/
        //    {
        //        return _ = GetSelectableComponent(_simpleSelectable, _objectIndex + 1);
        //    }
        //}
        #endregion

        #region Grid Navigation
        //        private Selectable GetGridNavigationUp(Selectable _selectable, int _currentIndex)
        //        {
        //            switch (m_gridSettings.constraint)
        //            {
        //                case GridLayoutGroup.Constraint.FixedColumnCount:
        //                case GridLayoutGroup.Constraint.FixedRowCount:
        //                {
        //                    bool topBorderFixedColumn = _currentIndex < m_gridSettings.constraintCount;
        //                    bool topBorderFixedRow = _currentIndex < m_gridSize.x;
        //                    bool topBorderIndexSwitch =
        //                        m_gridSettings.constraint == GridLayoutGroup.Constraint.FixedColumnCount ? topBorderFixedColumn : topBorderFixedRow;

        //                    int targetIndexFixedColumn = _currentIndex + m_gridSettings.constraintCount * m_gridSize.y - m_gridSettings.constraintCount;
        //                    int targetIndexFixedRow = _currentIndex + m_gridSize.x * m_gridSize.y - m_gridSize.x;
        //                    //int targetIndexSwitch = m_gridSettings.constraint == GridLayoutGroup.Constraint.FixedColumnCount ? targetIndexFixedColumn : targetIndexFixedRow;

        //                    int altTargetIndexFixedColumn =
        //                        _currentIndex + m_gridSettings.constraintCount * m_gridSize.y - (m_gridSettings.constraintCount * 2);
        //                    int altTargetIndexFixedRow =
        //                        _currentIndex + m_gridSize.x * m_gridSize.y - (m_gridSize.x * 2);
        //                    //int altTargetSwitch = m_gridSettings.constraint == GridLayoutGroup.Constraint.FixedColumnCount ? altTargetIndexFixedColumn : altTargetIndexFixedRow;

        //                    switch (topBorderIndexSwitch)
        //                    {
        //                        case false:
        //                        {
        //                            return m_gridSettings.constraint switch
        //                            {
        //                                GridLayoutGroup.Constraint.FixedColumnCount => _ = GetSelectableComponent(_selectable, _currentIndex - m_gridSettings.constraintCount),
        //                                GridLayoutGroup.Constraint.FixedRowCount => _ = GetSelectableComponent(_selectable, _currentIndex - m_gridSize.x),
        //                                _ => null,
        //                            };
        //                        }
        //                        case true:
        //                        {
        //                            switch (m_borderLoop)
        //                            {
        //                                case false:
        //                                    return null;
        //                                case true:
        //                                {
        //                                    switch (m_gridSettings.constraint)
        //                                    {
        //                                        case GridLayoutGroup.Constraint.FixedColumnCount:
        //                                        {
        //                                            if (targetIndexFixedColumn <= m_scrollViewContent.childCount - 1)
        //                                                return _ = GetSelectableComponent(_selectable, targetIndexFixedColumn);
        //                                            else if (altTargetIndexFixedColumn != _currentIndex)
        //                                                return _ = GetSelectableComponent(_selectable, altTargetIndexFixedColumn);
        //                                            else
        //                                                return null;    //'else if' and 'return null' prevent the object to set itself to navigate to.
        //                                        }
        //                                        case GridLayoutGroup.Constraint.FixedRowCount:
        //                                        {
        //                                            if (targetIndexFixedRow <= m_scrollViewContent.childCount - 1)
        //                                                return _ = GetSelectableComponent(_selectable, targetIndexFixedRow);
        //                                            else if (altTargetIndexFixedRow != _currentIndex)
        //                                                return _ = GetSelectableComponent(_selectable, altTargetIndexFixedRow);
        //                                            else
        //                                                return null;    //'else if' and 'return null' prevent the object to set itself to navigate to.
        //                                        }
        //                                        default:
        //                                            return null;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //                case GridLayoutGroup.Constraint.Flexible:
        //                {
        //                    return null;
        //                }
        //            }

        //            return null;
        //        }

        //        private Selectable GetGridNavigationDown(Selectable _selectable, int _currentIndex)
        //        {////'gridSize.y' == 'm_gridSettings.constraintCount' on FixedRowCount.
        //            switch (m_gridSettings.constraint)
        //            {
        //                case GridLayoutGroup.Constraint.FixedColumnCount:
        //                case GridLayoutGroup.Constraint.FixedRowCount:
        //                {
        //                    bool indexOutOfRangeFixedColumn = _currentIndex + m_gridSettings.constraintCount > m_scrollViewContent.childCount - 1;
        //                    bool indexOutOfRangeFixedRow = _currentIndex + m_gridSize.x > m_scrollViewContent.childCount - 1;

        //                    bool indexOufOfRangedSwitch =
        //                        m_gridSettings.constraint == GridLayoutGroup.Constraint.FixedColumnCount ? indexOutOfRangeFixedColumn : indexOutOfRangeFixedRow;

        //                    switch (indexOufOfRangedSwitch)
        //                    {
        //                        case false:
        //                        {
        //                            switch (m_gridSettings.constraint)
        //                            {
        //                                case GridLayoutGroup.Constraint.FixedColumnCount:
        //                                {
        //                                    return _ = GetSelectableComponent(_selectable, _currentIndex + m_gridSettings.constraintCount);
        //                                }
        //                                case GridLayoutGroup.Constraint.FixedRowCount:
        //                                {
        //                                    return _ = GetSelectableComponent(_selectable, _currentIndex + m_gridSize.x);
        //                                }
        //                                default:
        //                                    return null;
        //                            }
        //                        }
        //                        case true:
        //                        {
        //                            switch (m_borderLoop)
        //                            {
        //                                case false:
        //                                    return null;
        //                                case true:
        //                                {
        //                                    switch (m_gridSettings.constraint)
        //                                    {
        //                                        case GridLayoutGroup.Constraint.FixedColumnCount:
        //                                        {
        //                                            if (_currentIndex % m_gridSettings.constraintCount != _currentIndex)
        //                                                return _ = GetSelectableComponent(_selectable, _currentIndex % m_gridSettings.constraintCount);
        //                                            else
        //                                                return null;    //'return null' prevents the object to set itself to navigate to.
        //                                        }
        //                                        case GridLayoutGroup.Constraint.FixedRowCount:
        //                                        {
        //                                            if (_currentIndex % m_gridSize.x != _currentIndex)
        //                                                return _ = GetSelectableComponent(_selectable, _currentIndex % m_gridSize.x);
        //                                            else
        //                                                return null;    //'return null' prevents the object to set itself to navigate to.
        //                                        }
        //                                        default:
        //                                            return null;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //                case GridLayoutGroup.Constraint.Flexible:
        //                {
        //                    return null;
        //                }
        //            }

        //            return null;
        //        }

        //        private Selectable GetGridNavigationLeft(Selectable _selectable, int _currentIndex)
        //        {
        //            bool indexSaveWithinRange = _currentIndex > 0;  //Index 0 gets handled in 'else case'.

        //            bool leftBorderFixedColumn = _currentIndex % m_gridSettings.constraintCount == 0;
        //            int lastChildIndexFixedColumn = (m_scrollViewContent.childCount - 1) % m_gridSettings.constraintCount;

        //            bool leftBorderFixedRow = _currentIndex % m_gridSize.x == 0;
        //            int lastChildIndexFixedRow = (m_scrollViewContent.childCount - 1) % m_gridSize.x;

        //            switch (m_gridSettings.constraint)
        //            {
        //                case GridLayoutGroup.Constraint.FixedColumnCount:
        //                case GridLayoutGroup.Constraint.FixedRowCount:
        //                {
        //                    bool leftBorderIndexSwitch =
        //                        m_gridSettings.constraint == GridLayoutGroup.Constraint.FixedColumnCount ? leftBorderFixedColumn : leftBorderFixedRow;

        //                    //int lastChildIndexSwitch =
        //                    //    m_gridSettings.constraint == GridLayoutGroup.Constraint.FixedColumnCount ? lastChildIndexFixedColumn : lastChildIndexFixedRow;

        //                    int targetIndex = m_gridSettings.constraint == GridLayoutGroup.Constraint.FixedColumnCount ? _currentIndex + (m_gridSettings.constraintCount - 1) : _currentIndex + (m_gridSize.x - 1);

        //                    if (indexSaveWithinRange)
        //                    {
        //                        switch (leftBorderIndexSwitch)
        //                        {
        //                            case false:
        //                                return _ = GetSelectableComponent(_selectable, _currentIndex - 1);
        //                            case true:
        //                            {
        //                                //HINT: childIndex 0 got excluded above to prevent an exception. So contentChild-IDs == _currentIndex.
        //                                switch (m_borderLoop)
        //                                {
        //                                    case false:
        //                                        return null;
        //                                    case true:
        //                                    {
        //                                        switch (targetIndex <= m_scrollViewContent.childCount - 1)
        //                                        {
        //                                            case true:  //TargetIndices are rightBorderIndices from full rows.
        //                                                return _ = GetSelectableComponent(_selectable, targetIndex);
        //                                            case false: //TargetIndex is the last childIndex, if the common TargetIndex would be out of range.
        //                                            {
        //                                                switch (m_gridSettings.constraint)
        //                                                {
        //                                                    case GridLayoutGroup.Constraint.FixedColumnCount:
        //                                                    {
        //                                                        if (_currentIndex % m_gridSettings.constraintCount != lastChildIndexFixedColumn)
        //                                                            return _ =
        //                                                                GetSelectableComponent(_selectable, _currentIndex + lastChildIndexFixedColumn);
        //                                                        //else
        //                                                        return null;    //'return null' prevents the object to set itself to navigate to.
        //                                                    }
        //                                                    case GridLayoutGroup.Constraint.FixedRowCount:
        //                                                    {
        //                                                        if (_currentIndex % m_gridSize.x != lastChildIndexFixedRow)
        //                                                            return _ = GetSelectableComponent(_selectable, _currentIndex + lastChildIndexFixedRow);
        //                                                        //else
        //                                                        return null;    //'return null' prevents the object to set itself to navigate to.
        //                                                    }
        //                                                    default:
        //                                                        return null;
        //                                                }
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (_currentIndex == 0)
        //                        {
        //                            switch (m_borderLoop)
        //                            {
        //                                case false:
        //                                {
        //                                    //TODO: //Get/Find components out of UI. Or last button of the same line on looping.
        //                                    return null;
        //                                }
        //                                case true: //Case for excluded Index 0 from 'leftBorderFixedColumn' or 'leftBorderFixedRow'.
        //                                {
        //                                    switch (m_gridSettings.constraint)
        //                                    {
        //                                        case GridLayoutGroup.Constraint.FixedColumnCount:
        //                                        {
        //                                            return _ = GetSelectableComponent(_selectable, _currentIndex + (m_gridSettings.constraintCount - 1));
        //                                        }
        //                                        case GridLayoutGroup.Constraint.FixedRowCount:
        //                                        {
        //                                            return _ = GetSelectableComponent(_selectable, _currentIndex + (m_gridSize.x - 1));
        //                                        }
        //                                        default:
        //                                            return null;
        //                                    }
        //                                }

        //                            }
        //                        }
        //                    }

        //                    return null;
        //                }
        //                case GridLayoutGroup.Constraint.Flexible:
        //                {
        //                    return null;
        //                }
        //            }

        //            return null;
        //        }

        //        private Selectable GetGridNavigationRight(Selectable _selectable, int _currentIndex)
        //        {
        //            bool indexSaveWithinRange = _currentIndex < m_scrollViewContent.childCount - 1;

        //            bool leftBorderFixedColumn = _currentIndex % m_gridSettings.constraintCount == 0;
        //            bool rightBorderFixedColumn = _currentIndex % m_gridSettings.constraintCount == m_gridSettings.constraintCount - 1;

        //#if UNITY_EDITOR
        //            //if (_currentIndex % m_gridSize.x == 0)
        //            //Debug.Log($"Modulo0: {_currentIndex} | Modulo-GridX: {_currentIndex + (m_gridSize.x + m_gridSize.x % m_gridSize.x - 1)}");
        //#endif
        //            bool leftBorderFixedRow = _currentIndex % m_gridSize.x == 0;
        //            bool rightBorderFixedRow = _currentIndex % m_gridSize.x == m_gridSize.x - 1;

        //            switch (m_gridSettings.constraint)
        //            {
        //                case GridLayoutGroup.Constraint.FixedColumnCount:
        //                case GridLayoutGroup.Constraint.FixedRowCount:
        //                {
        //                    bool rightBorderIndexSwitch = m_gridSettings.constraint == GridLayoutGroup.Constraint.FixedColumnCount ? rightBorderFixedColumn : rightBorderFixedRow;

        //                    if (indexSaveWithinRange)
        //                    {
        //                        switch (rightBorderIndexSwitch)
        //                        {
        //                            case false:
        //                                return _ = GetSelectableComponent(_selectable, _currentIndex + 1);
        //                            case true:
        //                            {
        //                                switch (m_borderLoop)
        //                                {
        //                                    case false:
        //                                        return null;
        //                                    case true:
        //                                    {
        //                                        switch (m_gridSettings.constraint)
        //                                        {
        //                                            case GridLayoutGroup.Constraint.FixedColumnCount:
        //                                            {
        //                                                //Loop for full filled rows with 'rightBorderFixedColumn'.
        //                                                return _ = GetSelectableComponent(_selectable, _currentIndex - (m_gridSettings.constraintCount - 1));
        //                                            }
        //                                            case GridLayoutGroup.Constraint.FixedRowCount:
        //                                            {
        //                                                //Loop for full filled rows with 'rightBorderFixedRow'.
        //                                                return _ = GetSelectableComponent(_selectable, _currentIndex - (m_gridSize.x - 1));
        //                                            }
        //                                            default:
        //                                                return null;
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (_currentIndex == m_scrollViewContent.childCount - 1)
        //                        {
        //                            switch (m_borderLoop)
        //                            {
        //                                case false:
        //                                {
        //                                    //TODO: //Get/Find components out of UI. Or first button of the same line on looping.
        //                                    return null;
        //                                }
        //                                case true: //Case for incomplete rows without 'rightBorderFixedColumn' or 'rightBorderFixedRow'.
        //                                {
        //                                    //'else return null' prevents the object to set itself to navigate to.
        //                                    switch (m_gridSettings.constraint)
        //                                    {
        //                                        case GridLayoutGroup.Constraint.FixedColumnCount:
        //                                        {
        //                                            if (!leftBorderFixedColumn)
        //                                                return _ = GetSelectableComponent(_selectable, _currentIndex - (_currentIndex % m_gridSettings.constraintCount));
        //                                            else
        //                                                return null; //TODO: //Get/Find components out of UI. Or first button of the same line on looping.
        //                                        }
        //                                        case GridLayoutGroup.Constraint.FixedRowCount:
        //                                        {
        //                                            if (!leftBorderFixedRow)
        //                                                return _ = GetSelectableComponent(_selectable, _currentIndex - (_currentIndex % m_gridSize.x));
        //                                            else
        //                                                return null; //TODO: //Get/Find components out of UI. Or first button of the same line on looping.
        //                                        }
        //                                        default:
        //                                            return null;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }

        //                    return null;
        //                }
        //                case GridLayoutGroup.Constraint.Flexible:
        //                {
        //                    return null;
        //                }
        //                default:
        //                    return null;
        //            }
        //        }
        #endregion
        #endregion

        //private Selectable GetSelectableComponent(Selectable _selectableObject, int _targetIndex)
        //{
        //    Toggle selectableToggle;
        //    Slider selectableSlider;
        //    Button selectableButton;

        //    switch (_selectableObject)
        //    {
        //        case Toggle:
        //        {
        //            selectableToggle =
        //                m_scrollViewContent.transform.GetChild(_targetIndex).GetComponent<Toggle>();
        //            return selectableToggle.GetComponent<Selectable>();
        //        }
        //        case Slider:
        //        {
        //            selectableSlider =
        //                m_scrollViewContent.transform.GetChild(_targetIndex).GetComponent<Slider>();
        //            return selectableSlider.GetComponent<Selectable>();
        //        }
        //        case Button:
        //        {
        //            selectableButton =
        //                m_scrollViewContent.transform.GetChild(_targetIndex).GetComponent<Button>();
        //            return selectableButton.GetComponent<Selectable>();
        //        }
        //        default:
        //            return null;
        //    }
        //}
        #endregion
    }
}