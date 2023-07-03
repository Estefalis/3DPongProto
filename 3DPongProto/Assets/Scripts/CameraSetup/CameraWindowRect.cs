using UnityEngine;
using ThreeDeePongProto.Managers;

//FullCamWindow: Width 536.4 - Height 302.

namespace ThreeDeePongProto.CameraSetup
{
    public class CameraWindowRect : MonoBehaviour
    {
        [SerializeField] private Camera m_playerCam1;
        [SerializeField] private Camera m_playerCam2;
        [SerializeField] private Camera m_playerCam3;
        [SerializeField] private Camera m_playerCam4;

        [SerializeField] private float m_fullWidthHor, m_halfWidthVer;
        [SerializeField] private float m_halfHeightHor, m_fullHeightVer;

        private Rect m_resetRect;
        private uint m_lastSetCameraMode;

        private void OnEnable()
        {
            m_resetRect = m_playerCam1.pixelRect;
            m_lastSetCameraMode = (uint)GameManager.Instance.ECameraMode;

            if (m_playerCam3 != null && m_playerCam4 != null && m_lastSetCameraMode == (uint)ECameraModi.FourSplit)
            {
                SetFourSplit(m_playerCam1, m_playerCam2, m_playerCam3, m_playerCam4);
                return; //TODO: May got to delete this return. <(<.<)>
            }
            else if (m_playerCam2 != null && m_lastSetCameraMode == (uint)ECameraModi.TwoVertical)
                SetCamerasVertical(m_playerCam1, m_playerCam2);
            else if (m_playerCam2 != null && m_lastSetCameraMode == (uint)ECameraModi.TwoHorizontal)
                SetCamerasHorizontal(m_playerCam1, m_playerCam2);
            else
                SetSingleCamera();
        }

