using System.Collections;
using ThreeDeePongProto.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeDeePongProto.Player.Inputs
{
    public class PlayerMovement : MonoBehaviour
    {
        private PlayerInputActions m_playerMovement;

        [SerializeField] private Rigidbody m_rigidbody;

        [Header("Side-Movement")]
        [SerializeField] private float m_movementSpeed;
        [SerializeField] private float m_rotationSpeed, m_maxRotationAngle;
        private float m_maxSideMovement;
        private Vector3 m_axisRotation, m_inputVector, m_readValueVector;

        [Header("Paddle-Scale")]
        [SerializeField] private float m_startWidthAdjustment;
        [SerializeField] private Vector3 m_localPaddleScale;

        [Header("Z-Axis-Movement")]
        [SerializeField] private float m_minGoalDistance;
        [SerializeField] private float m_maxGoalDistance;
        [SerializeField] private float m_lerpDuration;
        [SerializeField] private float m_delayRetreat, m_delayRepetition;
        private bool m_lerpForward = false;
        private IEnumerator m_lerpMovement;

        public uint PlayerID { get { return m_playerID; } }
        [SerializeField] private uint m_playerID;

        private void Awake()
        {
            GameManager.Instance.SetPaddleAdjustAmount(m_startWidthAdjustment);

            if (m_rigidbody == null)
            {
                m_rigidbody = GetComponentInChildren<Rigidbody>();
                ClampMoveRange();
            }

            m_lerpMovement = MoveForthAndBack(m_maxGoalDistance - m_minGoalDistance, m_lerpDuration);
        }

        /// <summary>
        /// PlayerMovement and UIControls need to be moved into 'Start()' and the PlayerInputActions of the UserInputManager into 'Awake()', to prevent Exceptions.
        /// </summary>
        private void Start()
        {
            m_playerMovement = UserInputManager.m_playerInputActions;
            m_playerMovement.PlayerActions.Enable();
            m_playerMovement.PlayerActions.ToggleGameMenu.performed += ToggleMenu;

            if (m_lerpMovement != null)
                StartCoroutine(m_lerpMovement);
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

            ClampMoveRange();
            ClampRotationAngle();

            if (Keyboard.current.wKey.wasPressedThisFrame)
            {
                m_lerpForward = true;
            }
        }

        private void FixedUpdate()
        {
            MovePlayer(m_playerID);
            TurnPlayer();
        }

        public void ClampMoveRange()
        {
            m_rigidbody.transform.localPosition = new Vector3(Mathf.Clamp(m_rigidbody.transform.localPosition.x, -m_maxSideMovement, m_maxSideMovement),
                m_rigidbody.transform.localPosition.y,
                Mathf.Clamp(m_rigidbody.transform.localPosition.z, -GameManager.Instance.MaxFieldLength * 0.5f - -m_minGoalDistance, -GameManager.Instance.MaxFieldLength * 0.5f - -m_maxGoalDistance));

            m_rigidbody.transform.localScale = new Vector3(m_localPaddleScale.x + GameManager.Instance.PaddleWidthAdjustment, m_localPaddleScale.y, m_localPaddleScale.z);

            m_maxSideMovement = GameManager.Instance.MaxFieldWidth * 0.5f - m_rigidbody.transform.localScale.x * 0.5f;
        }

        private void ClampRotationAngle()
        {
            Quaternion rotation = Quaternion.LookRotation(m_rigidbody.transform.forward, Vector3.up);
            rotation.ToAngleAxis(out float angle, out Vector3 axis);
            angle = Mathf.Clamp(angle, -m_maxRotationAngle, m_maxRotationAngle);
            m_rigidbody.transform.rotation = Quaternion.AngleAxis(angle, axis);
        }

        private void MovePlayer(uint playerID)
        {
            //Necessary invertions of playermovement/Directions for positive and negative Z-Positions.
            switch (playerID % 2 == 0)
            {
                case true:
                {
                    m_inputVector = -m_rigidbody.transform.localPosition;
                    m_readValueVector = -new Vector3(m_playerMovement.PlayerActions.SideMovement.ReadValue<Vector2>().x, 0, m_playerMovement.PlayerActions.SideMovement.ReadValue<Vector2>().y) * m_movementSpeed * Time.fixedDeltaTime;
                    break;
                }
                case false:
                {
                    m_inputVector = m_rigidbody.transform.localPosition;
                    m_readValueVector = new Vector3(m_playerMovement.PlayerActions.SideMovement.ReadValue<Vector2>().x, 0, m_playerMovement.PlayerActions.SideMovement.ReadValue<Vector2>().y) * m_movementSpeed * Time.fixedDeltaTime;
                    break;
                }
            }

            m_rigidbody.MovePosition(m_inputVector + m_readValueVector);
        }

        private void TurnPlayer()
        {
            Quaternion deltaRotation = Quaternion.Euler(m_axisRotation * m_rotationSpeed * Time.fixedDeltaTime);
            m_rigidbody.MoveRotation(m_rigidbody.rotation * deltaRotation);
        }

        private void ToggleMenu(InputAction.CallbackContext _callbackContext)
        {
            if (!GameManager.Instance.GameIsPaused && UserInputManager.m_playerInputActions.PlayerActions.enabled)
            {
                PauseTimescale();
                UserInputManager.ToggleActionMaps(UserInputManager.m_playerInputActions.UI);
            }
        }

        #region Lerps
        private IEnumerator MoveForthAndBack(float _moveDistance, float _lerpDuration)
        {
            float currentTime = 0;
            float startZPos = m_rigidbody.transform.localPosition.z;
            float endZPos = m_rigidbody.transform.localPosition.z - -_moveDistance;
            //repetitive pressing forward-button would decrease the lerp-distance w/o 'currentZPos' on 'Mathf.Lerp();'.
            //float currentZPos;

            while (currentTime < _lerpDuration)
            {
                if (m_lerpForward)
                {
                    currentTime += Time.deltaTime;

                    #region Mathf.MoveTowards
                    m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, endZPos = Mathf.MoveTowards(startZPos, endZPos, _lerpDuration)) + m_rigidbody.transform.forward;
                    m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, endZPos);

                    m_lerpForward = false;
                    yield return new WaitForSeconds(m_delayRetreat);

                    m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, startZPos = Mathf.MoveTowards(endZPos, startZPos, _lerpDuration)) + -m_rigidbody.transform.forward;
                    m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, startZPos);
                    #endregion

                    #region Mathf.Lerp
                    //currentZPos = startZPos;
                    //m_rigidbody.transform.position = new Vector3(m_rigidbody.transform.position.x, m_rigidbody.transform.position.y, currentZPos = Mathf.Lerp(startZPos, endZPos, currentTime / _lerpDuration)) + m_rigidbody.transform.forward;
                    //m_rigidbody.transform.position = new Vector3(m_rigidbody.transform.position.x, m_rigidbody.transform.position.y, endZPos);

                    //m_lerpForward = false;
                    //yield return new WaitForSeconds(m_delayRetreat);

                    //currentZPos = endZPos;
                    //m_rigidbody.transform.position = new Vector3(m_rigidbody.transform.position.x, m_rigidbody.transform.position.y, currentZPos = Mathf.Lerp(endZPos, startZPos, currentTime / _lerpDuration)) + -m_rigidbody.transform.forward;
                    //m_rigidbody.transform.position = new Vector3(m_rigidbody.transform.position.x, m_rigidbody.transform.position.y, startZPos);
                    #endregion
                }
                else
                {
                    yield return null;
                }
            }
        }
        #endregion

        private void PauseTimescale()
        {
            Time.timeScale = 0f;
            GameManager.Instance.GameIsPaused = true;
        }
    }
}