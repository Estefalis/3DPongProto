using UnityEngine;
using ThreeDeePongProto.Offline.Settings;

namespace ThreeDeePongProto.Offline.CameraSetup
{
    public class SetCameraRects : MonoBehaviour
    {
        #region Script-References
        [SerializeField] private GraphicSettings m_graphicsSettings;
        private CameraManager m_cameraManager;
        #endregion

        private float m_fullWidthHor = 1.0f;
        private float m_fullHeightVer = 1.0f;
        private float m_halfHeightHor = 0.5f;
        private float m_halfWidthVer = 0.5f;

        //private Rect m_fullsizeRect; //Moved to CameraManager.
        private int m_lastSetCameraMode;

        #region Scriptable Variables
        [SerializeField] private GraphicUiStates m_graphicUiStates;
        #endregion

        private void Awake()
        {
            m_cameraManager = GetComponent<CameraManager>();
        }

        private void OnEnable()
        {
            //Moved to CameraManager.
            //Debug.Log(m_cameraManager.AvailableCameras[0]); //Old m_playerCam1
            //Debug.Log(m_cameraManager.AvailableCameras[1]); //Old m_playerCam2
            //Debug.Log(m_cameraManager.AvailableCameras[2]); //Old m_playerCam3
            //Debug.Log(m_cameraManager.AvailableCameras[3]); //Old m_playerCam4

            UpdateFullsizeRect();
        }

        private void Start()
        {
            if (m_graphicUiStates == null)
            {
                m_lastSetCameraMode = (int)m_graphicsSettings.ECameraMode;
            }
            else
                m_lastSetCameraMode = (int)m_graphicUiStates.SetCameraMode;

            SetCameraMode(m_lastSetCameraMode);
        }

        private void SetCameraMode(int _cameraMode)
        {
            switch (_cameraMode)
            {
                case 0:
                {
                    SetSingleCamera();
                    break;
                }
                case 1:
                {
                    if (m_cameraManager.AvailableCameras[1] != null && m_lastSetCameraMode == (int)ECameraModi.TwoHorizontal)
                        SetCamerasHorizontal(m_cameraManager.AvailableCameras[0], m_cameraManager.AvailableCameras[1]);
                    break;
                }
                case 2:
                {
                    if (m_cameraManager.AvailableCameras[1] != null && m_lastSetCameraMode == (int)ECameraModi.TwoVertical)
                        SetCamerasVertical(m_cameraManager.AvailableCameras[0], m_cameraManager.AvailableCameras[1]);
                    break;
                }
                case 3:
                {
                    if (m_cameraManager.AvailableCameras[2] != null && m_cameraManager.AvailableCameras[3] != null && m_lastSetCameraMode == (int)ECameraModi.FourSplit)
                        SetFourSplit(m_cameraManager.AvailableCameras[0], m_cameraManager.AvailableCameras[1], m_cameraManager.AvailableCameras[2], m_cameraManager.AvailableCameras[3]);
                    break;
                }
                default:
                    m_lastSetCameraMode = (int)ECameraModi.SingleCam;
                    SetSingleCamera();
                    break;
            }
        }

        private void Update()
        {
            UpdateFullsizeRect();

            //SetRectSplit();
        }

        #region Direct Runtime ScreenChange
        //private void SetRectSplit()
        //{
        //    //if (m_lastSetCameraMode != (int)GameManager.Instance.ECameraMode)
        //    if (m_lastSetCameraMode != (int)m_matchUIStates.SetCameraMode)
        //    {
        //        m_lastSetCameraMode = (int)m_matchUIStates.SetCameraMode;
        //        SetCameraMode(m_lastSetCameraMode);
        //    }
        //}
        #endregion

        /// <summary>
        /// new Rect(xOrigin, yOrigin, width, height)
        /// </summary>
        private void UpdateFullsizeRect()
        {
            switch (m_lastSetCameraMode)
            {
                //SingleCamera
                case 0:
                {
                    CameraManager.RuntimeFullsizeRect = m_cameraManager.AvailableCameras[0].pixelRect;
                    break;
                }
                //TwoHorizontal
                case 1:
                {
                    CameraManager.RuntimeFullsizeRect = new Rect(0, 0, m_cameraManager.AvailableCameras[0].pixelRect.width, m_cameraManager.AvailableCameras[0].pixelRect.height + m_cameraManager.AvailableCameras[1].pixelRect.height);
                    break;
                }
                //TwoVertical
                case 2:
                {
                    CameraManager.RuntimeFullsizeRect = new Rect(0, 0, m_cameraManager.AvailableCameras[0].pixelRect.width + m_cameraManager.AvailableCameras[1].pixelRect.width, m_cameraManager.AvailableCameras[0].pixelRect.height);
                    break;
                }
                //FourSplit
                case 3:
                {
                    CameraManager.RuntimeFullsizeRect = new Rect(0, 0, m_cameraManager.AvailableCameras[0].pixelRect.width + m_cameraManager.AvailableCameras[1].pixelRect.width, m_cameraManager.AvailableCameras[0].pixelRect.height + m_cameraManager.AvailableCameras[1].pixelRect.height);
                    break;
                }
            }
        }

