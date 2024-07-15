using System;
using ThreeDeePongProto.Offline.Managers;
using ThreeDeePongProto.Shared.InputActions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeDeePongProto.Offline.Player.Inputs
{
    public abstract class PlayerControlMain : MonoBehaviour
    {
        #region Script-References
        protected PlayerInputActions m_playerMovement;
        #endregion

        #region SerializeField-Member-Variables
        [SerializeField] protected Rigidbody m_rigidbody;
        [SerializeField] protected AudioSource m_audioSource;

        [Header("Player Details")]
        [SerializeField] protected int m_playerId;
        [SerializeField, Range(1, 20)] protected float m_movementSpeed = 10.0f;
        [SerializeField, Range(1, 5)] protected float m_rotationSpeed = 2.5f;
        [SerializeField] private float m_maxRotationAngle;
        [SerializeField] protected bool m_defaultFrontLineUp;

        [Header("Forward-Movement")]
        //PushDistance for 'Mathf.MoveTowards'.
        [SerializeField] protected float m_lerpDuration = 1.5f;
        [SerializeField] protected float m_delayRetreat;
        [SerializeField] protected float m_delayRepetition;
        [SerializeField] protected bool m_enablePushDelay = false;
        [SerializeField] protected bool m_blockPushInput;

        #region Scriptable Objects
        [SerializeField] protected PlayerIDData m_playerIDData;
        [SerializeField] protected MatchUIStates m_matchUIStates;
        [SerializeField] protected MatchValues m_matchValues;
        [SerializeField] protected BasicFieldValues m_basicFieldValues;
        #endregion
        #endregion

        #region Non-SerializeField-Member-Variables
        #region Properties-Access
        public int PlayerId { get { return m_playerId; } }
        #endregion
        protected float m_maxSideMovement, m_groundWidth, m_groundLength;
        protected readonly float m_baseRotationSpeed = 100;
        protected float m_goalDistance;
        private float m_paddleWidthAdjustment;

        protected float m_maxPushDistance;
        protected bool m_tempBlocked = false;

        protected Vector3 m_localPaddleScale;                               //Saved PaddleScale for all.
        protected Vector3 m_rbPosition, m_readValueVector;
        protected Quaternion m_deltaRotation;

        protected MatchManager m_matchManager;
        #endregion

        //MatchManager pauses the Game. Coroutines and the Inputsystem.PlayerActions get disabled inside this class.
        public static event Action InGameMenuOpens;

        protected abstract void Awake();

        protected virtual void OnEnable()
        {
            //Gets GroundWidth, Ground-Length and goalDistance to set and clamp the playerPositions right after.
            GetFieldDetails();
            GetPlayerDetails();

            m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, -m_groundLength * 0.5f - -m_goalDistance);

            ClampMoveRange();
        }

        protected virtual void OnDisable()
        {
            m_playerMovement.PlayerActions.Disable();
            m_playerMovement.PlayerActions.ToggleGameMenu.performed -= ToggleMenu;
        }

        /// <summary>
        /// PlayerController and UIControls need to be moved into 'Start()' and the PlayerInputActions of the InputManager into 'Awake()', to prevent Exceptions.
        /// </summary>
        protected virtual void Start()
        {
            m_playerMovement = InputManager.m_playerInputActions;
            m_playerMovement.PlayerActions.Enable();
            m_playerMovement.PlayerActions.ToggleGameMenu.performed += ToggleMenu;
        }

        protected virtual void Update()
        {
            ClampMoveRange();
            ClampRotationAngle();

            //TODO: MUST be removed after testing is completed!!!_______________________________
            //TODO: MatchManager shall pass PaddleWidthAdjustment-Changes after hitting objects to each individual player.
            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                if (m_matchValues != null)
                {
                    m_matchManager = FindObjectOfType<MatchManager>();
                    m_matchValues.PaddleWidthAdjustment += m_matchManager.PaddleWidthAdjustStep;
                }
            }

            if (Keyboard.current.pKey.wasReleasedThisFrame)
            {
                if (m_matchValues != null)
                {
                    m_matchManager = FindObjectOfType<MatchManager>();
                    m_matchValues.PaddleWidthAdjustment -= m_matchManager.PaddleWidthAdjustStep;
                }
            }
            //__________________________________________________________________________________
        }

        protected abstract void FixedUpdate();

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

        /// <summary>
        /// Clamps the paddlemMovement on it's 'localPosition.x' and the calculated movementRange based on paddleWidth and fieldWidth.
        /// Also clamps the desired minimal and maximal moveDistance on the zAxis based on m_goalDistance and m_maxPushDistance to the playerGoals.
        /// </summary>
        public void ClampMoveRange()
        {
            m_maxSideMovement = m_groundWidth * 0.5f - m_rigidbody.transform.localScale.x * 0.5f;

            if (m_matchValues == null)
                m_paddleWidthAdjustment = 0;
            else
                m_paddleWidthAdjustment = m_matchValues.PaddleWidthAdjustment;

            m_rigidbody.transform.localScale = new Vector3(m_localPaddleScale.x + m_paddleWidthAdjustment, m_localPaddleScale.y, m_localPaddleScale.z);

            m_rigidbody.transform.localPosition = new Vector3(Mathf.Clamp(m_rigidbody.transform.localPosition.x, -m_maxSideMovement, m_maxSideMovement),
                m_rigidbody.transform.localPosition.y,
                Mathf.Clamp(m_rigidbody.transform.localPosition.z, -m_groundLength * 0.5f - -m_goalDistance, -m_groundLength * 0.5f - -(m_goalDistance + m_maxPushDistance)));
        }

        /// <summary>
        /// Clamps the maximal rotationAngle based on 'Quaternion.LookRotation, rigidbody's forwardVector and Vector3.up'.
        /// </summary>
        private void ClampRotationAngle()
        {
            Quaternion rotation = Quaternion.LookRotation(m_rigidbody.transform.forward, Vector3.up);
            rotation.ToAngleAxis(out float angle, out Vector3 axis);
            angle = Mathf.Clamp(angle, -m_maxRotationAngle, m_maxRotationAngle);
            m_rigidbody/*.transform*/.rotation = Quaternion.AngleAxis(angle, axis);
        }

        protected virtual void StartCoroutinesAndActions()
        {
            m_playerMovement.PlayerActions.Enable();
        }

        #region CallbackContext Methods
        protected void ToggleMenu(InputAction.CallbackContext _callbackContext)
        {
            m_matchManager = FindObjectOfType<MatchManager>();  //Required, if not catched with '[SerializeField]'.

            if (!m_matchManager.GameIsPaused && m_playerMovement.PlayerActions.enabled)
            {
                InGameMenuOpens?.Invoke();
                InputManager.ToggleActionMaps(InputManager.m_playerInputActions.UI);
            }
        }
        #endregion

        protected virtual void DisablePlayerActions()
        {
            m_playerMovement.PlayerActions.Disable();
        }
    }
}