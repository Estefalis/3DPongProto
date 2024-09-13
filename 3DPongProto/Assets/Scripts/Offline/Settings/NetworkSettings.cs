using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeDeePongProto.Offline.Settings
{
    public class NetworkSettings : MonoBehaviour
    {
        [SerializeField] private ScrollRect m_scrollViewRect;
        [SerializeField] private RectTransform m_scrollContentRT;
        [SerializeField] private LayoutGroup m_gridLayoutGroup;

        private GridLayoutGroup.Constraint m_gridConstraint;

        private int m_contentChildCount;
        private float m_rest;
        Vector2Int m_gridSize;

        private void Awake()
        {
            m_contentChildCount = m_scrollContentRT.childCount;
            //m_gridSize.x = m_gridLayoutGroup.GetComponent<GridLayoutGroup>().constraintCount;
            //m_gridSize.y = m_contentChildCount / m_gridSize.x + Mathf.Min(1, m_contentChildCount % m_gridSize.x);

            //m_rest = (float)m_contentChildCount / m_gridSize.x - m_contentChildCount / m_gridSize.x;
            //m_gridSize.y = m_rest > 0 ? m_gridSize.x + 1 : m_gridSize.x;
            //Debug.Log($"ChildCount: {m_contentChildCount} - GridSizeX: {m_gridSize.x} - GridSizeY: {m_gridSize.y} - Rest: {m_rest}");

            GetGridDetails(m_gridLayoutGroup);
        }

        private void GetGridDetails(LayoutGroup _layoutGroup)
        {
            var gridSettings = _layoutGroup.GetComponent<GridLayoutGroup>();

            m_gridConstraint = gridSettings.constraint;
            var constraintCount = _layoutGroup.GetComponent<GridLayoutGroup>().constraintCount;
            float rest = 0.0f;
            switch (m_gridConstraint)
            {
                case GridLayoutGroup.Constraint.Flexible:
                    break;
                case GridLayoutGroup.Constraint.FixedColumnCount:
                    m_gridSize.x = constraintCount;
                    //rest = (float)m_contentChildCount / m_gridSize.x - m_gridSize.x;
                    //m_gridSize.y = rest > 0 ? m_gridSize.x + 1 : m_gridSize.x;
                    m_gridSize.y = GetOtherAxisCount(m_contentChildCount, m_gridSize.x);
                    break;
                case GridLayoutGroup.Constraint.FixedRowCount:
                    m_gridSize.y = constraintCount;
                    //rest = (float)m_contentChildCount / m_gridSize.y - m_gridSize.y;
                    //m_gridSize.x = rest > 0 ? m_gridSize.y + 1 : m_gridSize.y;
                    m_gridSize.x = GetOtherAxisCount(m_contentChildCount, m_gridSize.y);
                    break;
                default:
                    break;
            }
        }

        private static int GetOtherAxisCount(int m_contentChildCount, int _constraintAxisCount)
        {
            float rest = (float)m_contentChildCount / _constraintAxisCount - _constraintAxisCount;
            float addedCount = rest - (rest % 1);
            int otherAxisCount = rest <= 0 ? _constraintAxisCount + (int)addedCount : _constraintAxisCount + (int)addedCount + 1;
#if UNITY_EDITOR
            Debug.Log($"KnownAxis: {_constraintAxisCount} - Unknown: {otherAxisCount} - Rest: {rest} - AddedCount {addedCount}");
#endif
            return otherAxisCount;
        }
    }
}

//return m_contentChildCount / _constraintAxisCount + Mathf.Min(1, m_contentChildCount % _constraintAxisCount);