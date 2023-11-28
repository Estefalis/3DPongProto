using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeDeePongProto.Offline.CameraSetup
{
    public class CameraManager : MonoBehaviour
    {
        public List<Camera> AvailableCameras { get => m_availableCameras; }
        [SerializeField] private List<Camera> m_availableCameras = new();
        //public List<Camera> AvailableCameras { get => m_availableCameras; }
        //[SerializeField] private List<Camera> m_availableCameras = new();

        public Dictionary<Camera, Rect> CameraRectDict { get => m_cameraRectDict; }
        private Dictionary<Camera, Rect> m_cameraRectDict = new();

        //public List<Camera> SeparatedDictCameras { get => m_dictCameras; }
        private List<Camera> m_dictCameras = new();

        //public List<Rect> SeparatedDictsRects { get => m_dictRects; }
        private List<Rect> m_dictRects = new();

        private static Queue<Camera> m_QueueRegisterCamera;
        private static Queue<Camera> m_QueueRemoveCamera;
        private static event Action<Queue<Camera>, int> m_ActionRegisterCamera;
        private static event Action<Queue<Camera>, int> m_ActionRemoveCamera;

        public static Rect RuntimeFullsizeRect { get; set; }

        private void Awake()
        {
            m_ActionRegisterCamera += AddIncomingCamera;
            m_ActionRemoveCamera += RemoveAddedCameras;

            m_QueueRegisterCamera = new();
            m_QueueRemoveCamera = new();

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

        private void OnDisable()
        {
            m_ActionRegisterCamera -= AddIncomingCamera;
            m_ActionRemoveCamera -= RemoveAddedCameras;
        }

#if UNITY_EDITOR
        private void Update()
        {
            //Debug.Log("Dictionary Rect 0: " + m_cameraRectDict[m_availableCameras[0]]);
            //Debug.Log("Dictionary Rect 1: " + m_cameraRectDict[m_availableCameras[1]]);
            //Debug.Log("Dictionary Rect 2: " + m_cameraRectDict[m_availableCameras[2]]);
            //Debug.Log("Dictionary Rect 3: " + m_cameraRectDict[m_availableCameras[3]]);

            //Debug.Log($"DictCamera 0 {m_dictCameras[0]} + DictRects 0 {m_dictRects[0]}");
            //Debug.Log($"DictCamera 1 {m_dictCameras[1]} + DictRects 1 {m_dictRects[1]}");
            //Debug.Log($"DictCamera 2 {m_dictCameras[2]} + DictRects 2 {m_dictRects[2]}");
            //Debug.Log($"DictCamera 3 {m_dictCameras[3]} + DictRects 3 {m_dictRects[3]}");
        }
#endif

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

        public static void LetsRegisterCameras(Camera _camera, int _cameraID)
        {
            m_QueueRegisterCamera.Enqueue(_camera);
            m_ActionRegisterCamera?.Invoke(m_QueueRegisterCamera, _cameraID);
        }

        private void AddIncomingCamera(Queue<Camera> _queue, int _cameraID)
        {
            if (_queue.Count > 0)
            {
                Camera camera = _queue.Peek();
                //m_availableCameras[_cameraID] = camera;
                m_availableCameras.Add(camera);
                _queue.Dequeue();
            }
        }

        public static void LetsRemoveCamera(Camera _camera, int _cameraID)
        {
            m_QueueRemoveCamera.Enqueue(_camera);
            m_ActionRemoveCamera?.Invoke(m_QueueRemoveCamera, _cameraID);
        }

        private void RemoveAddedCameras(Queue<Camera> _queue, int _cameraID)
        {
            Camera camera = _queue.Peek();

            if (m_availableCameras.Contains(camera))
            {
                //m_availableCameras[_cameraID] = null;
                m_availableCameras.Remove(camera);
                _queue.Dequeue();
            }
        }

        public void UpdateRectDimensions(Camera _keyCamera, Rect _newValueRect)
        {
            //Update the 'pair.Value' of the 'pair.Key _keyCamera'.
            m_cameraRectDict[_keyCamera] = _newValueRect;
            //And additionally clear and update the separated Lists of Camera and Rects. (It's easier to do/follow, if we do it here.)
            UpdateSplittedCamRectLists();
        }

        private void UpdateSplittedCamRectLists()
        {
            m_dictCameras.Clear();
            m_dictRects.Clear();

            foreach (var pair in m_cameraRectDict)
            {
                Camera camera = pair.Key;
                Rect rect = pair.Value;

                m_dictCameras.Add(camera);
                m_dictRects.Add(rect);
            }
        }

        //Methode-Variant of the public 'CameraRectDict'-property.
        //public Dictionary<Camera, Rect> GetDictionary()
        //{
        //    return m_cameraRectDict;
        //}
    }
}