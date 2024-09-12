using ThreeDeePongProto.Offline.AudioManagement;
using ThreeDeePongProto.Offline.Managers;
using UnityEngine;

namespace ThreeDeePongProto.Shared.Player
{
    public class PlayerController : MonoBehaviour
    {
        internal MatchManager m_matchManager;
        [SerializeField] internal Rigidbody m_rigidbody;
        [SerializeField] protected AudioSource m_audioSource;

        [SerializeField] internal Transform m_inputAndCamComponent;
        [SerializeField] internal PlayerInputReceiver m_playerInputReceiver;
        [SerializeField] internal PlayerMovement m_playerMovement;
        [SerializeField] internal PlayerInteractions m_playerInteractions;
        [SerializeField] internal PlayerHealth m_playerHealth;
        [SerializeField] internal PlayerCameraController m_playerCameraControl;

        [Header("Player Details")]
        [SerializeField] internal int m_playerId;
        [SerializeField] protected bool m_defaultFrontLineUp;

        #region Scriptable References
        [Header("Scriptable References")]
        [SerializeField] internal PlayerIDData m_playerIDData;
        [SerializeField] internal ControlUIStates m_controlUIStates;
        [SerializeField] internal ControlUIValues m_controlUIValues;
        [SerializeField] internal MatchUIStates m_matchUIStates;
        [SerializeField] internal MatchValues m_matchValues;
        [SerializeField] internal BasicFieldValues m_basicFieldValues;
        #endregion

        internal float m_groundWidth, m_groundLength;
        internal float m_goalDistance;
        internal float m_maxPushDistance;
        internal Vector3 m_localPaddleScale;

        private void Awake()
        {
            m_matchManager = FindObjectOfType<MatchManager>();
            if(m_rigidbody == null)
                m_rigidbody = GetComponent<Rigidbody>();

            m_playerIDData.PlayerId = m_playerId;
            AudioManager.LetsRegisterAudioSources(m_audioSource);

            GetFieldDetails();
            GetPlayerDetails();
        }

        private void GetFieldDetails()
        {
            if (m_matchValues == null)
            {
                m_groundWidth = m_matchManager.DefaultFieldWidth;
                m_groundLength = m_matchManager.DefaultFieldLength;
            }
            else
            {
                m_groundWidth = m_basicFieldValues.SetGroundWidth;
                m_groundLength = m_basicFieldValues.SetGroundLength;
            }
        }

        private void GetPlayerDetails()
        {
            if (m_playerIDData == null ^ m_matchValues == null)
            {
                switch (m_defaultFrontLineUp)
                {
                    case true:
                        m_goalDistance = m_matchManager.DefaultFrontLineDistance;
                        break;
                    case false:
                        m_goalDistance = m_matchManager.DefaultBackLineDistance;
                        break;
                }

                m_maxPushDistance = m_matchManager.MaxPushDistance;
                m_localPaddleScale = m_matchManager.DefaultPaddleScale;
            }
            else
            {
                switch (m_playerIDData.PlayerOnFrontline)
                {
                    case true:
                        m_goalDistance = m_basicFieldValues.MinFrontLineDistance + m_basicFieldValues.FrontlineAdjustment + m_basicFieldValues.BacklineAdjustment;
                        break;
                    case false:
                        m_goalDistance = m_basicFieldValues.MinBackLineDistance + m_basicFieldValues.BacklineAdjustment;
                        break;
                }

                m_maxPushDistance = m_matchValues.MaxPushDistance;
                m_localPaddleScale = new Vector3(m_matchValues.XPaddleScale, m_matchValues.YPaddleScale, m_matchValues.ZPaddleScale);
            }
        }
    }
}