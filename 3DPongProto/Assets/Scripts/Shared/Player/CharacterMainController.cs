using UnityEngine;

namespace ThreeDeePongProto.Shared.PlayerCharacter
{
    public class CharacterMainController : MonoBehaviour
    {
        [SerializeField] internal Transform m_inputAndCamComponent;

        //Direct Children
        [SerializeField] internal CharacterInputHandler m_playerInputHandler;
        [SerializeField] internal CharacterCameraController m_playerCameraController;

        //AvatarControls children
        internal CharacterMovement m_playerMovement { get; private set; }
        internal CharacterInteractions m_playerInteractions { get; private set; }
        internal CharacterHealth m_playerHealth { get; private set; }

        private int m_playerID;
        //private bool m_inputDisabled = false;

        private PlayerProfileData m_profileData;
        private MatchSettingsData m_matchConfigData;
        private ControlSettingsData m_controlConfigData;
        private bool m_isInitialized = false;

        private void Awake()
        {
            m_playerInputHandler = GetComponentInChildren<CharacterInputHandler>();
            m_playerCameraController = GetComponentInChildren<CharacterCameraController>();
            m_playerMovement = GetComponentInChildren<CharacterMovement>();

            if (m_playerMovement == null)
                Debug.LogError("CharacterMovement konnte nicht gefunden werden!", this);
        }

        private void Start()
        {
            InitializeScripts();
        }

        //private void Update()
        //{
        //    //if (m_playerInputHandler == null && !m_inputDisabled)
        //    //{
        //    //    m_inputDisabled = true;
        //    //    StartCoroutine(GetNewInputHandler());
        //    //}
        //}

        public void StoreData(ControlSettingsData _controlData, MatchSettingsData _matchData, PlayerProfileData _playerProfile)
        {
            m_controlConfigData = _controlData;
            m_matchConfigData = _matchData;
            m_profileData = _playerProfile;
            m_isInitialized = true;
            m_playerID = _playerProfile.PlayerID;
        }

        private void InitializeScripts()
        {
            if (!m_isInitialized)
            {
                Debug.LogError("CharacterMainController is not initialized!", this);
                return;
            }

            //Get all children that use the interface and submit the ID.
            IProvidePlayerID[] idReceivers = GetComponentsInChildren<IProvidePlayerID>();
            foreach (var receiver in idReceivers)
                receiver.SetPlayerID(m_playerID);

            ////Send relevant Data to the Sub-Components. (Currently not needed, because Interface sends playerID already.)
            //if (m_playerInputHandler != null)
            //    m_playerInputHandler.StoreData(m_profileData);

            if (m_playerMovement != null)
            {
                m_playerMovement.Initialize(m_controlConfigData, m_matchConfigData, m_profileData);
            }
        }

        ///// <summary>
        ///// Get a new CharacterInputHandler script, if the old version has to be replaced.
        ///// </summary>
        ///// <returns></returns>
        //private IEnumerator GetNewInputHandler()
        //{
        //    while (m_playerInputHandler == null)
        //    {
        //        m_playerInputHandler = GetComponentInChildren<CharacterInputHandler>();
        //        yield return new WaitForSeconds(1.0f);
        //    }
        //    //If a new 'CharacterInputHandler'-Script is found.
        //    m_inputDisabled = false;
        //}
    }
}