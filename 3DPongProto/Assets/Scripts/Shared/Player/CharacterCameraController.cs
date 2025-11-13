using System;
using ThreeDeePongProto.Offline.CameraSetup;
using ThreeDeePongProto.Shared.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeDeePongProto.Shared.PlayerCharacter
{
    internal class CharacterCameraController : MonoBehaviour, IProvidePlayerID
    {
        [SerializeField] internal CharacterMainController m_playerController;

        [Header("Camera-Positions")]
        [SerializeField] private float m_lowestHeight;
        [SerializeField] private float m_maximalHeight;
        private float m_currentHeight, m_cameraZPos;
        private Vector3 m_cameraPosition;

        [Header("Smooth Following")]
        [SerializeField] internal Camera m_followCamera;
        [SerializeField] private bool m_enableSmoothFollow = true;
        [SerializeField] private Vector3 m_desiredOffset;
        [Range(1.0f, 30.0f)]
        [SerializeField] private float m_smoothfactor;
        internal Rigidbody m_rigidbody;

        [Header("Camera-Zoom")]
        [SerializeField, Min(0.01f)] private float m_minAbsLimit = 0.05f;
        [SerializeField, Min(0.001f)] private float m_zoomSpeed;
        [SerializeField] private float m_zoomStep;
        [SerializeField] private float m_zoomDampening;
        private Vector3 m_zoomTarget;
        internal Vector2 m_mousePosition;

        private CameraManager m_cameraManager;

        internal int m_playerID;

        private void Awake()
        {
            m_cameraManager = FindObjectOfType<CameraManager>();
        }

        private void OnEnable()
        {
            if (m_playerController == null)
                m_playerController.GetComponentInParent<CharacterMainController>();

            m_followCamera.transform.position = m_playerController.transform.position - new Vector3(m_desiredOffset.x, -m_desiredOffset.y, m_desiredOffset.z);
        }

        private void OnDisable()
        {
            CameraManager.Instance.RemoveCamera(m_followCamera);
            CharacterInputHandler.ASendMousePosition -= NewMousePosition;
        }

        private void Start()
        {
            //After Players are instantiated and the LocalMatchManager invokes the Event, the Camera can add itself to the availableCamera-List in the CameraManager.
            CameraManager.Instance.RegisterCamera(m_followCamera);
            CharacterInputHandler.ASendMousePosition += NewMousePosition;

            //Saved vector to keep the playerCamera-startposition.
            CameraPositions(m_cameraManager.AvailableCameras[m_playerID]);
        }

        private void FixedUpdate()
        {
            if (m_rigidbody == null)
                return;

            if (!m_enableSmoothFollow)
                FollowUnsmoothed();

            if (m_enableSmoothFollow)
                FollowSmoothly();

            UpdateZoomPosition();
        }

        #region Custom-Methods
        public void SetPlayerID(int _playerID)
        {
            m_playerID = _playerID;
        }

        private void CameraPositions(Camera _camera)
        {
            m_cameraPosition = -(_camera.transform.localPosition - Vector3.zero);
            m_currentHeight = -m_cameraPosition.y;
            m_cameraZPos = m_cameraPosition.z;
            m_cameraPosition = new Vector3(m_cameraPosition.x, m_currentHeight, -m_cameraZPos);
        }

        private void FollowUnsmoothed()
        {
            //Follows directly in xPosition, but the visible push looks buggy, if not lerped.
            Vector3 desiredPosition = new Vector3(m_rigidbody.transform.localPosition.x/* + m_desiredOffset.x*/, m_rigidbody.transform.localPosition.y + m_desiredOffset.y, m_rigidbody.transform.localPosition.z + -m_desiredOffset.z);
            //Vector3 smoothedFollowing = Vector3.Lerp(m_followCamera.transform.localPosition, desiredPosition, Mathf.Max(m_smoothfactor, 30.0f) * Time.fixedDeltaTime);
            m_followCamera.transform.localPosition = desiredPosition;
            //m_followCamera.transform.localPosition = smoothedFollowing;
        }

        private void FollowSmoothly()
        {
            Vector3 desiredPosition = m_rigidbody.transform.localPosition + new Vector3(m_desiredOffset.x, m_desiredOffset.y, -m_desiredOffset.z);
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

        private bool IsMouseInGameWindow(Vector2 _mousePosition)
        {
            //Vector3 mousePosition = Input.mousePosition;
            return _mousePosition.x >= 0 && _mousePosition.x < Screen.width && _mousePosition.y >= 0 && _mousePosition.y < Screen.height;
        }
        #endregion

        #region CallbackContext-Methods
        /// <summary>
        /// Current Zoom works with manual set playerID
        /// </summary>
        /// <param name="_scrollVector"></param>
        /// <param name="_contextDevice"></param>
        /// <param name="_playerID"></param>
        internal void Zoom(Vector2 _scrollVector, InputDevice _contextDevice, int _playerID)
        {
            if (_playerID != m_playerID)
                Debug.LogWarning("IDs from the CameraController and InputHandler do not match! Possible influence from outside!");

            bool allowScrollWheelZoom = IsMouseInGameWindow(m_mousePosition) && Application.isFocused;
            if (!allowScrollWheelZoom)
                return;

            switch (_contextDevice)
            {
                case Gamepad:
                {
                    float zoomValue = -_scrollVector.y * m_zoomSpeed;
                    if (Mathf.Abs(zoomValue) > m_minAbsLimit)
                    {
                        m_currentHeight = m_followCamera.transform.localPosition.y + zoomValue * m_zoomStep;

                        if (m_currentHeight < m_lowestHeight)
                            m_currentHeight = m_lowestHeight;
                        else if (m_currentHeight > m_maximalHeight)
                            m_currentHeight = m_maximalHeight;
                    }
                    break;
                }
                case Keyboard:  //If changes towards Keyboard-Buttons are desired.
                case Mouse:     //Else just use the Mouse-ScrollWheel.
                {
                    if (UserInputManager.FocusedKeyboardPlayerID == m_playerID)
                    {
                        float zoomValue = -_scrollVector.y * m_zoomSpeed;
                        if (Mathf.Abs(zoomValue) > m_minAbsLimit)
                        {
                            m_currentHeight = m_followCamera.transform.localPosition.y + zoomValue * m_zoomStep;

                            if (m_currentHeight < m_lowestHeight)
                                m_currentHeight = m_lowestHeight;
                            else if (m_currentHeight > m_maximalHeight)
                                m_currentHeight = m_maximalHeight;
                        }
                    }
                    break;
                }
            }
        }
        #endregion
    }
}