using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeDeePongProto.Offline.UI.Menu
{
	public class AutoScrollView : MonoBehaviour
	{
        [SerializeField] internal MenuOrganisation m_menuOrganisation;
        //TODO: OnActivation each ScrollRect shall register itself on the MenuManager/MenuNavigation script.
    }
}