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
        [SerializeField] private Vector3 m_localPaddleScale;

        private float m_maxSideMovement;
        private Vector3 m_movement;

        private void Awake()
        {
            GameManager.Instance.SetPaddleAdjustAmount(m_startWidthAdjustment);

            if (m_rigidbody == null)
            {
                m_rigidbody = GetComponentInChildren<Rigidbody>();
                ClampPaddleMoverange();
            }
        }

        /// <summary>
        /// PlayerMovement and UIControls need to be moved into 'Start()' and the PlayerInputActions of the UserInputManager into 'Awake()', to prevent Exceptions.
        /// </summary>
        private void Start()
        {
            //GetMaxSideMovement();

            //m_playerMovement = new PlayerInputActions();
            m_playerMovement = UserInputManager.m_playerInputActions;
            m_playerMovement.PlayerActions.Enable();
            //m_playerMovement.PlayerActions.SideMovement.performed += SideMovement;
            //m_playerMovement.PlayerActions.SideMovement.canceled += StopSideMovement;
            m_playerMovement.PlayerActions.ToggleGameMenu.performed += ToggleMenu;
        }

        private void OnDisable()
        {
            m_playerMovement.PlayerActions.Disable();
            //m_playerMovement.PlayerActions.SideMovement.performed -= SideMovement;
            //m_playerMovement.PlayerActions.SideMovement.canceled -= StopSideMovement;
            m_playerMovement.PlayerActions.ToggleGameMenu.performed -= ToggleMenu;
        }

        private void Update()
        {
            //m_movement = new Vector3(Input.GetAxis(m_horizontAxis), 0, 0);

            m_rigidbody.transform.position = new Vector3(Mathf.Clamp(m_rigidbody.transform.position.x, -m_maxSideMovement, m_maxSideMovement),
                m_rigidbody.transform.position.y, m_rigidbody.transform.position.z);

            //TODO: MUST be removed after testing is completed!!!___
            if (Keyboard.current.wKey.wasPressedThisFrame)
            {
                GameManager.Instance.SetPaddleAdjustAmount(0.5f);
                ClampPaddleMoverange();
            }

            if (Keyboard.current.wKey.wasReleasedThisFrame)
            {
                GameManager.Instance.SetPaddleAdjustAmount(-0.5f);
                ClampPaddleMoverange();
            }
            //______________________________________________________
        }

        private void FixedUpdate()
        {
            //MovePlayer(m_movement);
            //m_rigidbody.MovePosition(m_rigidbody.transform.position + (m_movement * m_movementSpeed * Time.fixedDeltaTime));
            m_rigidbody.MovePosition(m_rigidbody.transform.position + (new Vector3(m_playerMovement.PlayerActions.SideMovement.ReadValue<Vector2>().x, 0, m_playerMovement.PlayerActions.SideMovement.ReadValue<Vector2>().y) * m_movementSpeed * Time.fixedDeltaTime));
        }

        public void ClampPaddleMoverange()
        {

            m_rigidbody.transform.localScale = new Vector3(m_localPaddleScale.x + GameManager.Instance.WidthAdjustment, m_localPaddleScale.y, m_localPaddleScale.z);

            m_maxSideMovement = GameManager.Instance.MaxFieldWidth * 0.5f - m_rigidbody.transform.localScale.x * 0.5f;
        }

        //private void GetMaxSideMovement()
        //{
        //}

        //private void MovePlayer(Vector3 _moveDirection)
        //{
        //    m_rigidbody.MovePosition(m_rigidbody.transform.position + (_moveDirection * m_movementSpeed * Time.fixedDeltaTime));
        //}

        //private void SideMovement(InputAction.CallbackContext _callbackContext)
        //{
        //    m_movement = _callbackContext.ReadValue<Vector2>();
        //}

        //private void StopSideMovement(InputAction.CallbackContext _callbackContext)
        //{
        //    m_movement = Vector2.zero;
        //}

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