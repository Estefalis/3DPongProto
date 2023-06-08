using ThreeDeePongProto.Managers;
using ThreeDeePongProto.Player.Input;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

//Inspired by OneWheelStudio.
//Default CameraTransform SingleCam - Position: X0, Y3, Z31, Rotation X-3.5f, Y0/180, Z0, Scale X1, Y1, Z1.
//Default CameraTransform TwoHorizontal - Position: X0, Y3, Z31, Rotation X-3.5f, Y0/180, Z0, Scale X1, Y1, Z1.
//Default CameraTransform TwoVertical - Position: X0, Y4, Z36, Rotation X-10f, Y0/180, Z0, Scale X1, Y1, Z1.

namespace ThreeDeePongProto.CameraSetup
{
    public class CameraBehaviour : MonoBehaviour
    {
        private PlayerInputActions m_cameraInputActions;

        [Header("Camera-Positions")]
        [SerializeField] private Transform m_playerRBTransform;
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

        private float m_maxFieldWidth, m_maxSideMovement;
        private Transform m_cameraTransform;
        private Vector3 m_differenceVector;

        private float m_currentHeight;

        private void OnEnable()
        {
            m_cameraTransform = this.GetComponentInChildren<Camera>().transform;
            m_currentHeight = m_cameraTransform.localPosition.y;
            //Position for LookAtTarget, if wanted.
            m_differenceVector = m_cameraTransform.position - m_playerRBTransform.position;
        }

        private void Start()
        {
            m_maxFieldWidth = GameManager.Instance.MaxFieldWidth;
            //TODO: Calculation of the set FieldWidth adjusted by the PlayerPaddle-Width at runtime!!!
            m_maxSideMovement = m_maxFieldWidth;

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

        private void UpdateCameraPosition()
        {
            Vector3 zoomTarget = new Vector3(m_cameraTransform.localPosition.x, m_currentHeight, m_cameraTransform.localPosition.z);

            zoomTarget -= m_zoomStep * (m_currentHeight - m_cameraTransform.localPosition.y) * Vector3.forward;
            m_cameraTransform.localPosition = Vector3.Lerp(m_cameraTransform.localPosition, zoomTarget, Time.fixedDeltaTime * m_zoomDampening);
        }

        private void FollowDirectly()
        {
            //m_cameraTransform.position.z MUST NOT be localPosition, or the Camera2-Position flickers between + and - Z-Values.
            Vector3 desiredPosition = new Vector3(Mathf.Clamp(m_playerRBTransform.position.x, -m_maxSideMovement, m_maxSideMovement), m_cameraTransform.position.y, m_cameraTransform.position.z);
            m_cameraTransform.position = desiredPosition;
        }

        private void FollowWithOffset()
        {
            Vector3 desiredPosition = m_playerRBTransform.position + m_differenceVector;
            Vector3 smoothedFollowing = Vector3.Lerp(m_cameraTransform.position, desiredPosition, m_smoothfactor * Time.deltaTime);
            m_cameraTransform.position = smoothedFollowing;
        }

        private void Zooming(InputAction.CallbackContext _callbackContext)
        {
            //m_zoomDirection = _callbackContext.ReadValue<Vector2>();
            float zoomValue = -_callbackContext.ReadValue<Vector2>().y * m_zoomSpeed;
            ;

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