using System.Collections.Generic;
using UnityEngine;

namespace ThreeDeePongProto.Managers
{
    public class CameraManager : MonoBehaviour
    {
        public List <Camera> AvailableCameras { get => m_availableCameras; }
        [SerializeField] private List<Camera> m_availableCameras = new();

        public Dictionary<Camera, Rect> CameraRectDict { get => m_cameraRectDict; }
        private Dictionary<Camera, Rect> m_cameraRectDict = new();

        private void Awake()
        {
            if (m_availableCameras.Count > 0)
            {
                for (int i = 0; i < m_availableCameras.Count; i++)
                {
                    m_cameraRectDict.Add(m_availableCameras[i], m_availableCameras[i].pixelRect);
                }
            }
        }

        #region CameraRect Stats
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

        //private void Update()
        //{
        //    Debug.Log("Dictionary Rect 0: " + m_cameraRectDict[m_availableCameras[0]]);
        //    Debug.Log("Dictionary Rect 1: " + m_cameraRectDict[m_availableCameras[1]]);
        //    Debug.Log("Dictionary Rect 2: " + m_cameraRectDict[m_availableCameras[2]]);
        //    Debug.Log("Dictionary Rect 3: " + m_cameraRectDict[m_availableCameras[3]]);
        //}

        public void UpdateRectDimensions(Camera _keyCamera, Rect _newValueRect)
        {
            m_cameraRectDict[_keyCamera] = _newValueRect;
#if UNITY_EDITOR
            Debug.Log("Adjustment in CameraManager: " + m_cameraRectDict[_keyCamera]);
#endif
        }

        //public Dictionary<Camera, Rect> GetDictionary()
        //{
        //    return m_cameraRectDict;
        //}
    }
}