using System;
using ThreeDeePongProto.Shared.AudioManagement;
using ThreeDeePongProto.Shared.Managers;
using ThreeDeePongProto.Shared.PlayerCharacter;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
public class BallController : MonoBehaviour
{
    #region Core Settings
    [Header("Core Settings")]
    [SerializeField] private AudioSource m_ballAudioSource;
    [SerializeField] private Rigidbody m_rigidbody;
    [SerializeField, Range(10.0f, 40.0f)] private float m_initialBaseSpeed = 12.5f;
    [SerializeField, Min(10.0f)] private float m_minBallSpeed = 10.0f;
    [SerializeField] private float m_maxBallSpeed = 40f;
    [SerializeField, Min(0.1f)] private float m_offPaddleAngle = 0.1f;
    [SerializeField, Min(25.0f)] private float m_offWallAngle = 25.0f;
    #endregion

    #region Friction and Acceleration
    [Header("Friction and Acceleration")]
    [SerializeField, Range(0f, 1f)] private float m_paddleMomentumTransfer = 0.5f;
    [SerializeField] private float m_speedDecayRate = 0.5f;
    [SerializeField] private float m_wallFriction = 0.975f;
    #endregion

    #region References
    private LocalMatchManager m_matchManager;
    private Vector3 m_ballPopPosition;
    private Quaternion m_ballPopRotation;
    private int m_trackId = 0, m_firstPlayerID = -1;
    private float m_currentSpeedTarget; //The current target speed the ball is trying to maintain.
    #endregion

    #region Actions
    public static event Action OnHitGoalOne, OnHitGoalTwo;  //LocalMatchManager updates MatchUserInterface with OnScoreChanged.
    public static event Action<int> OnHitPlayer;            //LocalMatchManager keeps track of Players hit by Ball for goal-notifications.
    public static event Action OnFirstServe;                //LocalMatchManager saves MatchStartTime and sets 'MatchHasStarted'-Bool to true.
    public static event Action<ESoundEmittingObjects, EAudioType, int, bool> PlaySpecificAudio;
    //TODO: Audioplay-Structure: (Emitter, AudioSourceSettings (Diegetic/NonDiegetic), Track-ID (if not random), RandomBool);
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

    private void OnEnable()
    {
        AudioManager.LetsRegisterAudioSources(m_ballAudioSource);
        CharacterInputHandler.AKickBall += BallStart;
    }

    private void OnDisable()
    {
        AudioManager.LetsRemoveAudioSources(m_ballAudioSource);
        CharacterInputHandler.AKickBall -= BallStart;
    }

    private void FixedUpdate()
    {
        //STRICTLY limit Ball-GameObjects velocity to defined min- and max-values. But only, if the ball is actually moving.
        if (m_rigidbody.velocity.sqrMagnitude > 0.1f)
        {
            ApplySpeedDecay();
            EnforceSpeedLimits();
        }
    }

    #region State Management
    private void ResetBall()
    {
        //Rigibody-reset must happen first, or movementSpeed could be added multiple times and it's rotation would stay changed.
        m_rigidbody.velocity = Vector3.zero;
        m_rigidbody.position = m_ballPopPosition;
        m_rigidbody.rotation = m_ballPopRotation;
        m_firstPlayerID = -1;
        m_currentSpeedTarget = m_initialBaseSpeed; //Reset target speed to the startValue.
    }


    private void BallStart(int _masterID)
    {
        int firstPlayerID = -1;

        //Garanty to only apply the force on the ball only once.
        if (!m_matchManager.GameIsPaused && m_firstPlayerID == firstPlayerID)
        {
            m_firstPlayerID = _masterID;
            if (!m_matchManager.MatchIsActive)
                OnFirstServe?.Invoke();

            ApplyForceOnBall();
        }
    }

    private void ApplyForceOnBall()
    {
        if (m_rigidbody.velocity != Vector3.zero) return;

        //Determine a random direction, avoiding straight horizontal/vertical.
        float randomAngle = GetRandomServeAngle();
        
        //Convert angle to a direction vector (assuming Y is up, playing on X/Z plane).
        Vector3 serveDirection = new(Mathf.Sin(randomAngle * Mathf.Deg2Rad), 0, Mathf.Cos(randomAngle * Mathf.Deg2Rad));

        m_currentSpeedTarget = m_initialBaseSpeed;
        m_rigidbody.velocity = serveDirection * m_currentSpeedTarget;
        
        //In AudioManager: (AudioType, EAudioType 2D/3D, List/Array-ID, Track-ID (if not random), SpatialBlend, RandomBool);.
        PlaySpecificAudio?.Invoke(ESoundEmittingObjects.Ball, EAudioType.NonDiegetic, m_trackId, false);    //BallstartSound
    }

