using ThreeDeePongProto.Offline.CameraSetup;
using UnityEngine;

namespace ThreeDeePongProto.Shared.PlayerCharacter
{
    internal class CharacterCameraController : MonoBehaviour
    {
        [SerializeField] internal CharacterMainController m_playerController;

        [Header("Camera-Positions")]
        [SerializeField] private float m_lowestHeight;
        [SerializeField] private float m_maximalHeight;
        private float m_currentHeight, m_cameraZPos;
        private Vector3 m_cameraPosition;

        [Header("Smooth Following")]
        [SerializeField] private Rigidbody m_RbPlayer;
        [SerializeField] internal Camera m_followCamera;
        [SerializeField] private bool m_enableSmoothFollow = true;
        [SerializeField] private Vector3 m_desiredOffset;
        [Range(1.0f, 30.0f)]
        [SerializeField] private float m_smoothfactor;
        //'m_maxSideMovement' set in method 'MaxSideMovement()'.
        private float m_maxSideMovement, m_setGroundWidth;

        [Header("Camera-Zoom")]
        [SerializeField, Min(0.01f)] private float m_minAbsLimit = 0.05f;
        [SerializeField, Min(0.001f)] private float m_zoomSpeed;
        [SerializeField] private float m_zoomStep;
        [SerializeField] private float m_zoomDampening;
        private Vector3 m_zoomTarget;
        internal Vector2 m_mousePosition;

        private CameraManager m_cameraManager;

        #region Scriptable Variables
        [SerializeField] private GraphicUIStates m_graphicUiStates;
        [SerializeField] private BasicFieldValues m_basicFieldValues;
        #endregion

        internal int m_playerId;
        private int m_playerWindowId;

        private void Awake()
        {
            m_cameraManager = FindObjectOfType<CameraManager>();

            if (m_RbPlayer == null)
                m_RbPlayer = GetComponentInParent<Rigidbody>();

            m_playerId = m_playerController.m_playerId;
            m_setGroundWidth = m_basicFieldValues.SetGroundWidth;
        }

        private void OnDisable()
        {
            CameraManager.LetsRemoveCamera(m_followCamera, m_playerId);

            CharacterInputHandler.ASendMousePosition -= NewMousePosition;
        }

        private void Start()
        {
            //3. After Players are instantiated and the LocalMatchManager invokes the Event, the Camera can add itself to the availableCamera-List in the CameraManager.
            CameraManager.LetsRegisterCamera(m_followCamera, m_playerId);

            MaxSideMovement();
            //Saved vector to keep the playerCamera-startposition.
            CameraPositions(m_cameraManager.AvailableCameras[m_playerId]);

            CharacterInputHandler.ASendMousePosition += NewMousePosition;
        }

        #region Custom-Methods
        private void Update()
        {
            SelectCameraToZoom(m_mousePosition);
        }

        private void FixedUpdate()
        {
            if (!m_enableSmoothFollow)
                FollowUnsmoothed();

            if (m_enableSmoothFollow)
                FollowSmoothly();

            UpdateZoomPosition();
        }

        private void MaxSideMovement()
        {
            m_maxSideMovement = m_setGroundWidth * 0.5f - m_RbPlayer.transform.localScale.x * 0.5f;
        }

        private void CameraPositions(Camera _camera)
        {
            m_cameraPosition = -(_camera.transform.localPosition - Vector3.zero);
            m_currentHeight = -m_cameraPosition.y;
            m_cameraZPos = m_cameraPosition.z;
            m_cameraPosition = new Vector3(m_cameraPosition.x, m_currentHeight, -m_cameraZPos);
        }

