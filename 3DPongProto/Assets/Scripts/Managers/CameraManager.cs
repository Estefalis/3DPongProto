using System.Collections.Generic;
using UnityEngine;

namespace ThreeDeePongProto.Managers
{
    public class CameraManager : MonoBehaviour
    {
        [SerializeField] private List<Camera> m_availableCameras = new();

        public Dictionary<Camera, Rect> CameraRects { get => m_cameraRectDict; }
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

        private void Update()
        {
            Debug.Log("Dictionary Rect 0: " + m_cameraRectDict[m_availableCameras[0]]);
            Debug.Log("Dictionary Rect 1: " + m_cameraRectDict[m_availableCameras[1]]);
            Debug.Log("Dictionary Rect 2: " + m_cameraRectDict[m_availableCameras[2]]);
            Debug.Log("Dictionary Rect 3: " + m_cameraRectDict[m_availableCameras[3]]);
        }

        public void UpdateRectDimensions(Camera _keyCamera, Rect _newValueRect)
        {
            m_cameraRectDict[_keyCamera] = _newValueRect;
#if UNITY_EDITOR
            //Debug.Log("CameraManager: " + m_cameraRectDict[_keyCamera]);
#endif
        }
    }
}