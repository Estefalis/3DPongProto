using System.Collections.Generic;
using ThreeDeePongProto.Offline.CameraSetup;
using ThreeDeePongProto.Shared.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeDeePongProto.Offline.UI
{
    public class MatchUserInterface : MonoBehaviour
    {
        #region Script-References
        [SerializeField] private LocalMatchManager m_matchManager;
        [SerializeField] private CameraManager m_cameraManager;
        #endregion

        #region UI-References
        [Header("Round Details")]
        [SerializeField] private TextMeshProUGUI m_roundNr;
        [SerializeField] private TextMeshProUGUI m_elapsedTime;
        [SerializeField] private TextMeshProUGUI m_pointsVsPoints;

        [Header("Player Informations")]
        [SerializeField] private List<Transform> m_playerDetailsParent;
        [SerializeField] private List<TextMeshProUGUI> m_playerNames;
        [SerializeField] private List<TextMeshProUGUI> m_playerTotalPoints;
        [SerializeField] private List<GameObject> m_playerAvatarList = new();

        [Header("Music-Details")]
        [SerializeField] private TextMeshProUGUI m_artistNames;
        [SerializeField] private TextMeshProUGUI m_songTitle;
        [SerializeField] private Image m_songImage;
        #endregion

        [Header("Count Setup & UI Positioning")]
        //[SerializeField] private bool m_timerShallCountUp = true;
        [SerializeField] private bool m_showMilliseconds = false;
        [SerializeField] private float m_playerInfoXPos = 2.0f;
        [SerializeField] private float m_playerInfoYPos = -2.0f;

        #region Private State
        private bool m_isMatchActive = false;
        private float m_matchStartTime = 0f;
        private List<PlayerProfileData> m_playerProfiles;
        [SerializeField] private List<Transform> m_tempVisibleTransform = new();
        #endregion

        private void OnEnable()
        {
            m_playerProfiles = PlayerProfileManager.Instance.PlayerProfiles;

            if (LocalMatchManager.Instance != null)
            {
                CameraManager.InitializeMatchUI += InitializeUI;
                LocalMatchManager.Instance.OnScoreChanged += UpdateScoreDisplays;
                LocalMatchManager.Instance.OnRoundChanged += UpdateRoundDisplay;
            }

            Ball.OnFirstServe += OnMatchStarted;
        }

        private void OnDisable()
        {
            if (LocalMatchManager.Instance != null)
            {
                CameraManager.InitializeMatchUI -= InitializeUI;
                LocalMatchManager.Instance.OnScoreChanged -= UpdateScoreDisplays;
                LocalMatchManager.Instance.OnRoundChanged -= UpdateRoundDisplay;
            }

            Ball.OnFirstServe -= OnMatchStarted;
        }

        private void Update()
        {
            //The timer runs independently once the match is active.
            if (m_isMatchActive)
                DisplayTime(Time.time - m_matchManager.MatchStartTime);
        }

        /// <summary>
        /// Triggered once by the LocalMatchManager after all data is ready.
        /// </summary>
        internal void InitializeUI()
        {
            //Get the current Settings
            var graphicData = SettingsManager.Instance.CurrentSettings.Graphic;
            var matchData = SettingsManager.Instance.CurrentSettings.Match;

            //Set up player names and visibility
            for (int i = 0; i < m_playerDetailsParent.Count; i++)
            {
                bool isPlayerActive = i < matchData.PlayerCount;
                m_playerDetailsParent[i].gameObject.SetActive(isPlayerActive);
                if (isPlayerActive)
                {
                    m_playerNames[i].text = m_playerProfiles[i].PlayerName;
                }
            }

            //Set up initial scores and round display
            UpdateScoreDisplays(0, 0);
            UpdateRoundDisplay(1);

            //Position the UI elements based on the camera setup
            PositionPlayerUI(matchData.PlayerCount);
        }

        #region Event-Handler-Methods
        private void UpdateScoreDisplays(int _scoreTeam1, int _scoreTeam2)
        {
            //Team 1 consists of player(Indices) 0 and 2
            m_playerTotalPoints[0].text = $"Total: {_scoreTeam1}";
            m_playerTotalPoints[2].text = $"Total: {_scoreTeam1}";

            //Team 2 consists of player(Indices) 1 and 3
            m_playerTotalPoints[1].text = $"Total: {_scoreTeam2}";
            m_playerTotalPoints[3].text = $"Total: {_scoreTeam2}";

            m_pointsVsPoints.text = $"{_scoreTeam1} : {_scoreTeam2}";     //May move this in an extra method, should it be necessary.
        }

        private void UpdateRoundDisplay(int currentRound)
        {
            m_roundNr.text = $"Round {currentRound}";
        }

        private void OnMatchStarted()
        {
            m_isMatchActive = true;
            m_matchStartTime = Time.time;
        }
        #endregion

        #region UI-Positioning
        /// <summary>
        /// Handles the positioning of the player UI panels for your custom SplitScreen.
        /// </summary>
        private void PositionPlayerUI(int _playerCount)
        {
            var panelRects = new List<RectTransform>();
            foreach (var panel in m_playerDetailsParent)
            {
                panelRects.Add(panel.GetComponent<RectTransform>());
            }

            switch (_playerCount)
            {
                case 1:
                {
                    //SingleCam
                    panelRects[0].anchorMin = new Vector2(0, 1);        //Left ScreenBorder, Upper ScreenBorder
                    panelRects[0].anchorMax = new Vector2(0, 1);
                    panelRects[0].pivot = new Vector2(0, 1);
                    panelRects[0].anchoredPosition = new Vector2(m_playerInfoXPos, -m_playerInfoYPos);

                    UpdateVisibleTransformList(m_playerDetailsParent[0]);
                    break;
                }
                case 2:
                {
                    var graphCamMode = SettingsManager.Instance.CurrentSettings.Graphic.splitDdValue;
                    switch (graphCamMode)
                    {
                        case 0:
                        {
                            //Vertical
                            panelRects[0].anchorMin = new Vector2(0, 1);        //Left ScreenBorder, Upper ScreenBorder
                            panelRects[0].anchorMax = new Vector2(0, 1);
                            panelRects[0].pivot = new Vector2(0, 1);
                            panelRects[0].anchoredPosition = new Vector2(m_playerInfoXPos, -m_playerInfoYPos);

                            panelRects[1].anchorMin = new Vector2(0.5f, 1);     //Middle ScreenWidth, Upper ScreenBorder
                            panelRects[1].anchorMax = new Vector2(0.5f, 1);
                            panelRects[1].pivot = new Vector2(0, 1);
                            panelRects[1].anchoredPosition = new Vector2(m_playerInfoXPos, -m_playerInfoYPos);

                            UpdateVisibleTransformList(m_playerDetailsParent[0], m_playerDetailsParent[1]);
                            break;
                        }
                        default:
                        case 1:
                        {
                            //Horizontal
                            panelRects[0].anchorMin = new Vector2(0, 0.5f);     //Left ScreenBorder, Middle ScreenHeight
                            panelRects[0].anchorMax = new Vector2(0, 0.5f);
                            panelRects[0].pivot = new Vector2(0, 1);
                            panelRects[0].anchoredPosition = new Vector2(m_playerInfoXPos, m_playerInfoYPos);

                            panelRects[1].anchorMin = new Vector2(0, 1);        //Left ScreenBorder, Upper ScreenBorder
                            panelRects[1].anchorMax = new Vector2(0, 1);
                            panelRects[1].pivot = new Vector2(0, 1);
                            panelRects[1].anchoredPosition = new Vector2(m_playerInfoXPos, -m_playerInfoYPos);

                            UpdateVisibleTransformList(m_playerDetailsParent[0], m_playerDetailsParent[1]);
                            break;
                        }
                    }

                    ////Old Horizontal
                    //panelRects[0].anchorMin = new Vector2(0, 0.5f);     //Left ScreenBorder, Middle ScreenHeight
                    //panelRects[0].anchorMax = new Vector2(0, 0.5f);
                    //panelRects[0].pivot = new Vector2(0, 1);
                    //panelRects[0].anchoredPosition = new Vector2(m_playerInfoXPos, m_playerInfoYPos);

                    //panelRects[1].anchorMin = new Vector2(0, 1);        //Left ScreenBorder, Upper ScreenBorder
                    //panelRects[1].anchorMax = new Vector2(0, 1);
                    //panelRects[1].pivot = new Vector2(0, 1);
                    //panelRects[1].anchoredPosition = new Vector2(m_playerInfoXPos, -m_playerInfoYPos);

                    //UpdateVisibleTransformList(m_playerDetailsParent[0], m_playerDetailsParent[1]);
                    break;
                }
                //case ECameraModi.Vertical:
                //{
                //    //panelRects[0].anchorMin = new Vector2(0, 1);        //Left ScreenBorder, Upper ScreenBorder
                //    //panelRects[0].anchorMax = new Vector2(0, 1);
                //    //panelRects[0].pivot = new Vector2(0, 1);
                //    //panelRects[0].anchoredPosition = new Vector2(m_playerInfoXPos, -m_playerInfoYPos);

                //    //panelRects[1].anchorMin = new Vector2(0.5f, 1);     //Middle ScreenWidth, Upper ScreenBorder
                //    //panelRects[1].anchorMax = new Vector2(0.5f, 1);
                //    //panelRects[1].pivot = new Vector2(0, 1);
                //    //panelRects[1].anchoredPosition = new Vector2(m_playerInfoXPos, -m_playerInfoYPos);

                //    //UpdateVisibleTransformList(m_playerDetailsParent[0], m_playerDetailsParent[1]);
                //    break;
                //}
                case 4:
                {
                    //Quartet
                    panelRects[0].anchorMin = new Vector2(0, 0.5f);     //Left ScreenBorder, Middle ScreenHeight
                    panelRects[0].anchorMax = new Vector2(0, 0.5f);
                    panelRects[0].pivot = new Vector2(0, 1);
                    panelRects[0].anchoredPosition = new Vector2(m_playerInfoXPos, m_playerInfoYPos);

                    panelRects[1].anchorMin = new Vector2(0.5f, 0.5f);  //Middle ScreenWidth, Middle ScreenHeight
                    panelRects[1].anchorMax = new Vector2(0.5f, 0.5f);
                    panelRects[1].pivot = new Vector2(0, 1);
                    panelRects[1].anchoredPosition = new Vector2(m_playerInfoXPos, -m_playerInfoYPos);

                    panelRects[2].anchorMin = new Vector2(0, 1);        //Left ScreenBorder, Upper ScreenBorder
                    panelRects[2].anchorMax = new Vector2(0, 1);
                    panelRects[2].pivot = new Vector2(0, 1);
                    panelRects[2].anchoredPosition = new Vector2(m_playerInfoXPos, -m_playerInfoYPos);

                    panelRects[3].anchorMin = new Vector2(0.5f, 1);     //Middle ScreenWidth, Upper ScreenBorder
                    panelRects[3].anchorMax = new Vector2(0.5f, 1);
                    panelRects[3].pivot = new Vector2(0, 1);
                    panelRects[3].anchoredPosition = new Vector2(m_playerInfoXPos, -m_playerInfoYPos);

                    UpdateVisibleTransformList(m_playerDetailsParent[0], m_playerDetailsParent[1], m_playerDetailsParent[2], m_playerDetailsParent[3]);
                    break;
                }
            }

            UpdatePlayerInfoVisibility();
        }
        #endregion

        private void UpdateVisibleTransformList(Transform _parent1, Transform _parent2 = null, Transform _parent3 = null, Transform _parent4 = null)
        {
            m_tempVisibleTransform.Clear();
            m_tempVisibleTransform.Add(_parent1);
            if (_parent2 != null)
                m_tempVisibleTransform.Add(_parent2);
            if (_parent3 != null)
                m_tempVisibleTransform.Add(_parent3);
            if (_parent4 != null)
                m_tempVisibleTransform.Add(_parent4);
        }

        private void UpdatePlayerInfoVisibility()
        {
            for (int i = 0; i < m_playerDetailsParent.Count; i++)
            {
                if (m_tempVisibleTransform.Contains(m_playerDetailsParent[i].transform))
                {
                    m_playerDetailsParent[i].gameObject.SetActive(true);
                }
                else
                    m_playerDetailsParent[i].gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Source: https://www.youtube.com/watch?v=HmHPJL-OcQE to display the round time counter.
        /// </summary>
        /// <param name="_timeToDisplay"></param>
        private void DisplayTime(float _timeToDisplay)
        {
            #region Optional CountDown with an adjusted Start- and RoundTime.
            //if (!m_timerShallCountUp)
            //{
            //    if (_timeToDisplay < 0)
            //        _timeToDisplay = 0;
            //    else if (!m_showMilliseconds)
            //    {
            //        _timeToDisplay += 1;
            //    }

            //    //Calculating Minutes.
            //    float minutes = Mathf.FloorToInt(_timeToDisplay / 60);
            //    //Calculating Seconds.
            //    float seconds = Mathf.FloorToInt(_timeToDisplay % 60);
            //    //Calculating Milliseconds.
            //    if (m_showMilliseconds)
            //    {
            //        float milliseconds = _timeToDisplay % 1 * 1000;
            //        m_elapsedTime.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
            //    }
            //    else
            //    {
            //        m_elapsedTime.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            //    }
            //}
            //else
            //{
            #endregion
            #region Basic CountUp
            //if (m_timerShallCountUp)
            //{
            //Mathf.FloorToInt(days: '_timeToDisplay / 86400', hours: '_timeToDisplay / 3600' minutes: '_timeToDisplay / 60', seconds: '_timeToDisplay % 60'.
            //Calculating Hours.
            float hours = Mathf.FloorToInt(_timeToDisplay / 3600);
            //Calculating Minutes.
            float minutes = Mathf.FloorToInt(_timeToDisplay / 60 % 60);
            //Calculating Seconds.
            float seconds = Mathf.FloorToInt(_timeToDisplay % 60);
            //Calculating Milliseconds.
            if (m_showMilliseconds)
            {
                float milliseconds = _timeToDisplay % 1 * 1000;
                //m_elapsedTime.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
                m_elapsedTime.text = string.Format("{0:00}:{1:00}:{2:00}:{3:000}", hours, minutes, seconds, milliseconds);
            }
            else
            {
                //m_elapsedTime.text = string.Format("{0:00}:{1:00}", minutes, seconds);
                m_elapsedTime.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
            }
            //}
            #endregion
            //}     //NOTE: Part of region: Optional CountDown with an adjusted Start- and RoundTime!
        }
    }
}