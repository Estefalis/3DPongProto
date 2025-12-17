using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThreeDeePongProto.Shared.Managers;
using ThreeDeePongProto.Shared.PlayerCharacter;
using UnityEngine;

namespace ThreeDeePongProto.Offline.CameraSetup
{
    public class CameraManager : PersistentSingleton<CameraManager>
    {
        public List<Camera> AvailableCameras { get => m_availableCameras; }
        [SerializeField] private List<Camera> m_availableCameras = new();

        public static event Action InitializeMatchUI;       //Tells MatchUserInterface to initialize it's UI.     

        protected override void Awake()
        {
            base.Awake();
        }

        private IEnumerator Start()
        {
            yield return new WaitUntil(CamerasEqualPlayerCount);

            ApplyCameraLayout();
        }

        /// <summary>
        /// Central Methode to set the SplitScreen-Layouts.
        /// </summary>
        public void ApplyCameraLayout()
        {
            //Get the current Settings from the SettingsManager.
            var graphicData = SettingsManager.Instance.CurrentSettings.Graphic;
            var matchData = SettingsManager.Instance.CurrentSettings.Match;

            //Deactivate and reset all Cameras into a Start-state
            foreach (var cam in m_availableCameras)
            {
                cam.gameObject.SetActive(false);
            }

            switch (matchData.PlayerCount)
            {
                case 1:
                {
                    if (m_availableCameras.Count > 0)
                    {
                        m_availableCameras[0].gameObject.SetActive(true);
                        m_availableCameras[0].rect = new Rect(0, 0, 1, 1); //Fullscreen
                    }
                    break;
                }
                case 2:
                {
                    //Apply the Layout based on the splitDdValue
                    switch (graphicData.splitDdValue)
                    {
                        case 0:
                        {
                            //Two Player. Vertical split
                            if (m_availableCameras.Count > 1)
                            {
                                m_availableCameras[0].gameObject.SetActive(true);
                                m_availableCameras[0].rect = new Rect(0, 0, 0.5f, 1); //Left half

                                m_availableCameras[1].gameObject.SetActive(true);
                                m_availableCameras[1].rect = new Rect(0.5f, 0, 0.5f, 1); //Right half
                            }
                            break;
                        }
                        default:
                        case 1:
                        {
                            //Two Player. Horizontal split
                            if (m_availableCameras.Count > 1)
                            {
                                m_availableCameras[0].gameObject.SetActive(true);
                                m_availableCameras[0].rect = new Rect(0, 0, 1, 0.5f); //Lower half

                                m_availableCameras[1].gameObject.SetActive(true);
                                m_availableCameras[1].rect = new Rect(0, 0.5f, 1, 0.5f); //Upper half
                            }
                            break;
                        }
                    }
                    break;
                }
                case 4:
                {
                    //Four Player. Four Split
                    if (m_availableCameras.Count > 3)
                    {
                        m_availableCameras[0].gameObject.SetActive(true);
                        m_availableCameras[0].rect = new Rect(0, 0, 0.5f, 0.5f); //Lower left (P1)

                        m_availableCameras[1].gameObject.SetActive(true);
                        m_availableCameras[1].rect = new Rect(0.5f, 0, 0.5f, 0.5f); //Lower right (P2)

                        m_availableCameras[2].gameObject.SetActive(true);
                        m_availableCameras[2].rect = new Rect(0, 0.5f, 0.5f, 0.5f); //Upper left (P3)

                        m_availableCameras[3].gameObject.SetActive(true);
                        m_availableCameras[3].rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f); //Upper right (P4)
                    }
                    break;
                }
                default:
                {
                    if (m_availableCameras.Count > 0)
                    {
                        m_availableCameras[0].gameObject.SetActive(true);
                        m_availableCameras[0].rect = new Rect(0, 0, 1, 1); //Fullscreen
                    }
                    break;
                }
            }

            InitializeMatchUI?.Invoke();
        }

        /// <summary>
        /// Only returns true, after the activated PlayerCameras added themselves to the 'AvailableCameras'-List are equal to the registered PlayerSOData.
        /// </summary>
        /// <returns></returns>
        private bool CamerasEqualPlayerCount()
        {
            return AvailableCameras.Count == SettingsManager.Instance.CurrentSettings.Match.PlayerCount;
        }

        internal void RegisterCamera(Camera _camera)
        {
            #region Old Code
            ////if (_queue.Count > 0)
            ////{
            ////    Camera camera = _queue.Peek();
            //m_availableCameras.Add(_camera);
            ////    _queue.Dequeue();
            ////}
            #endregion
            if (!m_availableCameras.Contains(_camera))
            {
                m_availableCameras.Add(_camera);
                m_availableCameras.OrderBy(camera => camera.GetComponentInParent<CharacterCameraController>().m_playerID).ToList();
            }
        }

        internal void RemoveCamera(Camera _camera)
        {
            if (m_availableCameras.Contains(_camera))
                m_availableCameras.Remove(_camera);
        }
    }
}