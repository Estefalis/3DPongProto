using ThreeDeePongProto.Managers;
using ThreeDeePongProto.Player.Inputs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeDeePongProto.CameraSetup
{
    public class CameraBehaviour : MonoBehaviour
    {
        private PlayerInputActions m_cameraInputActions;
        [SerializeField] private PlayerMovement m_playerMovement;

        [Header("Camera-Positions")]
        //[SerializeField] private Transform m_rbTransform;
        [SerializeField] private float m_lowestHeight;
        [SerializeField] private float m_maximalHeight;

        [Header("Smooth Following")]
        [SerializeField] private Rigidbody m_RbPlayer;
        [SerializeField] private bool m_enableOffset;
        [SerializeField] private Vector3 m_desiredOffset;
        [Range(1f, 20.0f)]
        [SerializeField] private float m_smoothfactor;

        [Header("Camera-Zoom")]
        [SerializeField, Min(0.001f)] private float m_zoomSpeed;
        [SerializeField] private float m_zoomStep;
        [SerializeField] private float m_zoomDampening;

        private float m_maxSideMovement, m_directFollowVectorX;
        private Transform m_cameraTransform;
        private Vector3 m_cameraStartPosition;

        private uint m_playerID;

        private float m_currentHeight, m_cameraZPos;

        private void OnEnable()
        {
            if (m_RbPlayer == null)
                m_RbPlayer = GetComponent<Rigidbody>();

            m_cameraTransform = this.GetComponentInChildren<Camera>().transform;
            //Add Position for LookAtTarget here, if wanted.

            //Saved vector to keep the playercamera-startposition.
            SetCameraStartPosition();
        }

        private void Start()
        {
            GetMaxSideMovement();

            m_cameraInputActions = UserInputManager.m_playerInputActions;
            m_cameraInputActions.Enable();
            m_cameraInputActions.PlayerActions.ZoomModuUneven.performed += Zooming;

            m_playerID = m_playerMovement.GetComponent<PlayerMovement>().PlayerID;
        }

        private void OnDisable()
        {
            m_cameraInputActions?.Disable();
            m_cameraInputActions.PlayerActions.ZoomModuUneven.performed -= Zooming;
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
            if (m_enableOffset)
            {
                FollowWithOffset();
            }

            UpdateCameraPosition();
        }

        private void GetMaxSideMovement()
        {
            m_maxSideMovement = GameManager.Instance.MaxFieldWidth * 0.5f - m_RbPlayer.transform.localScale.x * 0.5f;
        }

        private void SetCameraStartPosition()
        {
            m_cameraStartPosition = -(m_cameraTransform.localPosition - Vector3.zero);
            m_currentHeight = -m_cameraStartPosition.y;
            m_cameraZPos = m_cameraStartPosition.z;
            m_cameraStartPosition = new Vector3(m_cameraStartPosition.x, m_currentHeight, -m_cameraZPos);
        }

        private void UpdateCameraPosition()
        {
            Vector3 zoomTarget = new Vector3(m_cameraTransform.localPosition.x, m_currentHeight, m_cameraTransform.localPosition.z);

            zoomTarget -= m_zoomStep * (m_currentHeight - m_cameraTransform.localPosition.y) * Vector3.forward;
            m_cameraTransform.localPosition = Vector3.Lerp(m_cameraTransform.localPosition, zoomTarget, Time.fixedDeltaTime * m_zoomDampening);
        }

        private void FollowDirectly()
        {
            switch (m_playerID % 2 == 0)
            {
                case true:
                    m_directFollowVectorX = -m_RbPlayer.transform.localPosition.x; break;
                case false:
                    m_directFollowVectorX = m_RbPlayer.transform.localPosition.x;  break;
            }

            //m_cameraTransform.position.z MUST NOT be localPosition, or the Camera2-Position flickers between + and - Z-Values.
            Vector3 desiredPosition = new Vector3(Mathf.Clamp(m_directFollowVectorX,
                -m_maxSideMovement + (GameManager.Instance.PaddleWidthAdjustment * 0.5f),
                m_maxSideMovement - (GameManager.Instance.PaddleWidthAdjustment * 0.5f)),
                m_cameraTransform.position.y, m_cameraTransform.position.z);

            m_cameraTransform.position = desiredPosition;
        }

        private void FollowWithOffset()
        {
            Vector3 desiredPosition = m_RbPlayer.transform.localPosition + new Vector3(m_desiredOffset.x, m_desiredOffset.y, -m_desiredOffset.z);
            Vector3 smoothedFollowing = Vector3.Lerp(m_cameraTransform.localPosition, desiredPosition, m_smoothfactor * Time.deltaTime);
            m_cameraTransform.localPosition = smoothedFollowing;
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