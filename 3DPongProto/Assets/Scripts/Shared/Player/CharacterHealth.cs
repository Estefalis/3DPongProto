using UnityEngine;

namespace ThreeDeePongProto.Shared.PlayerCharacter
{
    internal class CharacterHealth : MonoBehaviour
	{
		[SerializeField] internal CharacterMainController m_playerController;

        private void Awake()
        {
            m_playerController = GetComponentInParent<CharacterMainController>();
            if (m_playerController == null)
            {
                Debug.LogError("CharacterMovement could not find CharacterMainController!", this);
            }
        }
    } 
}