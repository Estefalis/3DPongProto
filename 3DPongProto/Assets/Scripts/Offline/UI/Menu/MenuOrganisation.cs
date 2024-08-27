using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ThreeDeePongProto.Offline.UI.Menu
{
    public class MenuOrganisation : MonoBehaviour
    {
        [SerializeField] internal EventSystem m_eventSystem;
        [SerializeField] internal MenuNavigation m_menuNavigation;
        [SerializeField] internal InGameMenuActions m_inGameMenuActions;
        [SerializeField] internal AutoScrollView m_autoScrollView;
    }
}