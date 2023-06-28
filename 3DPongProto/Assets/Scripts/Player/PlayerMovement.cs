using System.Collections;
using ThreeDeePongProto.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeDeePongProto.Player.Inputs
{
    public class PlayerMovement : MonoBehaviour
    {
        private PlayerInputActions m_playerMovement;
        public uint PlayerID { get { return m_playerID; } }
        [SerializeField] private uint m_playerID;

        [SerializeField] private Rigidbody m_rigidbody;

        [Header("Side-Movement")]
        [SerializeField] private float m_movementSpeed;
        [SerializeField] private float m_rotationSpeed, m_maxRotationAngle;
        private float m_maxSideMovement;
        private Vector3 m_axisRotation, m_localRbPosition, m_readValueVector;

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

        private bool m_pushForward = false;
        private bool m_pushesRestricted = false;
        private IEnumerator m_pushMovement;

        private void Awake()
        {
            //Adjustment of the paddleWidth for ingame power ups and debuffs.
            GameManager.Instance.SetPaddleAdjustAmount(m_startWidthAdjustment);

            if (m_rigidbody == null)
            {
                m_rigidbody = GetComponentInChildren<Rigidbody>();
                ClampMoveRange();
            }

            m_pushMovement = MoveForthAndBack(m_maxPushDistance);
        }

        /// <summary>
        /// PlayerMovement and UIControls need to be moved into 'Start()' and the PlayerInputActions of the UserInputManager into 'Awake()', to prevent Exceptions.
        /// </summary>
        private void Start()
        {
            m_playerMovement = UserInputManager.m_playerInputActions;
            m_playerMovement.PlayerActions.Enable();
            m_playerMovement.PlayerActions.ToggleGameMenu.performed += ToggleMenu;

            if (m_pushMovement != null)
                StartCoroutine(m_pushMovement);
        }

        private void OnDisable()
        {
            m_playerMovement.PlayerActions.Disable();
            m_playerMovement.PlayerActions.ToggleGameMenu.performed -= ToggleMenu;

            StopAllCoroutines();
        }

        private void Update()
        {
            m_axisRotation = new Vector3(0, m_playerMovement.PlayerActions.TurnMovement.ReadValue<Vector2>().x, 0);

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
                m_pushForward = true;
            }
        }

        private void FixedUpdate()
        {
            MovePlayer(m_playerID);
            TurnPlayer();
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
                case true:
                {
                    m_localRbPosition = -m_rigidbody.transform.localPosition;
                    m_readValueVector = -new Vector3(m_playerMovement.PlayerActions.SideMovement.ReadValue<Vector2>().x, 0, m_playerMovement.PlayerActions.SideMovement.ReadValue<Vector2>().y) * m_movementSpeed * Time.fixedDeltaTime;
                    break;
                }
                case false:
                {
                    m_localRbPosition = m_rigidbody.transform.localPosition;
                    m_readValueVector = new Vector3(m_playerMovement.PlayerActions.SideMovement.ReadValue<Vector2>().x, 0, m_playerMovement.PlayerActions.SideMovement.ReadValue<Vector2>().y) * m_movementSpeed * Time.fixedDeltaTime;
                    break;
                }
            }

            m_rigidbody.MovePosition(m_localRbPosition + m_readValueVector);
        }

        /// <summary>
        /// Turns the playerPaddle based on Quaternion and 'rigidbody.rotation'.
        /// </summary>
        private void TurnPlayer()
        {
            Quaternion deltaRotation = Quaternion.Euler(m_axisRotation * m_rotationSpeed * Time.fixedDeltaTime);
            m_rigidbody.MoveRotation(m_rigidbody.rotation * deltaRotation);
        }

        #region IEnumerators
        /// <summary>
        /// MoveForthAndBack handles forward- and backwardPushes of the playerPaddle.
        /// '_moveDistance' also works as maximal Time in the whileLoop, but could be replaced with a fix floatAmount.
        /// <param name="_moveDistance"></param>
        /// <returns></returns>
        private IEnumerator MoveForthAndBack(float _moveDistance)
        {
            float currentTime = 0;
            float startZPos = m_rigidbody.transform.localPosition.z;
            float endZPos = m_rigidbody.transform.localPosition.z - -_moveDistance;

            while (currentTime < _moveDistance)
            {
                if (m_pushForward)
                {
                    currentTime += Time.deltaTime;

                    #region Mathf.MoveTowards
                    m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, endZPos = Mathf.MoveTowards(startZPos, endZPos, _moveDistance)) + m_rigidbody.transform.forward;
                    m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, endZPos);

                    yield return new WaitForSeconds(m_delayRetreat);
                    m_pushForward = false;

                    m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, startZPos = Mathf.MoveTowards(endZPos, startZPos, _moveDistance)) + -m_rigidbody.transform.forward;
                    m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, startZPos);

                    #region Nested Coroutine
                    //Coroutine to restrict paddleForwardMovement by a certain amount of time.
                    if (m_enablePushRestriction)
                    {
                        m_pushesRestricted = true;
                        Coroutine pushRestriction = StartCoroutine(RestrictPushes());
                        yield return pushRestriction;
                        m_pushesRestricted = false;
                    }
                    #endregion
                    #endregion
                }
                else
                {
                    yield return null;
                }
            }
        }

        private IEnumerator RestrictPushes()
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