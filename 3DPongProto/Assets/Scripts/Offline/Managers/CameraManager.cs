using System;
using System.Collections;
using System.Collections.Generic;
using ThreeDeePongProto.Offline.Settings;
using UnityEngine;

namespace ThreeDeePongProto.Offline.CameraSetup
{
    public class CameraManager : MonoBehaviour
    {
        #region SerializeField-Member-Variables
        public List<Camera> AvailableCameras { get => m_availableCameras; }
        [SerializeField] private List<Camera> m_availableCameras = new();
        #endregion

        #region Lists and Dictionaries
        private readonly List<Camera> m_dictCameras = new();
        private readonly List<Rect> m_dictRects = new();
        private readonly Dictionary<Camera, Rect> m_cameraRectDict = new();
        #endregion

        #region DeRegister Cameras with Queues and Actions
        private static Queue<Camera> m_QRegisterCamera;
        private static Queue<Camera> m_QRemoveCamera;
        private static event Action<Queue<Camera>, int> m_ARegisterCamera;
        private static event Action<Queue<Camera>, int> m_ARemoveCamera;
        #endregion

        private const float m_FullWidthHor = 1.0f;
        private const float m_FullHeightVer = 1.0f;
        private const float m_HalfHeightHor = 0.5f;
        private const float m_HalfWidthVer = 0.5f;

        private ECameraModi m_lastSetCameraMode;
        internal static Rect RuntimeFullsizeRect { get; private set; }

        #region Scriptable Objects
        [SerializeField] private GraphicSettings m_graphicsSettings;
        [SerializeField] private GraphicUIStates m_graphicUIStates;
        [SerializeField] private MatchValues m_matchValues;
        #endregion

        private void Awake()
        {
            m_ARegisterCamera += AddIncomingCamera;
            m_ARemoveCamera += RemoveAddedCameras;

            m_QRegisterCamera = new();
            m_QRemoveCamera = new();
        }

        private void OnDisable()
        {
            m_ARegisterCamera -= AddIncomingCamera;
            m_ARemoveCamera -= RemoveAddedCameras;
        }

        private IEnumerator Start()
        {
            if (m_graphicUIStates == null)
            {
                m_lastSetCameraMode = m_graphicsSettings.ECameraMode;
            }
            else
                m_lastSetCameraMode = m_graphicUIStates.SetCameraMode;

            yield return new WaitUntil(CamerasEqualPlayerCount);

            if (m_availableCameras.Count > 0)
            {
                for (int i = 0; i < m_availableCameras.Count; i++)
                {
                    if (!m_cameraRectDict.ContainsKey(m_availableCameras[i]))
                        m_cameraRectDict.Add(m_availableCameras[i], m_availableCameras[i].pixelRect);
                }

                UpdateCamRectDicts();
            }

            SetCameraMode(m_lastSetCameraMode);

            UpdateFullsizeRect();
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
        }

        /// <summary>
        /// Only returns true, after the activated PlayerCameras added themselves to the 'AvailableCameras'-List are equal to the registered PlayerIDData.
        /// </summary>
        /// <returns></returns>
        private bool CamerasEqualPlayerCount()
        {
            switch (AvailableCameras.Count == m_matchValues.PlayerData.Count)
            {
                case true:
                    return true;
                case false:
                    return false;
            }
        }

        #region Register and Add Cameras
        public static void LetsRegisterCameras(Camera _camera, int _cameraID)
        {
            m_QRegisterCamera.Enqueue(_camera);
            m_ARegisterCamera?.Invoke(m_QRegisterCamera, _cameraID);
        }

        private void AddIncomingCamera(Queue<Camera> _queue, int _cameraID)
        {
            if (_queue.Count > 0)
            {
                Camera camera = _queue.Peek();
                m_availableCameras.Add(camera);
                _queue.Dequeue();
            }
        }
        #endregion

        #region DeRegister and Remove Cameras
        public static void LetsRemoveCamera(Camera _camera, int _cameraID)
        {
            m_QRemoveCamera.Enqueue(_camera);
            m_ARemoveCamera?.Invoke(m_QRemoveCamera, _cameraID);
        }