    private float GetRandomServeAngle()
    {
        int sideChoice = UnityEngine.Random.Range(0, 4);
        return sideChoice switch
        {
            0 => UnityEngine.Random.Range(m_offPaddleAngle, 90f - m_offWallAngle),
            1 => UnityEngine.Random.Range(90f + m_offWallAngle, 180f - m_offPaddleAngle),
            2 => UnityEngine.Random.Range(180f + m_offPaddleAngle, 270f - m_offWallAngle),
            _ => UnityEngine.Random.Range(270f + m_offWallAngle, 360f - m_offPaddleAngle),
        };
    }
    #endregion

    #region Physics & Collision Logic
    private void ApplySpeedDecay()
    {
        //Whenever the current ballSpeed gets higher than the targetSpeed,...
        if (m_currentSpeedTarget > m_initialBaseSpeed)
        {
            //reduce the ballSpeed over time and
            m_currentSpeedTarget -= m_speedDecayRate * Time.fixedDeltaTime;

            //keep the ballSpeed from falling lower than the initialBaseSpeed.
            m_currentSpeedTarget = Mathf.Max(m_currentSpeedTarget, m_initialBaseSpeed);
        }
    }

    private void EnforceSpeedLimits()
    {
        //Clamp the target speed between the defined min- and max-value.
        m_currentSpeedTarget = Mathf.Clamp(m_currentSpeedTarget, m_minBallSpeed, m_maxBallSpeed);

        //Force the Rigidbody to maintain this exact speed along its current trajectory.
        m_rigidbody.velocity = m_rigidbody.velocity.normalized * m_currentSpeedTarget;
    }

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.CompareTag("GoalOne"))
        {
            //LocalMatchManager: WinCondition-Check & increase Points of Team2/Player2 - MatchUserInterface: Updates MatchUI - PlayerControls: Resets Paddle on Goal
            OnHitGoalOne?.Invoke();
            HandleGoalScored();
        }
        else if (_other.CompareTag("GoalTwo"))
        {
            //LocalMatchManager: WinCondition-Check & increase Points of Team1/Player1 - MatchUserInterface: Updates MatchUI - PlayerControls: Resets Paddle on Goal
            OnHitGoalTwo?.Invoke();
            HandleGoalScored();
        }
    }

    private void HandleGoalScored()
    {
        //In AudioManager: (AudioType, EAudioType 2D/3D, List/Array-ID, Track-ID (if not random), SpatialBlend, RandomBool);
        PlaySpecificAudio?.Invoke(ESoundEmittingObjects.Ball, EAudioType.NonDiegetic, m_trackId, false);
        ResetBall();
    }

    private void OnCollisionEnter(Collision _collision)
    {
        GameObject hitObject = _collision.gameObject;

        //On Paddles.
        if (hitObject.CompareTag("TpOne") || hitObject.CompareTag("TpTwo"))
        {
            HandlePlayerCollision(_collision);
        }
        //On Walls.
        else if (hitObject.CompareTag("EastWall") || hitObject.CompareTag("WestWall"))
        {
            ApplyWallFriction();
        }
        #if UNITY_EDITOR
        Debug.Log($"Rb velocity: {m_rigidbody.velocity} | Limit: {m_currentSpeedTarget}");
        #endif
    }

    private void HandlePlayerCollision(Collision _collision)
    {
        PlaySpecificAudio?.Invoke(ESoundEmittingObjects.Ball, EAudioType.NonDiegetic, m_trackId, false);

        if(_collision.transform.parent.TryGetComponent<CharacterMovement>(out var player))
        {
            OnHitPlayer?.Invoke(player.m_playerID);

            if (player.TryGetComponent<Rigidbody>(out var playerRb))
            {
                //Get players velocity.
                float playerImpact = playerRb.velocity.magnitude;
                //Increase BallSpeed based on playerImpact.
                m_currentSpeedTarget += playerImpact * m_paddleMomentumTransfer;
                Debug.Log($"BallSpeed after hitting a Player: {m_rigidbody.velocity}");                
            }
        }
    }

    private void ApplyWallFriction()
    {
        PlaySpecificAudio?.Invoke(ESoundEmittingObjects.Ball, EAudioType.Diegetic, m_trackId, false);
            
        m_currentSpeedTarget *= m_wallFriction;
        Debug.Log($"BallSpeed after hitting a wall: {m_rigidbody.velocity}");
    }
    #endregion
}