using System;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeDeePongProto.Shared.HelperClasses
{
    internal static class CustomGridLayoutSetup
    {
        internal static Vector2Int GetGridSize(GridLayoutGroup _gridLayoutGroup)
        {
            int childCount = _gridLayoutGroup.transform.childCount;
            int constraintCount = _gridLayoutGroup.constraintCount;
            Vector2Int gridSize = Vector2Int.zero;

            if (childCount == 0)
                return gridSize;

            switch (_gridLayoutGroup.constraint)
            {
                case GridLayoutGroup.Constraint.Flexible:
                    gridSize = GetFlexibleGridSize(_gridLayoutGroup, childCount);
                    break;
                case GridLayoutGroup.Constraint.FixedColumnCount:
                    gridSize.x = constraintCount;
                    gridSize.y = GetOtherAxisCount(gridSize.x, childCount);
                    break;
                case GridLayoutGroup.Constraint.FixedRowCount:
                    gridSize.y = constraintCount;
                    gridSize.x = GetOtherAxisCount(gridSize.y, childCount);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unexpected constraint type: {_gridLayoutGroup.constraint}");
            }

            return gridSize;
        }

        private static Vector2Int GetFlexibleGridSize(GridLayoutGroup _gridLayoutGroup, int _childCount)
        {
            if (_childCount == 0)
                return Vector2Int.zero;

            int xAxisCount = 0, yAxisCount = 0;

            float squareRoot = Mathf.Sqrt(_childCount);
            xAxisCount = Mathf.CeilToInt(squareRoot);   //Ceil > ceiling > up!
            yAxisCount = GetOtherAxisCount(xAxisCount, _childCount);

#if UNITY_EDITOR
            //Debug.Log($"Column: {xAxisCount} - Row: {yAxisCount}");
#endif
            return new Vector2Int(xAxisCount, yAxisCount);  //Else return Vector2Int.zero;
        }

        private static int GetOtherAxisCount(int _constraintAxisCount, int _contentChildCount)
        {
            float lambdaSwitch = (float)_contentChildCount / _constraintAxisCount - _constraintAxisCount;
            float addedCount = lambdaSwitch - (lambdaSwitch % 1);
            int otherAxisCount = lambdaSwitch <= 0 ? _constraintAxisCount + (int)addedCount : _constraintAxisCount + (int)addedCount + 1;
            return otherAxisCount;
        }
    }
}