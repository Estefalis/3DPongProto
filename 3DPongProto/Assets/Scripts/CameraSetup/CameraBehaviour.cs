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
        private CameraManager m_cameraManager;

        [Header("Camera-Positions")]
        [SerializeField] private float m_lowestHeight;
        [SerializeField] private float m_maximalHeight;
        private float m_currentHeight, m_cameraZPos;
        private Vector3 m_cameraPosition;

        [Header("Smooth Following")]
        [SerializeField] private Rigidbody m_RbPlayer;
        [SerializeField] private Camera m_ownPlayerCamera;
        [SerializeField] private bool m_enableOffset;
        [SerializeField] private Vector3 m_desiredOffset;
        [Range(1f, 20.0f)]
        [SerializeField] private float m_smoothfactor;
        private float m_maxSideMovement, m_directFollowVectorX;

        [Header("Camera-Zoom")]
        [SerializeField, Min(0.001f)] private float m_zoomSpeed;
        [SerializeField] private float m_zoomStep;
        [SerializeField] private float m_zoomDampening;
        private Vector3 m_mousePosition, m_zoomTarget;
        private Camera m_mouseSelectedCamera;

        private uint m_playerId;

        private void Awake()
        {
            if (m_RbPlayer == null)
                m_RbPlayer = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            m_cameraManager = FindObjectOfType<CameraManager>();

            GetMaxSideMovement();

            //CameraActions need to be in Start to prevent NullReferenceExceptions due to relation to the UserInputManager.
            m_cameraInputActions = UserInputManager.m_playerInputActions;
            m_cameraInputActions.Enable();
            m_cameraInputActions.PlayerActions.ZoomModuUneven.performed += Zooming;

            m_playerId = m_playerMovement.GetComponent<PlayerMovement>().PlayerId;

            //Saved vector to keep the playerCamera-startposition.
            CameraPositions(m_cameraManager.AvailableCameras[(int)m_playerId]);
        }

        private void OnDisable()
        {
            m_cameraInputActions?.Disable();
            m_cameraInputActions.PlayerActions.ZoomModuUneven.performed -= Zooming;
        }

        private void Update()
        {
            GetMousePosition();

            if (!m_enableOffset)
            {
                FollowDirectly();
            }

            SelectCameraToZoom();
        }

        private void FixedUpdate()
        {
            if (m_enableOffset)
            {
                FollowWithOffset();
            }

            UpdateZoomPosition();
        }

        private void CameraPositions(Camera _camera)
        {
            m_cameraPosition = -(_camera.transform.localPosition - Vector3.zero);
            m_currentHeight = -m_cameraPosition.y;
            m_cameraZPos = m_cameraPosition.z;
            m_cameraPosition = new Vector3(m_cameraPosition.x, m_currentHeight, -m_cameraZPos);
        }

        private void SelectCameraToZoom()
        {
            //Set camera only within the gameWindow. (If the mouse is not less width or height == 0 and not more than full width or height.)
            if (!(m_mousePosition.x < (m_cameraManager.RuntimeFullsizeRect.width - m_cameraManager.RuntimeFullsizeRect.width)) &&
                !(m_mousePosition.x > m_cameraManager.RuntimeFullsizeRect.width) &&
                !(m_mousePosition.y < (m_cameraManager.RuntimeFullsizeRect.height - m_cameraManager.RuntimeFullsizeRect.height)) &&
                !(m_mousePosition.y > m_cameraManager.RuntimeFullsizeRect.height))
            {
                switch ((uint)GameManager.Instance.ECameraMode)
                {
                    //SingleCamera
                    case 0:
                    {
                        m_mouseSelectedCamera = m_cameraManager.AvailableCameras[0];
                        break;
                    }
                    //TwoHorizontal
                    case 1:
                    {
                        if (m_mousePosition.y >= m_cameraManager.AvailableCameras[1].pixelRect.yMin)
                        {
                            m_mouseSelectedCamera = m_cameraManager.AvailableCameras[1];
                        }
                        else
                        {
                            m_mouseSelectedCamera = m_cameraManager.AvailableCameras[0];
                        }
                        break;
                    }
                    //TwoVertical
                    case 2:
                    {
                        if (m_mousePosition.x >= m_cameraManager.AvailableCameras[1].pixelRect.xMin)
                        {
                            m_mouseSelectedCamera = m_cameraManager.AvailableCameras[1];
                        }
                        else
                        {
                            m_mouseSelectedCamera = m_cameraManager.AvailableCameras[0];
                        }
                        break;
                    }
                    //FourSplit
                    case 3:
                    {
                        if (m_mousePosition.x >= m_cameraManager.AvailableCameras[1].pixelRect.xMin && m_mousePosition.y < m_cameraManager.AvailableCameras[3].pixelRect.yMin)
                        {
                            m_mouseSelectedCamera = m_cameraManager.AvailableCameras[1];
                        }
                        else if (m_mousePosition.x < m_cameraManager.AvailableCameras[1].pixelRect.xMin && m_mousePosition.y >= m_cameraManager.AvailableCameras[2].pixelRect.yMin)
                        {
                            m_mouseSelectedCamera = m_cameraManager.AvailableCameras[2];
                        }
                        else if (m_mousePosition.x >= m_cameraManager.AvailableCameras[1].pixelRect.xMin && m_mousePosition.y >= m_cameraManager.AvailableCameras[3].pixelRect.yMin)
                        {
                            m_mouseSelectedCamera = m_cameraManager.AvailableCameras[3];
                        }
                        else
                        {
                            m_mouseSelectedCamera = m_cameraManager.AvailableCameras[0];
                        }
                        break;
                    }
                    default:
                    {
                        m_mouseSelectedCamera = m_cameraManager.AvailableCameras[0];
                        break;
                    }

                }
#if UNITY_EDITOR
                //Debug.Log(m_mouseSelectedCamera);
#endif
            }
        }

        private void GetMaxSideMovement()
        {
            m_maxSideMovement = GameManager.Instance.MaxFieldWidth * 0.5f - m_RbPlayer.transform.localScale.x * 0.5f;
        }

        private void GetMousePosition()
        {
#if ENABLE_INPUT_SYSTEM
            m_mousePosition = m_cameraInputActions.PlayerActions.MousePosition.ReadValue<Vector2>();
#else
            m_mousePosition = Input.mousePosition;
#endif
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
                m_ownPlayerCamera.transform.position.y, m_ownPlayerCamera.transform.position.z);

            m_ownPlayerCamera.transform.position = desiredPosition;
        }

        private void FollowWithOffset()
        {
            Vector3 desiredPosition = m_RbPlayer.transform.localPosition + new Vector3(m_desiredOffset.x, m_desiredOffset.y, -m_desiredOffset.z);
            Vector3 smoothedFollowing = Vector3.Lerp(m_ownPlayerCamera.transform.localPosition, desiredPosition, m_smoothfactor * Time.deltaTime);
            m_ownPlayerCamera.transform.localPosition = smoothedFollowing;
        }

        private void UpdateZoomPosition()
        {
            m_zoomTarget = new Vector3(m_ownPlayerCamera.transform.localPosition.x, m_currentHeight, m_ownPlayerCamera.transform.localPosition.z);

            m_zoomTarget -= m_zoomStep * (m_currentHeight - m_ownPlayerCamera.transform.localPosition.y) * Vector3.forward;
            m_ownPlayerCamera.transform.localPosition = Vector3.Lerp(m_ownPlayerCamera.transform.localPosition, m_zoomTarget, Time.fixedDeltaTime * m_zoomDampening);
        }

        private void Zooming(InputAction.CallbackContext _callbackContext)
        {
            //Zooming limited to the inside of the gameWindow.
            if (!(m_mousePosition.x < (m_cameraManager.RuntimeFullsizeRect.width - m_cameraManager.RuntimeFullsizeRect.width)) &&
                !(m_mousePosition.x > m_cameraManager.RuntimeFullsizeRect.width) &&
                !(m_mousePosition.y < (m_cameraManager.RuntimeFullsizeRect.height - m_cameraManager.RuntimeFullsizeRect.height)) &&
                !(m_mousePosition.y > m_cameraManager.RuntimeFullsizeRect.height))
            {
                float zoomValue = -_callbackContext.ReadValue<Vector2>().y * m_zoomSpeed;

                if (Mathf.Abs(zoomValue) > 0.1f)
                {
                    switch (m_mouseSelectedCamera.name)
                    {
                        case "PlayerCameraA":
                        {
                            m_ownPlayerCamera = m_cameraManager.AvailableCameras[0];
                            break;
                        }
                        case "PlayerCameraB":
                        {
                            m_ownPlayerCamera = m_cameraManager.AvailableCameras[1];
                            break;
                        }
                        case "CameraC":
                        {
                            m_ownPlayerCamera = m_cameraManager.AvailableCameras[2];
                            break;
                        }
                        case "CameraD":
                        {
                            m_ownPlayerCamera = m_cameraManager.AvailableCameras[3];
                            break;
                        }
                    }

                    m_currentHeight = m_ownPlayerCamera.transform.localPosition.y + zoomValue * m_zoomStep;
                    if (m_currentHeight < m_lowestHeight)
                        m_currentHeight = m_lowestHeight;
                    else if (m_currentHeight > m_maximalHeight)
                        m_currentHeight = m_maximalHeight;
                }
            }
        }
    }
}