        public void SetSingleCamera()
        {
            SetCamerasAndRects(m_lastSetCameraMode);
        }

        public void SetCamerasHorizontal(Camera _camera1, Camera _camera2)
        {
            SetCamerasAndRects(m_lastSetCameraMode);

            CamRectOut(_camera1, _camera2, out float Cam1X, out float Cam1Y, out float Cam1W, out float Cam1H, out float Cam2X, out float Cam2Y, out float Cam2W, out float Cam2H);

            _camera1.pixelRect = new Rect(Cam1X, Cam1Y, Cam1W * m_fullWidthHor, Cam1H * m_halfHeightHor);
            m_cameraManager.UpdateRectDimensions(_camera1, _camera1.pixelRect);

            Cam2Y += Cam2H * m_halfHeightHor;
            _camera2.pixelRect = new Rect(Cam2X, Cam2Y, Cam2W * m_fullWidthHor, Cam2H * m_halfHeightHor);
            m_cameraManager.UpdateRectDimensions(_camera2, _camera2.pixelRect);
        }

        public void SetCamerasVertical(Camera _camera1, Camera _camera2)
        {
            SetCamerasAndRects(m_lastSetCameraMode);

            CamRectOut(_camera1, _camera2, out float Cam1X, out float Cam1Y, out float Cam1W, out float Cam1H, out float Cam2X, out float Cam2Y, out float Cam2W, out float Cam2H);

            _camera1.pixelRect = new Rect(Cam1X, Cam1Y, Cam1W * m_halfWidthVer, Cam1H * m_fullHeightVer);
            m_cameraManager.UpdateRectDimensions(_camera1, _camera1.pixelRect);

            Cam2X += Cam2W * m_halfWidthVer;
            _camera2.pixelRect = new Rect(Cam2X, Cam2Y, Cam2W * m_halfWidthVer, Cam2H * m_fullHeightVer);
            m_cameraManager.UpdateRectDimensions(_camera2, _camera2.pixelRect);
        }

        public void SetFourSplit(Camera _camera1, Camera _camera2, Camera _camera3, Camera _camera4)
        {
            SetCamerasAndRects(m_lastSetCameraMode);

            CamRectOut(_camera1, _camera2, out float Cam1X, out float Cam1Y, out float Cam1W, out float Cam1H, out float Cam2X, out float Cam2Y, out float Cam2W, out float Cam2H);

            float Cam3X = _camera3.pixelRect.x;
            float Cam3Y = _camera3.pixelRect.y;
            float Cam3W = _camera3.pixelRect.width;
            float Cam3H = _camera3.pixelRect.height;

            float Cam4X = _camera4.pixelRect.x;
            float Cam4Y = _camera4.pixelRect.y;
            float Cam4W = _camera4.pixelRect.width;
            float Cam4H = _camera4.pixelRect.height;

            //Cam1W *= m_halfWidthVer;
            _camera1.pixelRect = new Rect(Cam1X, Cam1Y, Cam1W * m_halfWidthVer, Cam1H * m_halfHeightHor);
            m_cameraManager.UpdateRectDimensions(_camera1, _camera1.pixelRect);

            Cam2X += Cam2W * m_halfWidthVer;
            _camera2.pixelRect = new Rect(Cam2X, Cam2Y, Cam2W * m_halfWidthVer, Cam2H * m_halfHeightHor);
            m_cameraManager.UpdateRectDimensions(_camera2, _camera2.pixelRect);

            Cam3Y += Cam3H * m_halfHeightHor;
            _camera3.pixelRect = new Rect(Cam3X, Cam3Y, Cam3W * m_halfWidthVer, Cam3H * m_halfHeightHor);
            m_cameraManager.UpdateRectDimensions(_camera3, _camera3.pixelRect);

            Cam4X += Cam4W * m_halfWidthVer;
            Cam4Y += Cam4H * m_halfHeightHor;
            _camera4.pixelRect = new Rect(Cam4X, Cam4Y, Cam4W * m_halfWidthVer, Cam4H * m_halfHeightHor);
            m_cameraManager.UpdateRectDimensions(_camera4, _camera4.pixelRect);
        }

