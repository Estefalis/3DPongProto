using UnityEngine;
using ThreeDeePongProto.Offline.Settings;
using System.Collections;

namespace ThreeDeePongProto.Offline.CameraSetup
{
    public class SetCameraRects : MonoBehaviour
    {
        #region Script-References
        [SerializeField] private GraphicSettings m_graphicsSettings;
        [SerializeField] private CameraManager m_cameraManager;
        #endregion

        private const float m_FULLWIDTHHOR = 1.0f;
        private const float m_FULLHEIGHTVER = 1.0f;
        private const float m_HALFHEIGHTHOR = 0.5f;
        private const float m_HALFWIDTHVER = 0.5f;

        //private Rect m_fullsizeRect; //Moved to CameraManager.
        private ECameraModi m_lastSetCameraMode;

        #region Scriptable Variables
        [SerializeField] private GraphicUiStates m_graphicUiStates;
        [SerializeField] private MatchValues m_matchValues;
        #endregion

        private IEnumerator Start()
        {
            if (m_graphicUiStates == null)
            {
                m_lastSetCameraMode = m_graphicsSettings.ECameraMode;
            }
            else
                m_lastSetCameraMode = m_graphicUiStates.SetCameraMode;

            yield return new WaitUntil(DelegateBool);

            SetCameraMode(m_lastSetCameraMode);

            //AvailableCameras moved to CameraManager.
            UpdateFullsizeRect();
        }

        /// <summary>
        /// Only returns true, after the activated PlayerCameras added themselves to the 'AvailableCameras'-List equal to the registered PlayerIDData.
        /// </summary>
        /// <returns></returns>
        private bool DelegateBool()
        {
            if (m_cameraManager.AvailableCameras.Count == m_matchValues.PlayerData.Count)
                return true;
            else
                return false;
        }

        private void SetCameraMode(ECameraModi _cameraMode)
        {
            switch (_cameraMode)
            {
                case ECameraModi.SingleCam:
                {
                    SetSingleCamera();
                    break;
                }
                case ECameraModi.TwoVertical:
                {
                    if (m_cameraManager.AvailableCameras[1] != null && m_lastSetCameraMode == ECameraModi.TwoVertical)
                        SetCamerasVertical(m_cameraManager.AvailableCameras[0], m_cameraManager.AvailableCameras[1]);
                    break;
                }
                case ECameraModi.TwoHorizontal:
                {
                    if (m_cameraManager.AvailableCameras[1] != null && m_lastSetCameraMode == ECameraModi.TwoHorizontal)
                        SetCamerasHorizontal(m_cameraManager.AvailableCameras[0], m_cameraManager.AvailableCameras[1]);
                    break;
                }
                case ECameraModi.FourSplit:
                {
                    if (m_cameraManager.AvailableCameras[2] != null && m_cameraManager.AvailableCameras[3] != null && m_lastSetCameraMode == ECameraModi.FourSplit)
                        SetFourSplit(m_cameraManager.AvailableCameras[0], m_cameraManager.AvailableCameras[1], m_cameraManager.AvailableCameras[2], m_cameraManager.AvailableCameras[3]);
                    break;
                }
                default:    //TwoHorizontal
                    m_lastSetCameraMode = ECameraModi.TwoHorizontal;
                    if (m_cameraManager.AvailableCameras[1] != null && m_lastSetCameraMode == ECameraModi.TwoHorizontal)
                        SetCamerasHorizontal(m_cameraManager.AvailableCameras[0], m_cameraManager.AvailableCameras[1]);
                    break;
            }
        }

