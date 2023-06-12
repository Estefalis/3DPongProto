using UnityEngine;
using UnityEngine.InputSystem;
using ThreeDeePongProto.Managers;

namespace ThreeDeePongProto.Player.Input
{
    public class PlayerMovement : MonoBehaviour
    {
        private PlayerInputActions m_playerMovement;

        [SerializeField] private Rigidbody m_rigidbody;
        [SerializeField] private float m_movementSpeed, m_startWidthAdjustment;
        [SerializeField] private float m_rotationSpeed, m_maxRotationAngle;
        [SerializeField] private Vector3 m_localPaddleScale;

        private float m_maxSideMovement;
        private Vector3 m_axisRotation;

        private void Awake()
        {
            GameManager.Instance.SetPaddleAdjustAmount(m_startWidthAdjustment);

            if (m_rigidbody == null)
            {
                m_rigidbody = GetComponentInChildren<Rigidbody>();
                ClampMoveRange();
            }
        }

        /// <summary>
        /// PlayerMovement and UIControls need to be moved into 'Start()' and the PlayerInputActions of the UserInputManager into 'Awake()', to prevent Exceptions.
        /// </summary>
        private void Start()
        {
            m_playerMovement = UserInputManager.m_playerInputActions;
            m_playerMovement.PlayerActions.Enable();
            m_playerMovement.PlayerActions.ToggleGameMenu.performed += ToggleMenu;
        }

        private void OnDisable()
        {
            m_playerMovement.PlayerActions.Disable();
            m_playerMovement.PlayerActions.ToggleGameMenu.performed -= ToggleMenu;
        }

        private void Update()
        {
            m_axisRotation = new Vector3(0, m_playerMovement.PlayerActions.TurnMovement.ReadValue<Vector2>().x, 0);

            //TODO: MUST be removed after testing is completed!!!___
            if (Keyboard.current.wKey.wasPressedThisFrame)
            {
                GameManager.Instance.SetPaddleAdjustAmount(0.5f);
            }

            if (Keyboard.current.wKey.wasReleasedThisFrame)
            {
                GameManager.Instance.SetPaddleAdjustAmount(-0.5f);
            }
            //______________________________________________________

            ClampMoveRange();
            ClampRotationAngle();
        }

        private void FixedUpdate()
        {
            MovePlayer();
            TurnPlayer();
        }

        public void ClampMoveRange()
        {
            m_rigidbody.transform.position = new Vector3(Mathf.Clamp(m_rigidbody.transform.position.x, -m_maxSideMovement, m_maxSideMovement),
                m_rigidbody.transform.position.y, m_rigidbody.transform.position.z);

            m_rigidbody.transform.localScale = new Vector3(m_localPaddleScale.x + GameManager.Instance.WidthAdjustment, m_localPaddleScale.y, m_localPaddleScale.z);

            m_maxSideMovement = GameManager.Instance.MaxFieldWidth * 0.5f - m_rigidbody.transform.localScale.x * 0.5f;
        }

        private void ClampRotationAngle()
        {
            Quaternion rotation = Quaternion.LookRotation(m_rigidbody.transform.forward, Vector3.up);
            rotation.ToAngleAxis(out float angle, out Vector3 axis);
            angle = Mathf.Clamp(angle, -m_maxRotationAngle, m_maxRotationAngle);
            m_rigidbody.transform.rotation = Quaternion.AngleAxis(angle, axis);
        }

        private void MovePlayer()
        {
            m_rigidbody.MovePosition(m_rigidbody.transform.position + (new Vector3(m_playerMovement.PlayerActions.SideMovement.ReadValue<Vector2>().x, 0, m_playerMovement.PlayerActions.SideMovement.ReadValue<Vector2>().y) * m_movementSpeed * Time.fixedDeltaTime));
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

        private void PauseTimescale()
        {
            Time.timeScale = 0f;
            GameManager.Instance.GameIsPaused = true;
        }
    }
}