        private void CamRectOut(Camera _camera1, Camera _camera2, out float Cam1X, out float Cam1Y, out float Cam1W, out float Cam1H, out float Cam2X, out float Cam2Y, out float Cam2W, out float Cam2H)
        {
            Cam1X = _camera1.pixelRect.x;
            Cam1Y = _camera1.pixelRect.y;
            Cam1W = _camera1.pixelRect.width;
            Cam1H = _camera1.pixelRect.height;
            Cam2X = _camera2.pixelRect.x;
            Cam2Y = _camera2.pixelRect.y;
            Cam2W = _camera2.pixelRect.width;
            Cam2H = _camera2.pixelRect.height;
        }

        private void SetCamerasAndRects(int _cameraMode)
        {
            switch (_cameraMode)
            {
                case 0:
                {
                    m_cameraManager.AvailableCameras[1].gameObject.SetActive(false);
                    m_cameraManager.AvailableCameras[2].gameObject.SetActive(false);
                    m_cameraManager.AvailableCameras[3].gameObject.SetActive(false);
                    ResetViewportRects(m_cameraManager.AvailableCameras[0]);
                    break;
                }
                case 1:
                {
                    m_cameraManager.AvailableCameras[1].gameObject.SetActive(true);
                    m_cameraManager.AvailableCameras[2].gameObject.SetActive(false);
                    m_cameraManager.AvailableCameras[3].gameObject.SetActive(false);
                    ResetViewportRects(m_cameraManager.AvailableCameras[0], m_cameraManager.AvailableCameras[1]);
                    break;
                }
                case 2:
                {
                    m_cameraManager.AvailableCameras[1].gameObject.SetActive(true);
                    m_cameraManager.AvailableCameras[2].gameObject.SetActive(false);
                    m_cameraManager.AvailableCameras[3].gameObject.SetActive(false);
                    ResetViewportRects(m_cameraManager.AvailableCameras[0], m_cameraManager.AvailableCameras[1]);
                    break;
                }
                case 3:
                {
                    m_cameraManager.AvailableCameras[1].gameObject.SetActive(true);
                    m_cameraManager.AvailableCameras[2].gameObject.SetActive(true);
                    m_cameraManager.AvailableCameras[3].gameObject.SetActive(true);
                    ResetViewportRects(m_cameraManager.AvailableCameras[0], m_cameraManager.AvailableCameras[1], m_cameraManager.AvailableCameras[2], m_cameraManager.AvailableCameras[3]);
                    break;
                }
                default:
                    Debug.Log("Mode not implemented, yet! Please ask your personal Progger. <(~.^)'");
                    break;
            }
        }

        private void ResetViewportRects(Camera _camera1, Camera _camera2 = null, Camera _camera3 = null, Camera _camera4 = null)
        {
            if (m_cameraManager.AvailableCameras[0].pixelRect != CameraManager.RuntimeFullsizeRect)
            {
                m_cameraManager.UpdateRectDimensions(m_cameraManager.AvailableCameras[0], CameraManager.RuntimeFullsizeRect);
            }
            _camera1.pixelRect = CameraManager.RuntimeFullsizeRect;

            if (m_cameraManager.AvailableCameras[1].pixelRect != CameraManager.RuntimeFullsizeRect)
            {
                m_cameraManager.UpdateRectDimensions(m_cameraManager.AvailableCameras[1], CameraManager.RuntimeFullsizeRect);
            }
            if (_camera2 != null)
                _camera2.pixelRect = CameraManager.RuntimeFullsizeRect;

            if (m_cameraManager.AvailableCameras[2].pixelRect != CameraManager.RuntimeFullsizeRect)
            {
                m_cameraManager.UpdateRectDimensions(m_cameraManager.AvailableCameras[2], CameraManager.RuntimeFullsizeRect);
            }
            if (_camera3 != null)
                _camera3.pixelRect = CameraManager.RuntimeFullsizeRect;

            if (m_cameraManager.AvailableCameras[3].pixelRect != CameraManager.RuntimeFullsizeRect)
            {
                m_cameraManager.UpdateRectDimensions(m_cameraManager.AvailableCameras[3], CameraManager.RuntimeFullsizeRect);
            }
            if (_camera4 != null)
                m_cameraManager.AvailableCameras[3].pixelRect = CameraManager.RuntimeFullsizeRect;
        }
    }
}