using System;
using ThreeDeePongProto.Offline.Player.Inputs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeDeePongProto.Offline.CameraSetup
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
        [SerializeField] private Camera m_followCamera;
        [SerializeField] private bool m_enableSmoothFollow;
        [SerializeField] private Vector3 m_desiredOffset;
        [Range(1.0f, 30.0f)]
        [SerializeField] private float m_smoothfactor;
        private float m_maxSideMovement, m_setGroundWidth;

        [Header("Camera-Zoom")]
        [SerializeField, Min(0.001f)] private float m_zoomSpeed;
        [SerializeField] private float m_zoomStep;
        [SerializeField] private float m_zoomDampening;
        private Vector3 m_mousePosition, m_zoomTarget;

        #region Scriptable Variables
        [SerializeField] private MatchUiStates m_uiStates;
        [SerializeField] private MatchValues m_matchVariables;
        #endregion

        private int m_playerId;
        private int m_playerWindowId;

        private void Awake()
        {
            if (m_RbPlayer == null)
                m_RbPlayer = GetComponentInParent<Rigidbody>();

            m_playerId = m_playerMovement/*.GetComponent<PlayerMovement>()*/.PlayerId;

            m_setGroundWidth = m_matchVariables.SetGroundWidth;
            //TODO: Move Setting of 'm_enableSmoothFollow' boolean into UI-Settings.
            m_enableSmoothFollow = true;
        }

        private void Start()
        {
            m_cameraManager = FindObjectOfType<CameraManager>();

            MaxSideMovement();

            //CameraActions need to be in Start to prevent NullReferenceExceptions due to relation to the UserInputManager.
            m_cameraInputActions = UserInputManager.m_playerInputActions;
            m_cameraInputActions.Enable();
            m_cameraInputActions.PlayerActions.Zoom.performed += Zooming;

            //Saved vector to keep the playerCamera-startposition.
            CameraPositions(m_cameraManager.AvailableCameras[(int)m_playerId]);
        }

        private void OnDisable()
        {
            m_cameraInputActions?.Disable();
            m_cameraInputActions.PlayerActions.Zoom.performed -= Zooming;
        }

        private void Update()
        {
            GetMousePosition();

            SelectCameraToZoom();
        }

        private void FixedUpdate()
        {
            if (!m_enableSmoothFollow)
            {
                FollowUnsmoothed();
            }

            if (m_enableSmoothFollow)
            {
                FollowSmoothly();
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
            //Sets camera only within the gameWindow. (If the mouse is not less width or height == 0 and not more than full width or height.)
            if (!(m_mousePosition.x < (m_cameraManager.RuntimeFullsizeRect.width - m_cameraManager.RuntimeFullsizeRect.width)) &&
                !(m_mousePosition.x > m_cameraManager.RuntimeFullsizeRect.width) &&
                !(m_mousePosition.y < (m_cameraManager.RuntimeFullsizeRect.height - m_cameraManager.RuntimeFullsizeRect.height)) &&
                !(m_mousePosition.y > m_cameraManager.RuntimeFullsizeRect.height))
            {
                //switch ((uint)GameManager.Instance.ECameraMode)
                switch ((int)m_uiStates.SetCameraMode)
                {
                    //SingleCamera
                    case 0:
                    {
                        m_playerWindowId = m_cameraManager.AvailableCameras.IndexOf(m_cameraManager.AvailableCameras[0]);
                        break;
                    }
                    //TwoHorizontal
                    case 1:
                    {
                        if (m_mousePosition.y >= m_cameraManager.AvailableCameras[1].pixelRect.yMin)
                        {
                            m_playerWindowId = m_cameraManager.AvailableCameras.IndexOf(m_cameraManager.AvailableCameras[1]);
                        }
                        else
                        {
                            m_playerWindowId = m_cameraManager.AvailableCameras.IndexOf(m_cameraManager.AvailableCameras[0]);
                        }
                        break;
                    }
                    //TwoVertical
                    case 2:
                    {
                        if (m_mousePosition.x >= m_cameraManager.AvailableCameras[1].pixelRect.xMin)
                        {
                            m_playerWindowId = m_cameraManager.AvailableCameras.IndexOf(m_cameraManager.AvailableCameras[1]);
                        }
                        else
                        {
                            m_playerWindowId = m_cameraManager.AvailableCameras.IndexOf(m_cameraManager.AvailableCameras[0]);
                        }
                        break;
                    }
                    //FourSplit
                    case 3:
                    {
                        if (m_mousePosition.x >= m_cameraManager.AvailableCameras[1].pixelRect.xMin && m_mousePosition.y < m_cameraManager.AvailableCameras[3].pixelRect.yMin)
                        {
                            m_playerWindowId = m_cameraManager.AvailableCameras.IndexOf(m_cameraManager.AvailableCameras[1]);
                        }
                        else if (m_mousePosition.x < m_cameraManager.AvailableCameras[1].pixelRect.xMin && m_mousePosition.y >= m_cameraManager.AvailableCameras[2].pixelRect.yMin)
                        {
                            m_playerWindowId = m_cameraManager.AvailableCameras.IndexOf(m_cameraManager.AvailableCameras[2]);
                        }
                        else if (m_mousePosition.x >= m_cameraManager.AvailableCameras[1].pixelRect.xMin && m_mousePosition.y >= m_cameraManager.AvailableCameras[3].pixelRect.yMin)
                        {
                            m_playerWindowId = m_cameraManager.AvailableCameras.IndexOf(m_cameraManager.AvailableCameras[3]);
                        }
                        else
                        {
                            m_playerWindowId = m_cameraManager.AvailableCameras.IndexOf(m_cameraManager.AvailableCameras[0]);
                        }
                        break;
                    }
                    default:
                    {
                        m_playerWindowId = m_cameraManager.AvailableCameras.IndexOf(m_cameraManager.AvailableCameras[0]);
                        break;
                    }

                }
#if UNITY_EDITOR
                //Debug.Log("WindowId: " + m_playerWindowId);
#endif
            }
        }

        private void MaxSideMovement()
        {
            m_maxSideMovement = m_setGroundWidth * 0.5f - m_RbPlayer.transform.localScale.x * 0.5f;
        }

        private void GetMousePosition()
        {
#if ENABLE_INPUT_SYSTEM
            m_mousePosition = m_cameraInputActions.PlayerActions.MousePosition.ReadValue<Vector2>();
#else
            m_mousePosition = Input.mousePosition;
#endif
        }

        private void FollowUnsmoothed()
        {
            //Follows directly in xPosition, but the visible push looks buggy, if not lerped.
            Vector3 desiredPosition = new Vector3(m_RbPlayer.transform.localPosition.x/* + m_desiredOffset.x*/, m_RbPlayer.transform.localPosition.y + m_desiredOffset.y, m_RbPlayer.transform.localPosition.z + -m_desiredOffset.z);
            Vector3 smoothedFollowing = Vector3.Lerp(m_followCamera.transform.localPosition, desiredPosition, Mathf.Max(m_smoothfactor, 30.0f) * Time.fixedDeltaTime);
            //m_followCamera.transform.localPosition = desiredPosition;
            m_followCamera.transform.localPosition = smoothedFollowing;
        }

        private void FollowSmoothly()
        {
            Vector3 desiredPosition = m_RbPlayer.transform.localPosition + new Vector3(m_desiredOffset.x, m_desiredOffset.y, -m_desiredOffset.z);
            Vector3 smoothedFollowing = Vector3.Lerp(m_followCamera.transform.localPosition, desiredPosition, m_smoothfactor * Time.fixedDeltaTime);
            m_followCamera.transform.localPosition = smoothedFollowing;
        }

        private void UpdateZoomPosition()
        {
            //The 'zoomTarget' and 'm_currentHeight' shall only change, while the PlayerMovement in the 'PlayerActions' is enabled.
            if (m_cameraInputActions.PlayerActions.enabled)
            {
                m_zoomTarget = new Vector3(m_followCamera.transform.localPosition.x, m_currentHeight, m_followCamera.transform.localPosition.z);

                m_zoomTarget -= m_zoomStep * (m_currentHeight - m_followCamera.transform.localPosition.y) * Vector3.forward;
                m_followCamera.transform.localPosition = Vector3.Lerp(m_followCamera.transform.localPosition, m_zoomTarget, Time.fixedDeltaTime * m_zoomDampening);
            }
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
                    //Limits the zoom to the window of each player.
                    if (m_playerWindowId == m_playerId)
                        m_currentHeight = m_followCamera.transform.localPosition.y + zoomValue * m_zoomStep;

                    if (m_currentHeight < m_lowestHeight)
                        m_currentHeight = m_lowestHeight;
                    else if (m_currentHeight > m_maximalHeight)
                        m_currentHeight = m_maximalHeight;
                }
            }
        }
    }
}