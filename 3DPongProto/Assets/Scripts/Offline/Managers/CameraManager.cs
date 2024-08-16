using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeDeePongProto.Offline.CameraSetup
{
    public class CameraManager : MonoBehaviour
    {
        public List<Camera> AvailableCameras { get => m_availableCameras; }
        [SerializeField] private List<Camera> m_availableCameras = new();

        [SerializeField] private MatchValues m_matchValues;

        //public Dictionary<Camera, Rect> CameraRectDict { get => m_cameraRectDict; }
        private Dictionary<Camera, Rect> m_cameraRectDict = new();

        //public List<Camera> SeparatedDictCameras { get => m_dictCameras; }
        private List<Camera> m_dictCameras = new();
        //public List<Rect> SeparatedDictsRects { get => m_dictRects; }
        private List<Rect> m_dictRects = new();

        private static Queue<Camera> m_QRegisterCamera;
        private static Queue<Camera> m_QRemoveCamera;
        private static event Action<Queue<Camera>, int> m_ARegisterCamera;
        private static event Action<Queue<Camera>, int> m_ARemoveCamera;

        internal static Rect RuntimeFullsizeRect { get; set; }

        private void Awake()
        {
            m_ARegisterCamera += AddIncomingCamera;
            m_ARemoveCamera += RemoveAddedCameras;

            m_QRegisterCamera = new();
            m_QRemoveCamera = new();
        }

        private IEnumerator Start()
        {
            yield return new WaitUntil(CamerasEqualPlayerCount);

            if (m_availableCameras.Count > 0)
            {
                for (int i = 0; i < m_availableCameras.Count; i++)
                {
                    if (!m_cameraRectDict.ContainsKey(m_availableCameras[i]))
                        m_cameraRectDict.Add(m_availableCameras[i], m_availableCameras[i].pixelRect);
                }

                UpdateSplittedCamRectLists();
            }
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

        private void OnDisable()
        {
            m_ARegisterCamera -= AddIncomingCamera;
            m_ARemoveCamera -= RemoveAddedCameras;
        }

        #region CameraRect Stats Example
        //Single Camera
        //Rect0(x:0.00,   y:0.00,   width:506.00, height:285.00)  -  Fullsize in width and height
        //Rect1(x:0.00,   y:0.00,   width:506.00, height:285.00)  -  Fullsize in width and height

        //Two Horizontal
        //Rect0(x:0.00,   y:142.50, width:506.00, height:142.50)  -  Fullsize width and halfsize height
        //Rect1(x:0.00,   y:142.50, width:506.00, height:142.50)  -  Fullsize width and halfsize height

        //Two Vertical
        //Rect0(x:0.00,   y:0.00,   width:253.00, height:285.00)  -  Halfsize width and fullsize height
        //Rect1(x:253.00, y:0.00,   width:253.00, height:285.00)  -  Halfsize width and fullsize height

        //Four Split
        //Rect0(x:0.00,   y:0.00,   width:253.00, height:142.50)  -  Halfsize width and fullsize height
        //Rect1(x:253.00, y:0.00,   width:253.00, height:142.50)  -  Halfsize width and fullsize height
        //Rect2(x:0.00,   y:142.50, width:253.00, height:142.50)  -  Halfsize width and halfsize height
        //Rect3(x:253.00, y:142.50, width:253.00, height:142.50)  -  Halfsize width and halfsize height
        #endregion

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
            UpdateSplittedCamRectLists();
        }

        private void UpdateSplittedCamRectLists()
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
    }
}