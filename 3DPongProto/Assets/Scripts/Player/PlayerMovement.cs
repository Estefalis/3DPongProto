using UnityEngine;

namespace ThreeDeePongProto.Player.Movement
{
    public class PlayerMovement : MonoBehaviour
    {
        private PlayerInputActions m_playerMovement;

        [SerializeField] private Rigidbody m_rigidbody;
        //[SerializeField] private readonly string m_horizontAxis = "Horizontal";
        [SerializeField] private float m_movementSpeed;
        [SerializeField] private float m_maxMoveRange;

        private Vector3 m_movement;

        private void OnEnable()
        {
            m_playerMovement = new PlayerInputActions();
            m_playerMovement.PlayerActions.Enable();
            //m_playerMovement.PlayerActions.SideMovement.performed += SideMovement;
            //m_playerMovement.PlayerActions.SideMovement.canceled += StopSideMovement;
        }

        private void OnDisable()
        {
            m_playerMovement.PlayerActions.Disable();
            //m_playerMovement.PlayerActions.SideMovement.performed -= SideMovement;
            //m_playerMovement.PlayerActions.SideMovement.canceled -= StopSideMovement;
        }

        private void Awake()
        {
            if (m_rigidbody is null)
                m_rigidbody = GetComponentInChildren<Rigidbody>();
        }

        private void Update()
        {
            //m_movement = new Vector3(Input.GetAxis(m_horizontAxis), 0, 0);

            m_rigidbody.transform.position = new Vector3(Mathf.Clamp(m_rigidbody.transform.position.x, -m_maxMoveRange, m_maxMoveRange),
                    m_rigidbody.transform.position.y, m_rigidbody.transform.position.z);
        }

        private void FixedUpdate()
        {
            //MovePlayer(m_movement);
            //m_rigidbody.MovePosition(m_rigidbody.transform.position + (m_movement * m_movementSpeed * Time.fixedDeltaTime));
            m_rigidbody.MovePosition(m_rigidbody.transform.position + (new Vector3(m_playerMovement.PlayerActions.SideMovement.ReadValue<Vector2>().x, 0, m_playerMovement.PlayerActions.SideMovement.ReadValue<Vector2>().y) * m_movementSpeed * Time.fixedDeltaTime));
        }

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
    }
}