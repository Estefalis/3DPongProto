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
        Vector2Int m_gridSize;

        private void Awake()
        {
            m_contentChildCount = m_scrollContentRT.childCount;
            GetGridDetails(m_gridLayoutGroup);
        }

        private void GetGridDetails(LayoutGroup _layoutGroup)
        {
            var gridSettings = _layoutGroup.GetComponent<GridLayoutGroup>();

            m_gridConstraint = gridSettings.constraint;
            var constraintCount = _layoutGroup.GetComponent<GridLayoutGroup>().constraintCount;
            switch (m_gridConstraint)
            {
                case GridLayoutGroup.Constraint.Flexible:
                    break;
                case GridLayoutGroup.Constraint.FixedColumnCount:
                    m_gridSize.x = constraintCount;
                    m_gridSize.y = GetOtherAxisCount(m_contentChildCount, m_gridSize.x);
                    break;
                case GridLayoutGroup.Constraint.FixedRowCount:
                    m_gridSize.y = constraintCount;
                    m_gridSize.x = GetOtherAxisCount(m_contentChildCount, m_gridSize.y);
                    break;
                default:
                    break;
            }
        }

        private static int GetOtherAxisCount(int m_contentChildCount, int _constraintAxisCount)
        {
            float lambdaSwitch = (float)m_contentChildCount / _constraintAxisCount - _constraintAxisCount;
            float addedCount = lambdaSwitch - (lambdaSwitch % 1);
            int otherAxisCount = lambdaSwitch <= 0 ? _constraintAxisCount + (int)addedCount : _constraintAxisCount + (int)addedCount + 1;
#if UNITY_EDITOR
            Debug.Log($"ConstraintAxis: {_constraintAxisCount} - OtherAxis: {otherAxisCount} - Rest: {lambdaSwitch} - AddedCount {addedCount}");
#endif
            return otherAxisCount;
        }
    }
}