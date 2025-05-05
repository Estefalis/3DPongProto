using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    } 
}