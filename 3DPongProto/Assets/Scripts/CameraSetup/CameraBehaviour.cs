using ThreeDeePongProto.Managers;
using ThreeDeePongProto.Player.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeDeePongProto.CameraSetup
{
    public class CameraBehaviour : MonoBehaviour
    {
        private PlayerInputActions m_cameraInputActions;

        [Header("Camera-Positions")]
        [SerializeField] private Rigidbody m_playerRb;
        [SerializeField] private float m_lowestHeight;
        [SerializeField] private float m_maximalHeight;

        [Header("Smooth Following")]
        [SerializeField] private bool m_enableOffset;
        [Range(1f, 20.0f)]
        [SerializeField] private float m_smoothfactor;

        [Header("Camera-Zoom")]
        [SerializeField, Min(0.001f)] private float m_zoomSpeed;
        [SerializeField] private float m_zoomStep;
        [SerializeField] private float m_zoomDampening;

        private float m_maxSideMovement;
        private Transform m_cameraTransform;
        private Vector3 m_differenceVector;

        private float m_currentHeight;

        private void OnEnable()
        {
            if (m_playerRb == null)
                m_playerRb = GetComponent<Rigidbody>();

            m_cameraTransform = this.GetComponentInChildren<Camera>().transform;
            m_currentHeight = m_cameraTransform.localPosition.y;
            //Add Position for LookAtTarget here, if wanted.
            //Saved vector to keep the playercamera-startposition.
            m_differenceVector = m_cameraTransform.position - m_playerRb.transform.position;
        }

        private void Start()
        {
            GetMaxSideMovement();

            m_cameraInputActions = UserInputManager.m_playerInputActions;
            m_cameraInputActions.Enable();
            m_cameraInputActions.PlayerActions.Zoom.performed += Zooming;
        }

        private void OnDisable()
        {
            m_cameraInputActions?.Disable();
            m_cameraInputActions.PlayerActions.Zoom.performed -= Zooming;
        }

        private void Update()
        {
            if (!m_enableOffset)
            {
                FollowDirectly();
            }
        }

        private void FixedUpdate()
        {
            UpdateCameraPosition();

            if (m_enableOffset)
            {
                FollowWithOffset();
            }
        }

        private void GetMaxSideMovement()
        {
            m_maxSideMovement = GameManager.Instance.MaxFieldWidth * 0.5f - GameManager.Instance.WidthAdjustment * 0.5f;
        }

        private void UpdateCameraPosition()
        {
            Vector3 zoomTarget = new Vector3(m_cameraTransform.localPosition.x, m_currentHeight, m_cameraTransform.localPosition.z);

            zoomTarget -= m_zoomStep * (m_currentHeight - m_cameraTransform.localPosition.y) * Vector3.forward;
            m_cameraTransform.localPosition = Vector3.Lerp(m_cameraTransform.localPosition, zoomTarget, Time.fixedDeltaTime * m_zoomDampening);
        }

        private void FollowDirectly()
        {
            //m_cameraTransform.position.z MUST NOT be localPosition, or the Camera2-Position flickers between + and - Z-Values.
            Vector3 desiredPosition = new Vector3(Mathf.Clamp(m_playerRb.position.x, 
                -m_maxSideMovement + (GameManager.Instance.WidthAdjustment * 0.5f), 
                m_maxSideMovement - (GameManager.Instance.WidthAdjustment * 0.5f)), 
                m_cameraTransform.position.y, m_cameraTransform.position.z);

            m_cameraTransform.position = desiredPosition;
        }

        private void FollowWithOffset()
        {
            Vector3 desiredPosition = m_playerRb.position + m_differenceVector;
            Vector3 smoothedFollowing = Vector3.Lerp(m_cameraTransform.position, desiredPosition, m_smoothfactor * Time.deltaTime);
            m_cameraTransform.position = smoothedFollowing;
        }

        private void Zooming(InputAction.CallbackContext _callbackContext)
        {
            //m_zoomDirection = _callbackContext.ReadValue<Vector2>();
            float zoomValue = -_callbackContext.ReadValue<Vector2>().y * m_zoomSpeed;

            if (Mathf.Abs(zoomValue) > 0.1f)
            {
                m_currentHeight = m_cameraTransform.localPosition.y + zoomValue * m_zoomStep;
                if (m_currentHeight < m_lowestHeight)
                    m_currentHeight = m_lowestHeight;
                else if (m_currentHeight > m_maximalHeight)
                    m_currentHeight = m_maximalHeight;
            }
        }
    }
}