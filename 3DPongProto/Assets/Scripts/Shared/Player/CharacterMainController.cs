using System.Collections;
using ThreeDeePongProto.Shared.Managers;
using UnityEngine;

namespace ThreeDeePongProto.Shared.PlayerCharacter
{
    public class CharacterMainController : MonoBehaviour
    {
        [SerializeField] internal Transform m_inputAndCamComponent;
        [SerializeField] internal CharacterInputHandler m_playerInputHandler;
        [SerializeField] internal CharacterMovement m_playerMovement;
        [SerializeField] internal CharacterInteractions m_playerInteractions;
        [SerializeField] internal CharacterHealth m_playerHealth;
        [SerializeField] internal CharacterCameraController m_playerCameraController;

        //private UserInputManager m_userInputManager;
        internal LocalMatchManager m_localMatchManager;

        //[Header("Player Details")]
        //[SerializeField] private int m_playerID;
        [SerializeField] protected bool m_defaultFrontLineUp;

        #region Scriptable References
        [Header("Scriptable References")]
        [SerializeField] internal PlayerSOData[] m_playerSODatas;
        [SerializeField] internal ControlUIStates[] m_controlUIStates;
        [SerializeField] internal ControlUIValues[] m_controlUIValues;
        [SerializeField] internal MatchUIStates m_matchUIStates;
        [SerializeField] internal MatchValues m_matchValues;
        [SerializeField] internal BasicFieldValues m_basicFieldValues;
        #endregion

        private int m_setPlayerID = -1;
        internal float m_groundWidth, m_groundLength;
        internal float m_goalDistance;
        internal float m_maxPushDistance;
        internal Vector3 m_localPaddleScale;

        private bool m_inputDisabled = false;

        private void Awake()
        {
            //m_userInputManager = FindObjectOfType<UserInputManager>();
            m_localMatchManager = FindObjectOfType<LocalMatchManager>();
            transform.SetParent(m_localMatchManager.PrefabParent);

            GetFieldDetails();
            //GetPlayerDetails();
        }

        private void Update()
        {
            if (m_playerInputHandler == null && !m_inputDisabled)
            {
                m_inputDisabled = true;
                StartCoroutine(GetNewInputHandler());
            }
        }

        private void GetFieldDetails()
        {
            if (m_matchValues == null)
            {
                m_groundWidth = m_localMatchManager.DefaultFieldWidth;
                m_groundLength = m_localMatchManager.DefaultFieldLength;
            }
            else
            {
                m_groundWidth = m_basicFieldValues.SetGroundWidth;
                m_groundLength = m_basicFieldValues.SetGroundLength;
            }
        }

        internal void GetPlayerDetails()
        {
            if (m_playerSODatas[m_setPlayerID] == null ^ m_matchValues == null)
            {
                switch (m_defaultFrontLineUp)
                {
                    case true:
                        m_goalDistance = m_localMatchManager.DefaultFrontLineDistance;
                        break;
                    case false:
                        m_goalDistance = m_localMatchManager.DefaultBackLineDistance;
                        break;
                }

                m_maxPushDistance = m_localMatchManager.MaxPushDistance;
                m_localPaddleScale = m_localMatchManager.DefaultPaddleScale;
            }
            else
            {
                switch (m_playerSODatas[m_setPlayerID].PlayerOnFrontline)
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

        internal void ReceivePlayerID(int _playerID)
        {
            m_setPlayerID = _playerID;

            //Find all Components, that require the ID through Interface.
            IProvidePlayerID[] idReceivers = GetComponentsInChildren<IProvidePlayerID>();

            //Submit the playerID to each Script.
            foreach (var receiver in idReceivers)
            {
                receiver.SetPlayerID(m_setPlayerID);
            }

            GetPlayerDetails();
#if UNITY_EDITOR
            //Debug.Log($"Player is now set to ID Nr. {_playerID}.");
#endif
        }

        /// <summary>
        /// Get a new CharacterInputHandler script, if the old version has to be replaced.
        /// </summary>
        /// <returns></returns>
        private IEnumerator GetNewInputHandler()
        {
            while (m_playerInputHandler == null)
            {
                m_playerInputHandler = GetComponentInChildren<CharacterInputHandler>();
                yield return new WaitForSeconds(1.0f);
            }
            //If a new 'CharacterInputHandler'-Script is found.
            m_inputDisabled = false;
        }
    }
}