        private void Update()
        {
            if ((uint)GameManager.Instance.ECameraMode != m_lastSetCameraMode)
            {
                switch ((uint)GameManager.Instance.ECameraMode)
                {
                    case 0:
                    {
                        m_lastSetCameraMode = (uint)GameManager.Instance.ECameraMode;
                        SetSingleCamera();
                        break;
                    }
                    case 1:
                    {
                        m_lastSetCameraMode = (uint)GameManager.Instance.ECameraMode;
                        SetCamerasHorizontal(m_playerCam1, m_playerCam2);
                        break;
                    }
                    case 2:
                    {
                        m_lastSetCameraMode = (uint)GameManager.Instance.ECameraMode;
                        SetCamerasVertical(m_playerCam1, m_playerCam2);
                        break;
                    }
                    case 3:
                    {
                        m_lastSetCameraMode = (uint)GameManager.Instance.ECameraMode;
                        //2 Camera should be always available. Up to 4 Cameras is just a mindplay until further changes.
                        if (m_playerCam3 != null && m_playerCam4 != null)
                            SetFourSplit(m_playerCam1, m_playerCam2, m_playerCam3, m_playerCam4);
                        break;
                    }
                    default:
                        m_lastSetCameraMode = (uint)ECameraModi.SingleCam;
                        SetSingleCamera();
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

            float Cam1X, Cam1Y, Cam1W, Cam1H, Cam2X, Cam2Y, Cam2W, Cam2H;
            CamRectOut(_camera1, _camera2, out Cam1X, out Cam1Y, out Cam1W, out Cam1H, out Cam2X, out Cam2Y, out Cam2W, out Cam2H);

            _camera1.pixelRect = new Rect(Cam1X, Cam1Y, Cam1W * m_fullWidthHor, Cam1H * m_halfHeightHor);

            Cam2Y += Cam2H * m_halfHeightHor;
            _camera2.pixelRect = new Rect(Cam2X, Cam2Y, Cam2W * m_fullWidthHor, Cam2H * m_halfHeightHor);
        }

        public void SetCamerasVertical(Camera _camera1, Camera _camera2)
        {
            SetCamerasAndRects(m_lastSetCameraMode);

            float Cam1X, Cam1Y, Cam1W, Cam1H, Cam2X, Cam2Y, Cam2W, Cam2H;
            CamRectOut(_camera1, _camera2, out Cam1X, out Cam1Y, out Cam1W, out Cam1H, out Cam2X, out Cam2Y, out Cam2W, out Cam2H);

            _camera1.pixelRect = new Rect(Cam1X, Cam1Y, Cam1W * m_halfWidthVer, Cam1H * m_fullHeightVer);

            Cam2X += Cam2W * m_halfWidthVer;
            _camera2.pixelRect = new Rect(Cam2X, Cam2Y, Cam2W * m_halfWidthVer, Cam2H * m_fullHeightVer);
        }

        public void SetFourSplit(Camera _camera1, Camera _camera2, Camera _camera3, Camera _camera4)
        {
            SetCamerasAndRects(m_lastSetCameraMode);

            float Cam1X, Cam1Y, Cam1W, Cam1H, Cam2X, Cam2Y, Cam2W, Cam2H;
            CamRectOut(_camera1, _camera2, out Cam1X, out Cam1Y, out Cam1W, out Cam1H, out Cam2X, out Cam2Y, out Cam2W, out Cam2H);

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

            Cam2X += Cam2W * m_halfWidthVer;
            _camera2.pixelRect = new Rect(Cam2X, Cam2Y, Cam2W * m_halfWidthVer, Cam2H * m_halfHeightHor);

            Cam3Y += Cam3H * m_halfHeightHor;
            _camera3.pixelRect = new Rect(Cam3X, Cam3Y, Cam3W * m_halfWidthVer, Cam3H * m_halfHeightHor);

            Cam4X += Cam4W * m_halfWidthVer;
            Cam4Y += Cam4H * m_halfHeightHor;
            _camera4.pixelRect = new Rect(Cam4X, Cam4Y, Cam4W * m_halfWidthVer, Cam4H * m_halfHeightHor);
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

        private void SetCamerasAndRects(uint _cameraMode)
        {
            switch (_cameraMode)
            {
                case 0:
                {
                    //TODO: Here Camera Transform- & Rotation-Settings. (SetCameraAlignment())
                    m_playerCam2.gameObject.SetActive(false);
                    m_playerCam3.gameObject.SetActive(false);
                    m_playerCam4.gameObject.SetActive(false);
                    ResetViewportRects(m_playerCam1);
                    break;
                }
                case 1:
                {
                    m_playerCam2.gameObject.SetActive(true);
                    m_playerCam3.gameObject.SetActive(false);
                    m_playerCam4.gameObject.SetActive(false);
                    ResetViewportRects(m_playerCam1, m_playerCam2);
                    break;
                }
                case 2:
                {
                    m_playerCam2.gameObject.SetActive(true);
                    m_playerCam3.gameObject.SetActive(false);
                    m_playerCam4.gameObject.SetActive(false);
                    ResetViewportRects(m_playerCam1, m_playerCam2);
                    break;
                }
                case 3:
                {
                    m_playerCam2.gameObject.SetActive(true);
                    m_playerCam3.gameObject.SetActive(true);
                    m_playerCam4.gameObject.SetActive(true);
                    ResetViewportRects(m_playerCam1, m_playerCam2, m_playerCam3, m_playerCam4);
                    break;
                }
                default:
                    Debug.Log("Mode not implemented, yet! Please ask your personal Progger. <(~.^)'");
                    break;
            }
        }

        private void ResetViewportRects(Camera _camera1, Camera _camera2 = null, Camera _camera3 = null, Camera _camera4 = null)
        {
            _camera1.pixelRect = m_resetRect;

            if (_camera2 != null)
                _camera2.pixelRect = m_resetRect;
            if (_camera3 != null)
                _camera3.pixelRect = m_resetRect;
            if (_camera4 != null)
                m_playerCam4.pixelRect = m_resetRect;
        }
    }
}