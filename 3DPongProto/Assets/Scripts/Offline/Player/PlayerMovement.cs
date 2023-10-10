using System;
using System.Collections;
using ThreeDeePongProto.Managers;
using ThreeDeePongProto.Offline.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeDeePongProto.Offline.Player.Inputs
{
    public class PlayerMovement : MonoBehaviour
    {
        #region Script-References
        private PlayerInputActions m_playerMovement;
        [SerializeField] private MatchManager m_matchManager;
        #endregion

        #region SerializeField-Member-Variables
        [SerializeField] private Rigidbody m_rigidbody;
        [Header("Player Details")]
        [SerializeField] private int m_playerId;
        [SerializeField] private float m_movementSpeed;
        [SerializeField, Range(1, 5)] private float m_rotationSpeed;
        [SerializeField] private float m_maxRotationAngle;
        //[SerializeField] private Vector3 m_localPaddleScale;
        [SerializeField] private bool m_defaultFrontLineUp;

        [Header("Forward-Movement")]
        //PushDistance for 'Mathf.MoveTowards'.
        [SerializeField] private float m_moveDuration;
        [SerializeField] private float m_delayRetreat;
        [SerializeField] private float m_delayRepetition;
        [SerializeField] private bool m_enablePushDelay = false;
        [SerializeField] private bool m_blockPushInput;
        [Space]

        #region Scriptable Objects
        [SerializeField] private PlayerIDData m_playerIDData;
        [SerializeField] private MatchValues m_matchValues;
        [SerializeField] private BasicFieldValues m_basicFieldValues;
        [SerializeField] private MatchConnection m_matchConnection;
        #endregion
        #endregion

        #region Non-SerializeField-Member-Variables
        #region Properties-Access
        public int PlayerId { get { return m_playerId; } }
        //public int PlayerId { get { return m_playerIDData.PlayerId; } }
        #endregion

        private float m_maxSideMovement, m_groundWidth, m_groundLength;
        private float m_frontLineDistance, m_backLineDistance;
        private readonly float m_baseRotationSpeed = 100;
        private float m_goalDistance;
        private float m_paddleWidthAdjustment;

        private float m_maxPushDistance;
        private bool m_pushPlayerOne = false;
        private bool m_pushPlayerTwo = false;
        private bool m_tempBlocked = false;

        private Vector3 m_localPaddleScale;
        private Vector3 m_rbPosition, m_readValueVector;
        private Vector3 m_axisRotUneven, m_axisRotEven;
        private Quaternion m_deltaRotation;

        //GameManager pauses the Game. Coroutines and the Inputsystem.PlayerActions get disabled inside this class.
        public static event Action InGameMenuOpens;
        private IEnumerator m_paddleOneCoroutine, m_paddleTwoCoroutine;
        #endregion

        private void Awake()
        {
            m_playerIDData.PlayerId = m_playerId;

            if (m_rigidbody == null)
            {
                m_rigidbody = GetComponentInChildren<Rigidbody>();
            }

            m_paddleOneCoroutine = PushPaddleOne(m_moveDuration);
            m_paddleTwoCoroutine = PushPaddleTwo(m_moveDuration);
        }

        private void OnEnable()
        {
            //Gets GroundWidth, Ground-Length and goalDistance to set and clamp the playerPositions right after.
            GetFieldDetails();
            GetPlayerDetails();

            m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, -m_groundLength * 0.5f - -m_goalDistance);

            ClampMoveRange();
        }

        /// <summary>
        /// PlayerMovement and UIControls need to be moved into 'Start()' and the PlayerInputActions of the UserInputManager into 'Awake()', to prevent Exceptions.
        /// </summary>
        private void Start()
        {
            m_playerMovement = UserInputManager.m_playerInputActions;
            m_playerMovement.PlayerActions.Enable();
            m_playerMovement.PlayerActions.ToggleGameMenu.performed += ToggleMenu;
            m_playerMovement.PlayerActions.PushMoveModuUneven.performed += PushInputFirstPlayer;
            m_playerMovement.PlayerActions.PushMoveModuUneven.canceled += CanceledInputFirstPlayer;
            m_playerMovement.PlayerActions.PushMoveModuEven.performed += PushInputSecondPlayer;
            m_playerMovement.PlayerActions.PushMoveModuEven.canceled += CanceledInputSecondPlayer;

            InGameMenuOpens += DisablePlayerActions;

            MenuOrganisation.CloseInGameMenu += StartCoroutinesAndActions;
            StartCoroutinesAndActions();
        }

        private void OnDisable()
        {
            m_playerMovement.PlayerActions.Disable();
            m_playerMovement.PlayerActions.ToggleGameMenu.performed -= ToggleMenu;
            m_playerMovement.PlayerActions.PushMoveModuUneven.performed -= PushInputFirstPlayer;
            m_playerMovement.PlayerActions.PushMoveModuUneven.canceled -= CanceledInputFirstPlayer;
            m_playerMovement.PlayerActions.PushMoveModuEven.performed -= PushInputSecondPlayer;
            m_playerMovement.PlayerActions.PushMoveModuEven.canceled -= CanceledInputSecondPlayer;

            InGameMenuOpens -= DisablePlayerActions;
            MenuOrganisation.CloseInGameMenu -= StartCoroutinesAndActions;
            StopAllCoroutines();
        }

        private void Update()
        {
            ClampMoveRange();
            ClampRotationAngle();

            m_axisRotUneven = new Vector3(0, m_playerMovement.PlayerActions.TurnMovementUneven.ReadValue<Vector2>().x, 0);  //Modulo = Player1/Player3
            m_axisRotEven = new Vector3(0, m_playerMovement.PlayerActions.TurnMovementEven.ReadValue<Vector2>().x, 0);      //Modulo = Player2/Player4

            //TODO: MUST be removed after testing is completed!!!_______________________________
            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                if (m_matchValues != null)
                    m_matchValues.PaddleWidthAdjustment += m_matchManager.PaddleWidthAdjustStep;
            }

            if (Keyboard.current.pKey.wasReleasedThisFrame)
            {
                if (m_matchValues != null)
                    m_matchValues.PaddleWidthAdjustment -= m_matchManager.PaddleWidthAdjustStep;
            }
            //__________________________________________________________________________________
        }

        private void FixedUpdate()
        {
            MovePlayer(m_playerId);
            TurnPlayer(m_playerId);
        }

        private void GetFieldDetails()
        {
            if (m_matchValues == null)
            {
                m_groundWidth = m_matchManager.DefaultFieldWidth;
                m_groundLength = m_matchManager.DefaultFieldLength;
                m_maxPushDistance = m_matchManager.MaxPushDistance;               
            }
            else
            {
                m_groundWidth = m_basicFieldValues.SetGroundWidth;
                m_groundLength = m_basicFieldValues.SetGroundLength;
                m_maxPushDistance = m_matchValues.MaxPushDistance;
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

        /// <summary>
        /// MovePlayer requires a switch to handle the positive and negative paddlePositions and moveDirections based on a playerID.
        /// </summary>
        /// <param name="playerID"></param>
        private void MovePlayer(int playerID)
        {
            //Modulo is used to direct into the required codeline. false for Player1 and true for Player2.
            switch (playerID % 2 == 0)
            {
                //Modulo Uneven = Player1/Player3
                case true:
                {
                    m_rbPosition = m_rigidbody.transform.localPosition;
                    m_readValueVector = m_movementSpeed * Time.fixedDeltaTime * new Vector3(m_playerMovement.PlayerActions.SideMoveModuUneven.ReadValue<Vector2>().x, 0, m_playerMovement.PlayerActions.SideMoveModuUneven.ReadValue<Vector2>().y);
                    break;
                }
                //Modulo Even   = Player2/Player4
                case false:
                {
                    m_rbPosition = -m_rigidbody.transform.localPosition;
                    m_readValueVector = m_movementSpeed * Time.fixedDeltaTime * -new Vector3(m_playerMovement.PlayerActions.SideMoveModuEven.ReadValue<Vector2>().x, 0, m_playerMovement.PlayerActions.SideMoveModuEven.ReadValue<Vector2>().y);
                    break;
                }
            }

            m_rigidbody.MovePosition(m_rbPosition + m_readValueVector);
        }

        /// <summary>
        /// Turns the playerPaddle based on Quaternion and 'rigidbody.rotation'.
        /// </summary>
        private void TurnPlayer(int playerID)
        {
            switch (playerID % 2 == 0)
            {
                //Modulo Uneven = Player1/Player3
                case true:
                {
                    m_deltaRotation = Quaternion.Euler(m_axisRotUneven * m_rotationSpeed * m_baseRotationSpeed * Time.fixedDeltaTime);
                    break;
                }
                //Modulo Even   = Player2/Player4
                case false:
                {
                    m_deltaRotation = Quaternion.Euler(m_axisRotEven * m_rotationSpeed * m_baseRotationSpeed * Time.fixedDeltaTime);
                    break;
                }
            }

            m_rigidbody.MoveRotation(m_rigidbody.rotation * m_deltaRotation);
        }

        #region IEnumerators - Coroutines
        private void StartCoroutinesAndActions()
        {
            if (m_paddleOneCoroutine != null)
                StartCoroutine(m_paddleOneCoroutine);
            if (m_paddleTwoCoroutine != null)
                StartCoroutine(m_paddleTwoCoroutine);

            m_playerMovement.PlayerActions.Enable();
        }

        /// <summary>
        /// MoveForthAndBack handles forward- and backwardPushes of the playerPaddle.
        /// '_moveDuration' also works as maximal Time in the whileLoop, but could be replaced with a fix floatAmount.
        /// <param name="_moveDuration"></param>
        /// <returns></returns>
        private IEnumerator PushPaddleOne(float _moveDuration)
        {
            float currentTime = 0;

            while (currentTime < _moveDuration)
            {
                if (m_pushPlayerOne)
                {
                    currentTime += Time.deltaTime;

                    #region zFloat Mathf.MoveTowards
                    if (m_playerId == 0 || m_playerId == 2)
                    {
                        float startZPos = m_rigidbody.transform.localPosition.z;
                        float endZPos = m_rigidbody.transform.localPosition.z - -m_maxPushDistance;

                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, endZPos = Mathf.MoveTowards(startZPos, endZPos, _moveDuration)) + m_rigidbody.transform.forward;
                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, endZPos);

                        yield return new WaitForSeconds(m_delayRetreat);
                        m_pushPlayerOne = false;

                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, startZPos = Mathf.MoveTowards(endZPos, startZPos, _moveDuration)) + -m_rigidbody.transform.forward;
                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, startZPos);

                        #region Nested Coroutine
                        //Coroutine to restrict paddleForwardMovement by a certain amount of time.
                        if (m_enablePushDelay)
                        {
                            m_tempBlocked = true;
                            Coroutine pushRestriction = StartCoroutine(RestrictPushZero());
                            yield return pushRestriction;
                            m_tempBlocked = false;
                        }
                        #endregion
                    }
                    #endregion
                }
                else
                {
                    yield return null;
                }
            }
        }

        private IEnumerator PushPaddleTwo(float _moveDuration)
        {
            float currentTime = 0;

            while (currentTime < _moveDuration)
            {
                if (m_pushPlayerTwo)
                {
                    currentTime += Time.deltaTime;

                    #region zFloat Mathf.MoveTowards
                    if (m_playerId == 1 || m_playerId == 3)
                    {
                        float startZPos = m_rigidbody.transform.localPosition.z;
                        float endZPos = m_rigidbody.transform.localPosition.z - -m_maxPushDistance;

                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, endZPos = Mathf.MoveTowards(startZPos, endZPos, _moveDuration)) + m_rigidbody.transform.forward;
                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, endZPos);

                        yield return new WaitForSeconds(m_delayRetreat);
                        m_pushPlayerTwo = false;

                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, startZPos = Mathf.MoveTowards(endZPos, startZPos, _moveDuration)) + -m_rigidbody.transform.forward;
                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, startZPos);

                        #region Nested Coroutine
                        //Coroutine to restrict paddleForwardMovement by a certain amount of time.
                        if (m_enablePushDelay)
                        {
                            m_tempBlocked = true;
                            Coroutine pushRestriction = StartCoroutine(RestrictPushOne());
                            yield return pushRestriction;
                            m_tempBlocked = false;
                        }
                        #endregion
                    }
                    #endregion
                }
                else
                {
                    yield return null;
                }
            }
        }

        private IEnumerator RestrictPushZero()
        {
            float countdown = m_delayRepetition;

            while (countdown > 0)
            {
                countdown -= Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator RestrictPushOne()
        {
            float countdown = m_delayRepetition;

            while (countdown > 0)
            {
                countdown -= Time.deltaTime;
                yield return null;
            }
        }
        #endregion

        #region PlayerOne CallbackContexts
        private void PushInputFirstPlayer(InputAction.CallbackContext _callbackContext)
        {
            if (!m_blockPushInput)
            {
                if (!m_tempBlocked)
                {
                    //'ReadValueAsButton()' is only available inside these CallbackContext-Methods.
                    m_pushPlayerOne = _callbackContext.ReadValueAsButton();
                }
            }
        }

        private void CanceledInputFirstPlayer(InputAction.CallbackContext _callbackContext)
        {
            m_pushPlayerOne = false;
        }
        #endregion

        #region PlayerTwo CallbackContexts
        private void PushInputSecondPlayer(InputAction.CallbackContext _callbackContext)
        {
            if (!m_blockPushInput)
            {
                if (!m_tempBlocked)
                {
                    //'ReadValueAsButton()' is only available inside these CallbackContext-Methods.
                    m_pushPlayerTwo = _callbackContext.ReadValueAsButton();
                }
            }
        }

        private void CanceledInputSecondPlayer(InputAction.CallbackContext _callbackContext)
        {
            m_pushPlayerTwo = false;
        }
        #endregion

        private void ToggleMenu(InputAction.CallbackContext _callbackContext)
        {
            //if (!GameManager.Instance.GameIsPaused && m_playerMovement.PlayerActions.enabled)
            if (!m_matchManager.GameIsPaused && m_playerMovement.PlayerActions.enabled)
            {
                InGameMenuOpens?.Invoke();
                UserInputManager.ToggleActionMaps(UserInputManager.m_playerInputActions.UI);
            }
        }

        private void DisablePlayerActions()
        {
            StopAllCoroutines();
            m_playerMovement.PlayerActions.Disable();
        }
    }
}