using System.Collections.Generic;
using ThreeDeePongProto.Shared.Managers;
using ThreeDeePongProto.Offline.CameraSetup;
using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace ThreeDeePongProto.Offline.UI
{
    public class MatchUserInterface : MonoBehaviour
    {
        #region Script-References
        [SerializeField] private LocalMatchManager m_matchManager;
        [SerializeField] private CameraManager m_cameraManager;
        #endregion

        #region SerializeField-Member-Variables
        [Header("Player-Informations")]
        [SerializeField] private List<Transform> m_playerDetailsParent;
        [SerializeField] private List<TextMeshProUGUI> m_playerNames;
        [SerializeField] private List<TextMeshProUGUI> m_playerTotalPoints;
        [SerializeField] private List<GameObject> m_playerAvatarList = new();
        [SerializeField] private float m_playerInfoXPos = 2.0f;
        [SerializeField] private float m_playerInfoYPos = -2.0f;

        [Header("Round-Details")]
        [SerializeField] private TextMeshProUGUI m_roundNr;
        [SerializeField] private TextMeshProUGUI m_elapsedTime;
        [SerializeField] private TextMeshProUGUI m_pointsVsPoints;

        [SerializeField] private bool m_timerShallCountUp = true;
        [SerializeField] private bool m_showMilliseconds = false;

        [Header("Music-Details")]
        [SerializeField] private TextMeshProUGUI m_artistNames;
        [SerializeField] private TextMeshProUGUI m_songTitle;
        [SerializeField] private Image m_songImage;

        #region Scriptable Variables
        [SerializeField] private MatchValues m_matchValues;
        [SerializeField] private GraphicUIStates m_graphicUiStates;
        #endregion
        #endregion

        List<Transform> m_tempVisibleTransform = new();

        private Dictionary<List<TextMeshProUGUI>, List<TextMeshProUGUI>> m_playerPointsConnection = new Dictionary<List<TextMeshProUGUI>, List<TextMeshProUGUI>>();

        private void OnEnable()
        {
            //TODO: COULD HAVE - Eventually code to keep the 'source image width' equal to it's height.
            m_playerPointsConnection.Add(m_playerNames, m_playerTotalPoints);

            Ball.OnHitGoalOne += UpdateUserInterface;
            Ball.OnHitGoalTwo += UpdateUserInterface;
            LocalMatchManager.AStartNextRound += UpdateUserInterface;
        }

        private void OnDisable()
        {
            Ball.OnHitGoalOne -= UpdateUserInterface;
            Ball.OnHitGoalTwo -= UpdateUserInterface;
            LocalMatchManager.AStartNextRound -= UpdateUserInterface;
        }

        private IEnumerator Start()
        {
            //if (m_matchValues == null)
            //    return;

            yield return new WaitUntil(DelegateBool);
            DisplayPlayerNames();
            UpdateRoundTMPs();
            UpdatePlayerTMPs();

            yield return new WaitForSeconds(0.0000000000001f);  //Minimal delay to give the 'm_playerParentTransforms' time to get set on the correct position.
            SetPlayerInfoPositions(SettingsManager.Instance.CurrentSettings.Graphic.CameraMode);
        }

        /// <summary>
        /// Only returns true, after the activated PlayerCameras added themselves to the 'AvailableCameras'-List equal to the registered PlayerSOData.
        /// </summary>
        /// <returns></returns>
        private bool DelegateBool()
        {
            if (m_cameraManager.AvailableCameras.Count == m_matchValues.PlayerSOData.Count)
                return true;
            else if (m_matchValues == null)
                return false;
            else
                return false;
        }

        private void Update()
        {
            if (m_matchManager.MatchStarted)
                DisplayTime(Time.time - m_matchManager.MatchStartTime);
        }

        private void SetPlayerInfoPositions(int _eCameraModi)
        {
            UpdatePlayerInfoPositions(_eCameraModi, CameraManager.RuntimeFullsizeRect);
        }

        private void UpdatePlayerInfoPositions(int _eCameraModi, Rect _runtimeFullsizeRect)
        {
            switch ((ECameraModi)_eCameraModi)
            {
                case ECameraModi.SingleCam:
                {
                    m_playerDetailsParent[0].position = new Vector3(0, _runtimeFullsizeRect.height, 0);
                    UpdateVisibleTransformList(m_playerDetailsParent[0]);
                    break;
                }
                case ECameraModi.Vertical:
                {
                    m_playerDetailsParent[0].position = new Vector3(0 + m_playerInfoXPos, _runtimeFullsizeRect.height + m_playerInfoYPos, 0);
                    m_playerDetailsParent[1].position = new Vector3(_runtimeFullsizeRect.width * 0.5f + m_playerInfoXPos, _runtimeFullsizeRect.height + m_playerInfoYPos, 0);
                    UpdateVisibleTransformList(m_playerDetailsParent[0], m_playerDetailsParent[1]);
                    break;
                }
                case ECameraModi.Horizontal:
                {
                    m_playerDetailsParent[0].position = new Vector3(0 + m_playerInfoXPos, _runtimeFullsizeRect.height * 0.5f + m_playerInfoYPos, 0);
                    m_playerDetailsParent[1].position = new Vector3(0 + m_playerInfoXPos, _runtimeFullsizeRect.height + m_playerInfoYPos, 0);
                    UpdateVisibleTransformList(m_playerDetailsParent[0], m_playerDetailsParent[1]);
                    break;
                }
                case ECameraModi.Quartet:
                {
                    m_playerDetailsParent[0].position = new Vector3(0 + m_playerInfoXPos, _runtimeFullsizeRect.height * 0.5f + m_playerInfoYPos, 0);
                    m_playerDetailsParent[1].position = new Vector3(_runtimeFullsizeRect.width * 0.5f + m_playerInfoXPos, _runtimeFullsizeRect.height * 0.5f + m_playerInfoYPos, 0);
                    m_playerDetailsParent[2].position = new Vector3(0 + m_playerInfoXPos, _runtimeFullsizeRect.height + m_playerInfoYPos, 0);
                    m_playerDetailsParent[3].position = new Vector3(_runtimeFullsizeRect.width * 0.5f + m_playerInfoXPos, _runtimeFullsizeRect.height + m_playerInfoYPos, 0);
                    UpdateVisibleTransformList(m_playerDetailsParent[0], m_playerDetailsParent[1], m_playerDetailsParent[2], m_playerDetailsParent[3]);
                    break;
                }
            }

            UpdatePlayerInfoVisibility();
        }

        private void UpdateVisibleTransformList(Transform _parent1, Transform _parent2 = null, Transform _parent3 = null, Transform _parent4 = null)
        {
            m_tempVisibleTransform.Clear();
            m_tempVisibleTransform.Add(_parent1);
            m_tempVisibleTransform.Add(_parent2);
            m_tempVisibleTransform.Add(_parent3);
            m_tempVisibleTransform.Add(_parent4);
        }

        private void UpdatePlayerInfoVisibility()
        {
            for (int i = 0; i < m_playerDetailsParent.Count; i++)
            {
                if (m_playerDetailsParent[i] == m_tempVisibleTransform[i])
                    m_playerDetailsParent[i].gameObject.SetActive(true);
                else
                    m_playerDetailsParent[i].gameObject.SetActive(false);
            }
        }

        private void UpdateUserInterface()
        {
            if (m_matchValues == null)
            {
#if UNITY_EDITOR
                Debug.Log("Forgot to add the Scriptable Object in the Editor!");
#endif
                return;
            }

            UpdateRoundTMPs();
            UpdatePlayerTMPs();
        }

        private void DisplayPlayerNames()
        {
            for (int i = 0; i < m_matchValues.PlayerSOData.Count; i++)
            {
                if (m_matchValues.PlayerSOData[i] != null)
                    m_playerNames[i].text = m_matchValues.PlayerSOData[i].PlayerName;
            }
        }

        private void UpdateRoundTMPs()
        {
            m_roundNr.text = $"Round {m_matchValues.CurrentRoundNr}";
            m_pointsVsPoints.text = $"{m_matchValues.MatchPointsTPOne} : {m_matchValues.MatchPointsTPTwo}";
        }

        private void UpdatePlayerTMPs()
        {
            List<TextMeshProUGUI> playerTotalPointsTMP = m_playerPointsConnection[m_playerNames];

            playerTotalPointsTMP[0].text = $"Total: {m_matchValues.TotalPointsTPOne}";
            playerTotalPointsTMP[1].text = $"Total: {m_matchValues.TotalPointsTPTwo}";
            playerTotalPointsTMP[2].text = $"Total: {m_matchValues.TotalPointsTPOne}";
            playerTotalPointsTMP[3].text = $"Total: {m_matchValues.TotalPointsTPTwo}";
        }

        /// <summary>
        /// Source: https://www.youtube.com/watch?v=HmHPJL-OcQE to display the round time counter.
        /// </summary>
        /// <param name="_timeToDisplay"></param>
        private void DisplayTime(float _timeToDisplay)
        {
            #region Optional CountDown with an adjusted Start- and RoundTime.
            if (!m_timerShallCountUp)
            {
                if (_timeToDisplay < 0)
                    _timeToDisplay = 0;
                else if (!m_showMilliseconds)
                {
                    _timeToDisplay += 1;
                }

                //Calculating Minutes.
                float minutes = Mathf.FloorToInt(_timeToDisplay / 60);
                //Calculating Seconds.
                float seconds = Mathf.FloorToInt(_timeToDisplay % 60);
                //Calculating Milliseconds.
                if (m_showMilliseconds)
                {
                    float milliseconds = _timeToDisplay % 1 * 1000;
                    m_elapsedTime.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
                }
                else
                {
                    m_elapsedTime.text = string.Format("{0:00}:{1:00}", minutes, seconds);
                }
            }
            #endregion
            else
            {
                if (m_timerShallCountUp)
                {
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
                }
            }
        }
    }
}