        private void SelectCameraToZoom(Vector2 _mousePosition)
        {
            //Sets the camera only, if the mouse is within the gameWindow. width/height > 0 and not more than max width/height.)
            //xMin (Rect.width - Rect.width), yMin (Rect.height - Rect.height). xMax Rect.width, yMax Rect.height.
            if (!(_mousePosition.x < CameraManager.RuntimeFullsizeRect.xMin) && !(_mousePosition.x > CameraManager.RuntimeFullsizeRect.xMax) &&
                !(_mousePosition.y < CameraManager.RuntimeFullsizeRect.yMin) && !(_mousePosition.y > CameraManager.RuntimeFullsizeRect.yMax))
            {
                switch (m_graphicUiStates.SetCameraMode)
                {
                    //SingleCamera
                    case ECameraModi.SingleCam:
                    {
                        m_playerWindowId = m_cameraManager.AvailableCameras.IndexOf(m_cameraManager.AvailableCameras[0]);
                        break;
                    }
                    //TwoVertical
                    case ECameraModi.TwoVertical:
                    {
                        //if _mousePosition.x > Cam1.xMin m_playerWindowId = IndexOf Cam1, else IndexOf Cam0.
                        m_playerWindowId = _mousePosition.x >= m_cameraManager.AvailableCameras[1].pixelRect.xMin
                            ? m_cameraManager.AvailableCameras.IndexOf(m_cameraManager.AvailableCameras[1])
                            : m_cameraManager.AvailableCameras.IndexOf(m_cameraManager.AvailableCameras[0]);
                        break;
                    }
                    //TwoHorizontal
                    case ECameraModi.TwoHorizontal:
                    {
                        //if _mousePosition.y > Cam1.yMin m_playerWindowId = IndexOf Cam1, else IndexOf Cam0.
                        m_playerWindowId = _mousePosition.y >= m_cameraManager.AvailableCameras[1].pixelRect.yMin
                            ? m_cameraManager.AvailableCameras.IndexOf(m_cameraManager.AvailableCameras[1])
                            : m_cameraManager.AvailableCameras.IndexOf(m_cameraManager.AvailableCameras[0]);
                        break;
                    }
                    //FourSplit
                    case ECameraModi.FourSplit:
                    {
                        if (m_mousePosition.x < m_cameraManager.AvailableCameras[0].pixelRect.xMax && m_mousePosition.y < m_cameraManager.AvailableCameras[0].pixelRect.yMax)
                        {
                            m_playerWindowId = m_cameraManager.AvailableCameras.IndexOf(m_cameraManager.AvailableCameras[0]);
                        }

                        if (_mousePosition.x > m_cameraManager.AvailableCameras[1].pixelRect.xMin && _mousePosition.y < m_cameraManager.AvailableCameras[1].pixelRect.yMax)
                        {
                            m_playerWindowId = m_cameraManager.AvailableCameras.IndexOf(m_cameraManager.AvailableCameras[1]);
                        }

                        if (_mousePosition.x < m_cameraManager.AvailableCameras[2].pixelRect.xMax && _mousePosition.y > m_cameraManager.AvailableCameras[2].pixelRect.yMin)
                        {
                            m_playerWindowId = m_cameraManager.AvailableCameras.IndexOf(m_cameraManager.AvailableCameras[2]);
                        }

                        if (_mousePosition.x > m_cameraManager.AvailableCameras[3].pixelRect.xMin && _mousePosition.y > m_cameraManager.AvailableCameras[3].pixelRect.yMin)
                        {
                            m_playerWindowId = m_cameraManager.AvailableCameras.IndexOf(m_cameraManager.AvailableCameras[3]);
                        }
                        break;
                    }
                    default:
                    {
                        m_playerWindowId = m_cameraManager.AvailableCameras.IndexOf(m_cameraManager.AvailableCameras[0]);
                        break;
                    }
                }
            }
        }

        private void FollowUnsmoothed()
        {
            //Follows directly in xPosition, but the visible push looks buggy, if not lerped.
            Vector3 desiredPosition = new Vector3(m_RbPlayer.transform.localPosition.x/* + m_desiredOffset.x*/, m_RbPlayer.transform.localPosition.y + m_desiredOffset.y, m_RbPlayer.transform.localPosition.z + -m_desiredOffset.z);
            //Vector3 smoothedFollowing = Vector3.Lerp(m_followCamera.transform.localPosition, desiredPosition, Mathf.Max(m_smoothfactor, 30.0f) * Time.fixedDeltaTime);
            m_followCamera.transform.localPosition = desiredPosition;
            //m_followCamera.transform.localPosition = smoothedFollowing;
        }

        private void FollowSmoothly()
        {
            Vector3 desiredPosition = m_RbPlayer.transform.localPosition + new Vector3(m_desiredOffset.x, m_desiredOffset.y, -m_desiredOffset.z);
            Vector3 smoothedFollowing = Vector3.Lerp(m_followCamera.transform.localPosition, desiredPosition, m_smoothfactor * Time.fixedDeltaTime);
            m_followCamera.transform.localPosition = smoothedFollowing;
        }

        private void UpdateZoomPosition()
        {
            m_zoomTarget = new Vector3(m_followCamera.transform.localPosition.x, m_currentHeight, m_followCamera.transform.localPosition.z);

            m_zoomTarget -= m_zoomStep * (m_currentHeight - m_followCamera.transform.localPosition.y) * Vector3.forward;
            m_followCamera.transform.localPosition = Vector3.Lerp(m_followCamera.transform.localPosition, m_zoomTarget, Time.fixedDeltaTime * m_zoomDampening);
        }

        private void NewMousePosition(Vector2 _mousePosition)
        {
            m_mousePosition = _mousePosition;
        }

        /// <summary>
        /// Limit zoom to the inside of the current gameWindow by it's ID.
        /// </summary>
        /// <returns></returns>
        private int GetWindowId()
        {
            return m_playerWindowId;
        }

        private bool MouseIsWithinWindow(Vector2 _mousePosition)
        {
            Rect fullsizeRect = CameraManager.RuntimeFullsizeRect;
            //x/yMin == (Rect.width - Rect.width) or (Rect.height - Rect.height). x/yMax == Rect.width or Rect.height.
            bool mouseIsWithinWindow = _mousePosition.x > fullsizeRect.xMin && _mousePosition.x < fullsizeRect.xMax &&
                _mousePosition.y > fullsizeRect.yMin && _mousePosition.y < fullsizeRect.yMax;

            return mouseIsWithinWindow;
        }
        #endregion

        #region CallbackContext-Methods
        internal void Zooming(Vector2 _scrollVector)
        {
            if (m_playerId == GetWindowId() && MouseIsWithinWindow(m_mousePosition))
            {
                float zoomValue = -_scrollVector.y * m_zoomSpeed;
#if UNITY_EDITOR
                //Debug.Log($"ZoomValue: {zoomValue} PlayerWindowId: {m_playerWindowId} PlayerId: {m_playerId}");
#endif
                if (Mathf.Abs(zoomValue) > m_minAbsLimit)
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
        #endregion
    }
}