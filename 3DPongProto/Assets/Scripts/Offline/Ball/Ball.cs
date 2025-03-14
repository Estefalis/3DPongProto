using System;
using ThreeDeePongProto.Shared.AudioManagement;
using ThreeDeePongProto.Shared.Managers;
using ThreeDeePongProto.Shared.PlayerCharacter;
using UnityEngine;

public class Ball : MonoBehaviour
{
    #region #region SerializeField-Member-Variables
    [SerializeField] private Rigidbody m_rigidbody;
    [SerializeField] private float m_impulseForce;
    [SerializeField, Min(25.0f)] private float m_offWallAngle = 25.0f;
    [SerializeField, Min(0.1f)] private float m_offPaddleAngle = 0.1f;
    //[SerializeField] float m_onContactAddUp = 1.10f;
    #endregion

    #region Script-References
    private LocalMatchManager m_matchManager;
    [SerializeField] private AudioSource m_ballAudioSource;
    private int m_trackId = 0;
    #endregion

    #region #region Non-SerializeField-Member-Variables
    private Vector3 m_ballPopPosition;
    private Quaternion m_ballPopRotation;

    private readonly string m_goalOne = "GoalOne";
    private readonly string m_goalTwo = "GoalTwo";
    private readonly string m_teamPlayerOne = "TpOne";
    private readonly string m_teamPlayerTwo = "TpTwo";
    //[SerializeField] private readonly string m_teamPlayerThree = "TpThree";
    //[SerializeField] private readonly string m_teamPlayerFour = "TpFour";
    private readonly string m_eastWall = "EastWall";
    private readonly string m_westWall = "WestWall";
    #endregion

    #region Actions
    public static event Action HitGoalOne, HitGoalTwo;  //Updates UserInterface in MatchUserInterface.cs
    public static event Action RoundCountStarts;        //LocalMatchManager saves MatchStartTime and sets 'MatchHasStarted'-Bool to true.

    //TODO: Audioplay-Structure: (Emitter, AudioSourceSettings (Diegetic/NonDiegetic), Track-ID (if not random), RandomBool);
    public static event Action<ESoundEmittingObjects, EAudioType, int, bool> PlaySpecificAudio;
    #endregion

    private void Awake()
    {
        m_matchManager = FindObjectOfType<LocalMatchManager>();

        if (m_rigidbody == null)
            m_rigidbody = GetComponent<Rigidbody>();

        if (m_ballAudioSource == null)
            m_ballAudioSource = GetComponent<AudioSource>();

        m_ballPopPosition = m_rigidbody.position;
        m_ballPopRotation = m_rigidbody.rotation;
    }

    private void OnDisable()
    {
        AudioManager.LetsRemoveAudioSources(m_ballAudioSource);

        CharacterInputHandler.AKickBall -= BallStart;
    }

    private void Start()
    {
        AudioManager.LetsRegisterAudioSources(m_ballAudioSource);

        CharacterInputHandler.AKickBall += BallStart;
    }

    private void ResetBall()
    {
        //Rigibody-reset must happen first, or movementSpeed could be added multiple times and it's rotation would stay changed.
        m_rigidbody.velocity = Vector3.zero;
        m_rigidbody.position = m_ballPopPosition;
        m_rigidbody.rotation = m_ballPopRotation;
    }

    private void BallStart(int _masterID)
    {
        if (!m_matchManager.GameIsPaused && _masterID == 0) //Host on Online-Gaming?
        {
            if (!m_matchManager.MatchStarted)
                RoundCountStarts?.Invoke(); //Has to stay below 'ApplyForceOnBall();', if the ApplyForce button is the same as Submit/Click as now.

            ApplyForceOnBall();
        }
    }

