using UnityEngine;
using ThreeDeePongProto.Offline.Settings;
using System.Collections;

namespace ThreeDeePongProto.Offline.CameraSetup
{
    public class SetCameraRects : MonoBehaviour
    {
//        #region Script-References
//        [SerializeField] private CameraManager m_cameraManager;
//        #endregion

//        private const float m_FullWidthHor = 1.0f;
//        private const float m_FullHeightVer = 1.0f;
//        private const float m_HalfHeightHor = 0.5f;
//        private const float m_HalfWidthVer = 0.5f;

//        //private Rect m_fullsizeRect; //Moved to CameraManager.
//        private ECameraModi m_lastSetCameraMode;

//        #region Scriptable Variables
//        [SerializeField] private GraphicSettings m_graphicsSettings;
//        [SerializeField] private GraphicUIStates m_graphicUIStates;
//        [SerializeField] private MatchValues m_matchValues;
//        #endregion

//        private IEnumerator Start()
//        {
//            if (m_graphicUIStates == null)
//            {
//                m_lastSetCameraMode = m_graphicsSettings.ECameraMode;
//            }
//            else
//                m_lastSetCameraMode = m_graphicUIStates.SetCameraMode;

//            yield return new WaitUntil(CamerasEqualPlayerCount);

//            SetCameraMode(m_lastSetCameraMode);

//            //AvailableCameras moved to CameraManager.
//            UpdateFullsizeRect();
//        }

//        /// <summary>
//        /// Only returns true, after the activated PlayerCameras added themselves to the 'AvailableCameras'-List are equal to the registered PlayerIDData.
//        /// </summary>
//        /// <returns></returns>
//        private bool CamerasEqualPlayerCount()
//        {
//            switch (m_cameraManager.AvailableCameras.Count == m_matchValues.PlayerData.Count)
//            {
//                case true:
//                    return true;
//                case false:
//                    return false;
//            }
//        }

//        private void SetCameraMode(ECameraModi _cameraMode)
//        {
//            switch (_cameraMode)
//            {
//                case ECameraModi.SingleCam:
//                {
//                    SetSingleCamera();
//                    break;
//                }
//                case ECameraModi.TwoVertical:
//                {
//                    if (m_cameraManager.AvailableCameras[1] != null && m_lastSetCameraMode == ECameraModi.TwoVertical)
//                        SetCamerasVertical(m_cameraManager.AvailableCameras[0], m_cameraManager.AvailableCameras[1]);
//                    break;
//                }
//                case ECameraModi.TwoHorizontal:
//                {
//                    if (m_cameraManager.AvailableCameras[1] != null && m_lastSetCameraMode == ECameraModi.TwoHorizontal)
//                        SetCamerasHorizontal(m_cameraManager.AvailableCameras[0], m_cameraManager.AvailableCameras[1]);
//                    break;
//                }
//                case ECameraModi.FourSplit:
//                {
//                    if (m_cameraManager.AvailableCameras[2] != null && m_cameraManager.AvailableCameras[3] != null && m_lastSetCameraMode == ECameraModi.FourSplit)
//                        SetFourSplit(m_cameraManager.AvailableCameras[0], m_cameraManager.AvailableCameras[1], m_cameraManager.AvailableCameras[2], m_cameraManager.AvailableCameras[3]);
//                    break;
//                }
//                default:    //TwoHorizontal
//                    m_lastSetCameraMode = ECameraModi.TwoHorizontal;
//                    if (m_cameraManager.AvailableCameras[1] != null && m_lastSetCameraMode == ECameraModi.TwoHorizontal)
//                        SetCamerasHorizontal(m_cameraManager.AvailableCameras[0], m_cameraManager.AvailableCameras[1]);
//                    break;
//            }
//        }

//        private void Update()
//        {
//            UpdateFullsizeRect();
//#if UNITY_EDITOR
//            //Debug.Log($"Cam1 Height {m_cameraManager.AvailableCameras[0].pixelRect.height} -  Width {m_cameraManager.AvailableCameras[0].pixelRect.width}");
//            //Debug.Log($"Cam2 Height {m_cameraManager.AvailableCameras[1].pixelRect.height} -  Width {m_cameraManager.AvailableCameras[1].pixelRect.width}");
//            //Debug.Log($"Cam3 Height {m_cameraManager.AvailableCameras[2].pixelRect.height} -  Width {m_cameraManager.AvailableCameras[2].pixelRect.width}");
//            //Debug.Log($"Cam4 Height {m_cameraManager.AvailableCameras[3].pixelRect.height} -  Width {m_cameraManager.AvailableCameras[3].pixelRect.width}");
//#endif
//        }

//        /// <summary>
//        /// new Rect(xOrigin, yOrigin, width, height)
//        /// </summary>
//        private void UpdateFullsizeRect()
//        {
//            switch (m_lastSetCameraMode)
//            {
//                case ECameraModi.SingleCam:
//                {
//                    CameraManager.RuntimeFullsizeRect = m_cameraManager.AvailableCameras[0].pixelRect;
//                    break;
//                }
//                case ECameraModi.TwoVertical:
//                {
//                    CameraManager.RuntimeFullsizeRect = new Rect(0, 0, m_cameraManager.AvailableCameras[0].pixelRect.width + m_cameraManager.AvailableCameras[1].pixelRect.width, m_cameraManager.AvailableCameras[0].pixelRect.height);
//                    break;
//                }
//                case ECameraModi.TwoHorizontal:
//                {
//                    CameraManager.RuntimeFullsizeRect = new Rect(0, 0, m_cameraManager.AvailableCameras[0].pixelRect.width, m_cameraManager.AvailableCameras[0].pixelRect.height + m_cameraManager.AvailableCameras[1].pixelRect.height);
//                    break;
//                }
//                case ECameraModi.FourSplit:
//                {
//                    CameraManager.RuntimeFullsizeRect = new Rect(0, 0, m_cameraManager.AvailableCameras[0].pixelRect.width + m_cameraManager.AvailableCameras[1].pixelRect.width, m_cameraManager.AvailableCameras[0].pixelRect.height + m_cameraManager.AvailableCameras[1].pixelRect.height);
//                    break;
//                }
//                default:    //TwoHorizontal
//                {
//                    CameraManager.RuntimeFullsizeRect = new Rect(0, 0, m_cameraManager.AvailableCameras[0].pixelRect.width, m_cameraManager.AvailableCameras[0].pixelRect.height + m_cameraManager.AvailableCameras[1].pixelRect.height);
//                    break;
//                }
//            }
//        }

//        private void SetSingleCamera()
//        {
//            SetCamerasAndRects();
//        }

//        private void SetCamerasHorizontal(Camera _camera1, Camera _camera2)
//        {
//            SetCamerasAndRects();

//            CamRectOut(_camera1, _camera2, out float Cam1X, out float Cam1Y, out float Cam1W, out float Cam1H, out float Cam2X, out float Cam2Y, out float Cam2W, out float Cam2H);

//            _camera1.pixelRect = new Rect(Cam1X, Cam1Y, Cam1W * m_FullWidthHor, Cam1H * m_HalfHeightHor);
//            m_cameraManager.UpdateRectDimensions(_camera1, _camera1.pixelRect);

//            Cam2Y += Cam2H * m_HalfHeightHor;
//            _camera2.pixelRect = new Rect(Cam2X, Cam2Y, Cam2W * m_FullWidthHor, Cam2H * m_HalfHeightHor);
//            m_cameraManager.UpdateRectDimensions(_camera2, _camera2.pixelRect);
//        }

//        private void SetCamerasVertical(Camera _camera1, Camera _camera2)
//        {
//            SetCamerasAndRects();

//            CamRectOut(_camera1, _camera2, out float Cam1X, out float Cam1Y, out float Cam1W, out float Cam1H, out float Cam2X, out float Cam2Y, out float Cam2W, out float Cam2H);

//            _camera1.pixelRect = new Rect(Cam1X, Cam1Y, Cam1W * m_HalfWidthVer, Cam1H * m_FullHeightVer);
//            m_cameraManager.UpdateRectDimensions(_camera1, _camera1.pixelRect);

//            Cam2X += Cam2W * m_HalfWidthVer;
//            _camera2.pixelRect = new Rect(Cam2X, Cam2Y, Cam2W * m_HalfWidthVer, Cam2H * m_FullHeightVer);
//            m_cameraManager.UpdateRectDimensions(_camera2, _camera2.pixelRect);
//        }

//        private void SetFourSplit(Camera _camera1, Camera _camera2, Camera _camera3, Camera _camera4)
//        {
//            SetCamerasAndRects();

//            CamRectOut(_camera1, _camera2, out float Cam1X, out float Cam1Y, out float Cam1W, out float Cam1H, out float Cam2X, out float Cam2Y, out float Cam2W, out float Cam2H);

//            float Cam3X = _camera3.pixelRect.x;
//            float Cam3Y = _camera3.pixelRect.y;
//            float Cam3W = _camera3.pixelRect.width;
//            float Cam3H = _camera3.pixelRect.height;

//            float Cam4X = _camera4.pixelRect.x;
//            float Cam4Y = _camera4.pixelRect.y;
//            float Cam4W = _camera4.pixelRect.width;
//            float Cam4H = _camera4.pixelRect.height;

//            _camera1.pixelRect = new Rect(Cam1X, Cam1Y, Cam1W * m_HalfWidthVer, Cam1H * m_HalfHeightHor);
//            m_cameraManager.UpdateRectDimensions(_camera1, _camera1.pixelRect);

//            Cam2X += Cam2W * m_HalfWidthVer;
//            _camera2.pixelRect = new Rect(Cam2X, Cam2Y, Cam2W * m_HalfWidthVer, Cam2H * m_HalfHeightHor);
//            m_cameraManager.UpdateRectDimensions(_camera2, _camera2.pixelRect);

//            Cam3Y += Cam3H * m_HalfHeightHor;
//            _camera3.pixelRect = new Rect(Cam3X, Cam3Y, Cam3W * m_HalfWidthVer, Cam3H * m_HalfHeightHor);
//            m_cameraManager.UpdateRectDimensions(_camera3, _camera3.pixelRect);

//            Cam4X += Cam4W * m_HalfWidthVer;
//            Cam4Y += Cam4H * m_HalfHeightHor;
//            _camera4.pixelRect = new Rect(Cam4X, Cam4Y, Cam4W * m_HalfWidthVer, Cam4H * m_HalfHeightHor);
//            m_cameraManager.UpdateRectDimensions(_camera4, _camera4.pixelRect);
//        }

//        private void CamRectOut(Camera _camera1, Camera _camera2, out float Cam1X, out float Cam1Y, out float Cam1W, out float Cam1H, out float Cam2X, out float Cam2Y, out float Cam2W, out float Cam2H)
//        {
//            Cam1X = _camera1.pixelRect.x;
//            Cam1Y = _camera1.pixelRect.y;
//            Cam1W = _camera1.pixelRect.width;
//            Cam1H = _camera1.pixelRect.height;
//            Cam2X = _camera2.pixelRect.x;
//            Cam2Y = _camera2.pixelRect.y;
//            Cam2W = _camera2.pixelRect.width;
//            Cam2H = _camera2.pixelRect.height;
//        }

//        private void SetCamerasAndRects()
//        {
//            //switch (_cameraMode) for each ECameraModi with 'ReSetViewportRects(m_cameraManager.AvailableCameras[0-3]), 'null checks', 'SetActive' and updating 'pixelRect' replaced by this for loop.
//            #region Camera nullCheck with for loop
//            for (int i = 0; i < m_cameraManager.AvailableCameras.Count; i++)
//            {
//                if (m_cameraManager.AvailableCameras[i] != null)
//                {
//                    m_cameraManager.AvailableCameras[i].gameObject.SetActive(true);

//                    if (m_cameraManager.AvailableCameras[i].pixelRect != CameraManager.RuntimeFullsizeRect)
//                        m_cameraManager.UpdateRectDimensions(m_cameraManager.AvailableCameras[i], CameraManager.RuntimeFullsizeRect);

//                    m_cameraManager.AvailableCameras[i].pixelRect = CameraManager.RuntimeFullsizeRect;
//                }
//            }
//            #endregion
//        }
    }
}