        private void Update()
        {
            UpdateFullsizeRect();

#if UNITY_EDITOR
            //Debug.Log($"Cam1 Height {m_cameraManager.AvailableCameras[0].pixelRect.height} -  Width {m_cameraManager.AvailableCameras[0].pixelRect.width}");
            //Debug.Log($"Cam2 Height {m_cameraManager.AvailableCameras[1].pixelRect.height} -  Width {m_cameraManager.AvailableCameras[1].pixelRect.width}");
            //Debug.Log($"Cam3 Height {m_cameraManager.AvailableCameras[2].pixelRect.height} -  Width {m_cameraManager.AvailableCameras[2].pixelRect.width}");
            //Debug.Log($"Cam4 Height {m_cameraManager.AvailableCameras[3].pixelRect.height} -  Width {m_cameraManager.AvailableCameras[3].pixelRect.width}");
#endif
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
                case ECameraModi.SingleCam:
                {
                    CameraManager.RuntimeFullsizeRect = m_cameraManager.AvailableCameras[0].pixelRect;
                    break;
                }
                case ECameraModi.TwoVertical:
                {
                    CameraManager.RuntimeFullsizeRect = new Rect(0, 0, m_cameraManager.AvailableCameras[0].pixelRect.width + m_cameraManager.AvailableCameras[1].pixelRect.width, m_cameraManager.AvailableCameras[0].pixelRect.height);
                    break;
                }
                case ECameraModi.TwoHorizontal:
                {
                    CameraManager.RuntimeFullsizeRect = new Rect(0, 0, m_cameraManager.AvailableCameras[0].pixelRect.width, m_cameraManager.AvailableCameras[0].pixelRect.height + m_cameraManager.AvailableCameras[1].pixelRect.height);
                    break;
                }
                case ECameraModi.FourSplit:
                {
                    CameraManager.RuntimeFullsizeRect = new Rect(0, 0, m_cameraManager.AvailableCameras[0].pixelRect.width + m_cameraManager.AvailableCameras[1].pixelRect.width, m_cameraManager.AvailableCameras[0].pixelRect.height + m_cameraManager.AvailableCameras[1].pixelRect.height);
                    break;
                }
                default:    //TwoHorizontal
                {
                    CameraManager.RuntimeFullsizeRect = new Rect(0, 0, m_cameraManager.AvailableCameras[0].pixelRect.width, m_cameraManager.AvailableCameras[0].pixelRect.height + m_cameraManager.AvailableCameras[1].pixelRect.height);
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

            _camera1.pixelRect = new Rect(Cam1X, Cam1Y, Cam1W * m_FULLWIDTHHOR, Cam1H * m_HALFHEIGHTHOR);
            m_cameraManager.UpdateRectDimensions(_camera1, _camera1.pixelRect);

            Cam2Y += Cam2H * m_HALFHEIGHTHOR;
            _camera2.pixelRect = new Rect(Cam2X, Cam2Y, Cam2W * m_FULLWIDTHHOR, Cam2H * m_HALFHEIGHTHOR);
            m_cameraManager.UpdateRectDimensions(_camera2, _camera2.pixelRect);
        }

        public void SetCamerasVertical(Camera _camera1, Camera _camera2)
        {
            SetCamerasAndRects(m_lastSetCameraMode);

            CamRectOut(_camera1, _camera2, out float Cam1X, out float Cam1Y, out float Cam1W, out float Cam1H, out float Cam2X, out float Cam2Y, out float Cam2W, out float Cam2H);

            _camera1.pixelRect = new Rect(Cam1X, Cam1Y, Cam1W * m_HALFWIDTHVER, Cam1H * m_FULLHEIGHTVER);
            m_cameraManager.UpdateRectDimensions(_camera1, _camera1.pixelRect);

            Cam2X += Cam2W * m_HALFWIDTHVER;
            _camera2.pixelRect = new Rect(Cam2X, Cam2Y, Cam2W * m_HALFWIDTHVER, Cam2H * m_FULLHEIGHTVER);
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

            //Cam1W *= m_HALFWIDTHVER;
            _camera1.pixelRect = new Rect(Cam1X, Cam1Y, Cam1W * m_HALFWIDTHVER, Cam1H * m_HALFHEIGHTHOR);
            m_cameraManager.UpdateRectDimensions(_camera1, _camera1.pixelRect);

            Cam2X += Cam2W * m_HALFWIDTHVER;
            _camera2.pixelRect = new Rect(Cam2X, Cam2Y, Cam2W * m_HALFWIDTHVER, Cam2H * m_HALFHEIGHTHOR);
            m_cameraManager.UpdateRectDimensions(_camera2, _camera2.pixelRect);

            Cam3Y += Cam3H * m_HALFHEIGHTHOR;
            _camera3.pixelRect = new Rect(Cam3X, Cam3Y, Cam3W * m_HALFWIDTHVER, Cam3H * m_HALFHEIGHTHOR);
            m_cameraManager.UpdateRectDimensions(_camera3, _camera3.pixelRect);

            Cam4X += Cam4W * m_HALFWIDTHVER;
            Cam4Y += Cam4H * m_HALFHEIGHTHOR;
            _camera4.pixelRect = new Rect(Cam4X, Cam4Y, Cam4W * m_HALFWIDTHVER, Cam4H * m_HALFHEIGHTHOR);
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

        private void SetCamerasAndRects(ECameraModi _cameraMode)
        {
            switch (_cameraMode)
            {
                case ECameraModi.SingleCam:
                {
                    m_cameraManager.AvailableCameras[1].gameObject.SetActive(false);
                    m_cameraManager.AvailableCameras[2].gameObject.SetActive(false);
                    m_cameraManager.AvailableCameras[3].gameObject.SetActive(false);
                    ResetViewportRects(m_cameraManager.AvailableCameras[0]);
                    break;
                }
                case ECameraModi.TwoVertical:
                case ECameraModi.TwoHorizontal:
                {
                    m_cameraManager.AvailableCameras[1].gameObject.SetActive(true);
                    m_cameraManager.AvailableCameras[2].gameObject.SetActive(false);
                    m_cameraManager.AvailableCameras[3].gameObject.SetActive(false);
                    ResetViewportRects(m_cameraManager.AvailableCameras[0], m_cameraManager.AvailableCameras[1]);
                    break;
                }
                case ECameraModi.FourSplit:
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