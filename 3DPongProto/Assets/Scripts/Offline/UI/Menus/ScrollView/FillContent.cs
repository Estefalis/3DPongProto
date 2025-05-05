using System.Collections;
using System.Collections.Generic;
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
            int navObjCount = 0;

            bool containsToggle = m_spawnPrefab.TryGetComponent(out Toggle toggle);
            bool containsSlider = m_spawnPrefab.TryGetComponent(out Slider slider);
            bool containsButton = m_spawnPrefab.TryGetComponent(out Button button);

            foreach (Transform child in m_spawnPrefab.transform)
            {
                if (containsToggle)
                    navObjCount++;
                if (containsSlider)
                    navObjCount++;
                if (containsButton)
                    navObjCount++;
            }

            switch (navObjCount)    //determine how many NavigationObjects the contentChild Prefab has on each one.
            {
                case 0:
                    m_scrollViewController.m_navigationLevel = NavigationType.None;
                    return;
                case 1:
                {
                    m_scrollViewController.m_navigationLevel = NavigationType.Simple;

                    if (containsToggle)
                        m_scrollViewController.m_simpleSelectable = toggle;
                    if (containsSlider)
                        m_scrollViewController.m_simpleSelectable = slider;
                    if (containsButton)
                        m_scrollViewController.m_simpleSelectable = button;

                    break;
                }
                default:
                {
                    m_scrollViewController.m_navigationLevel = NavigationType.Nested;
                    break;
                }
            }

            for (int i = 0; i < m_createChildAmount; i++)
            {
                m_spawnPrefab.name = $"Btn-ID {i}";
                m_spawnPrefab.GetComponentInChildren<TextMeshProUGUI>().text = $"Btn-ID {i}";
                Instantiate(m_spawnPrefab, m_scrollViewController.m_scrollViewContent);
            }

            m_scrollViewController.m_childrenSpawned = true;
            m_scrollViewController.m_contentChildCount = m_scrollViewController.m_scrollViewContent.childCount;
        }
    } 
}