    private void ApplyForceOnBall()
    {
        if (m_rigidbody.velocity != Vector3.zero)   //Only apply force on the ball, if it stand still( in the middle of the field).
            return;

        int sideChoice;
        if (DateTime.Now.Millisecond < 500)
            sideChoice = Mathf.FloorToInt(UnityEngine.Random.Range(0, 4) + DateTime.Now.Millisecond / 1000);
        else
            sideChoice = Mathf.RoundToInt(UnityEngine.Random.Range(0, 4) + DateTime.Now.Millisecond / 1000);

        transform.eulerAngles = sideChoice switch
        {
            //0 = directly to the playerTwo-paddle, 90 = directly to the eastWall.
            0 => new Vector3(transform.eulerAngles.x, UnityEngine.Random.Range(0 + m_offPaddleAngle, 90 - m_offWallAngle), transform.eulerAngles.z),
            //90 = directly to the eastWall, 180 = directly to the playerOne-paddle.
            1 => new Vector3(transform.eulerAngles.x, UnityEngine.Random.Range(90 + m_offWallAngle, 180 - m_offPaddleAngle), transform.eulerAngles.z),
            //180 = directly to the playerOne-paddle, 270 = directly to the westWall.
            2 => new Vector3(transform.eulerAngles.x, UnityEngine.Random.Range(180 + m_offPaddleAngle, 270 - m_offWallAngle), transform.eulerAngles.z),
            //270 = directly to the westWall, (36)0 = directly to the playerTwo-paddle.
            3 => new Vector3(transform.eulerAngles.x, UnityEngine.Random.Range(270 + m_offWallAngle, 360 - m_offPaddleAngle), transform.eulerAngles.z),
            //Default case.
            _ => new Vector3(transform.eulerAngles.x, UnityEngine.Random.Range(0 + m_offPaddleAngle, 90 - m_offWallAngle), transform.eulerAngles.z),
        };

        m_rigidbody.AddRelativeForce(transform.forward * m_impulseForce, ForceMode.Impulse);

        //In AudioManager: (AudioType, EAudioType 2D/3D, List/Array-ID, Track-ID (if not random), SpatialBlend, RandomBool);
        PlaySpecificAudio?.Invoke(ESoundEmittingObjects.Ball, EAudioType.NonDiegetic, m_trackId, false);   //BallstartSound
    }

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.gameObject.CompareTag(m_goalOne))
        {
            //LocalMatchManager: WinCondition-Check & increases Match-Points of PlayerCharacter/Team 2 - MatchUserInterface: Updates MatchUI - PlayerControls: Resets Paddle on Goal.
            HitGoalOne?.Invoke();
            //In AudioManager: (AudioType, EAudioType 2D/3D, List/Array-ID, Track-ID (if not random), SpatialBlend, RandomBool);
            PlaySpecificAudio?.Invoke(ESoundEmittingObjects.Ball, EAudioType.NonDiegetic, m_trackId, false);
            ResetBall();
        }

        if (_other.gameObject.CompareTag(m_goalTwo))
        {
            //LocalMatchManager: WinCondition-Check & increases Match-Points of PlayerCharacter/Team 1 - MatchUserInterface: Updates MatchUI - PlayerControls: Resets Paddle on Goal.
            HitGoalTwo?.Invoke();
            //In AudioManager: (AudioType, EAudioType 2D/3D, List/Array-ID, Track-ID (if not random), SpatialBlend, RandomBool);
            PlaySpecificAudio?.Invoke(ESoundEmittingObjects.Ball, EAudioType.NonDiegetic, m_trackId, false);
            ResetBall();
        }
    }

    private void OnCollisionEnter(Collision _collision)
    {
        if (_collision.gameObject.CompareTag(m_teamPlayerOne))
        {
            //In AudioManager: (AudioType, EAudioType 2D/3D, List/Array-ID, Track-ID (if not random), SpatialBlend, RandomBool);
            PlaySpecificAudio?.Invoke(ESoundEmittingObjects.Ball, EAudioType.NonDiegetic, m_trackId, false);
        }

        if (_collision.gameObject.CompareTag(m_teamPlayerTwo))
        {
            //In AudioManager: (AudioType, EAudioType 2D/3D, List/Array-ID, Track-ID (if not random), SpatialBlend, RandomBool);
            PlaySpecificAudio?.Invoke(ESoundEmittingObjects.Ball, EAudioType.NonDiegetic, m_trackId, false);
        }

        if (_collision.gameObject.CompareTag(m_eastWall))
        {
            //In AudioManager: (AudioType, EAudioType 2D/3D, List/Array-ID, Track-ID (if not random), SpatialBlend, RandomBool);
            PlaySpecificAudio?.Invoke(ESoundEmittingObjects.Ball, EAudioType.Diegetic, m_trackId, false);
        }

        if (_collision.gameObject.CompareTag(m_westWall))
        {
            //In AudioManager: (AudioType, EAudioType 2D/3D, List/Array-ID, Track-ID (if not random), SpatialBlend, RandomBool);
            PlaySpecificAudio?.Invoke(ESoundEmittingObjects.Ball, EAudioType.Diegetic, m_trackId, false);
        }

        //TODO: PlayerCharacter 1 & 2, if AdditionalSpeed (m_onContactAddUp) shall be applied?
        //if (_collision.gameObject.CompareTag("PlayerCharacter"))
        //{
        //    m_rigidbody.AddForce(_collision.GetContact(0).normal * m_onContactAddUp, ForceMode.Impulse);
        //}
    }
}