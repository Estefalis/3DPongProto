using System.Collections.Generic;
using ThreeDeePongProto.Offline.Managers;
using ThreeDeePongProto.Offline.CameraSetup;
using TMPro;
using UnityEngine;

namespace ThreeDeePongProto.Offline.UI
{
    public class MatchUserInterface : MonoBehaviour
    {
        #region Script-References
        [SerializeField] private MatchManager m_matchManager;
        #endregion

        #region SerializeField-Member-Variables
        [Header("Player-Informations")]
        [SerializeField] private List<Transform> m_playerParentTransform;
        [SerializeField] private List<TextMeshProUGUI> m_playerNamesTMPList;
        [SerializeField] private List<TextMeshProUGUI> m_totalPointsTMPList;
        [SerializeField] private List<GameObject> m_playerAvatarList = new();
        [SerializeField] private float m_AdjustxPos = 5f;
        [SerializeField] private float m_AdjustyPos = -5f;

        [Header("Round-Details")]
        [SerializeField] private TextMeshProUGUI m_roundNrTMP;
        [SerializeField] private TextMeshProUGUI m_elapsedTimeTMP;
        [SerializeField] private TextMeshProUGUI m_zeroToZeroTMP;

        //[SerializeField] private bool m_timerShallCountUp;
        [SerializeField] private bool m_showMilliseconds = false;

        //[Header("Music-Details")]
        //[SerializeField] private TextMeshProUGUI m_artistNamesTMP;
        //[SerializeField] private TextMeshProUGUI m_songTitleTMP;
        //[SerializeField] private GameObject m_songIcon;

        #region Scriptable Variables
        [SerializeField] private MatchValues m_matchValues;
        [SerializeField] private GraphicUiStates m_graphicUiStates;
        #endregion
        #endregion

        List<Transform> m_tempVisibleTransform = new();

        private Dictionary<List<TextMeshProUGUI>, List<TextMeshProUGUI>> m_playerPointsConnection = new Dictionary<List<TextMeshProUGUI>, List<TextMeshProUGUI>>();

        private void OnEnable()
        {
            //TODO: COULD HAVE - Eventually code to keep the 'source image width' equal to it's height.
            m_playerPointsConnection.Add(m_playerNamesTMPList, m_totalPointsTMPList);

            BallBehaviour.HitGoalOne += UpdateUserInterface;
            BallBehaviour.HitGoalTwo += UpdateUserInterface;
            MatchManager.StartNextRound += UpdateUserInterface;

            if (m_matchValues == null)
                return;

            DisplayPlayerNames();
            UpdateRoundTMPs();
            UpdatePlayerTMPs();
        }

        private void OnDisable()
        {
            BallBehaviour.HitGoalOne -= UpdateUserInterface;
            BallBehaviour.HitGoalTwo -= UpdateUserInterface;
            MatchManager.StartNextRound -= UpdateUserInterface;
        }

        private void Start()
        {
            SetPlayerInfoPositions(m_graphicUiStates.SetCameraMode);
        }

        private void Update()
        {
            if (m_matchManager.MatchStarted)
                DisplayTime(Time.time - m_matchManager.MatchStartTime);
        }

        private void SetPlayerInfoPositions(ECameraModi eCameraModi)
        {
            UpdatePlayerInfoPositions(eCameraModi, CameraManager.RuntimeFullsizeRect);
        }

        private void UpdatePlayerInfoPositions(ECameraModi _eCameraModi, Rect _runtimeFullsizeRect)
        {
            switch (_eCameraModi)
            {
                case ECameraModi.SingleCam:
                {
                    m_playerParentTransform[0].position = new Vector3(0, _runtimeFullsizeRect.height, 0);
                    UpdateVisibleTransformList(m_playerParentTransform[0]);
                    break;
                }
                case ECameraModi.TwoVertical:
                {
                    m_playerParentTransform[0].position = new Vector3(0 + m_AdjustxPos, _runtimeFullsizeRect.height + m_AdjustyPos, 0);
                    m_playerParentTransform[1].position = new Vector3(_runtimeFullsizeRect.width * 0.5f + m_AdjustxPos, _runtimeFullsizeRect.height + m_AdjustyPos, 0);
                    UpdateVisibleTransformList(m_playerParentTransform[0], m_playerParentTransform[1]);
                    break;
                }
                case ECameraModi.TwoHorizontal:
                {
                    m_playerParentTransform[0].position = new Vector3(0 + m_AdjustxPos, _runtimeFullsizeRect.height * 0.5f + m_AdjustyPos, 0);
                    m_playerParentTransform[1].position = new Vector3(0 + m_AdjustxPos, _runtimeFullsizeRect.height + m_AdjustyPos, 0);
                    UpdateVisibleTransformList(m_playerParentTransform[0], m_playerParentTransform[1]);
                    break;
                }
                case ECameraModi.FourSplit:
                {
                    m_playerParentTransform[0].position = new Vector3(0 + m_AdjustxPos, _runtimeFullsizeRect.height * 0.5f + m_AdjustyPos, 0);
                    m_playerParentTransform[1].position = new Vector3(_runtimeFullsizeRect.width * 0.5f + m_AdjustxPos, _runtimeFullsizeRect.height * 0.5f + m_AdjustyPos, 0);
                    m_playerParentTransform[2].position = new Vector3(0 + m_AdjustxPos, _runtimeFullsizeRect.height + m_AdjustyPos, 0);
                    m_playerParentTransform[3].position = new Vector3(_runtimeFullsizeRect.width * 0.5f + m_AdjustxPos, _runtimeFullsizeRect.height + m_AdjustyPos, 0);
                    UpdateVisibleTransformList(m_playerParentTransform[0], m_playerParentTransform[1], m_playerParentTransform[2], m_playerParentTransform[3]);
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
            for (int i = 0; i < m_playerParentTransform.Count; i++)
            {
                if (m_playerParentTransform[i] == m_tempVisibleTransform[i])
                    m_playerParentTransform[i].gameObject.SetActive(true);
                else
                    m_playerParentTransform[i].gameObject.SetActive(false);
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
            for (int i = 0; i < m_matchValues.PlayersInGame.Count; i++)
            {
                if (m_matchValues.PlayersInGame[i] != null)
                m_playerNamesTMPList[i].text = m_matchValues.PlayersInGame[i].PlayerName;
            }
        }

        private void UpdateRoundTMPs()
        {
            m_roundNrTMP.text = $"Round {m_matchValues.CurrentRoundNr}";
            m_zeroToZeroTMP.text = $"{m_matchValues.MatchPointsTPOne} : {m_matchValues.MatchPointsTPTwo}";
        }

        private void UpdatePlayerTMPs()
        {
            List<TextMeshProUGUI> playerTotalPointsTMP = m_playerPointsConnection[m_playerNamesTMPList];

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
            //        m_elapsedTimeTMP.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
            //    }
            //    else
            //    {
            //        m_elapsedTimeTMP.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            //    }
            //}
            //else if (m_timerShallCountUp)
            #endregion
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
                //m_elapsedTimeTMP.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
                m_elapsedTimeTMP.text = string.Format("{0:00}:{1:00}:{2:00}:{3:000}", hours, minutes, seconds, milliseconds);
            }
            else
            {
                //m_elapsedTimeTMP.text = string.Format("{0:00}:{1:00}", minutes, seconds);
                m_elapsedTimeTMP.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
            }
            //}
        }
    }
}