        private void RemoveAddedCameras(Queue<Camera> _queue, int _cameraID)
        {
            Camera camera = _queue.Peek();

            if (m_availableCameras.Contains(camera))
            {
                m_availableCameras.Remove(camera);
                _queue.Dequeue();
            }
        }
        #endregion

        public void UpdateRectDimensions(Camera _keyCamera, Rect _newValueRect)
        {
            //Update the 'keyValuePair.Value' of the 'keyValuePair.Key _keyCamera'.
            m_cameraRectDict[_keyCamera] = _newValueRect;
            //And additionally clear and update the separated Lists of Camera and Rects. (It's easier to do/follow, if we do it here.)
            UpdateCamRectDicts();
        }

        private void UpdateCamRectDicts()
        {
            m_dictCameras.Clear();
            m_dictRects.Clear();

            foreach (var keyValuePair in m_cameraRectDict)
            {
                Camera camera = keyValuePair.Key;
                Rect rect = keyValuePair.Value;

                m_dictCameras.Add(camera);
                m_dictRects.Add(rect);
            }
        }

        #region Set Cameras and Rects (Old SetCameraRects.cs)
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
                    if (AvailableCameras[1] != null && m_lastSetCameraMode == ECameraModi.TwoVertical)
                        SetCamerasVertical(AvailableCameras[0], AvailableCameras[1]);
                    break;
                }
                case ECameraModi.TwoHorizontal:
                {
                    if (AvailableCameras[1] != null && m_lastSetCameraMode == ECameraModi.TwoHorizontal)
                        SetCamerasHorizontal(AvailableCameras[0], AvailableCameras[1]);
                    break;
                }
                case ECameraModi.FourSplit:
                {
                    if (AvailableCameras[2] != null && AvailableCameras[3] != null && m_lastSetCameraMode == ECameraModi.FourSplit)
                        SetFourSplit(AvailableCameras[0], AvailableCameras[1], AvailableCameras[2], AvailableCameras[3]);
                    break;
                }
                default:    //TwoHorizontal
                    m_lastSetCameraMode = ECameraModi.TwoHorizontal;
                    if (AvailableCameras[1] != null && m_lastSetCameraMode == ECameraModi.TwoHorizontal)
                        SetCamerasHorizontal(AvailableCameras[0], AvailableCameras[1]);
                    break;
            }
        }

        /// <summary>
        /// new Rect(xOrigin, yOrigin, width, height). Separated from 'SetCameraMode' because it gets continuously 'Update()'d.
        /// </summary>
        private void UpdateFullsizeRect()
        {
            switch (m_lastSetCameraMode)
            {
                case ECameraModi.SingleCam:
                {
                    RuntimeFullsizeRect = AvailableCameras[0].pixelRect;
                    break;
                }
                case ECameraModi.TwoVertical:
                {
                    RuntimeFullsizeRect = new Rect(0, 0, AvailableCameras[0].pixelRect.width + AvailableCameras[1].pixelRect.width, AvailableCameras[0].pixelRect.height);
                    break;
                }
                case ECameraModi.TwoHorizontal:
                {
                    RuntimeFullsizeRect = new Rect(0, 0, AvailableCameras[0].pixelRect.width, AvailableCameras[0].pixelRect.height + AvailableCameras[1].pixelRect.height);
                    break;
                }
                case ECameraModi.FourSplit:
                {
                    RuntimeFullsizeRect = new Rect(0, 0, AvailableCameras[0].pixelRect.width + AvailableCameras[1].pixelRect.width, AvailableCameras[0].pixelRect.height + AvailableCameras[1].pixelRect.height);
                    break;
                }
                default:    //TwoHorizontal
                {
                    RuntimeFullsizeRect = new Rect(0, 0, AvailableCameras[0].pixelRect.width, AvailableCameras[0].pixelRect.height + AvailableCameras[1].pixelRect.height);
                    break;
                }
            }
        }

        private void SetCamerasAndRects()
        {
            #region Camera nullCheck with for loop
            for (int i = 0; i < AvailableCameras.Count; i++)
            {
                if (AvailableCameras[i] != null)
                {
                    AvailableCameras[i].gameObject.SetActive(true);

                    if (AvailableCameras[i].pixelRect != RuntimeFullsizeRect)
                        UpdateRectDimensions(AvailableCameras[i], RuntimeFullsizeRect);

                    AvailableCameras[i].pixelRect = RuntimeFullsizeRect;
                }
            }
            #endregion
        }

        private void SetSingleCamera()
        {
            SetCamerasAndRects();
        }

        private void SetCamerasHorizontal(Camera _camera1, Camera _camera2)
        {
            SetCamerasAndRects();

            CamRectOut(_camera1, _camera2, out float Cam1X, out float Cam1Y, out float Cam1W, out float Cam1H, out float Cam2X, out float Cam2Y, out float Cam2W, out float Cam2H);

            _camera1.pixelRect = new Rect(Cam1X, Cam1Y, Cam1W * m_FullWidthHor, Cam1H * m_HalfHeightHor);
            UpdateRectDimensions(_camera1, _camera1.pixelRect);

            Cam2Y += Cam2H * m_HalfHeightHor;
            _camera2.pixelRect = new Rect(Cam2X, Cam2Y, Cam2W * m_FullWidthHor, Cam2H * m_HalfHeightHor);
            UpdateRectDimensions(_camera2, _camera2.pixelRect);
        }

        private void SetCamerasVertical(Camera _camera1, Camera _camera2)
        {
            SetCamerasAndRects();

            CamRectOut(_camera1, _camera2, out float Cam1X, out float Cam1Y, out float Cam1W, out float Cam1H, out float Cam2X, out float Cam2Y, out float Cam2W, out float Cam2H);

            _camera1.pixelRect = new Rect(Cam1X, Cam1Y, Cam1W * m_HalfWidthVer, Cam1H * m_FullHeightVer);
            UpdateRectDimensions(_camera1, _camera1.pixelRect);

            Cam2X += Cam2W * m_HalfWidthVer;
            _camera2.pixelRect = new Rect(Cam2X, Cam2Y, Cam2W * m_HalfWidthVer, Cam2H * m_FullHeightVer);
            UpdateRectDimensions(_camera2, _camera2.pixelRect);
        }

        private void SetFourSplit(Camera _camera1, Camera _camera2, Camera _camera3, Camera _camera4)
        {
            SetCamerasAndRects();

            CamRectOut(_camera1, _camera2, out float Cam1X, out float Cam1Y, out float Cam1W, out float Cam1H, out float Cam2X, out float Cam2Y, out float Cam2W, out float Cam2H);

            float Cam3X = _camera3.pixelRect.x;
            float Cam3Y = _camera3.pixelRect.y;
            float Cam3W = _camera3.pixelRect.width;
            float Cam3H = _camera3.pixelRect.height;

            float Cam4X = _camera4.pixelRect.x;
            float Cam4Y = _camera4.pixelRect.y;
            float Cam4W = _camera4.pixelRect.width;
            float Cam4H = _camera4.pixelRect.height;

            _camera1.pixelRect = new Rect(Cam1X, Cam1Y, Cam1W * m_HalfWidthVer, Cam1H * m_HalfHeightHor);
            UpdateRectDimensions(_camera1, _camera1.pixelRect);

            Cam2X += Cam2W * m_HalfWidthVer;
            _camera2.pixelRect = new Rect(Cam2X, Cam2Y, Cam2W * m_HalfWidthVer, Cam2H * m_HalfHeightHor);
            UpdateRectDimensions(_camera2, _camera2.pixelRect);

            Cam3Y += Cam3H * m_HalfHeightHor;
            _camera3.pixelRect = new Rect(Cam3X, Cam3Y, Cam3W * m_HalfWidthVer, Cam3H * m_HalfHeightHor);
            UpdateRectDimensions(_camera3, _camera3.pixelRect);

            Cam4X += Cam4W * m_HalfWidthVer;
            Cam4Y += Cam4H * m_HalfHeightHor;
            _camera4.pixelRect = new Rect(Cam4X, Cam4Y, Cam4W * m_HalfWidthVer, Cam4H * m_HalfHeightHor);
            UpdateRectDimensions(_camera4, _camera4.pixelRect);
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
        #endregion
    }
}