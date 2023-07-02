using System.Collections;
using ThreeDeePongProto.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeDeePongProto.Player.Inputs
{
    public class PlayerMovement : MonoBehaviour
    {
        private PlayerInputActions m_playerMovement;
        //private LevelBuildManager m_levelBuildManager;
        public uint PlayerId { get { return m_playerId; } }
        [SerializeField] private uint m_playerId;

        [SerializeField] private Rigidbody m_rigidbody;

        [Header("Side-Movement")]
        [SerializeField] private float m_movementSpeed;
        [SerializeField] private float m_rotationSpeed, m_maxRotationAngle;
        private float m_maxSideMovement;
        private Vector3 m_axisRotPUneven, m_axisRotPEven, m_localRbPosition, m_readValueVector;
        private Quaternion m_deltaRotation;

        [Header("Paddle-Scale")]
        [SerializeField] private float m_startWidthAdjustment;
        [SerializeField] private Vector3 m_localPaddleScale;

        [Header("Z-Axis-Movement")]
        //PushDistance for 'Mathf.MoveTowards'.
        [SerializeField] private float m_maxPushDistance;
        [SerializeField] private float m_minGoalDistance;
        [SerializeField] private float m_delayRetreat;
        [SerializeField] private float m_delayRepetition;
        [SerializeField] private bool m_enablePushRestriction = false;

        private bool m_pushPlayerOne = false;
        private bool m_pushPlayerTwo = false;
        private bool m_pushesRestricted = false;
        private IEnumerator m_paddleOneCoroutine, m_paddleTwoCoroutine;

        private void Awake()
        {
            //Adjustment of the paddleWidth for ingame power ups and debuffs.
            GameManager.Instance.SetPaddleAdjustAmount(m_startWidthAdjustment);

            if (m_rigidbody == null)
            {
                m_rigidbody = GetComponentInChildren<Rigidbody>();
                ClampMoveRange();
            }

            m_paddleOneCoroutine = PushPaddleOne(m_maxPushDistance);
            m_paddleTwoCoroutine = PushPaddleTwo(m_maxPushDistance);
        }

        /// <summary>
        /// PlayerMovement and UIControls need to be moved into 'Start()' and the PlayerInputActions of the UserInputManager into 'Awake()', to prevent Exceptions.
        /// </summary>
        private void Start()
        {
            m_playerMovement = UserInputManager.m_playerInputActions;
            m_playerMovement.PlayerActions.Enable();
            m_playerMovement.PlayerActions.ToggleGameMenu.performed += ToggleMenu;

            if (m_paddleOneCoroutine != null)
                StartCoroutine(m_paddleOneCoroutine);
            if (m_paddleTwoCoroutine != null)
                StartCoroutine(m_paddleTwoCoroutine);
        }

        private void OnDisable()
        {
            m_playerMovement.PlayerActions.Disable();
            m_playerMovement.PlayerActions.ToggleGameMenu.performed -= ToggleMenu;

            StopAllCoroutines();
        }

        private void Update()
        {
            m_axisRotPUneven = new Vector3(0, m_playerMovement.PlayerActions.TurnMoveModuUneven.ReadValue<Vector2>().x, 0);    //Modulo Uneven = Player1/Player3
            m_axisRotPEven = new Vector3(0, m_playerMovement.PlayerActions.TurnMoveModuEven.ReadValue<Vector2>().x, 0);      //Modulo Even   = Player2/Player4

            //TODO: MUST be removed after testing is completed!!!___
            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                GameManager.Instance.SetPaddleAdjustAmount(0.5f);
            }

            if (Keyboard.current.pKey.wasReleasedThisFrame)
            {
                GameManager.Instance.SetPaddleAdjustAmount(-0.5f);
            }
            //______________________________________________________

            ClampRotationAngle();
            ClampMoveRange();

            if (Keyboard.current.wKey.wasPressedThisFrame && !m_pushesRestricted)
            {
                m_pushPlayerOne = true;
            }

            if (Keyboard.current.upArrowKey.wasPressedThisFrame && !m_pushesRestricted)
            {
                m_pushPlayerTwo = true;
            }
        }

        private void FixedUpdate()
        {
            MovePlayer(m_playerId);
            TurnPlayer(m_playerId);
        }

        /// <summary>
        /// Clamps the paddlemMovement on it's 'localPosition.x' and the calculated movementRange based on paddleWidth and fieldWidth.
        /// Also clamps the desired minimal and maximal moveDistance on the zAxis based on m_minGoalDistance and m_maxPushDistance to the playerGoals.
        /// </summary>
        public void ClampMoveRange()
        {
            m_maxSideMovement = GameManager.Instance.MaxFieldWidth * 0.5f - m_rigidbody.transform.localScale.x * 0.5f;

            m_rigidbody.transform.localScale = new Vector3(m_localPaddleScale.x + GameManager.Instance.PaddleWidthAdjustment, m_localPaddleScale.y, m_localPaddleScale.z);

            m_rigidbody.transform.localPosition = new Vector3(Mathf.Clamp(m_rigidbody.transform.localPosition.x, -m_maxSideMovement, m_maxSideMovement),
                m_rigidbody.transform.localPosition.y,
                Mathf.Clamp(m_rigidbody.transform.localPosition.z, -GameManager.Instance.MaxFieldLength * 0.5f - -m_minGoalDistance, -GameManager.Instance.MaxFieldLength * 0.5f - -(m_minGoalDistance + m_maxPushDistance)));
        }

        /// <summary>
        /// Clamps the maximal rotationAngle based on 'Quaternion.LookRotation, rigidbody's forwardVector and Vector3.up'.
        /// </summary>
        private void ClampRotationAngle()
        {
            Quaternion rotation = Quaternion.LookRotation(m_rigidbody.transform.forward, Vector3.up);
            rotation.ToAngleAxis(out float angle, out Vector3 axis);
            angle = Mathf.Clamp(angle, -m_maxRotationAngle, m_maxRotationAngle);
            m_rigidbody.transform.rotation = Quaternion.AngleAxis(angle, axis);
        }

        /// <summary>
        /// MovePlayer requires a switch to handle the positive and negative paddlePositions and moveDirections based on a playerID.
        /// </summary>
        /// <param name="playerID"></param>
        private void MovePlayer(uint playerID)
        {
            //Modulo is used to direct into the required codeline. false for Player1 and true for Player2.
            switch (playerID % 2 == 0)
            {
                //Modulo Uneven = Player1/Player3
                case true:
                {
                    m_localRbPosition = m_rigidbody.transform.localPosition;
                    m_readValueVector = m_movementSpeed * Time.fixedDeltaTime * new Vector3(m_playerMovement.PlayerActions.SideMoveModuUneven.ReadValue<Vector2>().x, 0, m_playerMovement.PlayerActions.SideMoveModuUneven.ReadValue<Vector2>().y);
                    break;
                }
                //Modulo Even   = Player2/Player4
                case false:
                {
                    m_localRbPosition = -m_rigidbody.transform.localPosition;
                    m_readValueVector = m_movementSpeed * Time.fixedDeltaTime * -new Vector3(m_playerMovement.PlayerActions.SideMoveModuEven.ReadValue<Vector2>().x, 0, m_playerMovement.PlayerActions.SideMoveModuEven.ReadValue<Vector2>().y);
                    break;
                }
            }

            m_rigidbody.MovePosition(m_localRbPosition + m_readValueVector);
        }

        /// <summary>
        /// Turns the playerPaddle based on Quaternion and 'rigidbody.rotation'.
        /// </summary>
        private void TurnPlayer(uint playerID)
        {
            switch (playerID % 2 == 0)
            {
                //Modulo Uneven = Player1/Player3
                case true:
                {
                    m_deltaRotation = Quaternion.Euler(m_axisRotPUneven * m_rotationSpeed * Time.fixedDeltaTime);
                    break;
                }
                //Modulo Even   = Player2/Player4
                case false:
                {
                    m_deltaRotation = Quaternion.Euler(m_axisRotPEven * m_rotationSpeed * Time.fixedDeltaTime);
                    //m_rigidbody.MoveRotation(m_rigidbody.rotation * deltaRotation);
                    break;
                }
            }

            m_rigidbody.MoveRotation(m_rigidbody.rotation * m_deltaRotation);
        }

        #region IEnumerators
        /// <summary>
        /// MoveForthAndBack handles forward- and backwardPushes of the playerPaddle.
        /// '_moveDistance' also works as maximal Time in the whileLoop, but could be replaced with a fix floatAmount.
        /// <param name="_moveDistance"></param>
        /// <returns></returns>
        private IEnumerator PushPaddleOne(float _moveDistance)
        {
            float currentTime = 0;
            float startZPos = m_rigidbody.transform.localPosition.z;
            float endZPos = m_rigidbody.transform.localPosition.z - -_moveDistance;

            while (currentTime < _moveDistance)
            {
                if (m_pushPlayerOne)
                {
                    currentTime += Time.deltaTime;

                    //TODO: Limit rigidbodyMovement by PlayerID.
                    #region Mathf.MoveTowards
                    if (m_playerId == 0)
                    {
                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, endZPos = Mathf.MoveTowards(startZPos, endZPos, _moveDistance)) + m_rigidbody.transform.forward;
                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, endZPos);

                        yield return new WaitForSeconds(m_delayRetreat);
                        m_pushPlayerOne = false;

                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, startZPos = Mathf.MoveTowards(endZPos, startZPos, _moveDistance)) + -m_rigidbody.transform.forward;
                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, startZPos);

                        #region Nested Coroutine
                        //Coroutine to restrict paddleForwardMovement by a certain amount of time.
                        if (m_enablePushRestriction)
                        {
                            m_pushesRestricted = true;
                            Coroutine pushRestriction = StartCoroutine(RestrictPushZero());
                            yield return pushRestriction;
                            m_pushesRestricted = false;
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

        private IEnumerator PushPaddleTwo(float _moveDistance)
        {
            float currentTime = 0;
            float startZPos = m_rigidbody.transform.localPosition.z;
            float endZPos = m_rigidbody.transform.localPosition.z - -_moveDistance;

            while (currentTime < _moveDistance)
            {
                if (m_pushPlayerTwo)
                {
                    currentTime += Time.deltaTime;

                    //TODO: Limit rigidbodyMovement by PlayerID.
                    #region Mathf.MoveTowards
                    if (m_playerId == 1)
                    {
                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, endZPos = Mathf.MoveTowards(startZPos, endZPos, _moveDistance)) + m_rigidbody.transform.forward;
                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, endZPos);

                        yield return new WaitForSeconds(m_delayRetreat);
                        m_pushPlayerTwo = false;

                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, startZPos = Mathf.MoveTowards(endZPos, startZPos, _moveDistance)) + -m_rigidbody.transform.forward;
                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, startZPos);

                        #region Nested Coroutine
                        //Coroutine to restrict paddleForwardMovement by a certain amount of time.
                        if (m_enablePushRestriction)
                        {
                            m_pushesRestricted = true;
                            Coroutine pushRestriction = StartCoroutine(RestrictPushOne());
                            yield return pushRestriction;
                            m_pushesRestricted = false;
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

        private void ToggleMenu(InputAction.CallbackContext _callbackContext)
        {
            if (!GameManager.Instance.GameIsPaused && UserInputManager.m_playerInputActions.PlayerActions.enabled)
            {
                Time.timeScale = 0f;
                GameManager.Instance.GameIsPaused = true;
                UserInputManager.ToggleActionMaps(UserInputManager.m_playerInputActions.UI);
            }
        }
    }
}