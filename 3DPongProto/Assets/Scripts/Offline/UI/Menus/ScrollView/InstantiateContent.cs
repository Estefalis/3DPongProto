using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeDeePongProto.Shared.UI.Menu.ScrollViews
{
    //Instantiated Navigation made with help from Gemini 2.5 Pro.
    internal class InstantiateContent : MonoBehaviour
    {
        [SerializeField] internal ScrollViewController m_scrollViewController;

        [Header("Prefab Instantiation")]
        [SerializeField] internal GameObject m_spawnPrefab = null;
        [SerializeField] private int m_createChildAmount = 5;

        internal void SpawnContentChildren()
        {
            if (m_spawnPrefab == null)
            {
                Debug.LogWarning($"Prefab to spawn is not set!");
                return;
            }

            for (int i = 0; i < m_createChildAmount; i++)
            {
                GameObject newChild = Instantiate(m_spawnPrefab, m_scrollViewController.m_scrollViewContent);
                newChild.name = $"Btn-ID {i}";
                TextMeshProUGUI textComponent = newChild.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                    textComponent.text = $"Btn-ID {i}";
                else
                    Debug.LogWarning($"Prefab child 'Btn-ID {i}' has no TextMeshProUGUI component.");
            }

            m_scrollViewController.CacheSelectableChildren();
        }

        internal void SetupExplicitNavigation(bool _instantiated = false)
        {
            if (!_instantiated)
                return;

            var scrollControllerList = m_scrollViewController.ContainedSelectables;
            var loopNavigation = m_scrollViewController.m_loopNavigation == true ? ELoopOption.LastToFirstCount : ELoopOption.None;
            ELoopOption loopOption = m_scrollViewController.m_eLoopOption;

            if (scrollControllerList == null || scrollControllerList.Count == 0)
            {
                Debug.LogWarning("No Selectables found for Navigation-Setup. Cancelling.");
                return;
            }
#if UNITY_EDITOR
            //Debug.Log($"SetupExplicitNavigation started. Loop: {m_scrollViewController.m_loopNavigation}, Direction: {m_scrollViewController.EScrollDirection}, ElementCount: {scrollControllerList.Count}");
#endif

            for (int i = 0; i < scrollControllerList.Count; i++)
            {
                Selectable currentSelectable = scrollControllerList[i];
                if (currentSelectable == null)
                    continue;

                Navigation newNav = new() { mode = Navigation.Mode.Explicit };

                switch (m_scrollViewController.m_layoutGroup)
                {
                    case VerticalLayoutGroup:
                    {
                        newNav.selectOnUp = GetVerticalNavigation(i, -1, scrollControllerList, loopNavigation);
                        newNav.selectOnDown = GetVerticalNavigation(i, 1, scrollControllerList, loopNavigation);
                        //newNav.selectOnLeft = someExternalSelectable;
                        //newNav.selectOnRight = someOtherExternalSelectable;
                        break;
                    }
                    case HorizontalLayoutGroup:
                    {
                        newNav.selectOnLeft = GetHorizontalNavigation(i, -1, scrollControllerList, loopNavigation);
                        newNav.selectOnRight = GetHorizontalNavigation(i, 1, scrollControllerList, loopNavigation);
                        //newNav.selectOnUp = someExternalSelectable;
                        //newNav.selectOnDown = someOtherExternalSelectable;
                        break;
                    }
                    case GridLayoutGroup:
                    {
                        if (m_scrollViewController.ConstraintCount <= 0)
                            newNav.mode = Navigation.Mode.Automatic;
                        else
                        {
                            //Up/Down _direction are switched in Unity.
                            newNav.selectOnUp = GetGridNavigation(i, Vector2Int.down, scrollControllerList, loopOption);
                            newNav.selectOnDown = GetGridNavigation(i, Vector2Int.up, scrollControllerList, loopOption);
                            newNav.selectOnLeft = GetGridNavigation(i, Vector2Int.left, scrollControllerList, loopOption);
                            newNav.selectOnRight = GetGridNavigation(i, Vector2Int.right, scrollControllerList, loopOption);
                        }

                        break;
                    }
                }

                currentSelectable.navigation = newNav;
            }

            m_scrollViewController.m_ChildrenNavigationSet = true;
#if UNITY_EDITOR
            Debug.Log("Explicit Navigation set.");
#endif
        }


        private Selectable GetVerticalNavigation(int _currentIndex, int _direction, List<Selectable> _listItems, ELoopOption _eLoopOption)
        {
            int targetIndex = _currentIndex + _direction;
            if (_eLoopOption == ELoopOption.LastToFirstCount)
            {
                if (targetIndex < 0)
                    targetIndex = _listItems.Count - 1;             //Last element.
                else if (targetIndex >= _listItems.Count)
                    targetIndex = 0;                                //First element.
            }

            if (targetIndex >= 0 && targetIndex < _listItems.Count)
            {
                return _listItems[targetIndex];
            }
            return null;                                            //Returns null, if !_loopOption, or there is no targetElement.
        }

        private Selectable GetHorizontalNavigation(int _currentIndex, int _direction, List<Selectable> _listItems, ELoopOption _eLoopOption)
        {
            return GetVerticalNavigation(_currentIndex, _direction, _listItems, _eLoopOption);
        }

        private Selectable GetGridNavigation(int _currentIndex, Vector2Int _direction, List<Selectable> _listItems, ELoopOption _loopOption)
        {
            //Basic checks and Initializing.
            if (_listItems == null || _listItems.Count <= 1)
                return null;    //No navigation possible, if list is null, or if there are less than 2 elements.

            int totalItems = _listItems.Count;
            int constraintCount = m_scrollViewController.ConstraintCount;
            if (constraintCount <= 0)
                return null;    //No navigation possible, without a valid 'constraintCount'.

            int numCols = constraintCount;
            int numRows = Mathf.CeilToInt((float)totalItems / (float)numCols);
            int currentRow = _currentIndex / numCols;
            int currentCol = _currentIndex % numCols;

            //ELoopOption switch to structure navigation logics.
            switch (_loopOption)
            {
                //Special case: Strict looping within each Row/Column.
                case ELoopOption.LastToFirstRowColumn:
                {
                    //Horizontal navigation-loop within a row.
                    if (_direction.x != 0)
                    {
                        var itemsInCurrentRow = new List<Selectable>();
                        var indicesInCurrentRow = new List<int>();

                        //List-Collection of elements within the current row.
                        for (int c = 0; c < numCols; ++c)
                        {
                            int itemIndex = currentRow * numCols + c;
                            if (itemIndex < totalItems)
                            {
                                itemsInCurrentRow.Add(_listItems[itemIndex]);
                                indicesInCurrentRow.Add(c); //Saving Column-Index.
                            }
                        }

                        if (itemsInCurrentRow.Count == 0)
                            return null;

                        //Find the position of the current element in the filtered Row-List.
                        int currentItemSubIndex = indicesInCurrentRow.IndexOf(currentCol);
                        if (currentItemSubIndex == -1)
                            return null;

                        //Calculate the targetIndex and apply the loop.
                        int targetItemSubIndex = currentItemSubIndex + _direction.x;
                        if (targetItemSubIndex < 0)
                            targetItemSubIndex = itemsInCurrentRow.Count - 1;
                        else if (targetItemSubIndex >= itemsInCurrentRow.Count)
                            targetItemSubIndex = 0;

                        return itemsInCurrentRow[targetItemSubIndex];
                    }
                    //Vertical navigation-loop within the column.
                    else if (_direction.y != 0)
                    {
                        var itemsInCurrentCol = new List<Selectable>();
                        var indicesInCurrentCol = new List<int>();

                        //List-Collection of elements within the current column.
                        for (int r = 0; r < numRows; ++r)
                        {
                            int itemIndex = r * numCols + currentCol;
                            if (itemIndex < totalItems)
                            {
                                itemsInCurrentCol.Add(_listItems[itemIndex]);
                                indicesInCurrentCol.Add(r); //Saving Row-Index.
                            }
                        }

                        if (itemsInCurrentCol.Count == 0)
                            return null;

                        //Find the position of the current element in the filtered Column-List.
                        int currentItemSubIndex = indicesInCurrentCol.IndexOf(currentRow);
                        if (currentItemSubIndex == -1)
                            return null;

                        //Calculate the targetIndex and apply the loop.
                        int targetItemSubIndex = currentItemSubIndex + _direction.y;
                        if (targetItemSubIndex < 0)
                            targetItemSubIndex = itemsInCurrentCol.Count - 1;
                        else if (targetItemSubIndex >= itemsInCurrentCol.Count)
                            targetItemSubIndex = 0;

                        return itemsInCurrentCol[targetItemSubIndex];
                    }
                    break; //End of ELoopOption.LastToFirstRowColumn.
                }

                //STANDARD-NAVIGATION for ELoopOption.None & ELoopOption.LastToFirstCount: "Grid Wrap" horizontal, Standard vertical.
                case ELoopOption.None:                  //No loop between first and last contentChild.
                case ELoopOption.LastToFirstCount:      //Loop between first and last contentChild.
                {
                    if (_loopOption == ELoopOption.LastToFirstCount)
                    {
                        if (_currentIndex == 0 && (_direction.y < 0 || _direction.x < 0)) //V2 Up/Left on 1st element.
                        {
                            return _listItems[totalItems - 1];  //returns last child to loop to.
                        }
                        if (_currentIndex == totalItems - 1 && (_direction.y > 0 || _direction.x > 0)) //V2 Down/Right on last element.
                        {
                            return _listItems[0];               //returns first child to loop to.
                        }
                    }
                    //ELoopOption.None: return null, to enable manual navigation to external Selectables.

                    //Horizontal "Grid Wrap" Navigation.
                    if (_direction.x != 0)
                    {
                        //Go to next/back to previous line in the contentChildCount by adding incoming +/- _direction.x (GridWrap).
                        int targetIndex = _currentIndex + _direction.x;
                        if (targetIndex >= 0 && targetIndex < totalItems)
                        {
                            return _listItems[targetIndex];
                        }
                    }

                    //Standard vertical Navigation.
                    if (_direction.y != 0)
                    {
                        //Navigate to the element above/below in the same Column.
                        int targetRow = currentRow + _direction.y;
                        //Check, if the targetIndex is within the grid.
                        if (targetRow >= 0 && targetRow < numRows)
                        {
                            int targetIndex = targetRow * numCols + currentCol;
                            //Check, if the targeted Element does exist. Prevents jumping into incomplete last rows.
                            if (targetIndex >= 0 && targetIndex < totalItems)
                            {
                                return _listItems[targetIndex];
                            }
                        }
                    }

                    return null;    //Fallback, if no navigation cannot be found/set.
                }
            }

            return null;    //Final switch-Fallback.
        }

        #region InstaniateNavigation
        //private void SetInstantiateNavigation()
        //{
        //Navigation navigation;

        //switch (m_eScrollDirection)
        //{
        //    case EScrollDirection.None:
        //    default:
        //        break;
        //    case EScrollDirection.Vertical:
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
        //    case EScrollDirection.Horizontal:
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
        //    case EScrollDirection.Auto:  //Instantiate case gets set in 'GetScrollViewObjects()'.
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
        #endregion

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
    }
}