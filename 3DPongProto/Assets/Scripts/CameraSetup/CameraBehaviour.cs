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
        [SerializeField] private float m_lowestHeight;
        [SerializeField] private float m_maximalHeight;
        private float m_currentHeight, m_cameraZPos;
        private Vector3 m_cameraStartPosition;

        [Header("Smooth Following")]
        [SerializeField] private Rigidbody m_RbPlayer;
        [SerializeField] private bool m_enableOffset;
        [SerializeField] private Vector3 m_desiredOffset;
        [Range(1f, 20.0f)]
        [SerializeField] private float m_smoothfactor;
        private float m_maxSideMovement, m_directFollowVectorX;
        private Camera m_camera;

        [Header("Camera-Zoom")]
        [SerializeField, Min(0.001f)] private float m_zoomSpeed;
        [SerializeField] private float m_zoomStep;
        [SerializeField] private float m_zoomDampening;
        private Vector3 m_mousePosition;

        private uint m_playerId;

        private void Awake()
        {
            m_camera = GetComponentInChildren<Camera>();
            
            if (m_RbPlayer == null)
                m_RbPlayer = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            GetMaxSideMovement();

            //Saved vector to keep the playerCamera-startposition.
            SetCameraStartPosition();

            //CameraActions need to be in Start to prevent NullReferenceExceptions due to relation to the UserInputManager.
            m_cameraInputActions = UserInputManager.m_playerInputActions;
            m_cameraInputActions.Enable();
            m_cameraInputActions.PlayerActions.ZoomModuUneven.performed += Zooming;

            m_playerId = m_playerMovement.GetComponent<PlayerMovement>().PlayerId;
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
#if UNITY_EDITOR
            //if (m_mousePosition.x >= (m_camera.pixelRect.width - m_camera.pixelRect.width) && m_mousePosition.x <= m_camera.pixelRect.width &&
            //    m_mousePosition.y >= (m_camera.pixelRect.height - m_camera.pixelRect.height) && m_mousePosition.y <= m_camera.pixelRect.height)
            //{
            //    Debug.Log(m_mousePosition);
            //}
#endif
            UpdateCameraPosition();
        }

        private void GetMaxSideMovement()
        {
            m_maxSideMovement = GameManager.Instance.MaxFieldWidth * 0.5f - m_RbPlayer.transform.localScale.x * 0.5f;
        }

        private void SetCameraStartPosition()
        {
            m_cameraStartPosition = -(m_camera.transform.localPosition - Vector3.zero);
            m_currentHeight = -m_cameraStartPosition.y;
            m_cameraZPos = m_cameraStartPosition.z;
            m_cameraStartPosition = new Vector3(m_cameraStartPosition.x, m_currentHeight, -m_cameraZPos);
        }

        private void UpdateCameraPosition()
        {
#if ENABLE_INPUT_SYSTEM
            m_mousePosition = m_cameraInputActions.PlayerActions.MousePosition.ReadValue<Vector2>();
#else
            m_mousePosition = Input.mousePosition;
#endif
            Vector3 zoomTarget = new Vector3(m_camera.transform.localPosition.x, m_currentHeight, m_camera.transform.localPosition.z);

            zoomTarget -= m_zoomStep * (m_currentHeight - m_camera.transform.localPosition.y) * Vector3.forward;
            m_camera.transform.localPosition = Vector3.Lerp(m_camera.transform.localPosition, zoomTarget, Time.fixedDeltaTime * m_zoomDampening);
        }

        private void FollowDirectly()
        {
            switch (m_playerId % 2 == 0)
            {
                case true:
                    m_directFollowVectorX = m_RbPlayer.transform.localPosition.x;
                    break;
                case false:
                    m_directFollowVectorX = -m_RbPlayer.transform.localPosition.x;
                    break;
            }

            //m_cameraTransform.position.z MUST NOT be localPosition, or the Camera2-Position flickers between + and - Z-Values.
            Vector3 desiredPosition = new Vector3(Mathf.Clamp(m_directFollowVectorX,
                -m_maxSideMovement + (GameManager.Instance.PaddleWidthAdjustment * 0.5f),
                m_maxSideMovement - (GameManager.Instance.PaddleWidthAdjustment * 0.5f)),
                m_camera.transform.position.y, m_camera.transform.position.z);

            m_camera.transform.position = desiredPosition;
        }

        private void FollowWithOffset()
        {
            Vector3 desiredPosition = m_RbPlayer.transform.localPosition + new Vector3(m_desiredOffset.x, m_desiredOffset.y, -m_desiredOffset.z);
            Vector3 smoothedFollowing = Vector3.Lerp(m_camera.transform.localPosition, desiredPosition, m_smoothfactor * Time.deltaTime);
            m_camera.transform.localPosition = smoothedFollowing;
        }

        private void Zooming(InputAction.CallbackContext _callbackContext)
        {
            //TODO: Zoom-Funktion darauf beschraenken, dass es nur innerhalb des eigenen WindowRects funktioniert.)
            if(m_mousePosition.x >= (m_camera.pixelRect.width - m_camera.pixelRect.width) && m_mousePosition.x <= m_camera.pixelRect.width &&
                m_mousePosition.y >= (m_camera.pixelRect.height - m_camera.pixelRect.height) && m_mousePosition.y <= m_camera.pixelRect.height)
                {
                float zoomValue = -_callbackContext.ReadValue<Vector2>().y * m_zoomSpeed;

                if (Mathf.Abs(zoomValue) > 0.1f)
                {
                    m_currentHeight = m_camera.transform.localPosition.y + zoomValue * m_zoomStep;
                    if (m_currentHeight < m_lowestHeight)
                        m_currentHeight = m_lowestHeight;
                    else if (m_currentHeight > m_maximalHeight)
                        m_currentHeight = m_maximalHeight;
                }
            }
        }
    }
}