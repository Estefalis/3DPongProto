using System.Collections.Generic;
using UnityEngine;

namespace ThreeDeePongProto.Managers
{
    public class CameraManager : MonoBehaviour
    {
        public List<Camera> AvailableCameras { get => m_availableCameras; }
        [SerializeField] private List<Camera> m_availableCameras = new();

        public Dictionary<Camera, Rect> CameraRectDict { get => m_cameraRectDict; }
        private Dictionary<Camera, Rect> m_cameraRectDict = new();

        //public List<Camera> SeparatedDictCameras { get => m_dictCameras; }
        private List<Camera> m_dictCameras = new();

        public List<Rect> SeparatedDictsRects { get => m_dictRects; }
        private List<Rect> m_dictRects = new();

        private void Awake()
        {
            if (m_availableCameras.Count > 0)
            {
                for (int i = 0; i < m_availableCameras.Count; i++)
                {
                    m_cameraRectDict.Add(m_availableCameras[i], m_availableCameras[i].pixelRect);
                }

                UpdateSplittedCamRectLists();
            }
        }

        private void Update()
        {
            //Debug.Log("Dictionary Rect 0: " + m_cameraRectDict[m_availableCameras[0]]);
            //Debug.Log("Dictionary Rect 1: " + m_cameraRectDict[m_availableCameras[1]]);
            //Debug.Log("Dictionary Rect 2: " + m_cameraRectDict[m_availableCameras[2]]);
            //Debug.Log("Dictionary Rect 3: " + m_cameraRectDict[m_availableCameras[3]]);

            //Debug.Log($"{m_dictCameras[0]} + {m_dictRects[0]}");
            //Debug.Log($"{m_dictCameras[1]} + {m_dictRects[1]}");
            //Debug.Log($"{m_dictCameras[2]} + {m_dictRects[2]}");
            //Debug.Log($"{m_dictCameras[3]} + {m_dictRects[3]}");
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

        public void UpdateRectDimensions(Camera _keyCamera, Rect _newValueRect)
        {
            //Update the 'pair.Value' of the 'pair.Key _keyCamera'.
            m_cameraRectDict[_keyCamera] = _newValueRect;
            //And additionally clear and update the separated Lists of Camera and Rects. (It's easier to do/follow, if we do it here.)
            UpdateSplittedCamRectLists();
#if UNITY_EDITOR
            Debug.Log("Adjustment in CameraManager: " + m_cameraRectDict[_keyCamera]);
#endif
        }

        public void UpdateSplittedCamRectLists()
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
        #endregion

        //Methode-Variant of the public 'CameraRectDict'-property.
        //public Dictionary<Camera, Rect> GetDictionary()
        //{
        //    return m_cameraRectDict;
        //}
    }
}